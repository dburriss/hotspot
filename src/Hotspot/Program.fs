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
    let env = AppEnv()
    //(env :> ILog<RecommendationData>).Logger.Log
    //---------------------------------------------------------------------------------------------------------------
    // Console helpers
    //---------------------------------------------------------------------------------------------------------------
    
    let loadSccFile filePath =
        FileSystem.loadText env filePath
    
    //---------------------------------------------------------------------------------------------------------------
    // Usecases
    //---------------------------------------------------------------------------------------------------------------
    
    // Use case (default): Use LoC & print to console
    let sccMetrics root ignoreFile sccFile =
        sccFile
        |> loadSccFile
        |> SCC.parse
        |> SCC.toMetricsLookup root ignoreFile
        
    
    let printRecommendations metricsF projectFolder =
        Measure.measure (RepositoryDependencies.Live env) metricsF projectFolder
        >> Analyse.analyse 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    //---------------------------------------------------------------------------------------------------------------
    // Console setup
    //---------------------------------------------------------------------------------------------------------------
    let app = CommandApp()
    
    let defaultIncludeList = ["cs";"fs";]
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
    app.Configure(
        fun config ->
            // RECOMMEND
            //let recommendf = fun ctx settings -> 0
            let recommendf = fun (ctx : CommandContext) (settings : HotspotSetting) ->
                let repoDir = settings.RepositoryFolder
                let targetFolder = settings.TargetFolder
                
                printfn "REPOSITORY: %s" repoDir
                printfn "TARGET: %s" targetFolder
                let repository =  repoDir |> Repository.init (RepositoryDependencies.Live env) defaultIgnoreFile
                let useScc = settings.SccFile |> String.IsNullOrEmpty |> not
                if(useScc) then
                    printfn "Using scc data..."
                    repository |> Result.map (printRecommendations (sccMetrics repoDir defaultIgnoreFile settings.SccFile) targetFolder) |> terminate
                else
                    printfn "Using my metrics..."
                    repository |> Result.map (printRecommendations (Measure.myMetrics env) targetFolder) |> terminate
            config.AddDelegate<HotspotSetting>("recommend", Func<CommandContext, HotspotSetting, int>(recommendf)) |> ignore
    )
    

    //---------------------------------------------------------------------------------------------------------------
    // Execute
    //---------------------------------------------------------------------------------------------------------------
    app.Run(argv)
    
    
