namespace Hotspot.Git

open Hotspot.Helpers
open System

type Git(?gitBinPath) =
    let isInGitRepo = "rev-parse --is-inside-work-tree" // true/false
    let firstCommitCmd = "rev-list --max-parents=0 HEAD"
    let lastLogCmd = """log -1 --format=format:"%h,%ae,%aI"""
    let logOfFileCmd file = sprintf "log --format=format:\"%%h,%%ae,%%aI\" \"%s\"" file
    let logOfHashCmd hash = sprintf "log -1 --format=format:\"%%h,%%ae,%%aI\" %s" (String.trim hash)
    
    let checkedBinaryBinary = GitCommand.findGitBinary (defaultArg gitBinPath "")
    
    let git = GitCommand.run checkedBinaryBinary
    
    new() = Git(Environment.GetEnvironmentVariable("HOTSPOT_GIT_EXECUTABLE"))
        
    member this.IsGitRepository(repositoryPath) =
        git repositoryPath isInGitRepo
        |> Array.tryHead
        |> function
            | Some s -> Convert.ToBoolean(s)
            | None -> failwithf "No output received when executing 'get %s'" isInGitRepo
        
    member this.FirstLog(repositoryPath) =
        git repositoryPath firstCommitCmd
        |> Array.tryLast
        |> function 
            | Some hash ->
                let cmd = logOfHashCmd hash
                git repositoryPath cmd
                |> Array.tryLast
                |> Option.bind GitParse.lineToLog
            | None -> None
            
    member this.LastLog(repositoryPath) =
        git repositoryPath lastLogCmd
        |> Array.tryHead
        |> function 
            | Some log -> GitParse.lineToLog log
            | None -> None
            
    member this.GitLogOfFile(repositoryPath, filePath) =
        let cmd = logOfFileCmd filePath
        git repositoryPath cmd
        |> Array.map GitParse.lineToLog
            
    member this.RepositoryDateRange(repositoryPath) =
        let getDate msg opt =
            opt
            |> Option.map (fun x -> x.Date)
            |> function
                | Some x -> x
                | None -> failwith msg
                
        let first = this.FirstLog(repositoryPath) |> getDate (sprintf "Failed to find first commit date in %s" repositoryPath)
        let last = this.LastLog(repositoryPath) |> getDate (sprintf "Failed to find last commit date in %s" repositoryPath)
        (first,last)