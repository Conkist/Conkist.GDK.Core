using UnityEngine;
using System.Collections.Generic;

namespace Conkist.GDK.Utils.Attributes.Test
{
    //[CreateAssetMenu(fileName = "TestScriptableObjectB", menuName = "Conkist/Tools/Conkist.GDK.Utils.Attributes/TestScriptableObjectB")]
    public class _TestScriptableObjectB : ScriptableObject
    {
        [MinMaxSlider(0, 10)]
        public Vector2Int slider;
    }
}