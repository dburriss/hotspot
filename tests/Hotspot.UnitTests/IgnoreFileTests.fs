module IgnoreFileTests

open Hotspot
open Spectre.IO
open Xunit
open Swensen.Unquote

[<Fact>]
let ``default ignore excludes .cs, .fs, .ts``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "test.cs")
    let fsFile = fileSys.GetFile(FilePath "test.fs")
    let tsFile = fileSys.GetFile(FilePath "test.ts")
    let sqlFile = fileSys.GetFile(FilePath "test.sql")
    let dllFile = fileSys.GetFile(FilePath "test.dll")
    let shouldIgnore = Live.ignoreAllBut None
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = false @>
    test <@ (shouldIgnore tsFile) = false @>
    test <@ (shouldIgnore sqlFile) = true @>
    test <@ (shouldIgnore dllFile) = true @>
    