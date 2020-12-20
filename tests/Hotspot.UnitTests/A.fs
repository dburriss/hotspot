namespace Hotspot

open Spectre.IO
open Spectre.IO.Testing

type FileSystemBuilder() =
    let env = FakeEnvironment.CreateUnixEnvironment()
    let fs = FakeFileSystem(env)
    member this.WithFile (filePath) =
        fs.CreateFile(FilePath filePath) |> ignore
        this
        
    member this.WithFile(filePath, content) =
        fs.CreateFile(FilePath filePath).SetTextContent(content) |> ignore
        this
        
    member this.Build() = fs
        
module A = 
    open System
    
    let file name =
        let env = FakeEnvironment.CreateUnixEnvironment()
        let fs = FakeFileSystem(env)
        fs.CreateFile(FilePath name)
    
    module Code =
        let cSharpHelloWorld =
            """
using System;
// top level statements
Console.WriteLine("Hello World!");
"""

    module Date =
        let aDay = TimeSpan.FromHours 24.0
        let ofYesterday = DateTimeOffset.UtcNow - aDay
        let today = DateTimeOffset.UtcNow
        



