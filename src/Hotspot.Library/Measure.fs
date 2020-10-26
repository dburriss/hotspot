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

type IgnoreFile = string -> bool
type MeasureDependencies = {
    MeasureRepositoryFiles : IgnoreFile -> Repository -> (Measurement option) seq
}

module MeasureDependencies =
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

    let measureFiles ignoreFile (repository : Repository) =
        match repository with
        | GitRepository repo ->
            repo.Path
            |> FileSystem.mapFiles (fun (path, file) -> 
                let filePath = FileSystem.combine(path, file)

                if(filePath |> ignoreFile) then None
                else measureFileWithGitHistory repo filePath)
        | JustCode repo -> failwithf "Path %s is a non VCS code repository. Currently not supported." repo.Path

    let Live = {
        MeasureRepositoryFiles = measureFiles
    }

module Measure =
    
    /// Get all files with history and LoC
    let measure (deps : MeasureDependencies) projectFolder ignoreFile (repository : Repository) =
        // TODO: 26/10/2020 dburriss@xebia.com | Move file mapping and ignore to repository module
        {
            Path = repository |> Repository.path
            Project = projectFolder
            CreatedAt = repository |> Repository.createdAt
            LastUpdatedAt = repository |> Repository.lastUpdatedAt
            Measurements = (deps.MeasureRepositoryFiles ignoreFile repository) |> Seq.toList |> List.choose id
        }