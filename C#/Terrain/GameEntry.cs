using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spark.Scripting;

namespace SparkDemoTerrain;

public static class GameEntry
{
    [ModuleInitializer]
    internal static void RegisterGame() => GameBootstrap.Factory = static () => new TerrainGame();

    [UnmanagedCallersOnly]
    public static int Initialize(IntPtr hostApiPtr) => ScriptHostEntry.InitializeCore(hostApiPtr);
}
