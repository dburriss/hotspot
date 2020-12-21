module InspectorTests

open Hotspot

open Xunit
open Swensen.Unquote
open Spectre.IO

[<Fact>]
let ``inspect empty code repository returns no inspected files``() =
    let shouldIgnore = Live.defaultIgnoreFile None
    let fakeRepo = FakeCodeRepository(shouldIgnore)
    let fileInspector = A.emptyFileInspector
    
    let repo = fakeRepo :> CodeRepository
    let inspectedRepo = Inspector.inspect fileInspector repo

    test <@ inspectedRepo.InspectedFiles = [] @>

[<Fact>]
let ``inspect repository with only ignored files``() =
    let shouldIgnore : IgnoreFile = fun _ -> true
    let fakeRepo = FakeCodeRepository(shouldIgnore).AddFile("ignored.sql")
    let fileInspector = A.emptyFileInspector
    
    let repo = fakeRepo :> CodeRepository
    let inspectedRepo = Inspector.inspect fileInspector repo

    test <@ inspectedRepo.InspectedFiles = [] @>
    

[<Fact>]
let ``inspect code repository with matching file returns 1 inspected files``() =
    let loc = 10
    let shouldIgnore : IgnoreFile = fun file -> file.Path.GetExtension().ToLower() <> ".cs"
    let fakeRepo = FakeCodeRepository(shouldIgnore).AddFile("Program.cs")

    let fileInspectorGivesLoc = fun (file : IFile) -> InspectedFileBuilder(file.Path.FullPath).WithLoc(loc).Build()
    
    let repo = fakeRepo :> CodeRepository
    let inspectedRepo = Inspector.inspect fileInspectorGivesLoc repo

    let expectedMetrics = {
        LoC = Some loc
        CyclomaticComplexity = None
        InheritanceDepth = None
        Coupling = None
    }
    
    test <@ inspectedRepo.InspectedFiles.Head.File.FullPath = "/Working/Program.cs" @>
    test <@ inspectedRepo.InspectedFiles.Head.Metrics = Some expectedMetrics @>
