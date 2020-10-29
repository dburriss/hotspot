(**
# Basic usage

`hotspot`

This executes the default command which is **recommend**. It is the equivalent of executing:

`hotspot recommend --output console`

Note: `console` is also the default if no `--output` is provided.
*)

open System
open Hotspot
open Hotspot.Helpers
open Spectre.Cli

[<EntryPoint>]
let main argv =
    //---------------------------------------------------------------------------------------------------------------
    // Console helpers
    //---------------------------------------------------------------------------------------------------------------
    
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
    let app = CommandApp()
    
    let defaultIncludeList = ["cs";"fs";"js"]
    let defaultIgnoreFile filePath = defaultIncludeList |> List.contains (filePath |> FileSystem.ext) |> not
    
    //---------------------------------------------------------------------------------------------------------------
    // Options
    //---------------------------------------------------------------------------------------------------------------
    

    //---------------------------------------------------------------------------------------------------------------
    // Top arg values
    //---------------------------------------------------------------------------------------------------------------
//    let repoDir = repositoryFolderOption |> optionValue |> Option.defaultValue currentPath
//    let projectFolder = projectFolderOption |> optionValue |> Option.defaultValue "./"
    
    let terminate = function
        | Error err ->
            do eprintfn "%s" err
            -1
        | Ok _ -> 0
    
    //---------------------------------------------------------------------------------------------------------------
    // Commands setup
    //---------------------------------------------------------------------------------------------------------------
//    let repository =  repoDir |> Repository.init RepositoryDependencies.Live defaultIgnoreFile    
    let cmd = app.Configure(
                fun config ->
                    // RECOMMEND
                    //let recommendf = fun ctx settings -> 0
                    let recommendf = fun (ctx : CommandContext) (settings : HotspotSetting) ->
                        let repoDir = settings.RepositoryFolder
                        let targetFolder = settings.TargetFolder
                        printfn "REPOSITORY: %s" repoDir
                        printfn "TARGET: %s" targetFolder
                        let repository =  repoDir |> Repository.init RepositoryDependencies.Live defaultIgnoreFile
                        repository |> Result.map (printRecommendations targetFolder) |> terminate
                    config.AddDelegate<HotspotSetting>("recommend", Func<CommandContext, HotspotSetting, int>(recommendf)) |> ignore
        )
    

    //---------------------------------------------------------------------------------------------------------------
    // Execute
    //---------------------------------------------------------------------------------------------------------------
    app.Run(argv)
    
    
