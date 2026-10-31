#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=true
#:property TreatWarningsAsErrors=true

// Edits the single <Version> in Directory.Build.props. That value is the
// assembly version and the stable package version.
using System.Globalization;
using System.Text.RegularExpressions;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
    {
        Console.WriteLine("""
            usage:
              bump-version show
              bump-version major|minor|patch
              bump-version X.Y.Z
              bump-version check
            """);
        return args.Length == 0 ? 1 : 0;
    }

    try
    {
        var props = Path.Combine(FindRepositoryRoot(), "Directory.Build.props");
        switch (args[0])
        {
            case "check":
                Check();
                _ = ReadVersion(File.ReadAllText(props));
                Console.WriteLine("bump-version: clean");
                return 0;
            case "show":
                Console.WriteLine(ReadVersion(File.ReadAllText(props)));
                return 0;
            case "major":
            case "minor":
            case "patch":
                WriteVersion(props, Bump(ReadVersion(File.ReadAllText(props)), args[0]));
                return 0;
            default:
                WriteVersion(props, RequireVersion(args[0]));
                return 0;
        }
    }
    catch (Exception ex) when (ex is InvalidOperationException or FormatException)
    {
        Console.Error.WriteLine($"bump-version: {ex.Message}");
        return 1;
    }
}

static string Bump(string version, string part)
{
    var pieces = version.Split('.').Select(static value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
    switch (part)
    {
        case "major":
            pieces[0]++;
            pieces[1] = 0;
            pieces[2] = 0;
            break;
        case "minor":
            pieces[1]++;
            pieces[2] = 0;
            break;
        case "patch":
            pieces[2]++;
            break;
        default:
            throw new InvalidOperationException($"unknown bump '{part}'.");
    }

    return string.Join('.', pieces);
}

static void WriteVersion(string props, string version)
{
    var text = File.ReadAllText(props);
    _ = ReadVersion(text);
    var updated = VersionPattern().Replace(text, $"<Version>{version}</Version>", 1);
    File.WriteAllText(props, updated);
    Console.WriteLine(version);
}

static string ReadVersion(string text)
{
    var matches = VersionPattern().Matches(text);
    if (matches.Count != 1)
    {
        throw new InvalidOperationException($"expected one <Version>, found {matches.Count}.");
    }

    return RequireVersion(matches[0].Groups[1].Value);
}

static string RequireVersion(string version)
{
    if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant))
    {
        throw new FormatException($"'{version}' is not Major.Minor.Patch.");
    }

    return version;
}

static void Check()
{
    Expect(Bump("0.0.0", "patch"), "0.0.1");
    Expect(Bump("0.0.1", "minor"), "0.1.0");
    Expect(Bump("0.1.2", "major"), "1.0.0");
    Expect(RequireVersion("0.5.0"), "0.5.0");
}

static void Expect(string actual, string expected)
{
    if (!string.Equals(actual, expected, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"expected {expected}, got {actual}.");
    }
}

static Regex VersionPattern()
{
    return new Regex(
        @"(?<![A-Za-z])<Version>\s*([^<]+?)\s*</Version>",
        RegexOptions.CultureInvariant);
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "global.json"))
            && Directory.Exists(Path.Combine(directory.FullName, "eng")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("repository root was not found from the working directory.");
}
