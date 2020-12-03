namespace Hotspot

open System
open Hotspot.Helpers

type RecommendationData = {
    RelativePriority: int
    Metrics : Metrics option
    History : Git.Log list // TODO: 29/10/2020 dburriss@xebia.com | Make Option
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
    open Spectre.Console
    
    let private distinctAuthors (history : Git.Log list) = history |> List.distinctBy (fun h -> h.Author)

    let recommendations (data : RecommendationData) =
        [
            let nrChanges = data.History |> List.length
            let nrAuthors = data.History |> distinctAuthors |> List.length
            // how to handle?
            // if complexity & loc
            // if only loc
            // if history
            if(data.Metrics |> Metrics.hasLoC) then
                if data.Metrics |> Metrics.locPredicate (fun loc -> loc > 400) then 
                    if(data.RelativePriority >= 50 && nrChanges > 5) then 
                        yield sprintf "SEVERITY: MEDIUM | This file is large at %i lines of code and changes often. It is strongly suggested you break it up to avoid conflicting changes." (data.Metrics |> Metrics.locOrValue -1)
                    else 
                        yield sprintf "SEVERITY: LOW | You may want to break this file up into smaller files as it is %i lines of code." (data.Metrics |> Metrics.locOrValue -1)
                
                if(data.Metrics |> Metrics.locPredicate (fun loc -> loc > 100) && nrAuthors = 1) then 
                    if data.RelativePriority > 50 && data.RelativePriority < 80 then
                        yield "SEVERITY: MEDIUM | Bus factor is 1 on a significant file. Make sure covered by descriptive tests & try to spread knowledge across the team."
                    if data.RelativePriority >= 80 then
                        yield "SEVERITY: HIGH | Bus factor is 1 on a VERY significant file. Make sure covered by descriptive tests & try pair up working on this file to prioritize knowledge transfer."

            else
                if data.RelativePriority >= 80  && nrAuthors = 1 then
                    yield "SEVERITY: MEDIUM | This file seems to be significant based on changes. Make sure covered by descriptive tests & try get spread knowledge across the team."
                
                // if(data.Complexity >= 10 && data.RelativePriority >= 20) then 
                //     yield sprintf "PRIORITY: %i/100 | Due to cyclomatic complexity of %i and recency of changes, this should be simplified. See: http://codinghelmet.com/articles/reduce-cyclomatic-complexity-switchable-factory-methods" (data.RelativePriority) (data.Complexity)
            //else yield "No Comments"    
        
        ]

    let analysisRecommendation recommendations shiftPriority (analysis : Analysis) =
        let data = {
                RelativePriority = shiftPriority analysis.PriorityScore
                Metrics = analysis.Measurement.Metrics
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
                LoC = r.RecommendationData.Metrics |> Metrics.loc
                Complexity = r.RecommendationData.Metrics |> Metrics.loc
                Priority = r.RecommendationData.RelativePriority
                Comments = r.Comments
                Changes = r.RecommendationData.History |> List.length
                Authours = r.RecommendationData.History |> distinctAuthors |> List.length
                CreatedAt = first |> Option.map (fun x -> x.Date)
                CreatedBy = first |> Option.map (fun x -> x.Author)
                LastUpdate = last |> Option.map (fun x -> x.Date)
                LastUpdateBy = last |> Option.map (fun x -> x.Author)
            |})
        |> Array.sortByDescending (fun x -> x.Priority)
        |> Array.iter (fun x ->
            if(x.Comments.Length > 0) then
                let dtformat (dt : DateTimeOffset) = dt.ToLocalTime().ToString("yyyy-MM-dd")
                let sprintIfSome t = function Some x -> (sprintf t x) | None -> ""
                let sprintIfNotZero t i = if i > 0 then sprintf t i else ""
                let changeAuthour dt auth = match (dt,auth) with Some d, Some (Git.Author a) -> Some (sprintf "%s (%s)" (d |> dtformat) a) | _ -> None

                sprintf "===> %s" x.File |> TerminalPrint.text
                printfn ""
                sprintf "\t\tPriority : %i" x.Priority |> TerminalPrint.debug
                sprintIfNotZero "\tChanges : %i" x.Changes |> TerminalPrint.debug
                sprintIfSome "\tComplexity : %i" x.Complexity |> TerminalPrint.debug
                sprintIfSome "\tLoC : %i" x.LoC |> TerminalPrint.debug
                sprintIfNotZero "\tAuthours : %i" x.Authours |> TerminalPrint.debug
                sprintIfSome "\tCreated : %s" (changeAuthour x.CreatedAt x.CreatedBy) |> TerminalPrint.debug
                sprintIfSome "\tUpdated : %s" (changeAuthour x.LastUpdate x.LastUpdateBy) |> TerminalPrint.debug
                printfn ""
                x.Comments
                |> List.iter (fun s ->
                                if s.StartsWith("SEVERITY: HIGH") then
                                    sprintf "\t%s" s |> TerminalPrint.severe
                                    printfn ""
                                elif s.StartsWith("SEVERITY: MEDIUM") then
                                    sprintf "\t%s" s |> TerminalPrint.warning
                                    printfn ""
                                else
                                    sprintf "\t%s" s |> TerminalPrint.info
                                    printfn ""
                             )
        )
        report
