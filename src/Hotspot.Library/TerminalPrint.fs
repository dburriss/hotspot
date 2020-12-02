namespace Hotspot

module TerminalPrint =
    open Spectre.Console
    
    // https://coolors.co/d88c9a-f2d0a9-f1e3d3-99c1b9-8e7dbe
    let subdued (s : string) =
        AnsiConsole.Foreground <- Color.SteelBlue
        AnsiConsole.WriteLine(s)
        AnsiConsole.Reset()
        
    let highlight (s : string) =
        AnsiConsole.Foreground <- Color.Blue
        AnsiConsole.WriteLine(s)
        AnsiConsole.Reset()
        
    let text (s : string) =
        AnsiConsole.Foreground <- Color.NavajoWhite1
        AnsiConsole.Write(s)
        AnsiConsole.Reset()
        
    let warning (s : string) =
        AnsiConsole.Foreground <- Color.LightGoldenrod2_2
        AnsiConsole.Write(s)
        AnsiConsole.Reset()
        
    let severe (s : string) =
        AnsiConsole.Foreground <- Color.IndianRed
        AnsiConsole.Write(s)
        AnsiConsole.Reset()        
        
    let info (s : string) =
        AnsiConsole.Foreground <- Color.DarkSeaGreen3
        AnsiConsole.Write(s)
        AnsiConsole.Reset()
        
    let debug (s : string) =
        AnsiConsole.Foreground <- Color.MediumPurple1
        AnsiConsole.Write(s)
        AnsiConsole.Reset()