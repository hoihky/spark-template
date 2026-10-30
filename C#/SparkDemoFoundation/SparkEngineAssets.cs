namespace SparkDemoFoundation;

/// <summary>
/// Resolves bundled Spark engine assets. The script host runs with cwd = <c>SPARK_ENGINE_ROOT</c>,
/// so relative paths like <c>assets/models/Fox.glb</c> work; override with the env var when needed.
/// </summary>
public static class SparkEngineAssets
{
    public static string EngineRoot =>
        Environment.GetEnvironmentVariable("SPARK_ENGINE_ROOT")?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        ?? Directory.GetCurrentDirectory();

    public static string CombineUnderEngine(string relativePath) =>
        Path.GetFullPath(Path.Combine(EngineRoot, relativePath));

    public static string ModelPath(string fileName) => CombineUnderEngine(Path.Combine("assets", "models", fileName));

    public static string TexturePath(string relativeFromAssets) =>
        CombineUnderEngine(Path.Combine("assets", relativeFromAssets));
}
