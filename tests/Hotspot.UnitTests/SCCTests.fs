module SCCTests

open Expecto
open Hotspot
let tests =
    testList "SCC tests" [
        test "Can parse SCC json by file report into SccLine" {
            let json = """[{"Name":"F#","Bytes":67504,"CodeBytes":0,"Lines":1980,"Code":1527,"Comment":137,"Blank":316,"Complexity":115,"Count":27,"WeightedComplexity":0,"Files":[{"Language":"F#","PossibleLanguages":["F#"],"Filename":"build.fsx","Extension":"fsx","Location":"build.fsx","Symlocation":"","Bytes":7106,"Lines":216,"Code":152,"Comment":25,"Blank":39,"Complexity":5,"WeightedComplexity":0,"Hash":null,"Callback":null,"Binary":false,"Minified":false,"Generated":false}]}]"""
            let langLine = SCC.parse json |> Array.head
            let fileLine = langLine.Files |> Array.head
            Expect.equal langLine.Name "F#" "Name should be F#"
            Expect.equal fileLine.Code 152 "Name should be F#"
            Expect.equal fileLine.Complexity 5 "Name should be F#"
            Expect.equal fileLine.Filename "build.fsx" "Name should be F#"
        }

//        testProperty "Reverse of reverse of a list is the original list" (
//            fun (xs:list<int>) -> List.rev (List.rev xs) = xs
//        )
    ]