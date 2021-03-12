module LocTests

open Hotspot
open Spectre.IO
open Spectre.IO.Testing
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Check loc counts``() =
    let code = """
using System;
// top level statements
Console.WriteLine("Hello World!");
"""
    let environment = FakeEnvironment.CreateUnixEnvironment()
    let filesystem = FakeFileSystem(environment)
    let csFile = filesystem.CreateFile(FilePath "test.cs").SetTextContent(code)
    let loc = Loc.getLoc filesystem csFile
    test <@ loc <> None @>
    let v = loc |> Option.get
    
    test <@ v.CommentLines = 1 @>
    test <@ v.LoC = 2 @>

    