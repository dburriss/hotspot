namespace Hotspot

open System
open Hotspot.Git

//=====================================
// Repository
//=====================================

type IgnoreFile = string -> bool

/// Basic data for code repository
type RepositoryData = {
    Path : string
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    IgnoreFile : IgnoreFile
}

/// Represents different repository types
type Repository =
    | JustCode of RepositoryData
    | GitRepository of RepositoryData

/// Create a repository instance from a directory
type ReadRepository = IgnoreFile -> string -> Result<Repository, string>

type RepositoryMap<'a> = (string -> 'a option) -> Repository -> (string * 'a option) seq
type RepositoryDependencies<'a> = {
    IsGitRepository : string -> Result<bool,string>
    GitRepository : ReadRepository
    NoVcsRepository : ReadRepository
    FileSystemIter : RepositoryMap<'a>
}

module RepositoryDependencies =
    open Hotspot.Helpers
    let private gitRepository ignoreFile repoPath =
        GitParse.repositoryRange repoPath
        |> Result.map (fun (start, finish) -> 
        {
            Path = repoPath
            CreatedAt = start
            LastUpdatedAt = finish
            IgnoreFile = ignoreFile
        } |> GitRepository)
        
    let mapFiles<'a> (f : string -> 'a option) repository =
        let data = match repository with | GitRepository data -> data | JustCode data -> data
        let map = fun filePath ->
            //printfn "MAP FILE: %s" filePath
            if(filePath |> data.IgnoreFile) then filePath, None
            else filePath, (f filePath)
        FileSystem.mapFiles map data.Path
    
    let Live = {
        IsGitRepository = GitParse.isRepository
        GitRepository = gitRepository
        NoVcsRepository = fun ignore p -> Error (sprintf "%s is not under version control. Non version controlled repositories not supported." p)
        FileSystemIter = mapFiles
    }

module Repository =

    let path = function | JustCode r -> r.Path | GitRepository r -> r.Path
    let createdAt = function | JustCode r -> r.CreatedAt | GitRepository r -> r.CreatedAt
    let lastUpdatedAt = function | JustCode r -> r.LastUpdatedAt | GitRepository r -> r.LastUpdatedAt
    let data = function | GitRepository data -> data | JustCode data -> data
    
    /// Create a Repository instance. 
    let init (deps : RepositoryDependencies<'a>) : ReadRepository = fun ignoreFile path ->
        match (deps.IsGitRepository path) with
        | Ok false -> failwithf "%s is not a git repository. Only git currently supported." path
        | Ok true -> path |> deps.GitRepository ignoreFile
        | Error ex -> failwith ex
        
    let forEach (deps : RepositoryDependencies<'a>) f (repository : Repository) =
        deps.FileSystemIter f repository
        