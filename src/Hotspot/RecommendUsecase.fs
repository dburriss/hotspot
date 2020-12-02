namespace Hotspot

open System
open Hotspot.Helpers

type RecommendSetting = {
    RepositoryFolder : string
    TargetFolder : string
    SccFile : string
}
  
module RecommendCommand =
    open System.Diagnostics
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
        Debug.WriteLine(sprintf "SCC file: %s" sccFile)
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
        let repositoryFolder = settings.RepositoryFolder
        let targetFolder = settings.TargetFolder
        
        sprintf "REPOSITORY: %s" repositoryFolder |> TerminalPrint.highlight
        sprintf "TARGET: %s" targetFolder |> TerminalPrint.highlight
        let repository =  repositoryFolder |> Repository.init (RepositoryDependencies.Live env) (defaultIgnoreFile env)
        let useScc = settings.SccFile |> String.IsNullOrEmpty |> not
        if(useScc) then
            Debug.WriteLine("Metric source: SCC")
            sprintf "Metric source: SCC" |> TerminalPrint.subdued
            repository |> Result.map (printRecommendations env (sccMetrics env repositoryFolder (defaultIgnoreFile env) settings.SccFile) targetFolder) |> terminate
        else
            Debug.WriteLine("Metric source: LoC")
            sprintf "Metric source: LoC count" |> TerminalPrint.subdued
            repository |> Result.map (printRecommendations env (Measure.myMetrics env) targetFolder) |> terminate