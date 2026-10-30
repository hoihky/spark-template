using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spark.Scripting;

namespace SparkDemoSky;

public static class GameEntry
{
    [ModuleInitializer]
    internal static void RegisterGame() => GameBootstrap.Factory = static () => new SkyGame();

    [UnmanagedCallersOnly]
    public static int Initialize(IntPtr hostApiPtr) => ScriptHostEntry.InitializeCore(hostApiPtr);
}
