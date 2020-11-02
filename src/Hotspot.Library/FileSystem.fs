namespace Hotspot.Helpers

open Microsoft.Extensions.Logging
open Spectre.IO

module FileSystem =
    open System
    open System.IO
    
    let private getFiles path = IO.Directory.GetFiles(path)
    let private getDirs path = IO.Directory.GetDirectories(path)
    let private readLines filePath = File.ReadLines filePath
    let loadText filePath = File.ReadAllText filePath
    let relative (relativeTo : string) (path : string) = IO.Path.GetRelativePath(relativeTo, path)
    let combine (path, file) = IO.Path.Combine (path, file)
    let ext filePath = IO.FileInfo(filePath).Extension |> String.replace "." ""
    let fileLineMap f filePath = filePath |> readLines |> Seq.map f
    
    let rec mapFiles<'a> (f : string -> 'a) path =
        let dirs = path |> getDirs
        let files = path |> getFiles |> Seq.map (fun file -> combine(path, file))
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles f) dirs)
        }
        
    let rec mapFiles2 f path =
        let dirs = path |> getDirs
        let files = path |> getFiles |> Seq.map (fun file -> (path, file))
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles2 f) dirs)
        }
    
    // for globbing check
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing?view=dotnet-plat-ext-3.1

[<Interface>] type ILog<'a> = abstract Logger: ILogger<'a>

module Log =
    let live<'a> : ILogger<'a> =
            let factory =
                    LoggerFactory.Create(
                        fun builder ->
                            do builder.AddFilter("Microsoft", LogLevel.Warning) |> ignore
                            do builder.AddFilter("System", LogLevel.Warning) |> ignore
                            do builder.AddFilter("LoggingConsoleApp.Program", LogLevel.Debug) |> ignore
                            do builder.AddConsole() |> ignore
                            do builder.AddEventLog() |> ignore
                    )
            factory.CreateLogger()
        
    let debug (env: #ILog<'a>) fmt = Printf.kprintf env.Logger.LogDebug fmt
    let error (env: #ILog<'a>) fmt = Printf.kprintf env.Logger.LogError fmt

[<Interface>] type ILocalFileSystem = abstract FileSystem: IFileSystem
    
[<Struct>]
type AppEnv<'a> = 
    interface ILog<'a> with member _.Logger = Log.live<'a>
    interface ILocalFileSystem with member _.FileSystem = failwith "Not implemented"