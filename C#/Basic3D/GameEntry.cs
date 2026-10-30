using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spark.Scripting;

namespace SparkDemoBasic3D;

public static class GameEntry
{
    [ModuleInitializer]
    internal static void RegisterGame() => GameBootstrap.Factory = static () => new Basic3DGame();

    [UnmanagedCallersOnly]
    public static int Initialize(IntPtr hostApiPtr) => ScriptHostEntry.InitializeCore(hostApiPtr);
}
