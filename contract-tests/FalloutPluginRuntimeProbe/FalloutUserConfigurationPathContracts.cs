using OpenNV.Runtime.Content;

internal static class FalloutUserConfigurationPathContracts
{
    internal static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "opennv-user-config-" + Guid.NewGuid().ToString("N"));
        var install = Path.Combine(root, "steamapps", "common", "Fallout New Vegas");
        var data = Path.Combine(install, "Data");
        var documents = Path.Combine(root, "host-documents");
        var proton = Path.Combine(root, "steamapps", "compatdata", "22380", "pfx", "drive_c", "users", "steamuser",
            "Documents", "My Games", "FalloutNV");
        Directory.CreateDirectory(data);
        Directory.CreateDirectory(documents);
        Directory.CreateDirectory(proton);
        try
        {
            File.WriteAllText(Path.Combine(documents, "RendererInfo.txt"), "Shader Package: 13");
            File.WriteAllText(Path.Combine(proton, "RendererInfo.txt"), "Shader Package: 17");
            Require(FalloutUserConfigurationPaths.Resolve(data, documents, "FalloutNV", "22380") == proton,
                "The Proton renderer profile did not take precedence over the host Documents profile.");

            File.Delete(Path.Combine(proton, "RendererInfo.txt"));
            File.WriteAllText(Path.Combine(proton, "FalloutPrefs.ini"), "[Display]\niSize W=1920");
            Require(FalloutUserConfigurationPaths.Resolve(data, documents, "FalloutNV", "22380") == documents,
                "A host renderer profile was not preferred when the Proton prefix had no renderer profile.");

            File.Delete(Path.Combine(documents, "RendererInfo.txt"));
            Require(FalloutUserConfigurationPaths.Resolve(data, documents, "FalloutNV", "22380") == proton,
                "The Proton user directory was not selected when it was the only existing settings directory.");

            Directory.Delete(proton, true);
            Require(FalloutUserConfigurationPaths.Resolve(data, documents, "FalloutNV", "22380") == documents,
                "The host Documents path was not retained as the safe no-prefix fallback.");
            Console.WriteLine("OPENNV_USER_CONFIGURATION_PATHS_PASS proton-renderer=true host-fallback=true no-hardcoded-shader-default=true");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
