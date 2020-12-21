namespace Hotspot

open System
open Hotspot.Helpers
open Spectre.IO

type RecommendSetting = {
    RepositoryFolder : IDirectory
    SccFile : IFile option
}

type RecommendationsCmd = {
    FileSystem : IFileSystem
    CodeRepository : CodeRepository
    Settings : RecommendSetting
}
  
module RecommendUsecase =
    open System.Diagnostics
        
    let private loadSccFile fs file =
        FileSystem.loadText fs file

    // Use case (default): Use LoC & print to console
    let private sccMetrics (fs : IFileSystem) (root : IDirectory) (sccFile : IFile option) =
        if Option.isSome sccFile then
            SCC.loadFromFile fs sccFile.Value
            |> SCC.fetchMetrics root
        else fun (_ : IFile) -> None
        
    let private printRecommendations inspectFile =
        Inspector.inspect inspectFile
        >> Analyzer.analyze 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    let recommend (cmd : RecommendationsCmd) =
        let recommendf = fun (fs : IFileSystem) (repository : #CodeRepository) (settings : RecommendSetting) ->
            let repositoryFolder = settings.RepositoryFolder.Path.FullPath
            
            sprintf "REPOSITORY: %s" repositoryFolder |> TerminalPrint.highlight
            
            Debug.WriteLine("Metric source: SCC")
            sprintf "Metric source: SCC" |> TerminalPrint.subdued
            let scc = sccMetrics fs settings.RepositoryFolder settings.SccFile
            let loc = Loc.fetchMetrics fs
            let metrics : FetchCodeMetrics = Metrics.fetchMetricsOr scc loc
            let inspectFile : InspectFile = Inspect.withMetricsAndHistory metrics (repository.GetFileHistory)
            
            printRecommendations inspectFile repository |> ignore
            0
        recommendf cmd.FileSystem cmd.CodeRepository cmd.Settings
