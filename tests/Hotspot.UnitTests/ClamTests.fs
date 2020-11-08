module ClamTests

open Expecto
open Clam
let tests =
    testList "Clam tests" [
        testList "Builder tests" [
            test "Empty app only has help" {
                let app = App.create("test")
                Expect.equal app.name "test" "Name should be same"
            }
        ]
        
        testList "Match tests" [
            test "Empty app only has help" {
                let args = ""
                let app = App.create("test")
                Expect.equal app.name "test" "Name should be same"
            }
        ]
    ]
    