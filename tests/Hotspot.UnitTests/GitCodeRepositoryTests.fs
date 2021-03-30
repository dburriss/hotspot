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
    let shouldIgnore = IgnoreFile.init Array.empty
    GitCodeRepository(fs, root, "*", shouldIgnore, git) :> CodeRepository
    
let setupGitCodeRepoWithIgnore(shouldIgnore) =
    let fs = FileSystem()
    let root = fs.Directory.Retrieve(DirectoryPath ".").Path
    let git = Git()
    GitCodeRepository(fs, root, "*", shouldIgnore, git) :> CodeRepository

[<Fact>]
[<Trait("Category","Communication")>]
let ``Root path is set``() =
    let fs = FileSystem()
    let root = fs.Directory.Retrieve(DirectoryPath ".").Path
    let git = Git()
    let shouldIgnore = IgnoreFile.init Array.empty
    let repo = GitCodeRepository(fs, root, "*", shouldIgnore, git) :> CodeRepository
   
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
    let ignoreF : IgnoreFile = IgnoreFile.init [||]
    let repo = setupGitCodeRepoWithIgnore(ignoreF)
    let isDll (file : IFile) = if file.Path.GetExtension() = ".dll" then Some file else None // as test runs in bin
    let values = repo.Choose isDll
    test <@ values |> Seq.isEmpty |> not = true @>
    