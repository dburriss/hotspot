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
/// // TODO: 23/01/2021 dburriss@xebia.com | Change to an array of types?
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
    File : FilePath
    CreatedAt : DateTimeOffset option
    LastTouchedAt : DateTimeOffset option
    History : History option
    Metrics : CodeMetrics option
}

/// Data for a code repository 
type InspectedRepositoryCode = {
    Directory : DirectoryPath
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    InspectedFiles : InspectedFile list
}

// Analyzed file with raw inspection data as well as a priority score
type Analysis = {
    File : FilePath
    InspectedFile : InspectedFile
    PriorityScore : int64
}

type AnalyzedRepositoryCode = {
    Directory : DirectoryPath
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
    File : FilePath
    Comments : string list
    RecommendationData : RecommendationData
}

type RecommendationReport = {
    Directory : DirectoryPath
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
    let empty : History = Array.empty 
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
            
        let multiplier coeff = coeff * multiplierNumber
        let touchScores =
            match data.History with
            | None | Some [||] -> multiplierNumber
            | Some history ->
                history
                |> (Array.map (fun log -> log.Date |> calcCoeff))
                |> (Array.sumBy multiplier)
        touchScores    

    let prioritize scoreCalculator (inspectedFile : InspectedFile) =
        let priority = scoreCalculator inspectedFile
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