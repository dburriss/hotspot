namespace Hotspot

module Inspect =
    
    open Spectre.IO
    open System.Diagnostics
    
    let withMetricsAndHistory (fetchMetrics : FetchCodeMetrics) (fetchHistory : FetchHistory) (file : IFile) =
        let metricsOpt = fetchMetrics file
        let history = fetchHistory file
        Some {
            File = file.Path
            CreatedAt = History.createdAt history
            LastTouchedAt = History.lastUpdatedAt history
            History = if Array.isEmpty history then None else Some history
            Metrics = metricsOpt
        }

    let private measureFiles (repository : CodeRepository) (inspectFile : InspectFile) =
        (repository.Choose inspectFile) |> Seq.toList
    
    let inspect inspectFile : InspectRepository =
        fun repository ->
            Debug.WriteLine(sprintf "Inspect: repository=%s" repository.RootDirectory.Path.FullPath) 
            {
                Directory = repository.RootDirectory.Path
                CreatedAt = repository.CreatedAt()
                LastUpdatedAt = repository.LastUpdatedAt()
                InspectedFiles = measureFiles repository inspectFile 
            }            