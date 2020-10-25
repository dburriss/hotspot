namespace Hotspot

open System

type Analysis = {
    Path : string
    Measurement : Measurement
    PriorityScore : int64
}

type AnalyzedRepository = {
    Path : string
    Project : ProjectFolder
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    Analysis : Analysis list
}

type AnalyzeRepository = MeasuredRepository -> AnalyzedRepository

module Analyse =
    let calcPriorityFromHistory (repository : MeasuredRepository) (data : Measurement) =
        let calcCoeff = Stats.calculateCoeffiecient repository.CreatedAt repository.LastUpdatedAt
        let multiplier coeff = coeff * 1L//(data.LoC |> Option.get |> int64) // We want to do on cyclomatic complexity rather than LoC
        let touchScores = 
            data.History 
            |> Option.map (List.map (fun log -> log.Date |> calcCoeff))
            |> Option.map (List.sumBy multiplier)
        touchScores

    let analyzeData calcPriority (repository : MeasuredRepository) (data : Measurement) =
        {
            Path = data.Path
            Measurement = data
            PriorityScore  = calcPriority repository data |> Option.get // TODO: 25/10/2020 dburriss@xebia.com | Make better life choices
        }

    /// Analyze the data
    let performAnalysis analyzeData (repository : MeasuredRepository) =
        let analyze = analyzeData repository
        {
            Path = repository.Path
            Project = repository.Project
            CreatedAt = repository.CreatedAt
            LastUpdatedAt = repository.LastUpdatedAt
            Analysis = repository.Measurements |> List.map analyze
        }
