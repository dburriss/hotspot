open System
open Expecto

[<EntryPoint>]
let main args =
    let allTests = testList "All Tests" [
        SCCTests.tests
        //ClamTests.tests
    ]
    runTestsWithArgs defaultConfig args allTests
