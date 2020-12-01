namespace Hotspot.Git

module GitCommand =
    
    open System.Diagnostics
    open System
    open Hotspot.Helpers
    
    let (|Windows|Linux|OSX|) osNameAndVersion =
        let osv = String.lower osNameAndVersion
        if osv.Contains("windows") then Windows
        elif osv.Contains("nix") then Linux
        elif String.containsAnyOf ["darwin"] osv then OSX
        else failwithf "Unknown OS found when trying to find git bin: %s" osNameAndVersion
            
    
    let gitBin() =
        let p = Environment.GetEnvironmentVariable("HOTSPOT_GIT_EXECUTABLE")
        if not (String.IsNullOrEmpty(p)) then
            p
        else
            // TODO: 01/12/2020 dburriss@xebia.com | This needs to be fleshed out or not supported at all.. prob check existence of a few places then panic with instructions to set HOTSPOT_GIT_EXECUTABLE
            match System.Runtime.InteropServices.RuntimeInformation.OSDescription with
            | Windows ->  "C:/Program Files/Git/mingw64/libexec/git-core"
            | Linux ->  "/usr/bin/git"
            | OSX ->  "/usr/local/bin/git"
        
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
    