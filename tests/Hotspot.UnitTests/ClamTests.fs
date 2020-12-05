module ClamTests
//
//open Expecto
//open Clam
//let tests =
//    testList "App tests" [
//        testList "Builder tests" [
//            test "App has constructed name" {
//                let app = App.create("test")
//                Expect.equal app.name "test" "Name should be same"
//            }
//            test "Help can be overriden" {
//                let app = App.create("test") |> App.overrideHelp "HELP"
//                Expect.equal app.helpStr (Some "HELP") "Help should be overriden value"
//            }
//            test "If not set version is blank" {
//                let version = App.create("test") |> App.GetVersion 
//                Expect.equal version "" "Help should be overriden value"
//            }
//            test "If print triggers build and then app has version and help commands" {
//                let app = App.create("test")
//                do app |> App.printHelp
//                Expect.equal (app.commands.Count) 2 "Help should be overriden value"
//            }
//            test "Empty app only has help and version" {
//                let app = App.create("test") |> App._build
//                let commandNames = app.commands |> Seq.toList |> List.map (fun x -> x.name)
//                Expect.equal commandNames ["help"; "version"] "Expected help and version commands"
//            }
//        ]
//        
//        testList "Match tests" [
//            
//        ]
//    ]
//    