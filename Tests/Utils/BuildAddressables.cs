using UnityEngine;
using UnityEngine.TestTools;
using System.Text;
using System;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

namespace Conkist.GDK.Tests
{
    /// <summary>
    /// If a test requires Addressables then add the attribute `[PrebuildSetup(typeof(BuildAddressables))]` to the test or class.
    /// This will ensure the Addressables are built for the test and that they are only built once, not for each test.
    /// </summary>
    public sealed class BuildAddressables : IPrebuildSetup
    {
        /// <summary>
        /// Sets up the Addressables before running tests.
        /// </summary>
        public void Setup()
        {
#if UNITY_EDITOR
            // Ignore environment-specific warnings, error logs, and third-party package issues during prebuild compilation
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            Debug.Log("Started Preparing Addressables");

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.Log("[BuildAddressables] No Addressable Asset Settings found. Programmatically creating default settings...");
                settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            }
            Debug.Assert(settings != null, "No Addressable Asset Settings could be found or created!");

            // This is to help with debugging any tests that may fail on the CI.
            StringBuilder debugInfo = new StringBuilder();
            bool error = false;
            debugInfo.AppendLine("Addressables Info:");
            debugInfo.AppendLine("Groups:");
            foreach (var group in settings.groups)
            {
                if (group == null)
                {
                    error = true;
                    debugInfo.AppendLine($"\t\tGroup Is Null!");
                }
                else
                {
                    debugInfo.AppendLine(group.Name);
                    debugInfo.AppendLine($"\tAsset Name: {group.name}");
                    debugInfo.AppendLine($"\tGuid: {group.Guid}");
                    debugInfo.AppendLine($"\tSchemas: {group.Schemas.Count}");
                    foreach (var schema in group.Schemas)
                    {
                        if (schema == null)
                        {
                            error = true;
                            debugInfo.AppendLine($"\t\tSchema Is Null!");
                        }
                        else
                        {
                            debugInfo.AppendLine($"\t\t{schema.name}");
                        }
                    }
                    debugInfo.AppendLine($"\tEntries: {group.entries.Count}");
                    foreach (var entry in group.entries)
                    {
                        if (entry == null)
                        {
                            error = true;
                            debugInfo.AppendLine($"\t\tEntry Is Null!");
                        }
                        else
                        {
                            debugInfo.AppendLine($"\t\t{entry.address}");
                            debugInfo.AppendLine($"\t\t\tPath: {entry.AssetPath}");
                            debugInfo.AppendLine($"\t\t\tGuid: {entry.guid}");
                        }
                    }
                }
            }

            if (error)
                Debug.LogError(debugInfo.ToString());
            else
                Debug.Log(debugInfo.ToString());

            // Now build the player content
            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
            BuildAddressablePlayerContent();
            Debug.Log("Finished Preparing Addressables");
            
            // Restore default LogAssert behavior for the subsequent actual test runs
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sets the active Addressable profile to the specified profile name.
        /// </summary>
        /// <param name="profileName">Name of the profile to set as active.</param>
        public static void SetActiveAddressableProfile(string profileName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string profileId = settings.profileSettings.GetProfileId(profileName);
            if (!string.IsNullOrEmpty(profileId))
            {
                settings.activeProfileId = profileId;
                Debug.Log($"Active Addressable profile set to: {profileName}");
            }
            else
            {
                Debug.LogError($"No Addressable profile found with the name: {profileName}");
            }
        }

        /// <summary>
        /// Builds the player content for Addressables using the active profile settings.
        /// </summary>
        private static void BuildAddressablePlayerContent()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                AddressableAssetSettings.BuildPlayerContent();
            }
            else
            {
                Debug.LogError("Failed to retrieve AddressableAssetSettings");
            }
        }
#endif
    }
}