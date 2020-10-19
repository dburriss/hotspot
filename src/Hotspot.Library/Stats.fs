namespace Hotspot

module Stats =
    
    open System

    let private ticks (dt:DateTimeOffset) = dt.UtcTicks

    // https://math.stackexchange.com/questions/914823/shift-numbers-into-a-different-range/914843#914843?newreg=7a69752f1d4a4a0d8cb7ab3b0b475c0e
    let inline private transform a b c d t =
        c + ( ( (d - c) / (b - a) ) * (t - a) )

    let inline shift toMin toMax fromMin fromMax value =
        transform (fromMin |> double) (fromMax |> double) (toMin |> double) (toMax |> double) (value |> double)

    let shiftTo100 fromMin fromMax value = shift 1 100 fromMin fromMax value |> int
    let shiftTo100L fromMin fromMax value = shift 1L 100L fromMin fromMax value |> int64

    // Only meaningful for codebase with consistent work going on.
    // TODO: If the last thing worked on was complex but a long time ago, it would still show up as meaningful.
    let calculateCoeffiecient startDt endDt nowDt =
        // linear but should possibly be exponential
        let now = nowDt |> ticks
        let start = startDt |> ticks
        let finish = endDt |> ticks
        shiftTo100L start finish now
        // let percentage = (now - start) / (finish - start)
        // percentage * (100L - 1L) + 1L



