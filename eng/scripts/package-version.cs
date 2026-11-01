#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=true
#:property TreatWarningsAsErrors=true

// CI builds use 0.0.0-build-yyyyMMdd.rN. A release tag names an area and a
// channel: std/vYYYY.M.D-r.N publishes the assembly version unchanged, and
// -alpha.N, -beta.N, or -rc.N is appended to the package version. Further
// areas use the same shape, for example extensions, entityframework, and ssh.
// The check packs Neotales.Core and reads the nuspec, because an explicit
// Version ignores VersionSuffix and the string helper cannot see that.
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
    {
        Console.WriteLine("""
            usage:
              package-version ci --date yyyyMMdd --revision N
              package-version tag <area/vYYYY.M.D-r.N|alpha.N|beta.N|rc.N> [--version X.Y.Z]
              package-version area <area/vYYYY.M.D-...>
              package-version label <area/vYYYY.M.D-...>
              package-version check
            """);
        return args.Length == 0 ? 1 : 0;
    }

    try
    {
        switch (args[0])
        {
            case "check":
                Check();
                Console.WriteLine("package-version: clean");
                return 0;
            case "ci":
                Console.WriteLine(CiVersion(Option(args, "date"), Option(args, "revision")));
                return 0;
            case "tag":
                if (args.Length < 2)
                {
                    throw new InvalidOperationException("tag requires the git tag.");
                }

                var version = Optional(args, "version") ?? ReadAssemblyVersion();
                Console.WriteLine(TagVersion(args[1], version));
                return 0;
            case "area":
                if (args.Length < 2)
                {
                    throw new InvalidOperationException("area requires the git tag.");
                }

                Console.WriteLine(TagArea(args[1]));
                return 0;
            case "label":
                if (args.Length < 2)
                {
                    throw new InvalidOperationException("label requires the git tag.");
                }

                Console.WriteLine(TagLabel(args[1]));
                return 0;
            default:
                throw new InvalidOperationException($"unknown command {args[0]}.");
        }
    }
    catch (Exception ex) when (ex is InvalidOperationException or FormatException)
    {
        Console.Error.WriteLine($"package-version: {ex.Message}");
        return 1;
    }
}

static string CiVersion(string date, string revision)
{
    if (!Regex.IsMatch(date, @"^\d{8}$", RegexOptions.CultureInvariant))
    {
        throw new FormatException($"date '{date}' is not yyyyMMdd.");
    }

    var month = int.Parse(date.AsSpan(4, 2), CultureInfo.InvariantCulture);
    var day = int.Parse(date.AsSpan(6, 2), CultureInfo.InvariantCulture);
    if (month is < 1 or > 12 || day is < 1 or > 31)
    {
        throw new FormatException($"date '{date}' is not a calendar day.");
    }

    if (!Regex.IsMatch(revision, @"^[1-9]\d*$", RegexOptions.CultureInvariant))
    {
        throw new FormatException($"revision '{revision}' is not a positive integer.");
    }

    return $"0.0.0-build-{date}.r{revision}";
}

static string TagVersion(string tag, string assemblyVersion)
{
    var version = AssemblyVersion(assemblyVersion);
    var label = TagLabel(tag);
    return label.Length == 0 ? version : $"{version}-{label}";
}

static string TagArea(string tag)
{
    return ParseTag(tag).Groups["area"].Value;
}

static string TagLabel(string tag)
{
    var channel = ParseTag(tag).Groups["channel"].Value;
    return channel.StartsWith("r.", StringComparison.Ordinal) ? "" : channel;
}

static Match ParseTag(string tag)
{
    var match = Regex.Match(
        tag,
        @"^(?<area>[a-z][a-z0-9]*)/v(?<year>\d{4})\.(?<month>\d{1,2})\.(?<day>\d{1,2})-(?<channel>r\.\d+|alpha\.\d+|beta\.\d+|rc\.\d+)$",
        RegexOptions.CultureInvariant);
    if (!match.Success)
    {
        throw new FormatException(
            $"tag '{tag}' is not <area>/vYYYY.M.D-r.N, <area>/vYYYY.M.D-alpha.N, <area>/vYYYY.M.D-beta.N, or <area>/vYYYY.M.D-rc.N. The area names the solution, for example std, extensions, entityframework, or ssh.");
    }

    var month = int.Parse(match.Groups["month"].ValueSpan, CultureInfo.InvariantCulture);
    var day = int.Parse(match.Groups["day"].ValueSpan, CultureInfo.InvariantCulture);
    if (month is < 1 or > 12 || day is < 1 or > 31)
    {
        throw new FormatException($"tag '{tag}' has a month or day outside the calendar.");
    }

    return match;
}

static string ReadAssemblyVersion()
{
    var props = Path.Combine(FindRepositoryRoot(), "Directory.Build.props");
    var text = File.ReadAllText(props);
    var matches = Regex.Matches(text, @"(?<![A-Za-z])<Version>\s*([^<]+?)\s*</Version>", RegexOptions.CultureInvariant);
    if (matches.Count != 1)
    {
        throw new InvalidOperationException($"expected one <Version> in {props}, found {matches.Count}.");
    }

    return AssemblyVersion(matches[0].Groups[1].Value);
}

static string AssemblyVersion(string version)
{
    if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant))
    {
        throw new FormatException($"assembly version '{version}' is not Major.Minor.Patch.");
    }

    return version;
}

static void Check()
{
    Expect(CiVersion("20261031", "1"), "0.0.0-build-20261031.r1");
    Expect(TagVersion("std/v2026.10.31-r.1", "0.0.0"), "0.0.0");
    Expect(TagVersion("std/v2026.10.31-alpha.0", "0.0.0"), "0.0.0-alpha.0");
    Expect(TagVersion("std/v2026.11.3-beta.3", "0.5.0"), "0.5.0-beta.3");
    Expect(TagVersion("entityframework/v2026.11.3-rc.1", "0.5.0"), "0.5.0-rc.1");
    Expect(TagArea("std/v2026.10.31-r.1"), "std");
    Expect(TagArea("ssh/v2026.11.3-beta.3"), "ssh");
    Expect(TagLabel("std/v2026.10.31-r.1"), "");
    Expect(TagLabel("extensions/v2026.10.31-alpha.0"), "alpha.0");
    ExpectThrow(() => TagVersion("v2026.10.31-r.1", "0.0.0"));
    ExpectThrow(() => TagVersion("std/v2026.10.31", "0.0.0"));
    ExpectThrow(() => TagVersion("std/v2026.10.31-r1", "0.0.0"));
    ExpectThrow(() => TagVersion("std/v2026.10.31-preview.1", "0.0.0"));
    ExpectThrow(() => CiVersion("20261301", "1"));
    ExpectThrow(() => CiVersion("20261031", "0"));
    _ = AssemblyVersion(ReadAssemblyVersion());
    CheckPackedPackageVersion();
}

static void CheckPackedPackageVersion()
{
    var root = FindRepositoryRoot();
    var project = Path.Combine(root, "std", "Core", "src", "Neotales.Core.csproj");
    if (!File.Exists(project))
    {
        throw new InvalidOperationException($"expected packable project {project}.");
    }

    ExpectPacked(root, project, "beta.3", null, null, "0.0.0-beta.3");
    ExpectPacked(root, project, "rc.1", "1.2.3", null, "1.2.3-rc.1");
    ExpectPacked(root, project, "beta.3", null, "9.9.9", "9.9.9");
    ExpectPacked(root, project, null, null, null, "0.0.0", "beta.3");
}

static void ExpectPacked(
    string root,
    string project,
    string? label,
    string? version,
    string? packageVersion,
    string expected,
    string? versionSuffix = null)
{
    var output = Directory.CreateTempSubdirectory("dn-lib-pack-");
    try
    {
        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("pack");
        start.ArgumentList.Add(project);
        start.ArgumentList.Add("--nologo");
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("Release");
        start.ArgumentList.Add("-o");
        start.ArgumentList.Add(output.FullName);
        if (label is not null)
        {
            start.ArgumentList.Add("-p:ReleaseLabel=" + label);
        }

        if (version is not null)
        {
            start.ArgumentList.Add("-p:Version=" + version);
        }

        if (packageVersion is not null)
        {
            start.ArgumentList.Add("-p:PackageVersion=" + packageVersion);
        }

        if (versionSuffix is not null)
        {
            start.ArgumentList.Add("-p:VersionSuffix=" + versionSuffix);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet pack did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet pack failed ({process.ExitCode}).{Environment.NewLine}{stdout.GetAwaiter().GetResult()}{stderr.GetAwaiter().GetResult()}");
        }

        _ = stdout.GetAwaiter().GetResult();
        _ = stderr.GetAwaiter().GetResult();

        string? nupkg = null;
        foreach (var path in Directory.EnumerateFiles(output.FullName, "*.nupkg"))
        {
            if (path.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            nupkg = path;
            break;
        }

        if (nupkg is null)
        {
            throw new InvalidOperationException($"dotnet pack wrote no nupkg to {output.FullName}.");
        }

        var actual = ReadNuspecVersion(nupkg);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"expected package version {expected}, nuspec has {actual}.");
        }
    }
    finally
    {
        output.Delete(recursive: true);
    }
}

static string ReadNuspecVersion(string nupkg)
{
    using var archive = ZipFile.OpenRead(nupkg);
    ZipArchiveEntry? entry = null;
    foreach (var candidate in archive.Entries)
    {
        if (candidate.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
        {
            entry = candidate;
            break;
        }
    }

    if (entry is null)
    {
        throw new InvalidOperationException($"nupkg {nupkg} has no nuspec.");
    }

    using var reader = new StreamReader(entry.Open());
    var xml = reader.ReadToEnd();
    var match = Regex.Match(xml, "<version>([^<]+)</version>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    if (!match.Success)
    {
        throw new InvalidOperationException($"nuspec in {nupkg} has no version.");
    }

    return match.Groups[1].Value;
}

static void Expect(string actual, string expected)
{
    if (!string.Equals(actual, expected, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"expected {expected}, got {actual}.");
    }
}

static void ExpectThrow(Action action)
{
    try
    {
        action();
    }
    catch (FormatException)
    {
        return;
    }

    throw new InvalidOperationException("expected the version rule to reject the input.");
}

static string Option(string[] args, string name)
{
    return Optional(args, name) ?? throw new InvalidOperationException($"missing --{name}.");
}

static string? Optional(string[] args, string name)
{
    var flag = "--" + name;
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], flag, StringComparison.Ordinal))
        {
            return args[i + 1];
        }
    }

    return null;
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
