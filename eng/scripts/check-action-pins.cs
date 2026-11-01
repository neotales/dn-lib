#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=true
#:property TreatWarningsAsErrors=true

// Every third-party action must be pinned to a commit SHA, with the release
// version in a trailing comment. A tag can be moved after review. A bare SHA
// is pinned and still unreviewable. Local actions and docker actions are this
// repository's own code.
using System.Text.RegularExpressions;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("check-action-pins: name the workflow files to check.");
        return 1;
    }

    if (args[0] is "-h" or "--help" or "help")
    {
        Console.WriteLine("""
            usage:
              check-action-pins <workflow.yml>...
              check-action-pins check
            """);
        return 0;
    }

    if (args[0] == "check")
    {
        Check();
        Console.WriteLine("check-action-pins: clean");
        return 0;
    }

    var failed = false;
    foreach (var file in args)
    {
        if (!File.Exists(file))
        {
            Console.Error.WriteLine($"check-action-pins: no such file: {file}");
            failed = true;
            continue;
        }

        var lineNumber = 0;
        foreach (var line in File.ReadLines(file))
        {
            lineNumber++;
            var error = PinError(line);
            if (error is null)
            {
                continue;
            }

            Console.Error.WriteLine($"check-action-pins: {file}:{lineNumber}: {error}");
            failed = true;
        }
    }

    if (failed)
    {
        return 1;
    }

    Console.WriteLine("check-action-pins: clean");
    return 0;
}

static string? PinError(string line)
{
    var match = UsesPattern().Match(line);
    if (!match.Success)
    {
        return null;
    }

    var reference = match.Groups["reference"].Value;
    if (reference.StartsWith("./", StringComparison.Ordinal)
        || reference.StartsWith("docker://", StringComparison.Ordinal))
    {
        return null;
    }

    var at = reference.LastIndexOf('@');
    if (at < 0)
    {
        return $"{reference} has no version reference";
    }

    var pin = reference[(at + 1)..];
    if (!ShaPattern().IsMatch(pin))
    {
        return $"{reference} is not pinned to a commit SHA";
    }

    if (!VersionCommentPattern().IsMatch(line))
    {
        return $"{reference} is pinned but has no version comment";
    }

    return null;
}

static void Check()
{
    const string Sha = "3d3c42e5aac5ba805825da76410c181273ba90b1";
    Expect(PinError($"        uses: actions/checkout@{Sha} # v7.0.1"), null);
    Expect(PinError("        uses: ./.github/actions/local"), null);
    Expect(PinError("        uses: docker://alpine:3"), null);
    if (PinError("        uses: actions/checkout@v7.0.1") is null)
    {
        throw new InvalidOperationException("a tag reference was accepted.");
    }

    if (PinError($"        uses: actions/checkout@{Sha}") is null)
    {
        throw new InvalidOperationException("a bare SHA was accepted.");
    }
}

static void Expect(string? actual, string? expected)
{
    if (!string.Equals(actual, expected, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"expected '{expected}', got '{actual}'.");
    }
}

static Regex UsesPattern()
{
    return new Regex(@"uses:\s*[""]?(?<reference>[^""'\s#]+)", RegexOptions.CultureInvariant);
}

static Regex ShaPattern()
{
    return new Regex(@"^[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant);
}

static Regex VersionCommentPattern()
{
    return new Regex(@"#\s*v?\d+\.\d+", RegexOptions.CultureInvariant);
}
