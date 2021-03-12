namespace Hotspot.Git

open System
open Hotspot.Helpers
open System.Diagnostics

type Commit = string
type Author = string

type LogItem = {
    Hash : Commit
    Author : Author
    Date : DateTimeOffset
}

module GitParse =
           
    let private arrToLog splitLine = 
        match splitLine with
        | [||] -> failwith "split Line empty"
        | [|sId;sEmail;sDate|] -> { Hash = sId; Author = sEmail; Date = DateTimeOffset.Parse(sDate)}
        | _ -> failwithf "Line did not match expected items: %A of 3 instead found: %i" splitLine splitLine.Length
        
    let lineToLog line =
        try
            if String.IsNullOrWhiteSpace line then None
            else
                let split = line |> String.split [|","|]
                split |> arrToLog |> Some
        with
        | e ->
            let msg = sprintf "Failed to parse git output %s." line
            Debug.WriteLine(msg)
            raise (FormatException(msg, e))