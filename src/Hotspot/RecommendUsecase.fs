namespace Hotspot

open System
open Hotspot.Helpers
open Spectre.IO

type RecommendSetting = {
    RepositoryFolder : IDirectory
    SccFile : IFile option
}
  
module RecommendCommand =
    open System.Diagnostics
        
    let private loadSccFile fs file =
        FileSystem.loadText fs file

    // Use case (default): Use LoC & print to console
    let private sccMetrics (fs : IFileSystem) (root : IDirectory) (sccFile : IFile) =
        SCC.loadFromFile fs sccFile
        |> SCC.fetchMetrics root
        
    let private printRecommendations inspectFile =
        Inspect.inspect inspectFile
        >> Analyzer.analyze 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    let recommendf = fun (fs : IFileSystem) (repository : #CodeRepository) (settings : RecommendSetting) ->
        let repositoryFolder = settings.RepositoryFolder.Path.FullPath
        
        sprintf "REPOSITORY: %s" repositoryFolder |> TerminalPrint.highlight
        
        Debug.WriteLine("Metric source: SCC")
        sprintf "Metric source: SCC" |> TerminalPrint.subdued
        let scc =
            if Option.isSome settings.SccFile then
                sccMetrics fs settings.RepositoryFolder settings.SccFile.Value
            else fun (_ : IFile) -> None
        let loc = Loc.fetchMetrics fs
        let metrics : FetchCodeMetrics = Metrics.fetchMetricsOr scc loc
        let inspectFile : InspectFile = Inspect.withMetricsAndHistory metrics (repository.GetFileHistory)
        
        printRecommendations inspectFile repository |> ignore
        0
