using Spark.Bindings;

namespace SparkDemoFoundation;

/// <summary>Tracks spawned roots for teardown (mirrors <c>DemoRootCollection</c> in Spark C++ demos).</summary>
public sealed class DemoRootCollection
{
    private readonly List<GameObject> _roots = [];

    public void Track(GameObject? go)
    {
        if (go is not null)
        {
            _roots.Add(go);
        }
    }

    public void DestroyAll(GameWorld world)
    {
        foreach (GameObject go in _roots)
        {
            world.DestroyGameObject(go);
        }

        _roots.Clear();
    }
}
