namespace Hotspot

open System
open Hotspot.Git
open Hotspot.Helpers
open Spectre.IO

type NoGitRepository(fileSystem : IFileSystem, rootPath : DirectoryPath) =
    interface CodeRepository with
        member val RootDirectory = fileSystem.GetDirectory(rootPath) with get
        member this.HasHistory() = false
        member this.CreatedAt() = DateTimeOffset.MinValue
        member this.LastUpdatedAt() = DateTimeOffset.MinValue
        member this.GetFileHistory : FetchHistory = failwith "Not implemented"
        member this.Choose f = failwith "Not implemented"
    
type GitCodeRepository( fileSystem : IFileSystem,
                        rootPath : DirectoryPath,
                        shouldIgnore : IgnoreFile,
                        git : Git) =
    let isGit = git.IsGitRepository(rootPath.FullPath)// should always be true
    let (createdAt, updatedAt) = git.RepositoryDateRange(rootPath.FullPath)
    interface CodeRepository with
        member val RootDirectory = fileSystem.GetDirectory(rootPath) with get
        member this.HasHistory() = isGit
        member this.CreatedAt() = createdAt
        member this.LastUpdatedAt() = updatedAt
        member this.GetFileHistory : FetchHistory =
                fun file ->
                    git.GitLogOfFile((this :> CodeRepository).RootDirectory.Path.FullPath, file.Path.FullPath)
                    |> Array.map
                           (function
                                | None -> None
                                | Some log -> Some { CommitId = log.Hash; Author = log.Author; Date = log.Date }
                            )
                    |> Array.choose id
                
            
        member this.Choose f =
            let map = fun file ->
                //printfn "MAP FILE: %s" filePath
                if shouldIgnore file then file, None
                else file, (f file)
            FileSystem.mapFiles fileSystem map (this :> CodeRepository).RootDirectory
            |> Seq.choose snd