module MetricsTests

open Hotspot
open Spectre.IO
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Load``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "test.cs")
    let fsFile = fileSys.GetFile(FilePath "test.fs")
    let tsFile = fileSys.GetFile(FilePath "test.ts")
    let shouldIgnore = Live.defaultIgnoreFile None
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = false @>
    test <@ (shouldIgnore tsFile) = false @>
    