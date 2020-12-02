namespace Hotspot.Git

open System.Diagnostics
open System.IO

module GitCommand =
    
    open System.Diagnostics
    open System
    open Hotspot.Helpers
    
    let private (|Windows|Linux|OSX|) osNameAndVersion =
        let osv = String.lower osNameAndVersion
        if osv.Contains("windows") then Windows
        elif osv.Contains("nix") then Linux
        elif String.containsAnyOf ["darwin"] osv then OSX
        else failwithf "Unknown OS found when trying to find git bin: %s" osNameAndVersion
    let mutable git_bin : string Option = None    
    let private gitBin() =
        match git_bin with
        | None ->
            let p = Environment.GetEnvironmentVariable("HOTSPOT_GIT_EXECUTABLE")
            Debug.WriteLineIf(String.isNotEmpty(p), sprintf "HOTSPOT_GIT_EXECUTABLE set to %s" p)
            let os = System.Runtime.InteropServices.RuntimeInformation.OSDescription
            let gitLocation =
                if String.isNotEmpty(p) then
                    p
                else
                    // TODO: 01/12/2020 dburriss@xebia.com | This needs to be fleshed out or not supported at all.. prob check existence of a few places then panic with instructions to set HOTSPOT_GIT_EXECUTABLE
                    match os with
                    | Windows ->
                        Debug.WriteLine("GitCommand: Detecting git on Windows...")
                        ["C:/Program Files/Git/mingw64/libexec/git-core/git.exe"] |> List.tryFind File.Exists |> Option.defaultValue ""
                    | Linux ->
                        Debug.WriteLine("GitCommand: Detecting git on Linux...")
                        ["/usr/bin/git"; "/usr/local/bin/git"; "/usr/lib/git-core/git"] |> List.tryFind File.Exists |> Option.defaultValue ""
                    | OSX ->
                        Debug.WriteLine("GitCommand: Detecting git on OSX...")
                        ["/usr/local/bin/git"; "/usr/bin/git"] |> List.tryFind File.Exists |> Option.defaultValue ""
            
            if String.isEmpty(gitLocation) then
                Debug.WriteLine("No git binary was found.")
                failwithf "No git binary was found for for the OS %s. %sConsider setting environment variable HOTSPOT_GIT_EXECUTABLE pointing to the `git` binary." os Environment.NewLine
            else Debug.WriteLine(sprintf "GitCommand: Using git binary found at %s" gitLocation)
            git_bin <- Some gitLocation
            gitLocation
        | Some gb -> gb
        
        
    // TODO: 01/12/2020 dburriss@xebia.com | Make this use IFileSystem? Maybe not... just make `run` replaceable.
    let run repository command =
        //printfn "RUNNING: 'git %s'" command 
        let info = ProcessStartInfo()
        info.WorkingDirectory <- repository//must be full path not using ~/
        info.FileName <- gitBin()
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
    let isInGitRepo = "rev-parse --is-inside-work-tree" // true/false
    