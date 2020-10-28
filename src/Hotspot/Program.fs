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
    
    let terminate = function
        | Error err ->
            do eprintfn "%s" err
            -1
        | Ok _ -> 0
    
    let repository =  repoDir |> Repository.init RepositoryDependencies.Live ignoreFile

    let printRecommendations =
        Measure.measure RepositoryDependencies.Live projFolder
        >> Analyse.analyse 
        >> Recommend.recommend
        >> Recommend.printRecommendations

    // Use case (default): Use LoC & print to console
    repository
    |> Result.map printRecommendations
    |> terminate
    
