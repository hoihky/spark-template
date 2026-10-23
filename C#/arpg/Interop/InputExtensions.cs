using Spark.Bindings;

namespace SparkArpg.Interop;

internal static class InputExtensions
{
    public static bool IsCursorCaptured(this IInput input)
    {
        unsafe
        {
            return Native.spark_input_is_cursor_captured(ArpgNative.Input(input)) != 0;
        }
    }
}
