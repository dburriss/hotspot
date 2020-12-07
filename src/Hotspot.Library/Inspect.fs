namespace Hotspot

module Measurement =
    
    let private get v1 v2 selector =
        let x1 = v1 |> selector
        let x2 = v2 |> selector
        match (x1, x2) with
        | Some _, _ -> x1
        | None, Some _ -> x2
        | None, None -> None

    let zip (m1 : Metrics) (m2 : Metrics) : Metrics =
        {
            LoC = get m1 m2 (fun x -> x.LoC)
            CyclomaticComplexity = get m1 m2 (fun x -> x.CyclomaticComplexity)
            InheritanceDepth = get m1 m2 (fun x -> x.InheritanceDepth)
            Coupling = get m1 m2 (fun x -> x.Coupling)
        }

module Inspect =
    
    open Hotspot.Helpers
    open Hotspot.Git
    open System.Diagnostics
    
    let myMetrics env filePath =
        {
            LoC = Loc.getStats env filePath |> fun x -> x.LoC |> Some
            CyclomaticComplexity = None
            InheritanceDepth = None
            Coupling = None
        } |> Some
        
    let private measureFiles (repository : CodeRepository) inspectFile =
        (repository.Choose inspectFile) |> Seq.toList
    
    let inspect : MeasureRepository =
        fun repository inspectFile ->
            let repositoryPath = repository.RootDirectory.Path.FullPath
            Debug.WriteLine(sprintf "Measure: repository=%s" repositoryPath) 
            {
                Path = repository.RootDirectory
                CreatedAt = repository.CreatedAt()
                LastUpdatedAt = repository.LastUpdatedAt()
                InspectedFiles = measureFiles repository inspectFile 
            }
    
    // TODO: 07/12/2020 dburriss@xebia.com | Move to a live module
//    module Live =
//        let inspectFile : InspectFile =
//            fun file ->
//                