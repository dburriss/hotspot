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
type CodeMetrics = {
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
    File : IFile
    CreatedAt : DateTimeOffset option
    LastTouchedAt : DateTimeOffset option
    History : History option
    Metrics : CodeMetrics option
}

/// Data for a code repository 
type InspectedRepositoryCode = {
    Directory : IDirectory
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    InspectedFiles : InspectedFile list
}

// Analyzed file with raw inspection data as well as a priority score
type Analysis = {
    File : IFile
    InspectedFile : InspectedFile
    PriorityScore : int64
}

type AnalyzedRepositoryCode = {
    Directory : IDirectory
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    AnalyzedFiles : Analysis list
}

type RecommendationData = {
    RelativePriority: int
    Metrics : CodeMetrics option
    History : History // TODO: 29/10/2020 dburriss@xebia.com | Make Option? Empty array fine?
}

type Recommendation = {
    File : IFile
    Comments : string list
    RecommendationData : RecommendationData
}

type RecommendationReport = {
    Directory : IDirectory
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    Recommendations : Map<string,Recommendation>
}

//----------------------------------------------------------------------------------------------------------------------
// STEPS: Actions that accomplish one goal
//----------------------------------------------------------------------------------------------------------------------
type FetchCodeMetrics = IFile -> CodeMetrics option
/// Represents the action of inspecting a files history (if available) and metrics (if available)
type InspectFile = IFile -> InspectedFile option

type InspectRepository = CodeRepository -> InspectedRepositoryCode

type AnalyzeRepository = InspectedRepositoryCode -> AnalyzedRepositoryCode

type MakeRecommendations = AnalyzedRepositoryCode -> RecommendationReport

//----------------------------------------------------------------------------------------------------------------------
// WORKFLOWS: Shared workflows built up from steps
//----------------------------------------------------------------------------------------------------------------------
    
//----------------------------------------------------------------------------------------------------------------------
// USE-CASES
//----------------------------------------------------------------------------------------------------------------------
       
module CodeMetrics =
    // LoC
    let hasLoC (metricsOpt : CodeMetrics option) =
        metricsOpt |> Option.map (fun m -> m.LoC |> Option.isSome)
        |> Option.defaultValue false
    let loc (metricsOpt : CodeMetrics option) = metricsOpt |> Option.bind (fun m -> m.LoC)
    let locOrValue v (metricsOpt : CodeMetrics option) = metricsOpt |> Option.bind (fun m -> m.LoC) |> Option.defaultValue v
    let locPredicate predicate (metricsOpt : CodeMetrics option) : bool =
        metricsOpt |> Option.bind (fun m -> m.LoC)
        |> Option.map predicate |> Option.defaultValue false
    
    // Complexity
    let complexity (metricsOpt : CodeMetrics option) = metricsOpt |> Option.bind (fun m -> m.CyclomaticComplexity)
    
module History =
    let createdAt (history : History) =
        history |> Array.map (fun x -> x.Date) |> Array.sort |> Array.tryHead
    let lastUpdatedAt (history : History) =
        history |> Array.map (fun x -> x.Date) |> Array.sort |> Array.tryLast
        
module Analysis =
    let calcPriorityFromHistory calculateCoeffiecient (createdAt, lastUpdatedAt) (data : InspectedFile) =
        let calcCoeff = calculateCoeffiecient createdAt lastUpdatedAt
        let multiplierNumber =
            match (data.Metrics) with
            | None -> 1L
            | Some m -> m.CyclomaticComplexity |> Option.defaultWith (fun () -> m.LoC |> Option.defaultValue 1) |> int64
            
        let multiplier coeff = coeff * multiplierNumber//(data.LoC |> Option.get |> int64) // We want to do on cyclomatic complexity rather than LoC
        let touchScores = 
            data.History 
            |> Option.map (Array.map (fun log -> log.Date |> calcCoeff))
            |> Option.map (Array.sumBy multiplier)
        touchScores    

    let analyzeFile priorityCalculator (inspectedFile : InspectedFile) =
        let priority =
            match priorityCalculator inspectedFile with
            | None -> 0L//failwithf "Unexpectedly there was no priority calculated for %s" data.File.Path.FullPath
            | Some p -> p
        {
            File = inspectedFile.File
            InspectedFile = inspectedFile
            PriorityScore = priority
        }
    
// TODO: 07/12/2020 dburriss@xebia.com | Move this to a Live module
module Live =

    let defaultIgnoreFile exts : IgnoreFile =
        let defaultIncludeList = defaultArg exts [|".cs";".fs";".ts"|]
        let defaultIgnoreFile (file : IFile) =
            let ext = file.Path.GetExtension()
            let r = defaultIncludeList |> Array.contains ext |> not
            r
        defaultIgnoreFile