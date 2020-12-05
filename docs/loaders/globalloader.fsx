#r "../_lib/Fornax.Core.dll"

type SiteInfo = {
    title: string
    description: string
    theme_variant: string option
    root_url: string
}

let config = {
    title = "Hotspot"
    description = "Hotspot is a CLI tool for inspecting source code in a git repository for possible hotpots you may want to look into to decrease maintenance and potential risk."
    theme_variant = Some "red"
    root_url =
      #if WATCH
        "http://localhost:8080/"
      #else
        "https://devonburriss.me/hotspot"
      #endif
}

let loader (projectRoot: string) (siteContet: SiteContents) =
    siteContet.Add(config)

    siteContet
