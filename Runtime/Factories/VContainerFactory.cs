using VContainer;

namespace Conkist.GDK.Factories
{
    /// <summary>
    /// A generic C# factory that uses VContainer's IObjectResolver to resolve or instantiate
    /// pure C# instances with automatic constructor dependency injection.
    /// </summary>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public class VContainerFactory<T> : IFactory<T> where T : class
    {
        private readonly IObjectResolver _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="VContainerFactory{T}"/> class.
        /// </summary>
        /// <param name="container">The VContainer object resolver injected automatically.</param>
        public VContainerFactory(IObjectResolver container)
        {
            _container = container;
        }

        /// <summary>
        /// Resolves or constructs a new instance of type T, automatically injecting dependencies.
        /// </summary>
        /// <returns>An injected instance of T.</returns>
        public T Create()
        {
            // Resolve using VContainer resolver. If the class is registered as transient, 
            // a new instance is created. If registered as singleton, the shared instance is returned.
            return _container.Resolve<T>();
        }
    }
}
