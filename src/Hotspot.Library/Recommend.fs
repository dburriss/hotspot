namespace Hotspot

open System
open Hotspot.Helpers

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

type MakeRecommendations = AnalyzedRepository -> RecommendationReport

module Recommend =
    open System.Runtime
    let private distinctAuthors (history : Git.Log list) = history |> List.distinctBy (fun h -> h.Author)

    let recommendations (data : RecommendationData) =
        [
            let nrChanges = data.History |> List.length
            let nrAuthors = data.History |> distinctAuthors |> List.length
            
            if(data.LoC > 400) then 
                if(data.RelativePriority >= 50 && nrChanges > 5) then 
                    yield sprintf "SEVERITY: MEDIUM | This file is large at %i lines of code and changes often. It is strongly suggested you break it up to avoid conflicting changes." data.LoC
                else 
                    yield sprintf "SEVERITY: LOW | You may want to break this file up into smaller files as it is %i lines of code." data.LoC
            
            if(data.LoC > 100 && nrAuthors = 1) then 
                if data.RelativePriority > 50 && data.RelativePriority < 80 then
                    yield "SEVERITY: MEDIUM | Bus factor is 1 on a significant file. Make sure covered by descriptive tests & try get spread knowledge across the team."
                if data.RelativePriority >= 80 then
                    yield "SEVERITY: HIGH | Bus factor is 1 on a VERY significant file. Make sure covered by descriptive tests & try pair up working on this file to prioritize knowledge transfer."

            else
                if data.RelativePriority >= 80 then
                    yield "SEVERITY: MEDIUM | This file seems to be significant based on complexity and changes. Make sure covered by descriptive tests & try get spread knowledge across the team."
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

    let recommend analyzedRepo =
        // TODO: this can be done more efficiently
        let scores = analyzedRepo.Analysis |> List.map (fun x -> x.PriorityScore) 
        let min = scores |> List.min
        let max = scores |> List.max

        makeRecommendationsWith (analysisRecommendation recommendations (Stats.shiftTo100L min max >> int)) analyzedRepo
        
    let printRecommendations report =
        
        report.Recommendations
        |> Map.toArray
        |> Array.map (fun (file, r) ->
            let first = r.RecommendationData.History |> List.tryHead
            let last = r.RecommendationData.History |> List.tryLast
            {|  File = FileSystem.relative report.Path file
                LoC = r.RecommendationData.LoC
                Priority = r.RecommendationData.RelativePriority
                Comments = r.Comments
                Changes = r.RecommendationData.History |> List.length
                Authours = r.RecommendationData.History |> distinctAuthors |> List.length
                CreatedAt = first |> Option.map (fun x -> x.Date)
                CreatedBy = first |> Option.map (fun x -> x.Author)
                LastUpdate = last |> Option.map (fun x -> x.Date)
                LastUpdateBy = last |> Option.map (fun x -> x.Author)
            |})
        |> Array.iter (fun x ->
            if(x.Comments.Length > 0) then
                let dtformat (dt : DateTimeOffset) = dt.ToLocalTime().ToString("yyyy-MM-dd")
                let printIfSome t = function Some x -> (printf t x) | None -> ()
                let printIfNotZero t i = if i > 0 then printf t i else ()
                let changeAuthour dt auth = match (dt,auth) with Some d, Some (Git.Author a) -> Some (sprintf "%s (%s)" (d |> dtformat) a) | _ -> None
                
                printfn "===> %s" x.File
                
                printf "\t\tPriority : %i" x.Priority
                printIfNotZero "\tChanges : %i" x.Changes
                printIfNotZero "\tLoC : %i" x.LoC
                printIfNotZero "\tAuthours : %i" x.Authours
                printIfSome "\tCreated : %s" (changeAuthour x.CreatedAt x.CreatedBy)
                printIfSome "\tUpdated : %s" (changeAuthour x.LastUpdate x.LastUpdateBy)
                printfn ""
                
                x.Comments |> List.iter (printfn "\t%s")
        )
        report
