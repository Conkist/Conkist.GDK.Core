#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace Conkist.GDK.Utils.Internal
{
	public static class ConkistToolsSettings
	{
		public static bool AutoSaveEnabled
		{
			get => Data.AutoSaveEnabled;
			set
			{
				if (Data.AutoSaveEnabled == value) return;
				Data.AutoSaveEnabled = value;
				SaveData(Data);
			}
		}

		public static bool CleanEmptyDirectoriesFeature
		{
			get => Data.CleanEmptyDirectoriesFeature;
			set
			{
				if (Data.CleanEmptyDirectoriesFeature == value) return;
				Data.CleanEmptyDirectoriesFeature = value;
				SaveData(Data);
			}
		}

		public static bool PrepareOnPlaymode
		{
			get => Data.PrepareOnPlaymode;
			set
			{
				if (Data.PrepareOnPlaymode == value) return;
				Data.PrepareOnPlaymode = value;
				SaveData(Data);
			}
		}

		public static bool EnableSOCheck
		{
			get => Data.EnableSOCheck;
			set
			{
				if (Data.EnableSOCheck == value) return;
				Data.EnableSOCheck = value;
				SaveData(Data);
			}
		}

		[Serializable]
		private class ConkistToolsSettingsData
		{
			// ReSharper disable MemberHidesStaticFromOuterClass
			public bool AutoSaveEnabled = true;
			public bool CleanEmptyDirectoriesFeature;
			public bool PrepareOnPlaymode = true;
			public bool EnableSOCheck = true;
			// ReSharper restore MemberHidesStaticFromOuterClass
		}

		private static ConkistToolsSettingsData Data => _data ?? (_data = LoadData());
		private static ConkistToolsSettingsData _data; 
		
		
		#region Save Load

		private static readonly string Directory = "ProjectSettings";
		private static readonly string Path = Directory + "/ConkistToolsSettings.asset";

		private static ConkistToolsSettingsData LoadData()
		{
			if (!File.Exists(Path)) return new ConkistToolsSettingsData();

			ConkistToolsSettingsData data;
			try
			{
				var jsonData = File.ReadAllText(Path);
				data = JsonUtility.FromJson<ConkistToolsSettingsData>(jsonData);
			}
			catch
			{
				data = new ConkistToolsSettingsData();
				// Try parse old settings file
				var fileContents = File.ReadAllLines(Path);
				foreach (var content in fileContents)
				{
					var value = content.Split(':');
					if (value[0].Contains("_autoSaveEnabled")) data.AutoSaveEnabled = int.Parse(value[1]) == 1;
					if (value[0].Contains("_cleanEmptyDirectoriesFeature")) data.CleanEmptyDirectoriesFeature = int.Parse(value[1]) == 1;
					if (value[0].Contains("_prepareOnPlaymode")) data.PrepareOnPlaymode = int.Parse(value[1]) == 1;
				}
				SaveData(data);
			} 
			return data;
		}
		
		private static void SaveData(ConkistToolsSettingsData data)
		{
			if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);
			try
			{
				File.WriteAllText(Path, JsonUtility.ToJson(data, true));
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to save ConkistToolsSettings!\n" + ex);
			}
		}

		#endregion
	}
}
#endif