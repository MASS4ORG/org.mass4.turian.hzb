namespace Turian.Tests;

/// <summary>This brick's checkout, which tests install into scratch projects as a <c>file:</c> source.</summary>
public static class TestBrick
{
    /// <summary>The brick id.</summary>
    public const string Id = "org.mass4.turian.hzb";

    /// <summary>The brick's root folder: the nearest ancestor of the test output that holds its manifest.</summary>
    public static string Folder { get; } = Find();

    static string Find()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var manifest = Path.Combine(directory.FullName, PackageManifest.FileName);
            if (File.Exists(manifest) && File.ReadAllText(manifest).Contains($"\"{Id}\"", StringComparison.Ordinal))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException($"The {Id} checkout is not an ancestor of {AppContext.BaseDirectory}.");
    }
}
