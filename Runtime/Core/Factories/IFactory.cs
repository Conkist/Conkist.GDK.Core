using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Factories
{
    /// <summary>
    /// A generic factory interface for instantiating objects of type T without parameters.
    /// </summary>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public interface IFactory<out T>
    {
        /// <summary>
        /// Creates a new instance of type T.
        /// </summary>
        /// <returns>A new instance of T.</returns>
        T Create();
    }

    /// <summary>
    /// A generic factory interface for instantiating objects of type T with a parameter of type TParam.
    /// </summary>
    /// <typeparam name="TParam">The type of parameter needed for instantiation.</typeparam>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public interface IFactory<in TParam, out T>
    {
        /// <summary>
        /// Creates a new instance of type T using the provided parameter.
        /// </summary>
        /// <param name="param">The parameter needed to construct the object.</param>
        /// <returns>A new instance of T.</returns>
        T Create(TParam param);
    }

    /// <summary>
    /// A generic asynchronous factory interface for instantiating objects of type T.
    /// Highly useful for loading assets asynchronously (e.g. via Addressables) before instantiating them.
    /// </summary>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public interface IAsyncFactory<T>
    {
        /// <summary>
        /// Asynchronously creates a new instance of type T.
        /// </summary>
        /// <returns>A UniTask returning the new instance of T.</returns>
        UniTask<T> CreateAsync();
    }

    /// <summary>
    /// A generic asynchronous factory interface for instantiating objects of type T with a parameter.
    /// </summary>
    /// <typeparam name="TParam">The type of parameter needed for instantiation.</typeparam>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public interface IAsyncFactory<in TParam, T>
    {
        /// <summary>
        /// Asynchronously creates a new instance of type T using the provided parameter.
        /// </summary>
        /// <param name="param">The parameter needed to construct the object.</param>
        /// <returns>A UniTask returning the new instance of T.</returns>
        UniTask<T> CreateAsync(TParam param);
    }
}
