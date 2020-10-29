# Hotspot

Hotspot is a dotnet command line tool for analyzing code hotspots by looking at git history, LoC, complexity, and test test coverage. More coming soon...

## Using SCC

[SCC](https://github.com/boyter/scc) is an awesome cli tool for getting info like LoC and cyclomatic complexity for many different code programming languages.
You can use it as follows to generate a file that can then be used by Hotspot.

`scc --by-file --format json > scc_out.json`

## Usage

`dotnet hotspot recommend -r /GitRepo --scc-file /scc_out.json`