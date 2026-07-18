using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Conkist.GDK.Tools
{
	[InitializeOnLoad]
	public class IPrepareFeature
	{
		public static bool IsEnabled = true;

		public static System.Action OnPrepareBefore;
		public static System.Action OnPrepare;
		public static System.Action OnPrepareAfter;

		static IPrepareFeature()
		{
			// Note: MyEditorEvents is now in Conkist.GDK.Tools
			MyEditorEvents.BeforePlaymode += PrepareOnPlay;
		}

		private static void PrepareOnPlay()
		{
			OnPrepareBefore?.Invoke();
			OnPrepare?.Invoke();
			OnPrepareAfter?.Invoke();
			
			if (IsEnabled) RunIPrepare();
		}
		
		/// <summary>
		/// Calls Prepare() on any MonoBehaviour with IPrepare interface. If Prepare() returns true, parent scene will be marked dirty
		/// </summary>
		public static void RunIPrepare()
		{
			var toPrepare = Conkist.GDK.Utils.MyExtensions.FindObjectsOfInterfaceAsComponents<Conkist.GDK.Utils.IPrepare>();

			HashSet<Scene> modifiedScenes = null;
			foreach (var prepare in toPrepare)
			{
				bool changed = prepare.Interface.Prepare();

				if (changed && prepare.Component != null)
				{
					if (modifiedScenes == null) modifiedScenes = new HashSet<Scene>();
					modifiedScenes.Add(prepare.Component.gameObject.scene);

					EditorUtility.SetDirty(prepare.Component);
					Debug.Log(prepare.Component.name + "." + prepare.Component.GetType().Name + ": Changed on Prepare", prepare.Component);
				}
			}

			if (modifiedScenes != null) EditorSceneManager.SaveScenes(modifiedScenes.ToArray());
		}
	}
}
