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
    
    let private gitFileRawData (repository : RepositoryData) file : Measurement option =
        let filePath = FileSystem.combine(repository.Path, file)
        let locStats = Loc.getStats filePath
        let history = GitLog.fileHistory repository.Path filePath |> function | Ok x -> x |> List.choose id | Error e -> failwith e
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

    let fileRawData inExtensionIncludeList (repository : Repository) =
        match repository with
        | GitRepository repo ->
            repo.Path
            |> FileSystem.mapFiles (fun (path, file) -> 
                let filePath = FileSystem.combine(path, file)

                if(filePath |> inExtensionIncludeList) then
                    gitFileRawData repo filePath
                else None)
        | JustCode repo -> failwithf "Path %s is a non VCS code repository. Currently not supported." repo.Path

    /// Get all files with history and LoC
    let gatherRepositoryRawData gatherRawData projectFolder (repository : Repository) =    
        {
            Path = repository |> Repository.path
            Project = projectFolder
            CreatedAt = repository |> Repository.createdAt
            LastUpdatedAt = repository |> Repository.lastUpdatedAt
            Measurements = (gatherRawData repository) |> Seq.toList |> List.choose id
        }