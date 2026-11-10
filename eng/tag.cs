#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=false
#:property NuGetAudit=false
#:property TreatWarningsAsErrors=true

// Create a dated release tag on neotales/dn-lib. The date is today in UTC.
// The channel is r unless --suffix names alpha, beta, or rc. The revision is
// one higher than the highest remote tag for that date and channel.
//
//   dotnet eng/tag.cs --dry-run
//   dotnet eng/tag.cs -s alpha --dry-run
//   dotnet eng/tag.cs --push
//
// --push sends the tag to the satellite. That starts dn-std-cd. A tag on the
// plab repository does not.
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
    {
        if (args.Length == 0)
        {
            return Create(args);
        }

        Console.WriteLine("""
            usage: eng/tag.cs [--suffix|-s r|alpha|beta|rc] [--group NAME] [--date YYYY-MM-DD] [--remote URL] [--ref REF] [--push] [--dry-run]
                   eng/tag.cs check

            The date is today, UTC. No suffix means a release, r.N. The revision
            is the next number on the remote for that date and channel.
            --push creates the tag on neotales/dn-lib. Without it, the tag is only printed.
            """);
        return 0;
    }

    try
    {
        if (args[0] == "check")
        {
            Check();
            Console.WriteLine("tag: clean");
            return 0;
        }

        return Create(args);
    }
    catch (Exception ex) when (ex is InvalidOperationException or FormatException)
    {
        Console.Error.WriteLine($"tag: {ex.Message}");
        return 1;
    }
}

static int Create(string[] args)
{
    var suffix = Optional(args, "suffix", "s") ?? "r";
    var group = Optional(args, "group") ?? "core";
    var date = ParseDate(Optional(args, "date"));
    var remote = Optional(args, "remote") ?? "git@github.com:neotales/dn-lib.git";
    var reference = Optional(args, "ref") ?? "dev";
    var push = args.Contains("--push");
    var dryRun = args.Contains("--dry-run");
    if (push && dryRun)
    {
        throw new InvalidOperationException("--push and --dry-run cannot be used together.");
    }

    var channel = Channel(suffix);
    var tags = RemoteTags(remote);
    var revision = NextRevision(tags, group, date, channel);
    var tag = FormatTag(group, date, channel, revision);
    var sha = RemoteHead(remote, reference);
    Console.WriteLine($"{tag} {sha}");
    if (!push)
    {
        Console.WriteLine("dry run: tag was not created. Pass --push to create it on the satellite.");
        return 0;
    }

    Git("push", remote, $"{sha}:refs/tags/{tag}");
    Console.WriteLine($"pushed {tag}");
    return 0;
}

static string Channel(string suffix)
{
    var channel = suffix.Trim().ToLowerInvariant();
    if (channel is not ("r" or "alpha" or "beta" or "rc"))
    {
        throw new FormatException($"suffix '{suffix}' is not r, alpha, beta, or rc.");
    }

    return channel;
}

static DateOnly ParseDate(string? text)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return DateOnly.FromDateTime(DateTime.UtcNow);
    }

    var formats = new[] { "yyyy-MM-dd", "yyyy.M.d", "yyyy.MM.dd" };
    if (!DateOnly.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
    {
        throw new FormatException($"date '{text}' is not YYYY-MM-DD.");
    }

    return date;
}

static int NextRevision(IEnumerable<string> tags, string group, DateOnly date, string channel)
{
    var prefix = $"{group}/v{date.Year}.{date.Month}.{date.Day}-{channel}.";
    var highest = 0;
    foreach (var tag in tags)
    {
        if (!tag.StartsWith(prefix, StringComparison.Ordinal))
        {
            continue;
        }

        var tail = tag[prefix.Length..];
        if (int.TryParse(tail, NumberStyles.None, CultureInfo.InvariantCulture, out var revision) && revision > highest)
        {
            highest = revision;
        }
    }

    return highest + 1;
}

static string FormatTag(string group, DateOnly date, string channel, int revision)
{
    if (!Regex.IsMatch(group, @"^[a-z][a-z0-9]*$", RegexOptions.CultureInvariant))
    {
        throw new FormatException($"group '{group}' is not a lowercase area name.");
    }

    if (revision < 1)
    {
        throw new FormatException($"revision {revision} is not a positive integer.");
    }

    return $"{group}/v{date.Year}.{date.Month}.{date.Day}-{channel}.{revision}";
}

static IEnumerable<string> RemoteTags(string remote)
{
    var output = Git("ls-remote", "--tags", remote);
    foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        var parts = line.Split('\t', 2);
        if (parts.Length != 2 || !parts[1].StartsWith("refs/tags/", StringComparison.Ordinal))
        {
            continue;
        }

        var name = parts[1]["refs/tags/".Length..];
        if (name.EndsWith("^{}", StringComparison.Ordinal))
        {
            continue;
        }

        yield return name;
    }
}

static string RemoteHead(string remote, string reference)
{
    var output = Git("ls-remote", remote, $"refs/heads/{reference}");
    var line = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    if (line is null)
    {
        throw new InvalidOperationException($"remote {remote} has no branch {reference}.");
    }

    var sha = line.Split('\t', 2)[0];
    if (sha.Length != 40)
    {
        throw new InvalidOperationException($"remote {remote} returned an unexpected head: {line}");
    }

    return sha;
}

static string Git(params string[] arguments)
{
    var start = new ProcessStartInfo("git")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    start.Environment["GIT_TERMINAL_PROMPT"] = "0";
    foreach (var argument in arguments)
    {
        start.ArgumentList.Add(argument);
    }

    using var process = Process.Start(start) ?? throw new InvalidOperationException("git did not start.");
    var stdout = process.StandardOutput.ReadToEnd();
    var stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        var detail = string.IsNullOrWhiteSpace(stderr) ? stdout.Trim() : stderr.Trim();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(detail) ? "git failed." : detail);
    }

    return stdout;
}

static string? Optional(string[] args, params string[] names)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        foreach (var name in names)
        {
            var flag = name.Length == 1 ? $"-{name}" : $"--{name}";
            if (args[i] == flag)
            {
                return args[i + 1];
            }
        }
    }

    return null;
}

static void Check()
{
    var date = new DateOnly(2026, 11, 3);
    Expect(FormatTag("core", date, "r", 1), "core/v2026.11.3-r.1");
    Expect(FormatTag("core", new DateOnly(2026, 11, 10), "alpha", 2), "core/v2026.11.10-alpha.2");
    Expect(NextRevision([], "core", date, "beta"), 1);
    Expect(
        NextRevision(
            ["core/v2026.11.3-alpha.1", "core/v2026.11.3-alpha.4", "core/v2026.11.3-beta.9", "core/v2026.11.4-alpha.1"],
            "core",
            date,
            "alpha"),
        5);
    Expect(Channel("Alpha"), "alpha");
    ExpectThrow(() => Channel("preview"));
    ExpectThrow(() => FormatTag("Core", date, "r", 1));
}

static void Expect(object actual, object expected)
{
    if (!Equals(actual, expected))
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

    throw new InvalidOperationException("expected a format error.");
}
