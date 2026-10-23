using Spark.Bindings;

namespace SparkArpg.Interop;

internal static class EngineContextExtensions
{
    public static void RequestWindowClose(this IEngineContext context)
    {
        unsafe
        {
            Native.spark_context_request_window_close(ArpgNative.Context(context));
        }
    }
}
