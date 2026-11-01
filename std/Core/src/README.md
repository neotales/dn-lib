# Neotales.Core

Core primitives for the other Neotales libraries. Results, options, environment
expansion, ordered maps, command-line tokens, file helpers, and small extension
members live here so each library can use the same shapes.

## Results

`Neotales.Results` carries a value or an error.

- `Result`, `Result<T>`, and `Result<T, E>` for a success value or an `Error`.
- `ValueResult`, `ValueResult<T>`, and `ValueResult<T, E>` for the same choice
  when the success side is a value type.
- `Error` and `Error<T>` for the failure payload, message, and code.
- `IResult` and `IResult<T>` for the shared surface.
- `ResultsList<T>` for a list of results.
- `ResultException` when a result has to cross into an exception.

`Map`, `MapError`, `MapErrorValue`, `Or`, `ValueOrDefault`, `ErrorOrDefault`,
`Expect`, and `ExpectError` work on those types.

## Options

`Neotales.Options` carries a value or its absence.

- `Option<T>` and `ValueOption<T>`.
- `Some<T>`, `None<T>`, and the untyped `None` marker.
- `IOption` and `OptionException`.

## Environment

`Neotales.Env` reads, writes, splits, joins, and unsets environment variables.
`Env.Expand` expands `${NAME}`, `$NAME`, and `$(command)` in a string.
`EnvExpandOptions` selects Windows-style expansion, Unix assignment handling,
command substitution, and the shell that substitution runs through.
`EnvironmentException` is the failure type for that work.

## Collections

`Neotales.Collections.Generic` has the maps and lists the libraries share.

- `Map`, `Map<TValue>`, and `Map<TKey, TValue>`.
- `OrderedMap`, `OrderedMap<TValue>`, and `OrderedMap<TKey, TValue>`.
- `StringMap`, `OrderedStringMap`, and `StringList`.

`Map<TValue>` and `StringMap` compare keys without case. The ordered types keep
insertion order.

## Command lines

`Neotales.Exec` turns a command line into arguments and tokens.

- `CommandArgs` splits a command line into arguments.
- `CommandLexer`, `CommandToken`, and `CommandTokenKind` expose the tokens
  inside that split.

## Files

`Neotales.IO` writes, copies, and inspects files.

- `TeeTextWriter` and `SimpleTeeTextWriter` write to two writers, each with its
  own leave-open flag.
- `DisposableFile` and `DisposableDirectory` are temporary paths that delete
  themselves.
- `UnixFileStatus` and `FileSystemEntryType` describe a file-system entry.
- `Neotales.IO.Extras` adds `File` and `Directory` members: recursive copy,
  `Chown`, `Chmod`, `Stat`, `LStat`, and `WriteTextAsync`.

`Chmod`, `Chown`, and `LStat` are marked `UnsupportedOSPlatform("windows")`.
`Stat` runs on Windows, and `UnixFileStatus` then reports `-1` for the user id,
the group id, the device id, and the inode number.

## Text and other members

- `Neotales.Text.StringBuilderCache` rents a per-thread `StringBuilder`.
- `Neotales.Text.Extras` adds `StringBuilder` span helpers, `AppendCliArgument`,
  and `UTF8NoBom`.
- `Neotales.Extras` adds string, span, array, exception, task, and
  `UnixFileMode` helpers.
- `Observer<T>`, `Never`, and `ResourceNotFoundException` are the remaining
  shared primitives.

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

The library targets the shared .NET framework and includes Windows, Linux, and
macOS members. It is trimmable, so a trimmed app can drop the primitives it
does not call.
