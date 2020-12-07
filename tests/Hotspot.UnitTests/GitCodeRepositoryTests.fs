module GitCodeRepositoryTests

open System
open Hotspot
open Spectre.IO
open Xunit
open Hotspot.Git
open Hotspot.Helpers
open Swensen.Unquote

let setupGitCodeRepo() =
    let fs = FileSystem()
    let root = fs.Directory.Retrieve(DirectoryPath ".").Path
    let git = Git()
    let shouldIgnore = Ignore.live None
    GitCodeRepository(fs, root, shouldIgnore, git) :> CodeRepository

[<Fact>]
[<Trait("Category","Communication")>]
let ``Root path is set``() =
    let fs = FileSystem()
    let root = fs.Directory.Retrieve(DirectoryPath ".").Path
    let git = Git()
    let shouldIgnore = Ignore.live None
    let repo = GitCodeRepository(fs, root, shouldIgnore, git) :> CodeRepository
   
    test <@ repo.RootDirectory.Path.FullPath = root.FullPath @>
    
[<Fact>]
[<Trait("Category","Communication")>]
let ``Created date is before LastUpdated``() =
    let repo = setupGitCodeRepo()
    test <@ repo.CreatedAt() <= repo.LastUpdatedAt() @>
       
[<Fact>]
[<Trait("Category","Communication")>]
let ``HasHistory is true``() =
    let repo = setupGitCodeRepo()   
    test <@ repo.HasHistory() = true @>
         
[<Fact>]
[<Trait("Category","Communication")>]
let ``Choose files in bin confirms some are DLLs``() =
    let repo = setupGitCodeRepo()
    let isDll (file : IFile) = Some (file.Path.GetExtension() = ".dll") // as test runs in bin
    let values = repo.Choose isDll
    let checkFSharp = values |> Seq.exists id
    test <@ checkFSharp = true @>
    