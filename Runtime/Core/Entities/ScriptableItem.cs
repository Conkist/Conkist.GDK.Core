using UnityEngine;

namespace Conkist.GDK
{
    /// <summary>
    /// An abstract base class for scriptable objects that represent items in a player's inventory.
    /// Items can be stored, traded, purchased, rewarded, or involved in transactions.
    /// </summary>
    public abstract class ScriptableItem : ScriptableObject
    {
        /// <summary>
        /// Gets the unique identifier for the item, which is the name of the ScriptableObject.
        /// </summary>
        public string Id => name;

        [SerializeField]
        [Tooltip("Localization key for the item's display name.")]
        protected string nameKey;

        [SerializeField]
        [Tooltip("Path to the image asset representing the item.")]
        protected Sprite image;

        [SerializeField]
        [TextArea(5, 8)]
        [Tooltip("Localization key for the item's description.")]
        protected string descriptionKey;

        /// <summary>
        /// Gets the localization key for the item's display name.
        /// </summary>
        public string NameKey => nameKey;

        /// <summary>
        /// Gets the path to the image asset representing the item.
        /// </summary>
        public Sprite ImageID => image;

        /// <summary>
        /// Gets the localization key for the item's description.
        /// </summary>
        public string DescriptionKey => descriptionKey;
    }
}