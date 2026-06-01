namespace Conkist.GDK.Demo
{
    /// <summary>
    /// Pure C# (non-MonoBehaviour) demo class using PureSingletonBehaviour with lazy initialization.
    /// </summary>
    public class PureSingletonDemo : PureSingleton<PureSingletonDemo>
    {
        public int CallCount { get; private set; } = 0;

        public void Increment()
        {
            CallCount++;
        }
    }
}
