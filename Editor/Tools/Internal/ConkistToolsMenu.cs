#if UNITY_EDITOR
using Conkist.GDK.Utils.Internal;
using UnityEditor;

namespace Conkist.GDK.Tools.Internal
{
	[InitializeOnLoad]
	public static class ConkistToolsMenu
	{
		[MenuItem("Conkist/Tools/Settings/AutoSave on Play", priority = 100)]
		private static void ToggleAutoSave() => ConkistToolsSettings.AutoSaveEnabled = !ConkistToolsSettings.AutoSaveEnabled;

		[MenuItem("Conkist/Tools/Settings/AutoSave on Play", true)]
		private static bool ToggleAutoSaveValidate()
		{
			Menu.SetChecked("Conkist/Tools/Settings/AutoSave on Play", ConkistToolsSettings.AutoSaveEnabled);
			return true;
		}

		[MenuItem("Conkist/Tools/Settings/Clean Empty Folders", priority = 100)]
		private static void ToggleCleanEmpty() => ConkistToolsSettings.CleanEmptyDirectoriesFeature = !ConkistToolsSettings.CleanEmptyDirectoriesFeature;

		[MenuItem("Conkist/Tools/Settings/Clean Empty Folders", true)]
		private static bool ToggleCleanEmptyValidate()
		{
			Menu.SetChecked("Conkist/Tools/Settings/Clean Empty Folders", ConkistToolsSettings.CleanEmptyDirectoriesFeature);
			return true;
		}

		[MenuItem("Conkist/Tools/Settings/SO processing", priority = 100)]
		private static void ToggleSOCheck() => ConkistToolsSettings.EnableSOCheck = !ConkistToolsSettings.EnableSOCheck;

		[MenuItem("Conkist/Tools/Settings/SO processing", true)]
		private static bool ToggleSOCheckValidate()
		{
			Menu.SetChecked("Conkist/Tools/Settings/SO processing", ConkistToolsSettings.EnableSOCheck);
			return true;
		}
	}
}
#endif
