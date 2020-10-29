namespace Hotspot

open System

/// A record representing a file with metrics
type Measurement = {
    Path : string
    CreatedAt : DateTimeOffset option
    LastTouchedAt : DateTimeOffset option
    History : (Git.Log list) option
    LoC : int option
    CyclomaticComplexity : int option
    InheritanceDepth : int option
    Coupling : int option
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

module Measurement =
    
    let private get v1 v2 selector =
        let x1 = v1 |> selector
        let x2 = v2 |> selector
        match (x1, x2) with
        | Some _, _ -> x1
        | None, Some _ -> x2
        | None, None -> None

    let zip (m1 : Measurement) (m2 : Measurement) : Measurement =
        {
            Path = m1.Path
            CreatedAt = get m1 m2 (fun x -> x.CreatedAt)
            LastTouchedAt = get m1 m2 (fun x -> x.LastTouchedAt)
            History = get m1 m2 (fun x -> x.History)
            LoC = get m1 m2 (fun x -> x.LoC)
            CyclomaticComplexity = get m1 m2 (fun x -> x.CyclomaticComplexity)
            InheritanceDepth = get m1 m2 (fun x -> x.InheritanceDepth)
            Coupling = get m1 m2 (fun x -> x.Coupling)
        }

module Measure =
    
    open Hotspot.Helpers
    open Hotspot.Git
    
    let private measureFileWithGitHistory (repository : RepositoryData) file : Measurement option =
        let filePath = FileSystem.combine(repository.Path, file)
        let locStats = Loc.getStats filePath
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
                LoC = locStats.LoC |> Some
                CyclomaticComplexity = None
                InheritanceDepth = None
                Coupling = None
            } |> Some

    let measureFile (repository : Repository) file =
        match repository with
        | GitRepository repo -> measureFileWithGitHistory repo file
        | JustCode repo -> failwithf "Path %s is a non VCS code repository. Currently not supported." repo.Path
    
    let measureFiles deps repository =
        let f = measureFile repository
        Repository.forEach deps f repository 
        |> Seq.toList |> List.map snd |> List.choose id
       
    /// Get all files with Measurements
    let measure (deps : RepositoryDependencies<Measurement>) projectFolder (repository : Repository) =
        {
            Path = repository |> Repository.path
            Project = projectFolder
            CreatedAt = repository |> Repository.createdAt
            LastUpdatedAt = repository |> Repository.lastUpdatedAt
            Measurements = measureFiles deps repository
        }