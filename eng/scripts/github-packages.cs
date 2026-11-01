#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=true
#:property TreatWarningsAsErrors=true
#:property NoWarn=SA1600;SA1611;SA1615;SA1642

// Lists, deletes, and prunes NuGet versions on GitHub Packages.
// Build versions match 0.0.0-build-yyyyMMdd.rN. prune-builds keeps the newest
// of those and leaves release, alpha, beta, and rc versions in place.
// The token is GITHUB_TOKEN or GH_TOKEN. It is never printed.
using System.Globalization;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    if (args.Length == 0)
    {
        Usage();
        return 1;
    }

    if (args[0] is "-h" or "--help" or "help")
    {
        Usage();
        return 0;
    }

    try
    {
        switch (args[0])
        {
            case "check":
                Check();
                Console.WriteLine("github-packages: clean");
                return 0;
            case "list":
                return await ListAsync(args);
            case "delete":
                return await DeleteAsync(args);
            case "prune-builds":
                return await PruneAsync(args);
            default:
                throw new InvalidOperationException($"unknown command {args[0]}.");
        }
    }
    catch (Exception ex) when (ex is InvalidOperationException or FormatException or HttpRequestException)
    {
        Console.Error.WriteLine($"github-packages: {ex.Message}");
        return 1;
    }
}

static async Task<int> ListAsync(string[] args)
{
    var package = Require(args, "package");
    using var client = CreateClient(args);
    foreach (var version in await client.ListAsync(package))
    {
        Console.WriteLine(
            string.Create(
                CultureInfo.InvariantCulture,
                $"{version.Id}\t{version.Name}\t{version.CreatedAt:O}"));
    }

    return 0;
}

static async Task<int> DeleteAsync(string[] args)
{
    var package = Require(args, "package");
    var version = Require(args, "version");
    var dryRun = Has(args, "dry-run");
    using var client = CreateClient(args);
    var match = (await client.ListAsync(package))
        .FirstOrDefault(candidate => string.Equals(candidate.Name, version, StringComparison.OrdinalIgnoreCase));
    if (match is null)
    {
        throw new InvalidOperationException($"{package} {version} was not found.");
    }

    if (dryRun)
    {
        Console.WriteLine($"dry-run delete {package} {match.Name}");
        return 0;
    }

    await client.DeleteAsync(package, match);
    Console.WriteLine($"deleted {package} {match.Name}");
    return 0;
}

static async Task<int> PruneAsync(string[] args)
{
    var keepText = Optional(args, "keep") ?? "5";
    if (!int.TryParse(keepText, NumberStyles.None, CultureInfo.InvariantCulture, out var keep) || keep < 0)
    {
        throw new FormatException($"--keep '{keepText}' is not zero or a positive integer.");
    }

    var packages = new List<string>();
    var package = Optional(args, "package");
    if (package is not null)
    {
        packages.Add(package);
    }

    var directory = Optional(args, "packages-dir");
    if (directory is not null)
    {
        packages.AddRange(PackageIds(directory));
    }

    if (packages.Count == 0)
    {
        throw new InvalidOperationException("prune-builds needs --package or --packages-dir.");
    }

    var dryRun = Has(args, "dry-run");
    using var client = CreateClient(args);
    foreach (var id in packages.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        var versions = await client.ListAsync(id);
        foreach (var version in BuildVersionsToDelete(versions, keep))
        {
            if (dryRun)
            {
                Console.WriteLine($"dry-run delete {id} {version.Name}");
                continue;
            }

            await client.DeleteAsync(id, version);
            Console.WriteLine($"deleted {id} {version.Name}");
        }
    }

    return 0;
}

static IEnumerable<string> PackageIds(string directory)
{
    if (!Directory.Exists(directory))
    {
        throw new InvalidOperationException($"packages directory '{directory}' does not exist.");
    }

    var ids = new List<string>();
    foreach (var path in Directory.EnumerateFiles(directory, "*.nupkg"))
    {
        if (path.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        ids.Add(ReadPackageId(path));
    }

    if (ids.Count == 0)
    {
        throw new InvalidOperationException($"no nupkg files in '{directory}'.");
    }

    return ids;
}

static string ReadPackageId(string path)
{
    using var archive = ZipFile.OpenRead(path);
    var entry = archive.Entries.FirstOrDefault(candidate =>
        candidate.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase)
        && !candidate.FullName.Contains('/', StringComparison.Ordinal)
        && !candidate.FullName.Contains('\\', StringComparison.Ordinal));
    if (entry is null)
    {
        throw new InvalidOperationException($"{path} has no nuspec.");
    }

    using var stream = entry.Open();
    var document = XDocument.Load(stream);
    var id = document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "metadata")
        ?.Elements().FirstOrDefault(element => element.Name.LocalName == "id")
        ?.Value;
    if (string.IsNullOrWhiteSpace(id))
    {
        throw new InvalidOperationException($"{path} nuspec has no package id.");
    }

    return id.Trim();
}

static bool IsBuildVersion(string name)
{
    return Regex.IsMatch(name, @"^0\.0\.0-build-\d{8}\.r\d+$", RegexOptions.CultureInvariant);
}

static IReadOnlyList<PackageVersion> BuildVersionsToDelete(IReadOnlyList<PackageVersion> versions, int keep)
{
    return versions
        .Where(version => IsBuildVersion(version.Name))
        .OrderByDescending(version => version.CreatedAt)
        .ThenByDescending(version => version.Id)
        .Skip(keep)
        .ToArray();
}

static void Check()
{
    var versions = new PackageVersion[]
    {
        new(1, "0.0.0", DateTimeOffset.Parse("2026-10-01T00:00:00Z", CultureInfo.InvariantCulture)),
        new(2, "0.5.0-beta.3", DateTimeOffset.Parse("2026-10-02T00:00:00Z", CultureInfo.InvariantCulture)),
        new(3, "0.0.0-build-20261030.r1", DateTimeOffset.Parse("2026-10-30T00:00:00Z", CultureInfo.InvariantCulture)),
        new(4, "0.0.0-build-20261031.r1", DateTimeOffset.Parse("2026-10-31T00:00:00Z", CultureInfo.InvariantCulture)),
        new(5, "0.0.0-build-20261031.r2", DateTimeOffset.Parse("2026-10-31T01:00:00Z", CultureInfo.InvariantCulture)),
        new(6, "0.0.0-alpha.0", DateTimeOffset.Parse("2026-10-31T02:00:00Z", CultureInfo.InvariantCulture)),
        new(7, "0.0.0-build-20261031.r3", DateTimeOffset.Parse("2026-10-31T03:00:00Z", CultureInfo.InvariantCulture)),
        new(8, "0.0.0-build-20261031.r4", DateTimeOffset.Parse("2026-10-31T04:00:00Z", CultureInfo.InvariantCulture)),
        new(9, "0.0.0-build-20261031.r5", DateTimeOffset.Parse("2026-10-31T05:00:00Z", CultureInfo.InvariantCulture)),
        new(10, "0.0.0-build-20261031.r6", DateTimeOffset.Parse("2026-10-31T06:00:00Z", CultureInfo.InvariantCulture)),
    };

    var deleted = BuildVersionsToDelete(versions, 5).Select(version => version.Name).ToArray();
    if (deleted.Length != 2
        || deleted[0] != "0.0.0-build-20261031.r1"
        || deleted[1] != "0.0.0-build-20261030.r1")
    {
        throw new InvalidOperationException("prune kept the wrong build versions.");
    }

    if (BuildVersionsToDelete(versions, 0).Count != 7)
    {
        throw new InvalidOperationException("keep 0 should delete every build version and nothing else.");
    }
}

static GitHubPackages CreateClient(string[] args)
{
    var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrWhiteSpace(token))
    {
        token = Environment.GetEnvironmentVariable("GH_TOKEN");
    }

    if (string.IsNullOrWhiteSpace(token))
    {
        throw new InvalidOperationException("GITHUB_TOKEN or GH_TOKEN is required.");
    }

    var owner = Optional(args, "owner") ?? Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");
    if (string.IsNullOrWhiteSpace(owner))
    {
        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        var slash = repository?.IndexOf('/', StringComparison.Ordinal) ?? -1;
        owner = slash > 0 ? repository![..slash] : null;
    }

    if (string.IsNullOrWhiteSpace(owner))
    {
        throw new InvalidOperationException("owner is required, from --owner or GITHUB_REPOSITORY_OWNER.");
    }

    return new GitHubPackages(token, owner);
}

static string Require(string[] args, string name)
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

static bool Has(string[] args, string name)
{
    return args.Contains("--" + name, StringComparer.Ordinal);
}

static void Usage()
{
    Console.WriteLine("""
        usage:
          github-packages list --package <id> [--owner <owner>]
          github-packages delete --package <id> --version <version> [--dry-run]
          github-packages prune-builds [--keep 5] [--package <id>] [--packages-dir <dir>] [--dry-run]
          github-packages check
        """);
}

internal sealed record PackageVersion(long Id, string Name, DateTimeOffset CreatedAt);

internal sealed class GitHubPackages : IDisposable
{
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private readonly string token;
    private readonly string owner;
    private string? ownerKind;

    private static string PackageName(string package)
    {
        return Uri.EscapeDataString(package.ToLowerInvariant());
    }

    private static PackageVersion ParseVersion(JsonElement element)
    {
        var id = element.GetProperty("id").GetInt64();
        var name = element.GetProperty("name").GetString()
            ?? throw new FormatException("a package version has no name.");
        var created = element.GetProperty("created_at").GetDateTimeOffset();
        return new PackageVersion(id, name, created);
    }

    private static string? NextLink(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Link", out var values))
        {
            return null;
        }

        foreach (var value in values)
        {
            foreach (var part in value.Split(','))
            {
                var pieces = part.Split(';');
                if (pieces.Length < 2 || !pieces[1].Contains("rel=\"next\"", StringComparison.Ordinal))
                {
                    continue;
                }

                var link = pieces[0].Trim();
                if (link.StartsWith('<') && link.EndsWith('>'))
                {
                    return link[1..^1];
                }
            }
        }

        return null;
    }

    private static string ApiError(string action, System.Net.HttpStatusCode status, string body)
    {
        var text = body.Length > 400 ? body[..400] : body;
        return $"{action} failed with {(int)status}. {text}";
    }

    internal GitHubPackages(string token, string owner)
    {
        this.token = token;
        this.owner = owner;
    }

    public void Dispose()
    {
        this.http.Dispose();
    }

    internal async Task<IReadOnlyList<PackageVersion>> ListAsync(string package)
    {
        var documents = new List<JsonElement>();
        var next = await this.FirstPageAsync(package);
        while (next is not null)
        {
            using var response = await this.SendAsync(HttpMethod.Get, next);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(ApiError("list", response.StatusCode, body));
            }

            using var document = JsonDocument.Parse(body);
            documents.AddRange(document.RootElement.EnumerateArray().Select(static item => item.Clone()));
            next = NextLink(response);
        }

        return documents.Select(ParseVersion).ToArray();
    }

    internal async Task DeleteAsync(string package, PackageVersion version)
    {
        if (this.ownerKind is null)
        {
            _ = await this.ListAsync(package);
        }

        var url = $"https://api.github.com/{this.ownerKind}/{Uri.EscapeDataString(this.owner)}/packages/nuget/{PackageName(package)}/versions/{version.Id.ToString(CultureInfo.InvariantCulture)}";
        using var response = await this.SendAsync(HttpMethod.Delete, url);
        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return;
        }

        throw new InvalidOperationException(ApiError("delete", response.StatusCode, body));
    }

    private async Task<string?> FirstPageAsync(string package)
    {
        var kinds = this.ownerKind is null ? new[] { "orgs", "users" } : new[] { this.ownerKind };
        string? userFailure = null;
        foreach (var kind in kinds)
        {
            var url = $"https://api.github.com/{kind}/{Uri.EscapeDataString(this.owner)}/packages/nuget/{PackageName(package)}/versions?per_page=100";
            using var response = await this.SendAsync(HttpMethod.Get, url);
            var body = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && this.ownerKind is null && kind == "orgs")
            {
                userFailure = ApiError("list", response.StatusCode, body);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = ApiError("list", response.StatusCode, body);
                if (userFailure is not null)
                {
                    message = userFailure + " " + message;
                }

                throw new InvalidOperationException(message);
            }

            this.ownerKind = kind;
            return url;
        }

        throw new InvalidOperationException(userFailure ?? "package was not found.");
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        request.Headers.UserAgent.ParseAdd("neotales-dn-lib");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        return await this.http.SendAsync(request);
    }
}
