# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.3]
### Removed
- target as an option on recommend as currently not working as expected

## [0.0.2]
### Added
- Fixes and improvements in finding machines git binary
- Panic with invalid setup or request and return better error message

## [0.0.1]
### Added
- the `recommend` command
- Hardcoded recommendations rules
- LoC for languages where comment is `//` (no multiline comments supported)
- SCC as a source for accurate LoC and Cyclomatic Complexity
