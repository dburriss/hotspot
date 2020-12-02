(**
# Basic usage

`hotspot`

This executes the default command which is **recommend**. It is the equivalent of executing:

`hotspot recommend --output console`

Note: `console` is also the default if no `--output` is provided.

-r|--repository-folder <REPOSITORY>
-t|--target-folder <TARGET>
--scc-file <SCC>
*)

open System
open Hotspot
open Hotspot.Helpers
open Argu

type RecommendArgs =
    | [<AltCommandLine("-r")>] Repository_Directory of repo_dir:string
    | [<AltCommandLine("-t")>] Target_Directory of target_dir:string
    | Scc_File of scc_file_path:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Repository_Directory _ -> "The root of the git repository. Default: Execution directory"
            | Target_Directory _ -> "The directory to run analysis on. Default: <repo_dir>"
            | Scc_File _ -> "The JSON file output of running SCC (example `scc --by-file --format json > scc_out.json`)"
            
type HotSpotCommands =
    | [<CliPrefix(CliPrefix.None)>] Recommend of ParseResults<RecommendArgs>
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Recommend _ -> "Generates recommendations of code hotspots."


[<EntryPoint>]
let main argv =
    //---------------------------------------------------------------------------------------------------------------
    // Bootstrapping
    //---------------------------------------------------------------------------------------------------------------
    let env = AppEnv()

    let defaultIncludeList = ["cs";"fs";]
    let defaultIgnoreFile filePath = defaultIncludeList |> List.contains (filePath |> FileSystem.ext) |> not
    
    //---------------------------------------------------------------------------------------------------------------
    // Commands setup
    //---------------------------------------------------------------------------------------------------------------
    let (|Help|_|) (result : ParseResults<HotSpotCommands>) : unit option =
        if(result.IsUsageRequested) then Some ()
        else None
    
    let (|RecommendCommand|_|) (result : ParseResults<HotSpotCommands>) : RecommendSetting option =
        let cmd = result.TryGetResult HotSpotCommands.Recommend
        match cmd with
        | None -> None
        | Some recommendArgs ->
            let executingFolder = Environment.CurrentDirectory
            Some {
               RepositoryFolder = recommendArgs.TryGetResult RecommendArgs.Repository_Directory |> Option.defaultValue executingFolder
               TargetFolder = recommendArgs.TryGetResult RecommendArgs.Target_Directory |> Option.defaultValue (recommendArgs.TryGetResult RecommendArgs.Repository_Directory |> Option.defaultValue executingFolder)
               SccFile = recommendArgs.TryGetResult RecommendArgs.Scc_File |> Option.defaultValue ""
            }
    //---------------------------------------------------------------------------------------------------------------
    // Execute
    //---------------------------------------------------------------------------------------------------------------
    let parser = ArgumentParser.Create<HotSpotCommands>(programName = "[dotnet] hotspot")
    try
        let result = parser.ParseCommandLine(inputs = argv, raiseOnUsage = false)
        match result with
        | RecommendCommand settings ->
            RecommendCommand.recommendf env settings
            |> ignore
        | _ ->
            parser.PrintUsage() |> TerminalPrint.text
        
        0
    with e ->
        sprintf "%s" e.Message |> TerminalPrint.severe
        printfn ""
        parser.PrintUsage() |> TerminalPrint.text
        1
    
    