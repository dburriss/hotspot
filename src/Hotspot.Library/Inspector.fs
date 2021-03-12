namespace Hotspot

module Inspect =
    
    open Spectre.IO
    
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
module Inspector =
    open System.Diagnostics
    let private inspectedFiles (repository : CodeRepository) (inspectFile : InspectFile) =
        (repository.Choose inspectFile) |> Seq.toList
    
    let inspect fileInspector : InspectRepository =
        fun repository ->
            Debug.WriteLine(sprintf "Inspect: repository=%s" repository.RootDirectory.Path.FullPath) 
            {
                Directory = repository.RootDirectory.Path
                CreatedAt = repository.CreatedAt()
                LastUpdatedAt = repository.LastUpdatedAt()
                InspectedFiles = inspectedFiles repository fileInspector 
            }