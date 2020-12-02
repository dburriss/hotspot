# Hotspot

Hotspot is a CLI tool for inspecting source code in a git repository for possible hotpots you may want to look into to decrease maintenance and potential risk.  

Support of multiple languages

## Installation

Prerequisite: [dotnet sdk](https://dotnet.microsoft.com/download)

`dotnet tool install -g hotspot`

## Basic Usage

Navigate to your git repository and run:  
`hotspot recommend`

For help, run:  
`hotspot --help`

You can also install it locally. See the [dotnet tool docs](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install)

## Features

- [x] Recommendations based on git changes & complexity
- [x] Consumption of [SCC](https://github.com/boyter/scc) to contribute metrics for analysis
- [ ] Integrate test coverage report in analysis
- [ ] Detailed control of the files that are included in the analysis
- [ ] Multiple supported output channels (console, API, file) and formats (text, markdown, html, json)
- [ ] Multiple metric sources: SCC, Visual Studio's Metric.exe, SonarQube

## Using SCC

[SCC](https://github.com/boyter/scc) is an awesome cli tool for getting info like LoC and cyclomatic complexity for many different code programming languages.
You can use it as follows to generate a file that can then be used by Hotspot.

`scc --by-file --format json > scc_out.json`
### Usage

`dotnet hotspot recommend -r /GitRepo --scc-file /scc_out.json`
## Language support

If using SCC, all [languages supported by SCC](https://github.com/boyter/scc/blob/master/LANGUAGES.md) are supported. If an SCC file is not used, Hotspot will count lines of code, attempting to ignore comments. The comment ignore is currently very rudimentary. See [Limitations](/limitations.html) for a more in-depth description.


