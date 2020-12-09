namespace Hotspot

module Metrics =
    
    /// Default strategy for applying FetchMetrics functions
    let fetchMetricsOr (scc : FetchCodeMetrics) (loc : FetchCodeMetrics) : FetchCodeMetrics =
        fun file ->
            scc file
            |> Option.orElseWith (fun () -> loc file)