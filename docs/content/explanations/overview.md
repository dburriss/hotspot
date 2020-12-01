---
title: Hotspot Overview
category: explanation
menu_order: 1
---

# Hotspot

Hotspot is a CLI tool for inspecting source code in a git repository for possible hotpots you may want to look into to decrease maintenance and potential risk.  

** Support of multiple languages

## Installation



## Features

- [x] Recommendations based on git changes & complexity
- [x] Consumption of [SCC](https://github.com/boyter/scc) to contribute metrics for analysis
- [ ] Integrate test coverage report in analysis
- [ ] Detailed control of the files that are included in the analysis
- [ ] Multiple supported output channels (console, API, file) and formats (text, markdown, html, json)
- [ ] Multiple metric sources: SCC, Visual Studio's Metric.exe, SonarQube

## * Language support

If using SCC, all [languages supported by SCC](https://github.com/boyter/scc/blob/master/LANGUAGES.md) are supported. If an SCC file is not used, Hotspot will count lines of code, attempting to ignore comments. The comment ignore is currently very rudimentary. See [Limitations](/limitations.html) for a more in-depth description.