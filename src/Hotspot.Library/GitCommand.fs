namespace Hotspot.Git

open System
type Commit = | Commit of string
type Author = | Author of string
type Lines = string list

type Log = {
    Commit : Commit
    Author : Author
    Date : DateTimeOffset
}
module GitCommand =
    
    open System.Diagnostics
    
    let runGitCommand repository command =
        //printfn "RUNNING: 'git %s'" command 
        let info = ProcessStartInfo()
        info.WorkingDirectory <- repository//must be full path not using ~/
        info.FileName <- "/usr/local/bin/git" //TODO: fix as this will only work on a *nix with this being the install dir - a config, if none then cycle through known locations
        info.Arguments <- command
        info.UseShellExecute <- false
        info.RedirectStandardOutput <- true
        info.RedirectStandardError <- true
        let p = Process.Start(info)

        let output = p.StandardOutput.ReadToEnd()
        let err = p.StandardError.ReadToEnd()
        p.WaitForExit()
        if String.IsNullOrEmpty(err) then
            if String.IsNullOrWhiteSpace(output) then
                None |> Ok
            else output |> Some |> Ok
        else Error (sprintf "ERROR running 'git %s' %s %s" command Environment.NewLine err)
        
    let logOfFileCmd file = sprintf "log --format=format:\"%%h,%%ae,%%aI\" --follow %s" file
    let logOfHashCmd hash = sprintf "log -1 --format=format:\"%%h,%%ae,%%aI\" %s" hash
    

    let private splitByNewline s = 
        //printfn "SPLITTING LINE: %A" s
        match s with
        | Some x -> x |> String.split [|Environment.NewLine|]
        | None -> Array.empty

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

    let private gitLogOfFile repository file = file |> logOfFileCmd |> runGitCommand repository

    let private gitLogByHash repository h =
        //Console.WriteLine("gitLogByHash")
        let cmd = sprintf "log -1 --format=format:\"%%h,%%ae,%%aI\" %s" h
        runGitCommand repository cmd

    let fileHistory repository file =
        match (gitLogOfFile repository file) with
        | Ok (Some output) -> output |> parseLogs |> Ok
        | Ok None -> [] |> Ok
        | Error err -> Error err

    let firstLog repository =
        //Console.WriteLine("firstLog")
        let run = runGitCommand repository
        let cmdGetFirstCommit = "rev-list --max-parents=0 HEAD"
        run cmdGetFirstCommit
        |> Result.map ( splitByNewline >> Array.tryLast)
        |> Result.bind (function 
                        | Some hash -> hash |> gitLogByHash repository
                        | None -> Ok None)
        |> Result.map (function
                        | Some line -> line |> lineToLog
                        | None -> None)

    let lastLog repository =
        //Console.WriteLine("lastLog")
        let run = runGitCommand repository
        let cmd = sprintf "log -1 --format=format:\"%%h,%%ae,%%aI\""
        run cmd
        //|> fun x -> printfn "DEBUG: %A" x ; x
        |> Result.map (Option.bind lineToLog)

    let repositoryRange repository =
        let first = repository |> firstLog
        let last = repository |> lastLog
        Hotspot.Result.map2 (fun f l -> (f |> Option.get |> fun x -> x.Date, l |> Option.get |> fun x -> x.Date)) first last

