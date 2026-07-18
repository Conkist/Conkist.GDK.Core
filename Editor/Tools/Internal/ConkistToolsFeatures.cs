#if UNITY_EDITOR
using UnityEditor;

namespace Conkist.GDK.Tools
{
	[InitializeOnLoad]
	public class ConkistToolsFeatures
	{
		private const string IPrepareMenuItemKey = "Conkist/Tools/Run Prepare on play";

		static ConkistToolsFeatures()
		{
			IPrepareIsEnabled = IPrepareIsEnabled;
		}


		#region IPrepare

		private static bool IPrepareIsEnabled
		{
			get => Conkist.GDK.Utils.Internal.ConkistToolsSettings.PrepareOnPlaymode;
			set
			{
				{
					Conkist.GDK.Utils.Internal.ConkistToolsSettings.PrepareOnPlaymode = value;
					IPrepareFeature.IsEnabled = value;
				}
			}
		}

		[MenuItem(IPrepareMenuItemKey, priority = 100)]
		private static void IPrepareMenuItem()
		{
			IPrepareIsEnabled = !IPrepareIsEnabled;
		}

		[MenuItem(IPrepareMenuItemKey, true)]
		private static bool IPrepareMenuItemValidation()
		{
			Menu.SetChecked(IPrepareMenuItemKey, IPrepareIsEnabled);
			return true;
		}

		#endregion
	}
}
#endif