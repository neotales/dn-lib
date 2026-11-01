# Neotales.Core changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package
follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added

- The package, which has not been released before. The surface is the result and
  option types, the environment variable helpers and expansion, the map and list
  types, the tee writers and disposable temporary paths, the file and directory
  extension members with the unix interop behind them, and the string, span,
  array, `StringBuilder` and task extension members.
- `ResultsList<T>` for holding several results and turning the errors in them
  into one `AggregateException`.
- `OrderedMap` and `OrderedStringMap`, plus the key and value tuple overloads
  that let a map be built from a collection of pairs.
- `StringList`, with the case insensitive contains and index-of-folder members
  that path handling needs.
