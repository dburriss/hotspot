namespace Hotspot

open System
open System.Collections.Generic
open Spectre.IO
open Spectre.IO.Testing
open Hotspot.Helpers

type FileSystemBuilder() =
    let env = FakeEnvironment.CreateUnixEnvironment()
    let fs = FakeFileSystem(env)
    member this.WithFile (filePath) =
        fs.CreateFile(FilePath filePath) |> ignore
        this
        
    member this.WithFile(filePath, content) =
        fs.CreateFile(FilePath filePath).SetTextContent(content) |> ignore
        this
        
    member this.Build() = fs

type FakeCodeRepository(shouldIgnore) =
    let env = FakeEnvironment.CreateUnixEnvironment()
    let fileSystem = FakeFileSystem(env)
    let mutable isGit = false
    let mutable createdAt = DateTimeOffset.Parse("2021/03/25")
    let mutable updatedAt = DateTimeOffset.Parse("2021/03/25")
    let mutable rootPath = DirectoryPath "/"
    do fileSystem.CreateDirectory(rootPath) |> ignore
    let history = Dictionary<string, History>()
    
    member this.IsGit() =
        isGit <- true
        this
    
    member this.AddFile filePath =
        fileSystem.CreateFile(FilePath filePath) |> ignore
        this
    
    interface CodeRepository with
        member val RootDirectory = fileSystem.GetDirectory(rootPath) with get
        member this.HasHistory() = isGit
        member this.CreatedAt() = createdAt
        member this.LastUpdatedAt() = updatedAt
        member this.GetFileHistory : FetchHistory =
                fun file ->
                    let hs =
                        match history.TryGetValue(file.Path.FullPath) with // TODO: 20/12/2020 dburriss@xebia.com | Make relative path
                        | (false, _) -> [||]
                        | (true, value) -> value
                    hs
            
        member this.Choose f =
            let map = fun file ->
                //printfn "MAP FILE: %s" filePath
                if shouldIgnore file then file, None
                else file, (f file)
            let mapped =
                FileSystem.mapFiles fileSystem map (this :> CodeRepository).RootDirectory
                |> Seq.choose snd
            mapped
    
    type InspectedFileBuilder(name) =
        let mutable _loc = None
        let mutable _complexity = None
        let mutable createdAt = DateTimeOffset.Parse("2021/03/25")
        let mutable lastTouchedAt = DateTimeOffset.Parse("2021/03/25")
        member this.WithLoc(loc) =
            _loc <- Some loc
            this
        member this.WithComplexity(complexity) =
            _complexity <- Some complexity
            this
        member this.Build() =
            let metrics =
                if Option.isSome _loc || Option.isSome _complexity then
                    Some {
                        LoC = _loc
                        CyclomaticComplexity = _complexity
                        InheritanceDepth = None
                        Coupling = None
                    }
                else None
            {
                File = FilePath name
                CreatedAt = Some createdAt
                LastTouchedAt = Some lastTouchedAt
                History = None
                Metrics = metrics
            } 
        
module A = 
    open System

    let emptyFileInspector : InspectFile =
        fun file -> InspectedFileBuilder(file.Path.FullPath).Build() |> Some
    
    module Date =
        let aDate = DateTimeOffset.Parse("2021/03/25")
        let aDay = TimeSpan.FromHours 24.0
        let theDayBefore = aDate - aDay
        
    module InspectedRepositoryCode =
        let withFiles (files : InspectedFile list) =
            let folderM map =
                fun dt (file : InspectedFile)  ->
                    if (map file) < dt then (map file)
                    else dt
            let mapCreated (f : InspectedFile) = (f.CreatedAt |> Option.defaultValue (Date.aDate))
            let mapUpdated (f : InspectedFile) = (f.LastTouchedAt |> Option.defaultValue (Date.aDate))
            let createdAt = files |> List.fold (folderM mapCreated) (Date.aDate)
            let updatedAt = files |> List.fold (folderM mapUpdated) (Date.aDate)
            let inspectedRepositoryCode : InspectedRepositoryCode = {
                Directory = DirectoryPath "/"
                CreatedAt = createdAt
                LastUpdatedAt = updatedAt
                InspectedFiles = files
            }
            inspectedRepositoryCode
        
