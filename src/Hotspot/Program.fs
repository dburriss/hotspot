open System
open Hotspot
open Hotspot.Helpers

[<EntryPoint>]
let main argv =
    
    let currentPath = Environment.CurrentDirectory
    printfn "Running against %s" currentPath
    let repoDir = argv |> Array.tryItem 0 |> Option.defaultValue currentPath
    let projFolder : ProjectFolder = argv |> Array.tryItem 1 |> Option.defaultValue "./"
    let includeList = argv |> Array.tryItem 2 |> Option.defaultValue "cs,fs,js" |> String.split [|","|] |> Array.toList
    let inExtensionIncludeList filePath = includeList |> List.contains (filePath |> FileSystem.ext)
    let repo =  repoDir |> Repository.init
    
    let repoData = repo |> Measure.gatherRepositoryRawData (Measure.fileRawData inExtensionIncludeList) projFolder

    let analyze = Analyse.performAnalysis (Analyse.analyzeData Analyse.calcPriorityFromHistory)
    let recommend analyzedRepo =
        // TODO: this can be done more efficiently
        let scores = analyzedRepo.Analysis |> List.map (fun x -> x.PriorityScore) 
        let min = scores |> List.min
        let max = scores |> List.max

        Recommend.makeRecommendationsWith (Recommend.analysisRecommendation Recommend.recommendations (Stats.shiftTo100L min max >> int)) analyzedRepo

    repoData 
    |> analyze 
    |> recommend
    |> Recommend.printRecommendations
    |> ignore

    0 // return an integer exit code
