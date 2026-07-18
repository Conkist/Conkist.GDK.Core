using UnityEngine;
using System.Collections.Generic;

namespace Conkist.GDK.Utils.Attributes.Test
{
    //[CreateAssetMenu(fileName = "TestScriptableObjectA", menuName = "Conkist/Tools/Conkist.GDK.Utils.Attributes/TestScriptableObjectA")]
    public class _TestScriptableObjectA : ScriptableObject
    {
        [Expandable]
        public List<_TestScriptableObjectB> listB;
    }
}