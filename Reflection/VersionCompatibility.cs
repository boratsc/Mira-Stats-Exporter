using System;
using System.Collections.Generic;

namespace TownOfUsStatsExporter.Reflection;

/// <summary>
/// Manages version compatibility checks for TOU Mira.
/// </summary>
public static class VersionCompatibility
{
    private static readonly HashSet<string> TestedVersions = new()
    {
        "1.2.1",
        "1.2.0",
    };

    private static readonly HashSet<string> IncompatibleVersions = new()
    {
        // Add any known incompatible versions here
    };

    /// <summary>
    /// Checks if a version is compatible.
    /// </summary>
    /// <param name="version">The version to check.</param>
    /// <returns>A string describing the compatibility status.</returns>
    public static string CheckVersion(string? version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return "Unsupported: Version unknown";
        }

        // Parse version
        if (!Version.TryParse(version, out var parsedVersion))
        {
            return $"Unsupported: Cannot parse version '{version}'";
        }

        // Check if explicitly incompatible
        if (IncompatibleVersions.Contains(version))
        {
            return $"Unsupported: Version {version} is known to be incompatible";
        }

        // Check if tested
        if (TestedVersions.Contains(version))
        {
            return $"Supported: Version {version} is tested and compatible";
        }

        // Check if it's a newer minor/patch version
        foreach (var testedVersion in TestedVersions)
        {
            if (Version.TryParse(testedVersion, out var tested))
            {
                // Same major version = probably compatible
                if (parsedVersion.Major == tested.Major)
                {
                    return $"Probably Compatible: Version {version} (tested with {testedVersion})";
                }
            }
        }

        return $"Unsupported: Version {version} has not been tested";
    }

    /// <summary>
    /// Adds a version to the tested versions list.
    /// </summary>
    /// <param name="version">The version to add.</param>
    public static void AddTestedVersion(string version)
    {
        TestedVersions.Add(version);
    }

    /// <summary>
    /// Adds a version to the incompatible versions list.
    /// </summary>
    /// <param name="version">The version to add.</param>
    public static void AddIncompatibleVersion(string version)
    {
        IncompatibleVersions.Add(version);
    }
}
