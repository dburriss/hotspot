namespace Hotspot

open System

type Metrics = {
    LoC : int option
    CyclomaticComplexity : int option
    InheritanceDepth : int option
    Coupling : int option
}

/// A record representing a file with metrics
type Measurement = {
    Path : string
    CreatedAt : DateTimeOffset option
    LastTouchedAt : DateTimeOffset option
    History : (Git.Log list) option
    Metrics : Metrics option
}

/// A specific folder in a repository that you would like to measure
type ProjectFolder = string

type MeasuredRepository = {
    Path : string
    Project : ProjectFolder
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    Measurements : Measurement list
}

module Metrics =
    // LoC
    let hasLoC (metricsOpt : Metrics option) =
        metricsOpt |> Option.map (fun m -> m.LoC |> Option.isSome)
        |> Option.defaultValue false
    let loc (metricsOpt : Metrics option) = metricsOpt |> Option.bind (fun m -> m.LoC)
    let locOrValue v (metricsOpt : Metrics option) = metricsOpt |> Option.bind (fun m -> m.LoC) |> Option.defaultValue v
    let locPredicate predicate (metricsOpt : Metrics option) : bool =
        metricsOpt |> Option.bind (fun m -> m.LoC)
        |> Option.map predicate |> Option.defaultValue false
    
    // Complexity
    let complexity (metricsOpt : Metrics option) = metricsOpt |> Option.bind (fun m -> m.CyclomaticComplexity)

module Measurement =
    
    let private get v1 v2 selector =
        let x1 = v1 |> selector
        let x2 = v2 |> selector
        match (x1, x2) with
        | Some _, _ -> x1
        | None, Some _ -> x2
        | None, None -> None

    let zip (m1 : Metrics) (m2 : Metrics) : Metrics =
        {
            LoC = get m1 m2 (fun x -> x.LoC)
            CyclomaticComplexity = get m1 m2 (fun x -> x.CyclomaticComplexity)
            InheritanceDepth = get m1 m2 (fun x -> x.InheritanceDepth)
            Coupling = get m1 m2 (fun x -> x.Coupling)
        }

module Measure =
    
    open Hotspot.Helpers
    open Hotspot.Git
    
    let myMetrics env filePath =
        {
            LoC = Loc.getStats env filePath |> fun x -> x.LoC |> Some
            CyclomaticComplexity = None
            InheritanceDepth = None
            Coupling = None
        } |> Some
    
    let private measureFileWithGitHistory measureMetrics (repository : RepositoryData) file : Measurement option =
        let filePath = FileSystem.combine(repository.Path, file)
        let history = GitParse.fileHistory repository.Path filePath |> function | Ok x -> x |> List.choose id | Error e -> failwith e
        match history with
        | [] -> None
        | hs ->
            let (fileCreated,lastTouchedAt) = (hs |> List.tryHead |> Option.map (fun x -> x.Date), history |> List.tryLast |> Option.map (fun x -> x.Date)) 
            {
                Path = filePath
                CreatedAt = fileCreated
                LastTouchedAt = lastTouchedAt
                History = history |> Some
                Metrics = measureMetrics filePath
            } |> Some

    let measureFile measureMetrics (repository : Repository) file =
        match repository with
        | GitRepository repo -> measureFileWithGitHistory measureMetrics repo file
        | JustCode repo -> failwithf "Path %s is a non VCS code repository. Currently not supported." repo.Path
    
    let measureFiles deps measureMetrics repository =
        let f = measureFile measureMetrics repository
        Repository.forEach deps f repository 
        |> Seq.toList |> List.map snd |> List.choose id
       
    /// Get all files with Measurements
    let measure<'a> (deps : RepositoryDependencies<'a>) measureMetrics projectFolder (repository : Repository) =
        {
            Path = repository |> Repository.path
            Project = projectFolder
            CreatedAt = repository |> Repository.createdAt
            LastUpdatedAt = repository |> Repository.lastUpdatedAt
            Measurements = measureFiles deps measureMetrics repository
        }