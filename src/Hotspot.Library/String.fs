namespace Hotspot.Helpers

module String =
    open System
    let split (sep:string []) (s:string) = s.Split(sep, StringSplitOptions.RemoveEmptyEntries)
    let splitLines s = split [|Environment.NewLine;"\n"|] s
    let startsWith (value:string) (s:string) = s.StartsWith(value)
    let sub start len (s:string) = s.Substring(start, len)
    let join<'a> (sep:string) (xs:'a seq) = String.Join(sep, xs)
    let replace (oldValue : string) newValue (s : string) = s.Replace(oldValue, newValue)
    let lower (s : string) = s.ToLowerInvariant()
    let contains (part : string) (s : string) = s.Contains(part)
    let containsAnyOf partList s = partList |> List.tryFind (fun part -> contains part s) |> Option.isSome
    let isEmpty = String.IsNullOrEmpty
    let isNotEmpty = isEmpty >> not
    let trim (s:string) = s.Trim()