---
title: Development
category: how-to
menu_order: 1
---

# Setting up for local development

1. Fork repository
2. Pull repository
3. If you intend on making a Pull Request, create a feature branch
4. Run `dotnet tool restore`
5. Run `dotnet paket install`
6. Run `dotnet fake build -t Test`

# Installing tool locally

1. (Optional) If you have installed the tool before you will need to clear your Nuget cache. `dotnet nuget locals all -c`. Or just bump the version by adding a new entry to CHANGELOG.md
2. Ensure `hotspot` is not in your .config/dotnet-tools.json manifest file
3. Run `dotnet fake build -t Install-Local`

## Running the docs locally

NOTE: On MacOSX I needed to comment out the compiler directives at top of `literalloader.fsx` and this change is checked in.

```fsharp
//#if !FORNAX
#load "contentloader.fsx"
open Contentloader
//#endif
```

1. Make sure paket restore has run successfully from root
2. Run `dotnet fake build -t BuildRelease` so there is definitely a DLL to pull docs from
3. Navigate to */docs* folder
4. Run `dotnet fornax watch`
5. Visit *http://localhost:8080/*
6. You can now make changes to the docs and they will update in real time