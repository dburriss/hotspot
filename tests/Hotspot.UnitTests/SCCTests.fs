module SCCTests

open Xunit
open Hotspot
open Swensen.Unquote

[<Fact>]
let ``Can parse SCC json by file report into SccLine``() =
    let json = """[{"Name":"F#","Bytes":67504,"CodeBytes":0,"Lines":1980,"Code":1527,"Comment":137,"Blank":316,"Complexity":115,"Count":27,"WeightedComplexity":0,"Files":[{"Language":"F#","PossibleLanguages":["F#"],"Filename":"build.fsx","Extension":"fsx","Location":"build.fsx","Symlocation":"","Bytes":7106,"Lines":216,"Code":152,"Comment":25,"Blank":39,"Complexity":5,"WeightedComplexity":0,"Hash":null,"Callback":null,"Binary":false,"Minified":false,"Generated":false}]}]"""
    let langLine = SCC.parse json |> Array.head
    let fileLine = langLine.Files |> Array.head
    test <@  langLine.Name = "F#" @>
    test <@  fileLine.Code = 152 @>
    test <@  fileLine.Complexity = 5  @>
    test <@  fileLine.Filename = "build.fsx" @>
    