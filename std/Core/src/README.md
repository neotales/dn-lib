# Neotales.Core

The runtime primitives the other packages in this repository are built on.

## What it does

- **Results and options.** `Neotales.Results` has `Result`, `Result<T>`,
  `Result<T, E>`, `ValueResult<T, E>`, `Error`, `IResult` and `ResultsList<T>`:
  a value or an error, with `Map`, `MapError`, `Bind`, `Or`, `OrDefault` and
  `TryCatch` to work with it without exceptions for expected failures.
  `Neotales.Options` has `Option<T>`, `ValueOption<T>`, `Some<T>`, `None<T>`
  and the untyped `None` marker, which is the same idea for a single value.
- **Environment variables.** `Neotales.Env` reads, writes, splits, joins and
  unsets environment variables, and `Env.Expand` expands `${NAME}`, `$NAME` and
  `$(command)` references in a string, with `EnvExpandOptions` deciding whether
  Windows style expansion, unix assignment handling and command substitution are
  honoured, and which shell the substitution runs through.
- **Collections.** `Neotales.Collections.Generic` has `Map`, `Map<TKey, TValue>`,
  `Map<TValue>`, `OrderedMap` and its two and three type parameter forms,
  `StringMap`, `OrderedStringMap`, and `StringList`.
- **Files and IO.** `Neotales.IO` has `TeeTextWriter` and
  `SimpleTeeTextWriter` (write to two writers at once, each with its own leave
  open flag), `DisposableFile` and `DisposableDirectory` (a temporary path that
  deletes itself), and `UnixFileStatus`. `Neotales.IO.Extras` adds the `File`
  and `Directory` extension members: recursive copy, `Chown`, `Chmod`, `Stat`,
  `LStat`, `WriteTextAsync` and the rest.
- **Extension members.** `Neotales.Extras` and `Neotales.Text.Extras` hold the
  string, span, array, `StringBuilder` and task helpers, and
  `Neotales.Text.StringBuilderCache` rents a per thread `StringBuilder`.

## What it does not do

- It does not resolve configuration. `Env` reads process environment variables
  and expands references inside a string; it does not read an appsettings file,
  a `.env` file or a user secrets store. A separate DotEnv package is deferred
  and is not part of this repository import.
- It does not run a process. `Neotales.Exec.CommandArgs` and `CommandLexer`
  parse a command line into arguments and tokens; nothing here starts a process.
  A separate process-execution package is deferred and is not part of this
  repository import.
- It does not store a secret. There is no masking, generation or credential
  store here. A separate secrets package is deferred and is not part of this
  repository import.
- It is not a general purpose collections library. The map and list types exist
  because this repository needs an ordered map and an object valued map with a
  single shape across areas, not to replace `Dictionary<TKey, TValue>`.
- The unix only members are unix only. `Chmod`, `Chown` and `LStat` are marked
  `UnsupportedOSPlatform("windows")` and throw `PlatformNotSupportedException`
  there, so the platform check happens at compile time where it can. `Stat` does
  work on Windows, and `UnixFileStatus` then reports `-1` for the user id, the
  group id, the device id and the inode number, because Windows does not expose
  them.

## Usage

```csharp
using Neotales.Results;
using static Neotales.Results.Result;

var parsed = Ok("10");
var number = parsed.Map(int.Parse);

var failed = Fail<string>(new InvalidOperationException("Failed to get value"));
Console.WriteLine(failed.Error.Message);
Console.WriteLine(failed.OrDefault("fallback"));
```

```csharp
using Neotales;

Console.WriteLine(Env.Expand("home is '${HOME}' or '${USERPROFILE}'"));
Env.PrependPath("/opt/tools/bin");
```

## Compatibility

The library has platform-specific code for Windows, Linux and macOS. This import
was validated on Linux only; other platforms have not been tested here. The
`Chmod`, `Chown` and `LStat` members are marked
`UnsupportedOSPlatform("windows")`.

This initial repository import contains Neotales.Core only. Related packages
and any coordinated group release/tag arrangement are deferred.
