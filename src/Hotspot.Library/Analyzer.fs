namespace Hotspot
        
module Analyzer =

    /// Analyze the data using the given 
    let performAnalysisWith analyze (repository : InspectedRepositoryCode) =
        {
            Directory = repository.Directory
            CreatedAt = repository.CreatedAt
            LastUpdatedAt = repository.LastUpdatedAt
            AnalyzedFiles = repository.InspectedFiles |> List.map analyze
        }

    /// Perform the default analysis on the 
    let analyze (repository : InspectedRepositoryCode) =
        let priorityCalculator = Analysis.calcPriorityFromHistory (Stats.calculateCoeffiecient) (repository.CreatedAt, repository.LastUpdatedAt)
        let analyzeFile = Analysis.analyzeFile priorityCalculator
        let performAnalysis = analyzeFile
        performAnalysisWith performAnalysis repository
        