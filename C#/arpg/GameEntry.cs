using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spark.Scripting;

namespace SparkArpg;

public static class GameEntry
{
    [ModuleInitializer]
    internal static void RegisterGame() => GameBootstrap.Factory = static () => new ArpgGame();

    [UnmanagedCallersOnly]
    public static int Initialize(IntPtr hostApiPtr) => ScriptHostEntry.InitializeCore(hostApiPtr);
}
