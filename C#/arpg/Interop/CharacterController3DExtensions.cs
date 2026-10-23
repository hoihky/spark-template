using Spark.Bindings;
using Spark.Bindings.Components;

namespace SparkArpg.Interop;

internal static class CharacterController3DExtensions
{
    public static void SetMoveInput(this CharacterController3DComponent controller, in Vector3 worldVelocity)
    {
        unsafe
        {
            var v = worldVelocity.ToNative();
            Native.spark_character_controller3_d_set_move_input(ArpgNative.Component(controller.Handle), &v);
        }
    }
}
