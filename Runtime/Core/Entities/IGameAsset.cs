namespace Conkist.GDK
{
    /// <summary>
    /// Interface for game assets with a generic identifier.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier for the game asset.</typeparam>
    public interface IGameAsset
    {
        /// <summary>
        /// Gets the identifier of the game asset.
        /// </summary>
        public string Guid { get; }
        //TODO guid? name? name -> guid -> asset?
        //TODO multiple keys?
    }

    /// <summary>
    /// Interface for game assets with a generic identifier.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier for the game asset.</typeparam>
    public interface IGameAsset<TId>
    {
        /// <summary>
        /// Gets the identifier of the game asset.
        /// </summary>
        string Id { get; }
        //TODO guid? name? name -> guid -> asset?
        //TODO multiple keys?
    }
}
