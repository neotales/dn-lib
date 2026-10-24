# dn-lib

A collection of .NET libraries maintained by Neotales. Packages are organized by
library area; each package documents its API, usage, and compatibility alongside
its source.

## Development

The repository uses the .NET SDK pinned in `global.json`. Restore and build the
solution with:

```sh
dotnet restore
dotnet build --no-restore
```

Individual package READMEs describe their capabilities and usage. Packages are
licensed under the MIT License; see [LICENSE.md](LICENSE.md).
