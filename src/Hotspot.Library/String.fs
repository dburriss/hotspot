namespace System

module String =
    open System
    let split (sep:string []) (s:string) = s.Split(sep, StringSplitOptions.RemoveEmptyEntries)
    let splitLines s = split [|Environment.NewLine|] s
    let startsWith (value:string) (s:string) = s.StartsWith(value)
    let sub start len (s:string) = s.Substring(start, len)
    let join<'a> (sep:string) (xs:'a seq) = String.Join(sep, xs)