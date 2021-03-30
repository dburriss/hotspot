(**
# Basic usage

`hotspot`

This executes the default command which is **recommend**. It is the equivalent of executing:

`hotspot recommend --output console`

Note: `console` is also the default if no `--output` is provided.

-r|--repository-directory <REPOSITORY>
--scc-file <SCC>
*)

open System
open Hotspot
open Argu
open Spectre.IO
open Hotspot.Git
open Hotspot.Helpers

type RecommendArgs =
    | [<AltCommandLine("-r")>] Repository_Directory of repo_dir:string
    | [<AltCommandLine("-i")>] Include of string
    | [<AltCommandLine("-e")>] Excludes of string
    | Scc_File of scc_file_path:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Repository_Directory _ -> "The root of the git repository. Default: Execution directory"
            | Include _ -> "Include glob to test files against."
            | Excludes _ -> "Comma delimited glob to exclude files. By default ignores .dll and .so files."
            | Scc_File _ -> "The JSON file output of running SCC (example `scc --by-file --format json > scc_out.json`)"
            
type HotSpotCommands =
    | [<CliPrefix(CliPrefix.None)>] Recommend of ParseResults<RecommendArgs>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Recommend _ -> "Generates recommendations of code hotspots."
            

[<EntryPoint>]
let main argv =
    let environment = Environment()
    let fileSystem = FileSystem()
    let getIncludeGlob (result : ParseResults<RecommendArgs>) : string =
        result.GetResult(RecommendArgs.Include, defaultValue = "*")//"./**.?s?"
    let getExcludeGlobs (result : ParseResults<RecommendArgs>) : string array =
        match result.TryGetResult(RecommendArgs.Excludes) with
        | None -> IgnoreFile.defaultIgnoreGlobs
        | Some exStr -> String.split [|","|] exStr

    //------------------------------------------------------------------------------------------------------------------
    // Commands setup
    //------------------------------------------------------------------------------------------------------------------
    let (|Help|_|) (result : ParseResults<HotSpotCommands>) : unit option =
        if(result.IsUsageRequested) then Some ()
        else None
    
    let (|RecommendCommand|_|) (result : ParseResults<HotSpotCommands>) : RecommendSetting option =
        let cmd = result.TryGetResult HotSpotCommands.Recommend
        match cmd with
        | None -> None
        | Some recommendArgs ->
            let executingFolder = Environment.CurrentDirectory
            let repositoryDirString = recommendArgs.TryGetResult RecommendArgs.Repository_Directory |> Option.defaultValue executingFolder
            let repositoryDir = fileSystem.Directory.Retrieve(DirectoryPath.FromString(repositoryDirString))
            let sccFileString = recommendArgs.TryGetResult RecommendArgs.Scc_File |> Option.defaultValue ""
            let sccFile = if String.IsNullOrEmpty sccFileString then None else Some (fileSystem.File.Retrieve(FilePath.FromString(sccFileString)))
            let excludes = getExcludeGlobs recommendArgs
            Some {
               RepositoryFolder = repositoryDir
               SccFile = sccFile
               IncludeGlob = getIncludeGlob recommendArgs
               ExcludeGlobs = excludes
            }
    //------------------------------------------------------------------------------------------------------------------
    // Execute
    //------------------------------------------------------------------------------------------------------------------
    let parser = ArgumentParser.Create<HotSpotCommands>(programName = "[dotnet] hotspot")
    try
        let result = parser.ParseCommandLine(inputs = argv, raiseOnUsage = false)
        match result with
        | RecommendCommand settings ->
            let root = settings.RepositoryFolder.Path
            let shouldIgnore = IgnoreFile.init settings.ExcludeGlobs
            let recommendationsCmd = {
                FileSystem = fileSystem;
                CodeRepository = GitCodeRepository(fileSystem, root, settings.IncludeGlob, shouldIgnore, Git());
                Settings = settings
            }
            RecommendUsecase.recommend recommendationsCmd
            |> ignore
        | _ ->
            parser.PrintUsage() |> TerminalPrint.text
        
        0
    with e ->
        sprintf "ERROR in %s" e.TargetSite.Name |> TerminalPrint.severe
        printfn ""
        sprintf "%s" e.Message |> TerminalPrint.severe
        printfn ""
        parser.PrintUsage() |> TerminalPrint.text
        1
    
    