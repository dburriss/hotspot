module AnalyzerTests

open Hotspot
open Spectre.IO
open Spectre.IO.Testing
open Xunit
open Swensen.Unquote

[<Fact>]
let ``analyzing inspected repository with no files results in analyzed repo with no files``() =
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
    test <@ analyzedRepo.AnalyzedFiles = [] @>

[<Fact>]
let ``analyzing inspected repository with 1 inspected file with no metrics and no history results in no analysis``() =
    let fs = FakeFileSystem(FakeEnvironment.CreateUnixEnvironment())
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
    
    test <@ analyzedRepo.CreatedAt = inspectedRepositoryCode.CreatedAt @>
    test <@ analyzedRepo.LastUpdatedAt = inspectedRepositoryCode.LastUpdatedAt @>
    test <@ analyzedRepo.Directory = inspectedRepositoryCode.Directory @>
    test <@ analyzedRepo.AnalyzedFiles = [] @>
