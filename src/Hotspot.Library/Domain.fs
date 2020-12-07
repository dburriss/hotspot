namespace Hotspot

open System
open Spectre.IO
//----------------------------------------------------------------------------------------------------------------------
// TYPES
//----------------------------------------------------------------------------------------------------------------------
/// A function that determines if a file should be ignored
type IgnoreFile = IFile -> bool

/// The unique key for this change
type CommitId = string

/// The author of a change to a file
type Author = string

/// An item in the history of a file
type Log = {
    CommitId : CommitId
    Author : Author
    Date : DateTimeOffset
}
/// The history of changes on a file
type History = Log array

/// A record with possible metrics for a file
type Metrics = {
    LoC : int option
    CyclomaticComplexity : int option
    InheritanceDepth : int option
    Coupling : int option
}

/// Fetch the history for a file
type FetchHistory = IFile -> History

/// Represents a code repository containing code files
type CodeRepository =
    abstract member RootDirectory : IDirectory
    abstract member HasHistory : unit -> bool
    abstract member CreatedAt : unit -> DateTimeOffset
    abstract member LastUpdatedAt : unit -> DateTimeOffset
    abstract member Choose<'a> : (IFile -> 'a option) -> 'a seq
    abstract member GetFileHistory: FetchHistory

/// A record representing a file with metrics
type InspectedFile = {
    Path : IFile
    CreatedAt : DateTimeOffset option
    LastTouchedAt : DateTimeOffset option
    History : History option
    Metrics : Metrics option
}

/// Data for a code repository 
type InspectedRepositoryCode = {
    Path : IDirectory
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    InspectedFiles : InspectedFile list
}

// Analyzed file with raw inspection data as well as a priority score
type Analysis = {
    Path : IFile
    InspectedFile : InspectedFile
    PriorityScore : int64
}

type AnalyzedRepositoryCode = {
    Path : IDirectory
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    AnalyzedFiles : Analysis list
}

type RecommendationData = {
    RelativePriority: int
    Metrics : Metrics option
    History : History // TODO: 29/10/2020 dburriss@xebia.com | Make Option
}

type Recommendation = {
    Path : IFile
    Comments : string list
    RecommendationData : RecommendationData
}

type RecommendationReport = {
    Path : IDirectory
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    Recommendations : Map<string,Recommendation>
}

//----------------------------------------------------------------------------------------------------------------------
// STEPS: Actions that accomplish one goal
//----------------------------------------------------------------------------------------------------------------------
type FetchMetrics = IFile -> Metrics option
/// Represents the action of inspecting a files history (if available) and metrics (if available)
type InspectFile = IFile -> InspectedFile option

type MeasureRepository = CodeRepository -> InspectFile -> InspectedRepositoryCode

type AnalyzeRepository = InspectedRepositoryCode -> AnalyzedRepositoryCode

type MakeRecommendations = AnalyzedRepositoryCode -> RecommendationReport

//----------------------------------------------------------------------------------------------------------------------
// WORKFLOWS: Shared workflows built up from steps
//----------------------------------------------------------------------------------------------------------------------
    
//----------------------------------------------------------------------------------------------------------------------
// USE-CASES
//----------------------------------------------------------------------------------------------------------------------

// TODO: 07/12/2020 dburriss@xebia.com | Move this to a Live module
module Ignore =
    let live exts : IgnoreFile =
        let defaultIncludeList = defaultArg exts ["cs";"fs";"ts"]
        let defaultIgnoreFile (file : IFile) = defaultIncludeList |> List.contains (file.Path.GetExtension())
        defaultIgnoreFile
        
module Metrics =
    // LoC
    let hasLoC (metricsOpt : Metrics option) =
        metricsOpt |> Option.map (fun m -> m.LoC |> Option.isSome)
        |> Option.defaultValue false
    let loc (metricsOpt : Metrics option) = metricsOpt |> Option.bind (fun m -> m.LoC)
    let locOrValue v (metricsOpt : Metrics option) = metricsOpt |> Option.bind (fun m -> m.LoC) |> Option.defaultValue v
    let locPredicate predicate (metricsOpt : Metrics option) : bool =
        metricsOpt |> Option.bind (fun m -> m.LoC)
        |> Option.map predicate |> Option.defaultValue false
    
    // Complexity
    let complexity (metricsOpt : Metrics option) = metricsOpt |> Option.bind (fun m -> m.CyclomaticComplexity)