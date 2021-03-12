namespace Hotspot.Git
module GitCommand =

    open System
    open Hotspot.Helpers
    open System.Diagnostics
    open System.IO
    
    let private (|Windows|Linux|OSX|) osNameAndVersion =
        let osv = String.lower osNameAndVersion
        if osv.Contains("windows") then Windows
        elif osv.Contains("nix") then Linux
        elif String.containsAnyOf ["darwin"] osv then OSX
        else failwithf "Unknown OS found when trying to find git bin: %s" osNameAndVersion
    
    let findGitBinary defaultPath =
        Debug.WriteLineIf(String.isNotEmpty(defaultPath), sprintf "HOTSPOT_GIT_EXECUTABLE set to %s" defaultPath)
        let os = System.Runtime.InteropServices.RuntimeInformation.OSDescription
        let gitLocation =
            if String.isNotEmpty(defaultPath) then
                if File.Exists(defaultPath) then defaultPath
                else failwithf "Did not find git binary at %s as expected." defaultPath 
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
        gitLocation
    
    let run gitBinary repositoryRoot command =
        //printfn "RUNNING: 'git %s'" command
        Debug.WriteLine(sprintf "RUNNING GIT COMMAND: %s" command)
        let info = ProcessStartInfo()
        info.WorkingDirectory <- repositoryRoot//must be full path not using ~/
        info.FileName <- gitBinary
        info.Arguments <- command
        info.UseShellExecute <- false
        info.RedirectStandardOutput <- true
        info.RedirectStandardError <- true
        let p = Process.Start(info)

        let output =
            [|
                while not p.StandardOutput.EndOfStream do
                    let o = p.StandardOutput.ReadLine()
                    if String.isNotEmpty o then o
           |]
            
        let err = p.StandardError.ReadToEnd()
        p.WaitForExit()
        if String.IsNullOrEmpty(err) then
            output
        else failwithf "ERROR running 'git %s' %s %s" command (Environment.NewLine) err

    