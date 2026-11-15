namespace OpenNV.Runtime.Content;

/// <summary>
/// Resolves the user-owned Fallout configuration directory without assuming
/// that the host OS Documents folder is the same directory used by Proton.
/// </summary>
internal static class FalloutUserConfigurationPaths
{
    internal static string Resolve(string contentRoot, string documents, string gameFolder, string appId)
    {
        if (string.IsNullOrWhiteSpace(contentRoot) || string.IsNullOrWhiteSpace(gameFolder) ||
            string.IsNullOrWhiteSpace(appId) || appId.Any(character => !char.IsAsciiDigit(character)))
            throw new ArgumentException("A game content root, game folder and numeric Steam app ID are required.");

        var candidates = Candidates(contentRoot, documents, gameFolder, appId);
        var existing = candidates.Where(Directory.Exists)
            .Distinct(PathComparer)
            .ToArray();

        // RendererInfo is generated for the selected GPU. Prefer the prefix
        // that actually owns it over a host Documents folder with stale INIs.
        var rendererOwner = existing.FirstOrDefault(path => File.Exists(Path.Combine(path, "RendererInfo.txt")));
        if (rendererOwner is not null) return rendererOwner;

        var iniOwner = existing.FirstOrDefault(path =>
            File.Exists(Path.Combine(path, "Fallout.ini")) ||
            File.Exists(Path.Combine(path, "FalloutPrefs.ini")));
        return iniOwner ?? candidates[0];
    }

    internal static IReadOnlyList<string> Candidates(string contentRoot, string documents, string gameFolder, string appId)
    {
        var result = new List<string>();
        if (!string.IsNullOrWhiteSpace(documents))
            result.Add(Path.GetFullPath(Path.Combine(documents, "My Games", gameFolder)));

        var fullContentRoot = Path.GetFullPath(contentRoot);
        var installRoot = Directory.GetParent(fullContentRoot)?.FullName;
        var commonDirectory = installRoot is null ? null : Directory.GetParent(installRoot)?.FullName;
        var steamApps = commonDirectory is null ? null : Directory.GetParent(commonDirectory)?.FullName;
        if (steamApps is not null)
        {
            var usersRoot = Path.Combine(steamApps, "compatdata", appId, "pfx", "drive_c", "users");
            if (Directory.Exists(usersRoot))
            {
                foreach (var user in Directory.EnumerateDirectories(usersRoot).Order(StringComparer.Ordinal))
                    result.Add(Path.Combine(user, "Documents", "My Games", gameFolder));
            }
            result.Add(Path.Combine(usersRoot, "steamuser", "Documents", "My Games", gameFolder));
        }

        if (result.Count == 0)
            result.Add(Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games", gameFolder)));
        return result.Distinct(PathComparer).ToArray();
    }

    private static StringComparer PathComparer =>
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
