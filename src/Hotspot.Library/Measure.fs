namespace Hotspot

open System

/// A record representing a file with metrics
type Measurement = {
    Path : string
    CreatedAt : DateTimeOffset
    LastTouchedAt : DateTimeOffset
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
            let (fileCreated,lastTouchedAt) = (hs |> List.head |> fun x -> x.Date, history |> List.last |> fun x -> x.Date) 
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