namespace Hotspot

open System

type Measurement = {
    Path : string
    CreatedAt : DateTimeOffset
    LastTouchedAt : DateTimeOffset
    History : (Git.Log list)
    LoC : int
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