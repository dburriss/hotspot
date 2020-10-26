namespace Hotspot.Helpers

module FileSystem =
    open System
    let getFiles path = IO.Directory.GetFiles(path)
    let getDirs path = IO.Directory.GetDirectories(path)
    let combine (path, file) = IO.Path.Combine (path, file)
    let ext filePath = IO.FileInfo(filePath).Extension |> String.replace "." ""
    let relative (relativeTo : string) (path : string) = IO.Path.GetRelativePath(relativeTo, path)

    let rec mapFiles f path =
        let dirs = path |> getDirs
        let files = path |> getFiles |> Seq.map (fun file -> (path, file))
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles f) dirs)
        }
    
    // for globbing check
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing?view=dotnet-plat-ext-3.1