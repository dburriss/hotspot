(**
# Basic usage

`hotspot`

This executes the default command which is **recommend**. It is the equivalent of executing:

`hotspot recommend --out console`

Note: `console` is also the default if no `--out` is provided.
*)

open System
open Hotspot
open Hotspot.Helpers
open McMaster.Extensions.CommandLineUtils

[<EntryPoint>]
let main argv =
    //---------------------------------------------------------------------------------------------------------------
    // Console helpers
    //---------------------------------------------------------------------------------------------------------------
    let enableHelpI (app : CommandLineApplication) = do app.HelpOption() |> ignore
    let optionValue (opt : CommandOption) =
        if opt.HasValue() then Some (opt.Value())
        else None
    //---------------------------------------------------------------------------------------------------------------
    // Usecases
    //---------------------------------------------------------------------------------------------------------------

    // Use case (default): Use LoC & print to console
    let printRecommendations projectFolder =
        Measure.measure RepositoryDependencies.Live projectFolder
        >> Analyse.analyse 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    //---------------------------------------------------------------------------------------------------------------
    // Console setup
    //---------------------------------------------------------------------------------------------------------------
    use app = new CommandLineApplication()
    app |> enableHelpI

    
    let currentPath = Environment.CurrentDirectory
    
    let defaultIncludeList = argv |> Array.tryItem 2 |> Option.defaultValue "cs,fs,js" |> String.split [|","|] |> Array.toList
    let defaultIgnoreFile filePath = defaultIncludeList |> List.contains (filePath |> FileSystem.ext) |> not
    
    //---------------------------------------------------------------------------------------------------------------
    // Options
    //---------------------------------------------------------------------------------------------------------------
    
//    let optionOutOption = app.Option("-o|--out <OUTPUT>",
//                                     "The output for the hotspot command. Options are: console, *.json, *.xml, *.csv",
//                                     CommandOptionType.SingleValue)
    let repositoryFolderOption = app.Option("-r|--repository-dir <REPOSITORY_FOLDER>",
                                         "The repository root of a VCS code repository. DEFAULT: Same dir hotspot is executed in",
                                         CommandOptionType.SingleValue)
    let projectFolderOption = app.Option("-p|--project-dir <PROJECT_FOLDER>",
                                         "If provided will narrow the search to specified directory. DEFAULT: Same as REPOSITORY_FOLDER",
                                         CommandOptionType.SingleValue)
    
    //---------------------------------------------------------------------------------------------------------------
    // Top arg values
    //---------------------------------------------------------------------------------------------------------------
    let repoDir = repositoryFolderOption |> optionValue |> Option.defaultValue currentPath
    let projectFolder = projectFolderOption |> optionValue |> Option.defaultValue "./"
    
    let terminate = function
        | Error err ->
            do eprintfn "%s" err
            -1
        | Ok _ -> 0
    
    //---------------------------------------------------------------------------------------------------------------
    // Commands setup
    //---------------------------------------------------------------------------------------------------------------
    let repository =  repoDir |> Repository.init RepositoryDependencies.Live defaultIgnoreFile    
    let cmd = app.Command("recommend",
                fun (cmd : CommandLineApplication) ->
                    cmd.OnExecute(fun i -> repository |> Result.map (printRecommendations projectFolder) |> terminate))
    

    app.OnExecute(fun () ->
        app.AddSubcommand(cmd)
    )
    
    //---------------------------------------------------------------------------------------------------------------
    // Execute
    //---------------------------------------------------------------------------------------------------------------
    app.Execute(argv)
    
    
