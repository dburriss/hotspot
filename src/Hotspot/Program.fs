// Learn more about F# at http://fsharp.org

open System
open Hotspot
open Hotspot.Git
open Hotspot.Helpers

// DATA TYPES

type Analysis = {
    Path : string
    Measurement : Measurement
    PriorityScore : int64
}

type AnalyzedRepository = {
    Path : string
    Project : ProjectFolder
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    Analysis : Analysis list
}

type RecommendationData = {
    RelativePriority: int
    //Complexity : int
    LoC : int
    History : Git.Log list
}

type Recommendation = {
    Path : string
    Comments : string list
    RecommendationData : RecommendationData
}

type RecommendationReport = {
    Path : string
    Project : ProjectFolder
    CreatedAt : DateTimeOffset
    LastUpdatedAt : DateTimeOffset
    Recommendations : Map<string,Recommendation>
}

// WORKFLOWS
type GatherRepositoryData = ProjectFolder -> MeasuredRepository -> MeasuredRepository
type AnalyzeRepository = MeasuredRepository -> AnalyzedRepository
type MakeRecommendations = AnalyzedRepository -> RecommendationReport

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
    | JustCode repo -> failwithf "Path %s is a non VCS code repository" repo.Path

/// Get all files with history and LoC
let gatherRepositoryRawData gatherRawData projectFolder (repository : Repository) =    
    {
        Path = repository |> Repository.path
        Project = projectFolder
        CreatedAt = repository |> Repository.createdAt
        LastUpdatedAt = repository |> Repository.lastUpdatedAt
        Measurements = (gatherRawData repository) |> Seq.toList |> List.choose id
    }


let calcPriorityFromHistory (repository : MeasuredRepository) (data : Measurement) =
    let calcCoeff = Stats.calculateCoeffiecient repository.CreatedAt repository.LastUpdatedAt
    let multiplier coeff = coeff * 1L//(data.LoC |> Option.get |> int64) // We want to do on cyclomatic complexity rather than LoC
    let touchScores = 
        data.History 
        |> Option.map (List.map (fun log -> log.Date |> calcCoeff))
        |> Option.map (List.sumBy multiplier)
    touchScores

let analyzeData calcPriority (repository : MeasuredRepository) (data : Measurement) =
    {
        Path = data.Path
        Measurement = data
        PriorityScore  = calcPriority repository data |> Option.get // TODO: 25/10/2020 dburriss@xebia.com | Make better life choices
    }

/// Analyze the data
let performAnalysis analyzeData (repository : MeasuredRepository) =
    let analyze = analyzeData repository
    {
        Path = repository.Path
        Project = repository.Project
        CreatedAt = repository.CreatedAt
        LastUpdatedAt = repository.LastUpdatedAt
        Analysis = repository.Measurements |> List.map analyze
    }

let distinctAuthors (history : Git.Log list) = history |> List.distinctBy (fun h -> h.Author)

let recommendations (data : RecommendationData) =
    [
        let nrChanges = data.History |> List.length
        let nrAuthors = data.History |> distinctAuthors |> List.length
        
        if(data.LoC > 400) then 
            if(data.RelativePriority >= 50 && nrChanges > 5) then 
                yield sprintf "PRIORITY: MEDIUM | This file is large at %i lines of code and changes often. It is strongly suggested you break it up to avoid conflicting changes." data.LoC
            else 
                yield sprintf "PRIORITY: LOW | You may want to break this file up into smaller files as it is %i lines of code." data.LoC
        
        if(data.LoC > 100 && nrAuthors = 1) then 
            if data.RelativePriority > 50 && data.RelativePriority < 80 then
                yield "PRIORITY: MEDIUM | Bus factor is 1 on a significant file. Make sure covered by descriptive tests & try get spread knowledge across the team."
            if data.RelativePriority >= 80 then
                yield "PRIORITY: HIGH | Bus factor is 1 on a VERY significant file. Make sure covered by descriptive tests & try pair up working on this file to prioritize knowledge transfer."

        else
            if data.RelativePriority >= 80 then
                yield "PRIORITY: MEDIUM | This file seems to be significant based on complexity and changes. Make sure covered by descriptive tests & try get spread knowledge across the team."
        // if(data.Complexity >= 10 && data.RelativePriority >= 20) then 
        //     yield sprintf "PRIORITY: %i/100 | Due to cyclomatic complexity of %i and recency of changes, this should be simplified. See: http://codinghelmet.com/articles/reduce-cyclomatic-complexity-switchable-factory-methods" (data.RelativePriority) (data.Complexity)
    ]

let analysisRecommendation recommendations shiftPriority (analysis : Analysis) =
    let data = {
            RelativePriority = shiftPriority analysis.PriorityScore
            //Complexity  = analysis.Raw.Metrics.Complexity
            LoC = analysis.Measurement.LoC |> Option.get // TODO: 25/10/2020 dburriss@xebia.com | Be smarter
            History = analysis.Measurement.History |> Option.get
        }
    let recommendation = {
            Path = analysis.Path
            Comments = recommendations data
            RecommendationData = data
        }
    (analysis.Measurement.Path, recommendation)    

let makeRecommendationsWith analysisRecommendation (analyzedRepository : AnalyzedRepository) =
    //let (min,max) = analyzedRepository.Analysis |> List.map (fun a -> a.PriorityScore) |> fun xs -> (xs |> List.min, xs |> List.max)
    //let shiftPriority = Stats.shiftTo100L min max >> int
    {
        Path = analyzedRepository.Path
        Project = analyzedRepository.Project
        CreatedAt = analyzedRepository.CreatedAt
        LastUpdatedAt = analyzedRepository.LastUpdatedAt
        Recommendations = analyzedRepository.Analysis |> List.map analysisRecommendation |> Map.ofList
    }

let printRecommendations report =
    
    printfn "REPOSITORY: %s" report.Path
    report.Recommendations
    |> Map.toArray
    |> Array.map (fun (file, r) ->
        let first = r.RecommendationData.History |> List.head
        let last = r.RecommendationData.History |> List.last
        let authour (Git.Author s) = s
        {|  File = IO.Path.GetRelativePath(report.Path, file)
            LoC = r.RecommendationData.LoC
            Priority = r.RecommendationData.RelativePriority
            Comments = r.Comments
            Authours = r.RecommendationData.History |> distinctAuthors |> List.length
            CreatedAt = first.Date
            CreatedBy = first.Author |> authour
            LastUpdate = last.Date
            LastUpdateBy = last.Author |> authour
        |})
    |> Array.iter (fun x ->
        if(x.Comments.Length > 0) then
            let dtformat (dt : DateTimeOffset) = dt.ToLocalTime().ToString("yyyy-MM-dd")
            printfn "===> %s" x.File
            printfn "           Priority : %i   LoC : %i    Authors : %i    Created : %s (%s)   LastUpdate : %s (%s)"  x.Priority x.LoC x.Authours (x.CreatedAt |> dtformat) x.CreatedBy (x.LastUpdate |> dtformat) x.LastUpdateBy
            x.Comments |> List.iter (printfn "      %s")
    )
    report

[<EntryPoint>]
let main argv =
    // execute
    let currentPath = Environment.CurrentDirectory
    printfn "Running against %s" currentPath
    let repoDir = argv |> Array.tryItem 0 |> Option.defaultValue currentPath
    let projFolder : ProjectFolder = argv |> Array.tryItem 1 |> Option.defaultValue "./"
    let includeList = argv |> Array.tryItem 2 |> Option.defaultValue "cs,fs,js" |> String.split [|","|] |> Array.toList
    let inExtensionIncludeList filePath = includeList |> List.contains (filePath |> FileSystem.ext)
    let repo =  repoDir |> Repository.init
    
    let repoData = repo |> gatherRepositoryRawData (fileRawData inExtensionIncludeList) projFolder

    let analyze = performAnalysis (analyzeData calcPriorityFromHistory)
    let recommend analyzedRepo =
        // TODO: this can be done more efficiently
        let scores = analyzedRepo.Analysis |> List.map (fun x -> x.PriorityScore) 
        let min = scores |> List.min
        let max = scores |> List.max

        makeRecommendationsWith (analysisRecommendation recommendations (Stats.shiftTo100L min max >> int)) analyzedRepo

    repoData 
    |> analyze 
    |> recommend
    |> printRecommendations
    |> ignore

    0 // return an integer exit code
