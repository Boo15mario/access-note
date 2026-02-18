using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AccessNote;

internal sealed class AppletPluginDiscoveryResult
{
    public AppletPluginDiscoveryResult(
        IReadOnlyList<IAppletRegistration> registrations,
        IReadOnlyList<string> warnings)
    {
        Registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public IReadOnlyList<IAppletRegistration> Registrations { get; }

    public IReadOnlyList<string> Warnings { get; }
}

internal sealed class TrustedAppletRegistrationLoader
{
    internal const string DefaultAllowlistFileName = "allowlist.txt";

    public AppletPluginDiscoveryResult Discover(
        string pluginDirectoryPath,
        string allowlistManifestPath)
    {
        if (string.IsNullOrWhiteSpace(pluginDirectoryPath))
        {
            throw new ArgumentException("Plugin directory path cannot be empty.", nameof(pluginDirectoryPath));
        }

        if (string.IsNullOrWhiteSpace(allowlistManifestPath))
        {
            throw new ArgumentException("Allowlist manifest path cannot be empty.", nameof(allowlistManifestPath));
        }

        var registrations = new List<IAppletRegistration>();
        var warnings = new List<string>();
        var allowedAssemblyFiles = ReadAllowlistedAssemblyFiles(allowlistManifestPath, warnings);
        if (allowedAssemblyFiles.Count == 0)
        {
            return new AppletPluginDiscoveryResult(registrations, warnings);
        }

        foreach (var assemblyFileName in allowedAssemblyFiles)
        {
            var assemblyPath = Path.Combine(pluginDirectoryPath, assemblyFileName);
            if (!File.Exists(assemblyPath))
            {
                warnings.Add($"Allowlisted plugin assembly not found: {assemblyPath}");
                continue;
            }

            TryDiscoverRegistrationsFromAssembly(assemblyPath, registrations, warnings);
        }

        return new AppletPluginDiscoveryResult(registrations, warnings);
    }

    private static IReadOnlyList<string> ReadAllowlistedAssemblyFiles(
        string allowlistManifestPath,
        List<string> warnings)
    {
        if (!File.Exists(allowlistManifestPath))
        {
            return Array.Empty<string>();
        }

        var allowlistedAssemblies = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(allowlistManifestPath);
        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var entry = lines[index].Trim();
            if (entry.Length == 0 || entry.StartsWith('#'))
            {
                continue;
            }

            if (!IsValidAssemblyEntry(entry))
            {
                warnings.Add($"Invalid allowlist entry on line {lineNumber}: '{entry}'. Expected a dll filename only.");
                continue;
            }

            if (!seen.Add(entry))
            {
                continue;
            }

            allowlistedAssemblies.Add(entry);
        }

        return allowlistedAssemblies;
    }

    private static bool IsValidAssemblyEntry(string entry)
    {
        if (!entry.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(entry, Path.GetFileName(entry), StringComparison.Ordinal))
        {
            return false;
        }

        return !Path.IsPathRooted(entry);
    }

    private static void TryDiscoverRegistrationsFromAssembly(
        string assemblyPath,
        List<IAppletRegistration> registrations,
        List<string> warnings)
    {
        Assembly assembly;
        try
        {
            assembly = Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception ex)
        {
            warnings.Add($"Failed to load plugin assembly '{assemblyPath}': {ex.Message}");
            return;
        }

        Type[] exportedTypes;
        try
        {
            exportedTypes = assembly.GetExportedTypes();
        }
        catch (Exception ex)
        {
            warnings.Add($"Failed to inspect plugin assembly '{assemblyPath}': {ex.Message}");
            return;
        }

        var registrationTypes = exportedTypes
            .Where(type => typeof(IAppletRegistration).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        if (registrationTypes.Length == 0)
        {
            warnings.Add($"No exported IAppletRegistration types found in '{assemblyPath}'.");
            return;
        }

        foreach (var registrationType in registrationTypes)
        {
            var constructor = registrationType.GetConstructor(Type.EmptyTypes);
            if (constructor is null)
            {
                warnings.Add($"Skipped registration type '{registrationType.FullName}' in '{assemblyPath}': missing public parameterless constructor.");
                continue;
            }

            try
            {
                if (Activator.CreateInstance(registrationType) is IAppletRegistration registration)
                {
                    registrations.Add(registration);
                    continue;
                }

                warnings.Add($"Skipped registration type '{registrationType.FullName}' in '{assemblyPath}': instance is not an IAppletRegistration.");
            }
            catch (Exception ex)
            {
                warnings.Add($"Failed to instantiate registration type '{registrationType.FullName}' in '{assemblyPath}': {ex.Message}");
            }
        }
    }
}
