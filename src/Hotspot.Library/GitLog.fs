namespace Hotspot.Git

open System
open Hotspot.Helpers

type Commit = | Commit of string
type Author = | Author of string
type Lines = string list

type Log = {
    Commit : Commit
    Author : Author
    Date : DateTimeOffset
}

module GitLog =
    let private arrToLog splitLine = 
        match splitLine with
        | [|sId;sEmail;sDate|] -> { Commit = (sId |> Commit); Author = (sEmail |> Author); Date = DateTimeOffset.Parse(sDate)}
        | _ -> failwithf "Line did not match expected items: %A" splitLine
        
    let private lineToLog line =
        if String.IsNullOrWhiteSpace line then None
        else
            let split = line |> String.split [|","|]
            split |> arrToLog |> Some

    let private folder logs line = 
        let log = lineToLog line
        [log] @ logs

    let private parseLogs (s:string) = 
        let lines = s |> String.splitLines |> Array.toList
        let data = []
        lines |> List.fold folder data

    let private gitLogOfFile repository file = file |> GitCommand.logOfFileCmd |> GitCommand.runGitCommand repository
        
    let fileHistory repository file =
        match (gitLogOfFile repository file) with
        | Ok (Some output) -> output |> parseLogs |> Ok
        | Ok None -> [] |> Ok
        | Error err -> Error err
        
    let private splitByNewline s = 
        //printfn "SPLITTING LINE: %A" s
        match s with
        | Some x -> x |> String.split [|Environment.NewLine|]
        | None -> Array.empty
        
    let firstLog repository =
        //Console.WriteLine("firstLog")
        let run = GitCommand.runGitCommand repository
        let cmdGetFirstCommit = "rev-list --max-parents=0 HEAD"
        run cmdGetFirstCommit
        |> Result.map ( splitByNewline >> Array.tryLast)
        |> Result.bind (function 
                        | Some hash -> hash |> GitCommand.logOfHashCmd |> GitCommand.runGitCommand repository
                        | None -> Ok None)
        |> Result.map (function
                        | Some line -> line |> lineToLog
                        | None -> None)

    let lastLog repository =
        //Console.WriteLine("lastLog")
        let run = GitCommand.runGitCommand repository
        let cmd = sprintf "log -1 --format=format:\"%%h,%%ae,%%aI\""
        run cmd
        //|> fun x -> printfn "DEBUG: %A" x ; x
        |> Result.map (Option.bind lineToLog)

    let repositoryRange repository =
        let first = repository |> firstLog
        let last = repository |> lastLog
        Result.map2 (fun f l -> (f |> Option.get |> fun x -> x.Date, l |> Option.get |> fun x -> x.Date)) first last