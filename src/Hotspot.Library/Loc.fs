namespace Hotspot

open Spectre.IO

module Loc =
    
    open System
    open Hotspot.Helpers
    
    type LineStats = {
        Ext : string
        Lines : int
        LoC : int
        CommentLines : int
    }

    type private LineType = | Comment | Code | Empty

    // TODO: 08/12/2020 dburriss@xebia.com | Could count here and do in single pass
    let private inspectLine (line : string) = 
        let mutable t = Empty
        let mutable prevWasSlash = false
        for c in line do
            if t = Empty && Char.IsWhiteSpace c then 
                prevWasSlash <- false
            elif t = Empty && c = '/' then
                if prevWasSlash then 
                    t <- Comment
                else prevWasSlash <- true
            else t <- Code
        t


    let getLoc (fileSystem : IFileSystem) (file : IFile) =
        if file.Exists then
            let lineTypes = FileSystem.fileLineMap fileSystem inspectLine file |> Seq.toList
            Some {
                Ext = file.Path.GetExtension()
                Lines = lineTypes |> List.length
                LoC = lineTypes |> List.filter (fun x -> x = Code) |> List.length
                CommentLines = lineTypes |> List.filter (fun x -> x = Comment) |> List.length
            }
        else None
        
    let fetchMetrics (fileSystem : IFileSystem) : FetchCodeMetrics =
        fun file ->
            getLoc fileSystem file
            |> function
                | Some stats ->
                    Some {
                        LoC = Some stats.LoC
                        CyclomaticComplexity = None
                        InheritanceDepth = None
                        Coupling = None
                    }
                | None -> None