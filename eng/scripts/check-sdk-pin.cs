#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=true
#:property TreatWarningsAsErrors=true

// NTL540: the SDK this repository pins must be one a clean machine can download.
//
// Why this gate exists
//
// `global.json` pinned the .NET SDK as `11.0.100-rc.1`. That version does not
// exist. The download hosts publish release candidate archives under their
// *full* four-part version — `11.0.100-rc.1.26425.128` — and there is no
// archive called `dotnet-sdk-11.0.100-rc.1-linux-x64.tar.gz`. Both hosts answer
// 404 for the three-part form.
//
// Nothing noticed for as long as it went unnoticed because the failure needs a
// machine that does not already have the SDK. `rollForward: latestFeature`
// means a developer's SDK satisfies a three-part pin whatever its patch, so the
// pin looked correct on every machine that had the SDK and was wrong on every
// machine that had to install one. A clean CI runner is the only place the two
// differ, so the first runner to execute a workflow found this, and nothing
// before it could.
//
// The lesson is the shape of it: a pin whose validation path differs from its
// use path is a pin that is only validated where it is least needed.
//
// Why this asks the server rather than checking the string
//
// It is tempting to reject a version that "looks short". That cannot be done
// reliably: `11.0.100-rc.1` splits into four dot-separated parts, so a part
// count gets a genuinely broken pin wrong in the other direction. The only
// authority on whether a version can be installed is whether the archive is
// there, so this asks the hosts, and mentions the likely cause in the message
// instead of branching on it. When a check can ask, it should not guess.
using System.Text.Json;

var repositoryRoot = FindRepositoryRoot();
var globalJson = Path.Combine(repositoryRoot, "global.json");
var relative = Path.GetRelativePath(repositoryRoot, globalJson);

if (!File.Exists(globalJson))
{
    Console.Error.WriteLine($"error: NTL540 {relative}: no such file, so there is no SDK pin to check.");
    return 1;
}

string pinned;
try
{
    using var document = JsonDocument.Parse(File.ReadAllText(globalJson));
    if (!document.RootElement.TryGetProperty("sdk", out var sdk)
        || !sdk.TryGetProperty("version", out var versionElement)
        || versionElement.GetString() is not { } version)
    {
        Console.Error.WriteLine(
            $"error: NTL540 {relative}: no sdk.version to check. A repository with no SDK pin takes whatever the machine has, and this gate is the only thing that would notice.");
        return 1;
    }

    pinned = version;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"error: NTL540 {relative}: {ex.Message}");
    return 1;
}

var architectures = new[] { "linux-x64", "linux-arm64", "osx-arm64", "osx-x64", "win-x64" };
var hosts = new[]
{
    "https://builds.dotnet.microsoft.com/dotnet",
    "https://ci.dot.net/public",
};

using var handler = new HttpClientHandler { AllowAutoRedirect = true };
using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("Neotales.DotNet.SdkPinCheck/1.0");

var unavailable = new List<string>();
var unreachable = new List<string>();
var checkedCount = 0;

foreach (var host in hosts)
{
    foreach (var architecture in architectures)
    {
        var url = $"{host}/Sdk/{pinned}/dotnet-sdk-{pinned}-{architecture}.tar.gz";
        checkedCount++;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                unavailable.Add($"{url} -> {(int)response.StatusCode}");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            unreachable.Add($"{url} -> {ex.GetType().Name}");
        }
    }
}

// A gate that cannot reach the network says so. Reporting a pass because nothing
// was checked is how an unparseable workflow was once reported as green here.
if (unreachable.Count > 0)
{
    Console.Error.WriteLine(
        $"error: NTL540 {relative}: the pin '{pinned}' could not be checked — {unreachable.Count} of {checkedCount} request(s) failed. " +
        "That is a network failure, not a verdict on the pin, and it is reported as an error rather than as a pass.");
    foreach (var line in unreachable.Take(3))
    {
        Console.Error.WriteLine($"        {line}");
    }

    return 1;
}

if (unavailable.Count > 0)
{
    Console.Error.WriteLine(
        $"error: NTL540 {relative}: the pin '{pinned}' is not on the download hosts, so a machine without the SDK already installed cannot install it. " +
        $"Every workflow in this repository fails at its first step.");
    Console.Error.WriteLine($"        checked {checkedCount} url(s): {hosts.Length} host(s) x {architectures.Length} architecture(s), all {unavailable.Count} unavailable.");
    foreach (var line in unavailable.Take(3))
    {
        Console.Error.WriteLine($"        {line}");
    }

    // The likely cause, offered as a hint. Deliberately not a branch: a version
    // that "looks short" cannot be recognised reliably from its shape.
    var channel = pinned.Split('-')[0];
    var majorMinor = string.Join('.', channel.Split('.').Take(2));
    try
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://builds.dotnet.microsoft.com/dotnet/Sdk/{majorMinor}/latest.version");
        using var response = await http.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var latest = (await response.Content.ReadAsStringAsync()).Trim();
            if (latest.Length > 0 && !string.Equals(latest, pinned, StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"        hint: the hosts publish {latest} for the {majorMinor} channel.");
                Console.Error.WriteLine(
                    $"        fix: a release candidate is published under its full four-part version, and a shorter pin resolves on a machine that already has the SDK while 404ing on one installing it.");
            }
        }
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
        // The hint is a courtesy. Its absence must not change the verdict, which
        // is already decided.
    }

    return 1;
}

Console.WriteLine($"check-sdk-pin: {pinned} is on {hosts.Length} host(s) x {architectures.Length} architecture(s) ({checkedCount} url(s))");
Console.WriteLine("check-sdk-pin: clean");
return 0;

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "global.json"))
            && Directory.Exists(Path.Combine(directory.FullName, "eng")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return Directory.GetCurrentDirectory();
}
