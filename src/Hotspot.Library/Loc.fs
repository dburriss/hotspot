namespace Hotspot

module Loc =
    
    open System
    open Hotspot.Helpers
    
    type LineStats = {
        Ext : string
        Lines : int
        LoC : int
        CommentLines : int
    }

    type LineType = | Comment | Code | Empty

    let inspectLine (line : string) = 
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


    let getStats filePath =
        let lineTypes = FileSystem.fileLineMap inspectLine filePath |> Seq.toList
        {
            Ext = FileSystem.ext filePath
            Lines = lineTypes |> List.length
            LoC = lineTypes |> List.filter (fun x -> x = Code) |> List.length
            CommentLines = lineTypes |> List.filter (fun x -> x = Comment) |> List.length
        }
