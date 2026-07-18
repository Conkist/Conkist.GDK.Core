namespace Conkist.GDK
{
    /// <summary>
    /// Interface for data assets that expose data of a specified type.
    /// </summary>
    /// <typeparam name="TData">The type of the data exposed by the asset.</typeparam>
    public interface IDataAsset<out TData>
    {
        /// <summary>
        /// Gets the data exposed by the asset.
        /// </summary>
        TData Data { get; }
    }
}
