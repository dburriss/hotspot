namespace Hotspot.Helpers

open Microsoft.Extensions.Logging
open Spectre.IO
open Spectre.IO

[<Interface>] type ILog<'a> = abstract Logger: ILogger<'a>
[<Interface>] type ILocalFileSystem = abstract FileSystem: IFileSystem

module FileSystem =
    open System
    open System.IO
    
    let private getDirs (env : ILocalFileSystem) path =
        //IO.Directory.GetDirectories(path)
        env.FileSystem.GetDirectory(path).GetDirectories("*", SearchScope.Current) |> Seq.map (fun d -> d.Path |> string)
        
    let private getFiles (env : ILocalFileSystem) path =
        //IO.Directory.GetFiles(path)
        env.FileSystem.GetDirectory(path).GetFiles("*", SearchScope.Current) |> Seq.map (fun f -> f.Path |> string)
    
    let private getFileLines (file : IFile) =
        use stream = file.OpenRead()
        use reader = new StreamReader(stream)
        seq {
            while not (reader.EndOfStream) do
                reader.ReadLine()
        }

    let private readLines (env : ILocalFileSystem) filePath =
        //File.ReadLines filePath
        let filePath = FilePath filePath
        let file = env.FileSystem.GetFile(filePath)
        getFileLines file
    
    let private getFileContents (file : IFile) =
        use stream = file.OpenRead()
        use reader = new StreamReader(stream)
        reader.ReadToEnd()
        
    let loadText (env : ILocalFileSystem) filePath =
        //File.ReadAllText filePath
        let filePath = FilePath filePath
        let file = env.FileSystem.GetFile(filePath)
        getFileContents file
    let relative (relativeTo : string) (path : string) =
        IO.Path.GetRelativePath(relativeTo, path)
        //(FilePath path).GetRelativePath(DirectoryPath relativeTo)
    let combine (path, file) = IO.Path.Combine (path, file)
    let ext filePath =
        IO.FileInfo(filePath).Extension |> String.replace "." ""
        
    let fileLineMap (env : ILocalFileSystem) f filePath = filePath |> readLines env |> Seq.map f
    
    let rec mapFiles<'a> (env : ILocalFileSystem) (f : string -> 'a) (path : string) =
        let dirPath = DirectoryPath path
        let dirs = dirPath |> (getDirs env)
        let files = dirPath |> (getFiles env) |> Seq.map (fun file -> combine(path, file))
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles env f) dirs)
        }
        
    let rec mapFiles2 (env : ILocalFileSystem) f (path : string) =
        let dirPath = DirectoryPath path
        let dirs = dirPath |> getDirs env
        let files = dirPath |> getFiles env |> Seq.map (fun file -> (path, file))
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles2 env f) dirs)
        }
    
    // for globbing check
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing?view=dotnet-plat-ext-3.1

module Log =
    let live<'a> : ILogger<'a> =
        let factory =
            LoggerFactory.Create(
                fun builder ->
                    do builder.AddFilter("Microsoft", LogLevel.Warning) |> ignore
                    do builder.AddFilter("System", LogLevel.Warning) |> ignore
                    do builder.AddFilter("Hotspot", LogLevel.Debug) |> ignore
                    do builder.AddConsole() |> ignore
                    do builder.AddEventLog() |> ignore
            )
        factory.CreateLogger()
        
    let debug (env: #ILog<'a>) fmt = Printf.kprintf env.Logger.LogDebug fmt
    let info (env: #ILog<'a>) fmt = Printf.kprintf env.Logger.LogInformation fmt
    let error (env: #ILog<'a>) fmt = Printf.kprintf env.Logger.LogError fmt

    
[<Struct>]
type AppEnv<'a> = 
    interface ILog<'a> with member _.Logger = Log.live<'a>
    interface ILocalFileSystem with member _.FileSystem = FileSystem() :> IFileSystem
        