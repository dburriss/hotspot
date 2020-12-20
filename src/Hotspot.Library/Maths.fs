namespace Hotspot

module Maths =

    // https://math.stackexchange.com/questions/914823/shift-numbers-into-a-different-range/914843#914843?newreg=7a69752f1d4a4a0d8cb7ab3b0b475c0e
    let inline private transform a b c d t =
        c + ( ( (d - c) / (b - a) ) * (t - a) )

    let inline shift toMin toMax fromMin fromMax value =
        transform (fromMin |> double) (fromMax |> double) (toMin |> double) (toMax |> double) (value |> double)

    let shiftTo100 fromMin fromMax value = shift 1 100 fromMin fromMax value
    let shiftTo100L (fromMin : int64) (fromMax : int64) (value : int64) = shift 1L 100L fromMin fromMax value |> int64