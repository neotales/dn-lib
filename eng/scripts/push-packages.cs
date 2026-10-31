#!/usr/bin/env dotnet
#:property GenerateDocumentationFile=true
#:property TreatWarningsAsErrors=true

// Pushes packed nupkgs. The API key is written to a temporary NuGet config and
// is not placed on the dotnet process command line.
using System.Diagnostics;
using System.Security;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
    {
        Console.WriteLine("""
            usage:
              push-packages github [directory]
              push-packages nuget [directory]
            """);
        return args.Length == 0 ? 1 : 0;
    }

    if (args[0] == "check")
    {
        if (SecurityElement.Escape("a&b") != "a&amp;b")
        {
            Console.Error.WriteLine("push-packages: xml escape failed.");
            return 1;
        }

        Console.WriteLine("push-packages: clean");
        return 0;
    }

    if (args.Length > 2)
    {
        Console.Error.WriteLine("push-packages: too many arguments.");
        return 1;
    }

    var directory = args.Length == 2 ? args[1] : ".artifacts/packages";
    string config;
    try
    {
        var files = PackageFiles(directory);
        config = WriteConfig(args[0]);
        var source = args[0] == "github" ? "github" : "nuget";
        return Push(files, source, config);
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
    {
        Console.Error.WriteLine($"push-packages: {ex.Message}");
        return 1;
    }
}

static string[] PackageFiles(string directory)
{
    if (!Directory.Exists(directory))
    {
        throw new InvalidOperationException($"packages directory '{directory}' does not exist.");
    }

    var files = Directory.EnumerateFiles(directory, "*.nupkg")
        .Where(path => !path.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
        .ToArray();
    if (files.Length == 0)
    {
        throw new InvalidOperationException($"no nupkg in '{directory}'.");
    }

    return files;
}

static string WriteConfig(string target)
{
    var (source, keyName, secret) = target switch
    {
        "github" => (
            $"https://nuget.pkg.github.com/{Required("GITHUB_REPOSITORY_OWNER")}/index.json",
            "github",
            Required("GITHUB_TOKEN")),
        "nuget" => (
            "https://api.nuget.org/v3/index.json",
            "nuget",
            Required("NUGET_API_KEY")),
        _ => throw new ArgumentException("target must be github or nuget."),
    };

    var config = Path.GetTempFileName();
    var xml = $"""
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <packageSources>
            <clear />
            <add key="{keyName}" value="{SecurityElement.Escape(source)}" />
          </packageSources>
          <packageSourceCredentials>
            <{keyName}>
              <add key="Username" value="apikey" />
              <add key="ClearTextPassword" value="{SecurityElement.Escape(secret)}" />
            </{keyName}>
          </packageSourceCredentials>
        </configuration>
        """;
    File.WriteAllText(config, xml);
    return config;
}

static int Push(string[] files, string source, string config)
{
    try
    {
        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
        };
        start.ArgumentList.Add("nuget");
        start.ArgumentList.Add("push");
        foreach (var file in files)
        {
            start.ArgumentList.Add(file);
        }

        start.ArgumentList.Add("--source");
        start.ArgumentList.Add(source);
        start.ArgumentList.Add("--configfile");
        start.ArgumentList.Add(config);
        start.ArgumentList.Add("--skip-duplicate");

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet nuget push did not start.");
        process.WaitForExit();
        return process.ExitCode;
    }
    finally
    {
        File.Delete(config);
    }
}

static string Required(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException(
            name == "NUGET_API_KEY"
                ? "NUGET_API_KEY is not set. Set the secret, or set SKIP_NUGET_ORG=true."
                : $"{name} is required.");
    }

    return value;
}
