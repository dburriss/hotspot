namespace Hotspot

open System
open Hotspot.Helpers
open Spectre.IO

type RecommendSetting = {
    RepositoryFolder : string
    SccFile : string
}
  
module RecommendCommand =
    open System.Diagnostics
    
    let private terminate = function
        | Error err ->
            do eprintfn "%s" err
            -1
        | Ok _ -> 0
        
    let private loadSccFile fs filePath =
        FileSystem.loadText fs filePath

    // Use case (default): Use LoC & print to console
    let private sccMetrics fs root ignoreFile sccFile =
        Debug.WriteLine(sprintf "SCC file: %s" sccFile)
        sccFile
        |> loadSccFile fs
        |> SCC.parse
        |> SCC.toMetricsLookup root
        
    
    let private printRecommendations env metricsF =
        Inspect.inspect metricsF
        >> Analyse.analyse 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    let recommendf = fun (fs : IFileSystem) (repository : #CodeRepository) (settings : RecommendSetting) ->
        let repositoryFolder = settings.RepositoryFolder
        
        sprintf "REPOSITORY: %s" repositoryFolder |> TerminalPrint.highlight
        
        let useScc = settings.SccFile |> String.IsNullOrEmpty |> not
        if(useScc) then
            Debug.WriteLine("Metric source: SCC")
            sprintf "Metric source: SCC" |> TerminalPrint.subdued
            repository |> (printRecommendations (sccMetrics fs repositoryFolder settings.SccFile)) |> ignore
            0
        else
            Debug.WriteLine("Metric source: LoC")
            sprintf "Metric source: LoC count" |> TerminalPrint.subdued
            repository |> (printRecommendations (Inspect.myMetrics)) |> ignore
            0