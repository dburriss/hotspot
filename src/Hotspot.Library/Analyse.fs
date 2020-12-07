namespace Hotspot

open System

module Analyse =
    let calcPriorityFromHistory (repository : InspectedRepositoryCode) (data : InspectedFile) =
        let calcCoeff = Stats.calculateCoeffiecient repository.CreatedAt repository.LastUpdatedAt
        let multiplier coeff = coeff * 1L//(data.LoC |> Option.get |> int64) // We want to do on cyclomatic complexity rather than LoC
        let touchScores = 
            data.History 
            |> Option.map (Array.map (fun log -> log.Date |> calcCoeff))
            |> Option.map (Array.sumBy multiplier)
        touchScores

    let analyzeData calcPriority (repository : InspectedRepositoryCode) (data : InspectedFile) =
        {
            Path = data.Path
            InspectedFile = data
            PriorityScore  = calcPriority repository data |> Option.get // TODO: 25/10/2020 dburriss@xebia.com | Make better life choices
        }

    /// Analyze the data
    let performAnalysis analyzeData (repository : InspectedRepositoryCode) =
        let analyze = analyzeData repository
        {
            Path = repository.Path
            CreatedAt = repository.CreatedAt
            LastUpdatedAt = repository.LastUpdatedAt
            AnalyzedFiles = repository.InspectedFiles |> List.map analyze
        }

    let analyse = performAnalysis (analyzeData calcPriorityFromHistory)