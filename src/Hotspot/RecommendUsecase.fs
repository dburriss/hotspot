namespace Hotspot

open Spectre.Cli

type HotspotSetting () =
    inherit CommandSettings()
    
    [<CommandOption("-r|--repository-folder <REPOSITORY>")>]
    member val RepositoryFolder = System.Environment.CurrentDirectory with get, set
    [<CommandOption("-t|--target-folder <TARGET>")>]
    member val TargetFolder = "./" with get, set
    
//type RecommendCommand () =
//    inherit Command<HotspotSetting>()
//    override this.Execute(context : CommandContext, remaining : HotspotSetting) =
//        1

module RecommendUsecase =
    let init = ()