# Third-party notices

`Neotales.Core` contains source derived from the projects listed below. Their
copyright and license notices are reproduced here because these terms travel
with the package. The repository's own MIT license remains in the repository
root; the package metadata declares `MIT` as its license expression.

## .NET runtime

| Repository source compiled into this package | Upstream source | License |
| --- | --- | --- |
| `cmn/src/ffi/unix/libs.cs` | [dotnet/runtime](https://github.com/dotnet/runtime) | MIT |
| `cmn/src/ffi/unix/errors.cs` | [dotnet/runtime](https://github.com/dotnet/runtime) | MIT |
| `cmn/src/ffi/unix/system.native/stat.cs` | [dotnet/runtime](https://github.com/dotnet/runtime) | MIT |
| `cmn/src/ffi/unix/system.native/getpwuid.cs` | [dotnet/runtime](https://github.com/dotnet/runtime) | MIT |
| `std/Core/src/Text/StringBuilderCache.cs` | [`StringBuilderCache.cs`](https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/Text/StringBuilderCache.cs) | MIT |

### .NET runtime MIT license

The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Upstream license: [dotnet/runtime LICENSE.TXT](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT).

The .NET runtime is a trademark of the .NET Foundation and its contributors. The
.NET Foundation does not endorse this package.

## ObviousAwait

`std/Core/src/Extras/TasksExtensions.cs` is derived from
[`ObviousExtensions.cs`](https://github.com/awaescher/ObviousAwait/blob/master/ObviousAwait/ObviousExtensions.cs)
in [ObviousAwait](https://github.com/awaescher/ObviousAwait), with extension
names adapted in this package.

### ObviousAwait MIT license

MIT License

Copyright (c) 2020 Andreas Wäscher

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Upstream license: [ObviousAwait LICENSE](https://github.com/awaescher/ObviousAwait/blob/master/LICENSE).
