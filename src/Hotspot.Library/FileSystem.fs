namespace Hotspot.Helpers

open Spectre.IO

module FileSystem =
    open System
    open System.IO
    open System.Diagnostics
    
    let private getDirs (fs : IFileSystem) (directory : IDirectory) =
        //IO.Directory.GetDirectories(path)
        if fs.Exist(directory.Path) then
            fs.GetDirectory(directory.Path).GetDirectories("*", SearchScope.Current) |> Seq.toArray
        else failwithf "FileSystem: Failed trying to find directories in %s, as the directory it does not exist." (directory.ToString())
        
    let private getFiles (fs : IFileSystem) (directory : IDirectory) =
        //IO.Directory.GetFiles(path)
        if fs.Exist(directory.Path) then
            fs.GetDirectory(directory.Path).GetFiles("*", SearchScope.Current) |> Seq.toArray
        else failwithf "FileSystem: Failed trying to find files in %s, as the directory it does not exist." (directory.ToString())
    
    let private getFileLines (file : IFile) =
        if file.Exists then
            use stream = file.OpenRead()
            use reader = new StreamReader(stream)
            seq {
                while not (reader.EndOfStream) do
                    reader.ReadLine()
            } |> Seq.toArray
        else failwithf "FileSystem: Failed trying to get lines from file %s, as the file does not exist." (file.Path.ToString())

    let private readLines (fs : IFileSystem) filePath =
        //File.ReadLines filePath
        let filePath = FilePath filePath
        let file = fs.GetFile(filePath)
        getFileLines file
    
    let private getFileContents (file : IFile) =
        if file.Exists then
            use stream = file.OpenRead()
            use reader = new StreamReader(stream)
            reader.ReadToEnd()
        else failwithf "FileSystem: Failed trying to get content from file %s, as the file does not exist." (file.Path.ToString())
        
    let loadText (fs : IFileSystem) filePath =
        //File.ReadAllText filePath
        let filePath = FilePath filePath
        let file = fs.GetFile(filePath)
        getFileContents file
    let relative (relativeTo : string) (path : string) =
        IO.Path.GetRelativePath(relativeTo, path)
        //(FilePath path).GetRelativePath(DirectoryPath relativeTo)
    let combine (path : string, file : string) =
        IO.Path.Combine (path, file)
        
    let ext filePath =
        IO.FileInfo(filePath).Extension |> String.replace "." ""
        
    let fileLineMap (fs : IFileSystem)
        f filePath = filePath |> readLines fs |> Seq.map f
    
    let rec mapFiles<'a> (fs : IFileSystem) (f : IFile -> 'a) (path : IDirectory) =
        let dirs = path |> (getDirs fs)
        let files = path |> (getFiles fs)
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles fs f) dirs)
        }
        
    let rec mapFiles2 (fs : IFileSystem) (f : (IFile) -> 'a) (directory : IDirectory) =
        let dirs = directory |> getDirs fs
        let files = directory |> getFiles fs
        seq {
            yield! (files |> Seq.map f)
            yield! (Seq.collect (mapFiles2 fs f) dirs)
        }
    
    // for globbing check
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing?view=dotnet-plat-ext-3.1

        