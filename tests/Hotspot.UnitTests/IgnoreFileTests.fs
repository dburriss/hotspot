module IgnoreFileTests

open Hotspot
open Spectre.IO
open Xunit
open Swensen.Unquote

[<Fact>]
let ``default ignore excludes dll and so``() =
    let fileSys = FileSystem()

    let dllFile = fileSys.GetFile(FilePath "test.dll")
    let soFile = fileSys.GetFile(FilePath "test.so")
    let shouldIgnore = IgnoreFile.init IgnoreFile.defaultIgnoreGlobs
    
    test <@ (shouldIgnore dllFile) = true @>
    test <@ (shouldIgnore soFile) = true @>
    
[<Fact>]
let ``default ignore includes .cs, .fs, .ts``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "test.cs")
    let fsFile = fileSys.GetFile(FilePath "test.fs")
    let tsFile = fileSys.GetFile(FilePath "test.ts")

    let shouldIgnore = IgnoreFile.init IgnoreFile.defaultIgnoreGlobs
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = false @>
    test <@ (shouldIgnore tsFile) = false @>
    
[<Fact>]
let ``custom ignore includes .cs, .fs,  but excludes .ts``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "test.cs")
    let fsFile = fileSys.GetFile(FilePath "test.fs")
    let tsFile = fileSys.GetFile(FilePath "test.ts")

    let shouldIgnore = IgnoreFile.init [|"*.ts"|]
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = false @>
    test <@ (shouldIgnore tsFile) = true @>

     
[<Fact>]
let ``custom ignore includes .cs, .fs but excludes .ts with paths``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "path/to/source/test.cs")
    let fsFile = fileSys.GetFile(FilePath "path/to/source/test.fs")
    let tsFile = fileSys.GetFile(FilePath "path/to/source/test.ts")

    let shouldIgnore = IgnoreFile.init [|"**/*.ts"|]
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = false @>
    test <@ (shouldIgnore tsFile) = true @>
     
[<Fact>]
let ``default ignore with real .fs path should not ignore .fs``() =
    let fileSys = FileSystem()
    let fsFile = fileSys.GetFile(FilePath "/Users/dburriss@xebia.com/GitHub/Hotspot/src/Hotspot.Library/String.fs")

    let shouldIgnore = IgnoreFile.init IgnoreFile.defaultIgnoreGlobs
    
    test <@ (shouldIgnore fsFile) = false @>
     
[<Fact>]
let ``custom ignore with real .fs path should ignore .fs``() =
    let fileSys = FileSystem()
    let csFile = fileSys.GetFile(FilePath "/Users/dburriss@xebia.com/GitHub/Hotspot/src/Hotspot.Library/String.cs")
    let fsFile = fileSys.GetFile(FilePath "/Users/dburriss@xebia.com/GitHub/Hotspot/src/Hotspot.Library/String.fs")

    let shouldIgnore = IgnoreFile.init [|"**/*.fs"|]
    
    test <@ (shouldIgnore csFile) = false @>
    test <@ (shouldIgnore fsFile) = true @>
      
[<Fact>]
let ``default ignores git``() =
    let fileSys = FileSystem()
    let history = fileSys.GetFile(FilePath "/Users/dburriss@xebia.com/GitHub/.git/xyz/String.cs")

    let shouldIgnore = IgnoreFile.init IgnoreFile.defaultIgnoreGlobs
    
    test <@ (shouldIgnore history) = true @>
    
