namespace Hotspot
        
    module Live =
        let fetchMetricsOr (scc : FetchMetrics) (loc : FetchMetrics) : FetchMetrics =
            fun file ->
                scc file
                |> Option.orElseWith (fun () -> loc file)
