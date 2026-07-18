namespace Conkist.GDK
{
    /// <summary>
    /// A generic pure C# base class for standard classes (non-MonoBehaviour) using lazy initialization.
    /// </summary>
    /// <typeparam name="T">Type of the class inheriting from this PureSingleton class.</typeparam>
    public class PureSingleton<T> where T : class, new()
    {
        private static readonly System.Lazy<T> _lazyInstance = new System.Lazy<T>(() => new T());
        public static T Instance => _lazyInstance.Value;
    }
}
