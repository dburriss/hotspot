namespace Hotspot

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
    open System.Diagnostics
    open Spectre.IO
    open Hotspot.Helpers
        
    
    let parse (json : string) =
        JsonSerializer.Deserialize<SccLine array>(json)
        
    let loadFromFile (fs : IFileSystem) (file : IFile) =
        if file.Exists then
            Debug.WriteLine(sprintf "SCC file: %s" file.Path.FullPath)
            FileSystem.loadText fs file |> parse |> Some
        else None
            
        
    // TODO: 07/12/2020 dburriss@xebia.com | This root needs to be the same root as scc was run at? or always repo root?
    let toMetricsLookup (root : IDirectory) (sccLinesOpt : SccLine array option) =
        match sccLinesOpt with
        | None -> fun _ -> None
        | Some sccLines -> 
            let fromFileLine (x : FileLine) =
                (root.Path.Combine(DirectoryPath.FromString(x.Location)).FullPath, { // TODO: 08/12/2020 dburriss@xebia.com | Make relative
                    LoC = x.Code |> Some
                    CyclomaticComplexity = x.Complexity |> Some
                    InheritanceDepth = None
                    Coupling = None
                })
                
            let lookup =
                sccLines
                |> Array.map (fun x -> x.Files)
                |> Array.concat
                |> Array.distinctBy (fun x -> x.Location)
                |> Array.map fromFileLine
                |> Map.ofArray
                
            Debug.WriteLine(sprintf "SCC file count: %i" (Array.sumBy (fun x -> x.Count) sccLines))
            //printfn "lookup %A" lookup
            (fun filePath -> lookup |> Map.tryFind filePath)
            
    let fetchMetrics (root : IDirectory) (sccLinesOpt : SccLine array option) : FetchCodeMetrics =
        let lookup = toMetricsLookup root sccLinesOpt
        fun file ->
            let filePath = file.Path.FullPath // TODO: 08/12/2020 dburriss@xebia.com | Make relative
            lookup filePath