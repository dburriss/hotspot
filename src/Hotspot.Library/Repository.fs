namespace Hotspot

open System
open Hotspot.Git

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
type ReadRepository = string -> Result<Repository, string>

type RepositoryConfig = {
    IsGitRepository : string -> Result<bool,string>
    GitRepository : ReadRepository
    NoVcsRepository : ReadRepository
}

module RepositoryDependencies =
    let private gitRepository repoPath =
        GitParse.repositoryRange repoPath
        |> Result.map (fun (start, finish) -> 
        {
            Path = repoPath
            CreatedAt = start
            LastUpdatedAt = finish
        } |> GitRepository)
    let Live = {
        IsGitRepository = GitParse.isRepository
        GitRepository = gitRepository
        NoVcsRepository = fun p -> Error (sprintf "%s is not under version control. Non version controlled repositories not supported." p)
    }

module Repository =
    open Hotspot.Git
    let path = function | JustCode r -> r.Path | GitRepository r -> r.Path
    let createdAt = function | JustCode r -> r.CreatedAt | GitRepository r -> r.CreatedAt
    let lastUpdatedAt = function | JustCode r -> r.LastUpdatedAt | GitRepository r -> r.LastUpdatedAt

    /// Create a Repository instance. 
    let init (deps : RepositoryConfig) : ReadRepository = fun path ->
        match (deps.IsGitRepository path) with
        | Ok false -> failwithf "%s is not a git repository. Only git currently supported." path
        | Ok true -> path |> deps.GitRepository
        | Error ex -> failwith ex
        