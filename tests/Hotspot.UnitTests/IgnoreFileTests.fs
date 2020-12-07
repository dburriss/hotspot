module IgnoreFileTests

open System
open Hotspot
open Spectre.IO
open Xunit
open Hotspot.Git
open Hotspot.Helpers
open Swensen.Unquote

[<Fact>]
let ``default ignore excludes .cs, .fs, .ts``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "test.cs")
    let fsFile = fileSys.GetFile(FilePath "test.fs")
    let tsFile = fileSys.GetFile(FilePath "test.ts")
    let shouldIgnore = Ignore.live None
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = false @>
    test <@ (shouldIgnore tsFile) = false @>
    