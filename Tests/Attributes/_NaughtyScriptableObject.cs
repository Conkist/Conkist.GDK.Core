using System.Collections.Generic;
using UnityEngine;

namespace Conkist.GDK.Utils.Attributes.Test
{
    //[CreateAssetMenu(fileName = "NaughtyScriptableObject", menuName = "Conkist/Tools/Conkist.GDK.Utils.Attributes/_NaughtyScriptableObject")]
    public class _NaughtyScriptableObject : ScriptableObject
    {
        [Expandable]
        public List<_TestScriptableObjectA> listA;
    }
}
