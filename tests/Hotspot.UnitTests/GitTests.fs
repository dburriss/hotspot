module GitTests

open System
open Xunit
open Hotspot.Git
open Hotspot.Helpers
open Swensen.Unquote

[<Fact>]
[<Trait("Category","Communication")>]
let ``IsGitRepository returns true``() =
    let git = Git()
    let isGit() = git.IsGitRepository("./")    
    test <@ isGit() = true @>
    
[<Fact>]
[<Trait("Category","Communication")>]
let ``First log on repo returns something``() =
    let git = Git()
    let first = git.FirstLog("./")
    test <@ first <> None @>
      
[<Fact>]
[<Trait("Category","Communication")>]
let ``First log on repo returns author and date``() =
    let git = Git()
    let first = git.FirstLog("./")
    
    test <@ first |> Option.map (fun l -> String.isEmpty l.Author) = Some false @>
    test <@ first |> Option.map (fun l -> l.Date) < Some (DateTimeOffset.UtcNow + TimeSpan.FromDays 1.) @>
    
[<Fact>]
[<Trait("Category","Communication")>]
let ``Last log on repo returns something``() =
    let git = Git()
    let first = git.LastLog("./")
    test <@ first <> None @>
      
[<Fact>]
[<Trait("Category","Communication")>]
let ``Last log on repo returns author and date``() =
    let git = Git()
    let first = git.LastLog("./")
    
    test <@ first |> Option.map (fun l -> String.isEmpty l.Author) = Some false @>
    test <@ first |> Option.map (fun l -> l.Date) < Some (DateTimeOffset.UtcNow + TimeSpan.FromDays 1.) @>
       
[<Fact>]
[<Trait("Category","Communication")>]
let ``Range on repo returns date range where first is before last``() =
    let git = Git()
    let (first, last) = git.RepositoryDateRange("./")
    
    test <@ first < last @>
   