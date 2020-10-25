namespace Hotspot

open System

//=====================================
// Repository
//=====================================

/// A a code repository
type RepositoryData = {
    Path : string
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
}

type Repository =
    | JustCode of RepositoryData
    | GitRepository of RepositoryData

type ReadRepository = string -> Repository

type ProjectFolder = string

module Repository =
    let path = function | JustCode r -> r.Path | GitRepository r -> r.Path
    let createdAt = function | JustCode r -> r.CreatedAt | GitRepository r -> r.CreatedAt
    let lastUpdatedAt = function | JustCode r -> r.LastUpdatedAt | GitRepository r -> r.LastUpdatedAt
