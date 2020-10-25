namespace Hotspot.Helpers

module Result =
    let map2 mapping r1 r2 =
        match (r1,r2) with
        | (Ok x1, Ok x2) -> mapping x1 x2 |> Ok
        | (Error e, _) -> Error e
        | (_, Error e) -> Error e

