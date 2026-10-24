# Third party notices

`Neotales.Core` links the following files from the shared source folder at
`std/core/Common/src`. They are compiled into this assembly but are not part of
this package's own code.

## .NET runtime interop

| File | Origin | Licence |
| --- | --- | --- |
| `std/core/Common/src/ffi/unix/libs.cs` | dotnet/runtime | MIT |
| `std/core/Common/src/ffi/unix/errors.cs` | dotnet/runtime | MIT |
| `std/core/Common/src/ffi/unix/system.native/stat.cs` | dotnet/runtime | MIT |
| `std/core/Common/src/ffi/unix/system.native/getpwuid.cs` | dotnet/runtime | MIT |

Each file carries the .NET Foundation licence header in full. The MIT licence
text is the same text this repository ships as its own licence, in
[LICENSE.md](../../../LICENSE.md).

The .NET runtime is a trademark of the .NET Foundation and its contributors. The
.NET Foundation does not endorse this package.
