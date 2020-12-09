namespace Hotspot

open System

module Analyse =
    let calcPriorityFromHistory (repository : InspectedRepositoryCode) (data : InspectedFile) =
        let calcCoeff = Stats.calculateCoeffiecient repository.CreatedAt repository.LastUpdatedAt
        // TODO: 09/12/2020 dburriss@xebia.com | Need to pick multiplier here
        let multiplier coeff = coeff * 1L//(data.LoC |> Option.get |> int64) // We want to do on cyclomatic complexity rather than LoC
        let touchScores = 
            data.History 
            |> Option.map (Array.map (fun log -> log.Date |> calcCoeff))
            |> Option.map (Array.sumBy multiplier)
        touchScores

    let analyzeData calcPriority (repository : InspectedRepositoryCode) (data : InspectedFile) =
        let priority =
            match calcPriority repository data with
            | None -> 0L//failwithf "Unexpectedly there was no priority calculated for %s" data.File.Path.FullPath
            | Some p -> p
        {
            File = data.File
            InspectedFile = data
            PriorityScore = priority
        }

    /// Analyze the data
    let performAnalysis analyzeData (repository : InspectedRepositoryCode) =
        let analyze = analyzeData repository
        {
            Directory = repository.Directory
            CreatedAt = repository.CreatedAt
            LastUpdatedAt = repository.LastUpdatedAt
            AnalyzedFiles = repository.InspectedFiles |> List.map analyze
        }

    let analyse = performAnalysis (analyzeData calcPriorityFromHistory)