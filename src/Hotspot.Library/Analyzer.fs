namespace Hotspot
        
module Analyzer =

    /// Analyze the data using the given `analyze` function
    let private performAnalysisWith analyze (repository : InspectedRepositoryCode) =
        let hasHistory (h : History option) =
            match h with
            | None -> false
            | Some [||] -> false
            | _ -> true
        
        let hasMetrics (m : CodeMetrics option) =
            match m with
            | None -> false
            | Some x when (Option.isNone x.Coupling &&
                           Option.isNone x.CyclomaticComplexity &&
                           Option.isNone x.InheritanceDepth &&
                           Option.isNone x.LoC) -> false
            | Some x -> true
            
        let filter (x : InspectedFile) =
            if hasHistory x.History || hasMetrics x.Metrics then true
            else false
            
        {
            Directory = repository.Directory
            CreatedAt = repository.CreatedAt
            LastUpdatedAt = repository.LastUpdatedAt
            AnalyzedFiles = repository.InspectedFiles |> List.filter filter |> List.map analyze
        }
        
    let defaultStrategy (repository : InspectedRepositoryCode) =
        // TODO: we actually only care about the dates...
        let priorityCalculator = Analysis.calcPriorityFromHistory (Weighting.calculate) (repository.CreatedAt, repository.LastUpdatedAt)
        Analysis.prioritize priorityCalculator

    /// Perform analysis on the `InspectedRepositoryCode` using `Analysis.calcPriorityFromHistory`.
    /// If a file contains no metrics or history, no analysis is given for file.
    let analyze (repository : InspectedRepositoryCode) =
        performAnalysisWith (defaultStrategy repository) repository
        