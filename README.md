# dn-lib

A collection of .NET libraries maintained by Neotales. Packages are organized by
library area; each package documents its API, usage, and compatibility alongside
its source.

## Development

Standard libraries live under `std/<Library>`. `cmn/src` is shared source,
compiled into consuming packages through explicit links rather than shipped as
a runtime dependency. A path-filtered build/release for a standard library must
include its project tree and every shared-source/configuration dependency (for
Core, `std/Core/**`, `cmn/src/**`, and the root build/solution files).
`dn.slnx` is the repository solution. `std.slnx` is what std CI builds.
`.github/workflows/std-ci.yml` builds and tests. `.github/workflows/std-cd.yml`
is continuous delivery: it pushes packages to NuGet.

The repository uses the .NET SDK pinned in `global.json`. Restore and build the
solution with:

```sh
dotnet restore dn.slnx
dotnet build dn.slnx --no-restore
```

Individual package READMEs describe their capabilities and usage. Packages are
licensed under the MIT License; see [LICENSE.md](LICENSE.md).
