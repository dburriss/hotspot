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