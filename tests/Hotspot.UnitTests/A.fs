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
    let mutable createdAt = DateTimeOffset.UtcNow
    let mutable updatedAt = DateTimeOffset.UtcNow
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
        let mutable createdAt = DateTimeOffset.UtcNow
        let mutable lastTouchedAt = DateTimeOffset.UtcNow
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
            Some {
                File = FilePath name
                CreatedAt = Some createdAt
                LastTouchedAt = Some lastTouchedAt
                History = None
                Metrics = metrics
            } 
        
module A = 
    open System

    let emptyFileInspector : InspectFile =
        fun file -> InspectedFileBuilder(file.Path.FullPath).Build()
    
    module Date =
        let aDay = TimeSpan.FromHours 24.0
        let ofYesterday = DateTimeOffset.UtcNow - aDay
        let today = DateTimeOffset.UtcNow
        
