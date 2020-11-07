namespace Hotspot

open System
open Hotspot.Helpers
open Spectre.Cli

type HotspotSetting () =
    inherit CommandSettings()
    
    [<CommandOption("-r|--repository-folder <REPOSITORY>")>]
    member val RepositoryFolder = System.Environment.CurrentDirectory with get, set
    
    [<CommandOption("-t|--target-folder <TARGET>")>]
    member val TargetFolder = "./" with get, set
    
    [<CommandOption("--scc-file <SCC>")>]
    member val SccFile = "" with get, set
    
type RecommendCommand (env : AppEnv<HotspotSetting>) =
    inherit Command<HotspotSetting>()
    
    let defaultIgnoreFile : IIgnoreFile = env :> IIgnoreFile
    
    let terminate = function
        | Error err ->
            do eprintfn "%s" err
            -1
        | Ok _ -> 0
        
    let loadSccFile filePath =
        FileSystem.loadText env filePath

    // Use case (default): Use LoC & print to console
    let sccMetrics root ignoreFile sccFile =
        sccFile
        |> loadSccFile
        |> SCC.parse
        |> SCC.toMetricsLookup root ignoreFile
        
    
    let printRecommendations metricsF projectFolder =
        Measure.measure (RepositoryDependencies.Live env) metricsF projectFolder
        >> Analyse.analyse 
        >> Recommend.recommend
        >> Recommend.printRecommendations
    
    let recommendf = fun (ctx : CommandContext) (settings : HotspotSetting) ->
        let repoDir = settings.RepositoryFolder
        let targetFolder = settings.TargetFolder
        
        printfn "REPOSITORY: %s" repoDir
        printfn "TARGET: %s" targetFolder
        let repository =  repoDir |> Repository.init (RepositoryDependencies.Live env) defaultIgnoreFile
        let useScc = settings.SccFile |> String.IsNullOrEmpty |> not
        if(useScc) then
            printfn "Using scc data..."
            repository |> Result.map (printRecommendations (sccMetrics repoDir (defaultIgnoreFile) settings.SccFile) targetFolder) |> terminate
        else
            printfn "Using my metrics..."
            repository |> Result.map (printRecommendations (Measure.myMetrics env) targetFolder) |> terminate
    override this.Execute(context : CommandContext, remaining : HotspotSetting) =
        
        1

module RecommendUsecase =
    let init = ()