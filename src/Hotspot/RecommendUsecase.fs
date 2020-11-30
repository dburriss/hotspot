namespace Hotspot

open System
open Hotspot.Helpers

type RecommendSetting = {
    RepositoryFolder : string
    TargetFolder : string
    SccFile : string
}
  
module RecommendCommand =
    
    let private defaultIgnoreFile env : IIgnoreFile = env :> IIgnoreFile
    
    let private terminate = function
        | Error err ->
            do eprintfn "%s" err
            -1
        | Ok _ -> 0
        
    let private loadSccFile env filePath =
        FileSystem.loadText env filePath

    // Use case (default): Use LoC & print to console
    let private sccMetrics env root ignoreFile sccFile =
        sccFile
        |> loadSccFile env
        |> SCC.parse
        |> SCC.toMetricsLookup root ignoreFile
        
    
    let private printRecommendations env metricsF projectFolder =
        Measure.measure (RepositoryDependencies.Live env) metricsF projectFolder
        >> Analyse.analyse 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    let recommendf = fun env (settings : RecommendSetting) ->
        let repoDir = settings.RepositoryFolder
        let targetFolder = settings.TargetFolder
        
        printfn "REPOSITORY: %s" repoDir
        printfn "TARGET: %s" targetFolder
        let repository =  repoDir |> Repository.init (RepositoryDependencies.Live env) (defaultIgnoreFile env)
        let useScc = settings.SccFile |> String.IsNullOrEmpty |> not
        if(useScc) then
            printfn "Using scc data..."
            repository |> Result.map (printRecommendations env (sccMetrics env repoDir (defaultIgnoreFile env) settings.SccFile) targetFolder) |> terminate
        else
            printfn "Using my metrics..."
            repository |> Result.map (printRecommendations env (Measure.myMetrics env) targetFolder) |> terminate