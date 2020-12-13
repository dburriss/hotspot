module MetricsTests

open Hotspot
open Spectre.IO
open Spectre.IO.Testing
open Xunit
open Swensen.Unquote


[<Fact>]
let ``Loc FetchMetrics returns code metrics``() =
    let code = """
using System;
// top level statements
Console.WriteLine("Hello World!");
"""
    let environment = FakeEnvironment.CreateUnixEnvironment()
    let filesystem = FakeFileSystem(environment)
    let csFile = filesystem.CreateFile(FilePath "Program.cs").SetTextContent(code)
    
    let locOpt = Loc.getLoc filesystem csFile
    let metricsOpt = Loc.fetchMetrics filesystem csFile
    test <@ metricsOpt <> None @>
    
    let loc = locOpt |> Option.map (fun x -> x.LoC) |> Option.defaultValue 0
    let metricsLoc = metricsOpt |> Option.map (fun x -> x.LoC |> Option.defaultValue -1) |> Option.defaultValue -1
    test <@ metricsLoc = loc @>

[<Fact>]
let ``Scc FetchMetrics returns code metrics``() =
    let code = """
using System;
// top level statements
Console.WriteLine("Hello World!");
"""
    let json = """[{"Name":"F#","Bytes":67504,"CodeBytes":0,"Lines":1980,"Code":1527,"Comment":137,"Blank":316,
"Complexity":115,"Count":27,"WeightedComplexity":0,"Files":[{"Language":"F#","PossibleLanguages":["F#"],"Filename":"build.fsx",
"Extension":"fsx","Location":"Working/build.fsx","Symlocation":"","Bytes":7106,"Lines":216,"Code":152,"Comment":25,"Blank":39,
"Complexity":5,"WeightedComplexity":0,"Hash":null,"Callback":null,"Binary":false,"Minified":false,"Generated":false}]}]
"""
    let sccLines = SCC.parse json |> Some
    let environment = FakeEnvironment.CreateUnixEnvironment()
    let filesystem = FakeFileSystem(environment)
    let root = filesystem.GetFakeDirectory(DirectoryPath "/")
    let fsxFile = filesystem.CreateFile(FilePath "build.fsx")
    let metricsOpt = SCC.fetchMetrics root sccLines fsxFile
    let metricsLoc = metricsOpt |> Option.map (fun x -> x.LoC |> Option.defaultValue -1) |> Option.defaultValue -1
    let metricsComplexity = metricsOpt |> Option.map (fun x -> x.CyclomaticComplexity |> Option.defaultValue -1) |> Option.defaultValue -1
    test <@ metricsLoc = 152 @>
    test <@ metricsComplexity = 5 @>


[<Fact>]
let ``FetchMetrics for Scc and Loc  returns code metrics``() =
    let environment = FakeEnvironment.CreateUnixEnvironment()
    let filesystem = FakeFileSystem(environment)
    let root = filesystem.GetFakeDirectory(DirectoryPath "/")
    
    let code = """
open System
// top level statements
Console.WriteLine("Hello World!")
"""
    let fsxFile = filesystem.CreateFile(FilePath "build.fsx").SetTextContent(code)
    let json = """[{"Name":"F#","Bytes":67504,"CodeBytes":0,"Lines":1980,"Code":1527,"Comment":137,"Blank":316,
"Complexity":115,"Count":27,"WeightedComplexity":0,"Files":[{"Language":"F#","PossibleLanguages":["F#"],"Filename":"build.fsx",
"Extension":"fsx","Location":"Working/build.fsx","Symlocation":"","Bytes":7106,"Lines":216,"Code":152,"Comment":25,"Blank":39,
"Complexity":5,"WeightedComplexity":0,"Hash":null,"Callback":null,"Binary":false,"Minified":false,"Generated":false}]}]
"""
    
    let sccLines = SCC.parse json |> Some
    
    let sccFetchMetrics = SCC.fetchMetrics root sccLines
    let locFetchMetrics = Loc.fetchMetrics filesystem
    let metricsOpt = Metrics.fetchMetricsOr sccFetchMetrics locFetchMetrics fsxFile
    let metricsLoc = metricsOpt |> Option.map (fun x -> x.LoC |> Option.defaultValue -1) |> Option.defaultValue -1
    let metricsComplexity = metricsOpt |> Option.map (fun x -> x.CyclomaticComplexity |> Option.defaultValue -1) |> Option.defaultValue -1
    test <@ metricsLoc = 152 @>
    test <@ metricsComplexity = 5 @>

    