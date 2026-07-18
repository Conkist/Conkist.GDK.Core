#if UNITY_EDITOR
using UnityEditor;
using System;

namespace Conkist.GDK.Utils.EditorTools
{
	public class IndentBlock : IDisposable
	{
		public IndentBlock()
		{
			EditorGUI.indentLevel++;
		}

		public void Dispose()
		{
			EditorGUI.indentLevel--;
		}
	}
}
#endif