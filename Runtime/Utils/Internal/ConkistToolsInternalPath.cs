#if UNITY_EDITOR
using Conkist.GDK.Utils.EditorTools;
using Conkist.GDK.Utils;
using UnityEngine;
using System.IO;

namespace Conkist.GDK.Utils.Internal
{
    /// <summary>
    /// SO is needed to determine the path to this script.
    /// Thereby it's used to get relative path to ConkistTools
    /// </summary>
    public class ConkistToolsInternalPath : ScriptableObject
    {
        /// <summary>
        /// Absolute path to ConkistTools folder
        /// </summary>
        public static DirectoryInfo ConkistToolsDirectory
        {
            get
            {
                if (_directoryChecked) return _conkistToolsDirectory;
                
                var internalPath = MyEditor.GetScriptAssetPath(Instance);
                var scriptDirectory = new DirectoryInfo(internalPath);

                // Script is in ConkistTools/Tools/Internal so we need to get dir two steps up in hierarchy
                if (scriptDirectory.Parent == null || scriptDirectory.Parent.Parent == null)
                {
                    _directoryChecked = true;
                    return null;
                }

                _conkistToolsDirectory = scriptDirectory.Parent.Parent;
                _directoryChecked = true;
                return _conkistToolsDirectory;
            }
        }

        private static DirectoryInfo _conkistToolsDirectory;
        private static bool _directoryChecked;

        private static ConkistToolsInternalPath Instance
        {
            get
            {
                if (_instance != null) return _instance;
                return _instance = CreateInstance<ConkistToolsInternalPath>();
            }
        }

        private static ConkistToolsInternalPath _instance;
    }
}
#endif