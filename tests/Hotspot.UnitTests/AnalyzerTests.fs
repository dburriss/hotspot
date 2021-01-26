module AnalyzerTests

open Hotspot
open Spectre.IO
open Spectre.IO.Testing
open Xunit
open Swensen.Unquote

[<Fact>]
let ``analyzing inspected repository has inspectedRepositoryCode dates``() =
    let inspectedRepositoryCode : InspectedRepositoryCode = {
        Directory = DirectoryPath "/"
        CreatedAt = A.Date.ofYesterday
        LastUpdatedAt = A.Date.today
        InspectedFiles = []
    }
    
    let analyzedRepo = Analyzer.analyze inspectedRepositoryCode
    
    test <@ analyzedRepo.CreatedAt = inspectedRepositoryCode.CreatedAt @>
    test <@ analyzedRepo.LastUpdatedAt = inspectedRepositoryCode.LastUpdatedAt @>
    test <@ analyzedRepo.Directory = inspectedRepositoryCode.Directory @>

[<Fact>]
let ``analyzing inspected repository with no files results in analyzed repo with no files``() =
    let inspectedRepositoryCode : InspectedRepositoryCode = {
        Directory = DirectoryPath "/"
        CreatedAt = A.Date.ofYesterday
        LastUpdatedAt = A.Date.today
        InspectedFiles = []
    }
    
    let analyzedRepo = Analyzer.analyze inspectedRepositoryCode
    
    test <@ analyzedRepo.AnalyzedFiles = [] @>

[<Fact>]
let ``analyzing inspected repository with 1 inspected file with no metrics and no history results in no analysis``() =
    let inspectedFile1 : InspectedFile = {
        File = FilePath "Program.cs"
        CreatedAt = Some A.Date.today
        LastTouchedAt = Some A.Date.today
        History = None
        Metrics = None
    } 
    let inspectedRepositoryCode : InspectedRepositoryCode = {
        Directory = DirectoryPath "/"
        CreatedAt = A.Date.ofYesterday
        LastUpdatedAt = A.Date.today
        InspectedFiles = [ inspectedFile1 ]
    }
    
    let analyzedRepo = Analyzer.analyze inspectedRepositoryCode
    
    test <@ analyzedRepo.AnalyzedFiles = [] @>

[<Fact>]
let ``analyzing inspected repository with 1 inspected file with metrics and no history results in a analysis``() =
    let inspectedFile1 = InspectedFileBuilder("Program.cs").WithLoc(10).WithComplexity(10).Build()
    let inspectedRepositoryCode = A.InspectedRepositoryCode.withFiles [inspectedFile1]
    
    let analyzedRepo = Analyzer.analyze inspectedRepositoryCode
    
    test <@ analyzedRepo.AnalyzedFiles.Length = 1 @>

[<Fact>]
let ``analyzing 2 files the one with higher LoC has higher priority``() =
    
    let getFile name analyzedRepo =
        analyzedRepo.AnalyzedFiles |> List.find (fun f -> f.File.GetFilename().ToString() = name)
    let inspectedFile1 = InspectedFileBuilder("Program.cs").WithLoc(10).Build()
    let inspectedFile2 = InspectedFileBuilder("Data.cs").WithLoc(100).Build()
    let inspectedRepositoryCode = A.InspectedRepositoryCode.withFiles [inspectedFile1; inspectedFile2]
    
    let analyzedRepo = Analyzer.analyze inspectedRepositoryCode
    
    let programFileAnalysis = getFile "Program.cs" analyzedRepo
    let dataFileAnalysis = getFile "Data.cs" analyzedRepo
    
    test <@ programFileAnalysis.PriorityScore < dataFileAnalysis.PriorityScore @>
    
[<Fact>]
let ``analyzing 2 files the one with higher Cyclomatic Complexity has higher priority``() =
    
    let getFile name analyzedRepo =
        analyzedRepo.AnalyzedFiles |> List.find (fun f -> f.File.GetFilename().ToString() = name)
    let inspectedFile1 = InspectedFileBuilder("Program.cs").WithComplexity(1).Build()
    let inspectedFile2 = InspectedFileBuilder("Data.cs").WithComplexity(10).Build()
    let inspectedRepositoryCode = A.InspectedRepositoryCode.withFiles [inspectedFile1; inspectedFile2]
    
    let analyzedRepo = Analyzer.analyze inspectedRepositoryCode
    
    let programFileAnalysis = getFile "Program.cs" analyzedRepo
    let dataFileAnalysis = getFile "Data.cs" analyzedRepo
    
    test <@ programFileAnalysis.PriorityScore < dataFileAnalysis.PriorityScore @>
