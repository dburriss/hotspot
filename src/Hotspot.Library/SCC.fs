namespace Hotspot

open Hotspot.Helpers

[<CLIMutable>]
type FileLine = {
    Extension : string
    Filename : string
    Language : string
    Location : string
    Blank : int
    Bytes : int64    
    Code : int
    CodeBytes : int64
    Comment : int
    Complexity : int
    Lines : int
    WeightedComplexity : int
    Binary : bool
    Generated : bool
    Minified : bool
    PossibleLanguages : string array
}

[<CLIMutable>]
type SccLine = {
    Name : string
    Blank : int
    Bytes : int64    
    Code : int
    CodeBytes : int64
    Comment : int
    Complexity : int
    Count : int
    Lines : int
    WeightedComplexity : int
    Files : FileLine array
}


module SCC =
    open System.Text.Json
    let parse (json : string) =
        JsonSerializer.Deserialize<SccLine array>(json)
        
    let toMetricsLookup root (ignoreFile : IIgnoreFile) (sccLines : SccLine array) =
        let fromFileLine (x : FileLine) =
            (FileSystem.combine(root, x.Location), {
                LoC = x.Lines |> Some
                CyclomaticComplexity = x.Complexity |> Some
                InheritanceDepth = None
                Coupling = None
            })
        let lookup =
            sccLines
            |> Array.map (fun x -> x.Files)
            |> Array.concat
            |> Array.filter (fun x -> x.Filename |> (ignoreFile.IgnoreFile) |> not)
            |> Array.distinctBy (fun x -> x.Location)
            |> Array.map fromFileLine
            |> Map.ofArray
        //printfn "lookup %A" lookup
        fun filePath ->
            lookup |> Map.tryFind filePath