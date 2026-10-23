using System.Text;

namespace SparkArpg.Interop;

internal static class Utf8Path
{
    public static unsafe void WithUtf8(string? text, Action<IntPtr> action)
    {
        if (text is null)
        {
            action(IntPtr.Zero);
            return;
        }

        int byteCount = Encoding.UTF8.GetByteCount(text);
        var buffer = new byte[byteCount + 1];
        Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
        fixed (byte* p = buffer)
        {
            action((IntPtr)p);
        }
    }

    public static unsafe T WithUtf8<T>(string? text, Func<IntPtr, T> func)
    {
        if (text is null)
        {
            return func(IntPtr.Zero);
        }

        int byteCount = Encoding.UTF8.GetByteCount(text);
        var buffer = new byte[byteCount + 1];
        Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
        fixed (byte* p = buffer)
        {
            return func((IntPtr)p);
        }
    }
}
