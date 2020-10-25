namespace Hotspot.Git

module GitCommand =
    
    open System.Diagnostics
    open System
    
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
    
    let private gitLogByHash repository h =
        //Console.WriteLine("gitLogByHash")
        let cmd = sprintf "log -1 --format=format:\"%%h,%%ae,%%aI\" %s" h
        runGitCommand repository cmd


