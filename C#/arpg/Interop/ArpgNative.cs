using Spark.Bindings;

namespace SparkArpg.Interop;

internal static unsafe class ArpgNative
{
    public static SparkGameWorld* World(GameWorld world) => (SparkGameWorld*)world.Handle;

    public static SparkGameObject* Object(GameObject obj) => (SparkGameObject*)obj.Handle;

    public static SparkInput* Input(IInput input) => (SparkInput*)input.Handle;

    public static SparkEngineContext* Context(IEngineContext context) => (SparkEngineContext*)context.Handle;

    public static SparkGameComponent* Component(nint handle) => (SparkGameComponent*)handle;
}
