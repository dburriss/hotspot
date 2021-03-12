namespace Hotspot

module Weighting =
    
    open System

    let private ticks (dt:DateTimeOffset) = dt.UtcTicks

    // Only meaningful for codebase with consistent work going on.
    // TODO: If the last thing worked on was complex but a long time ago, it would still show up as meaningful.
    let calculate (startDt, endDt) nowDt =
        // linear but should possibly be exponential
        let now = nowDt |> ticks
        let start = startDt |> ticks
        let finish = endDt |> ticks
        Maths.shiftTo100L start finish now
        // let percentage = (now - start) / (finish - start)
        // percentage * (100L - 1L) + 1L



