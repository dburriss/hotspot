namespace Hotspot

open System

//=====================================
// Repository
//=====================================

/// Basic data for code repository
type RepositoryData = {
    Path : string
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
}

/// Represents different repository types
type Repository =
    | JustCode of RepositoryData
    | GitRepository of RepositoryData

/// Create a repository instance from a directory
type ReadRepository = string -> Repository

/// A specific folder in a repository that you would like to measure
type ProjectFolder = string

module Repository =
    open Hotspot.Git
    let path = function | JustCode r -> r.Path | GitRepository r -> r.Path
    let createdAt = function | JustCode r -> r.CreatedAt | GitRepository r -> r.CreatedAt
    let lastUpdatedAt = function | JustCode r -> r.LastUpdatedAt | GitRepository r -> r.LastUpdatedAt
    
    let private gitRepository repoPath =
        let (start, finish) = GitLog.repositoryRange repoPath |> function | Ok x -> x | Error e -> failwith e
        {
            Path = repoPath
            CreatedAt = start
            LastUpdatedAt = finish
        }

    /// Create a Repository instance. 
    let init : ReadRepository = fun path ->
        // TODO: 25/10/2020 dburriss@xebia.com | Determine if a git repository or not
        path |> gitRepository |> GitRepository
        