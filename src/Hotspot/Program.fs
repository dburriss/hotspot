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
    let ignoreFile filePath = includeList |> List.contains (filePath |> FileSystem.ext) |> not
    let repo =  repoDir |> Repository.init RepositoryDependencies.Live
    
    repo
    |> Result.map (Measure.measure MeasureDependencies.Live projFolder ignoreFile)
    |> Result.map Analyse.analyse 
    |> Result.map Recommend.recommend
    |> Result.map Recommend.printRecommendations
    |> ignore

    0 // return an integer exit code
