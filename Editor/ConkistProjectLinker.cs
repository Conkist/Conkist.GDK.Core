using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Conkist.GDK.Editor
{
    public static class ConkistProjectLinker
    {
        private const string ServicesGitUrl = "https://github.com/Conkist/Conkist.GDK.Services.git";
        private const string SmartAgentsGitUrl = "https://github.com/Conkist/Conkist.GDK.SmartAgents.git";

        [MenuItem("Conkist/GDK/Import Services and Smart Agents")]
        public static void ImportAdditionalPackages()
        {
            string manifestPath = "Packages/manifest.json";

            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"[Conkist Linker] manifest.json not found in the current project: {manifestPath}");
                return;
            }

            try
            {
                if (ConfigureProjectManifest(manifestPath))
                {
                    Debug.Log("[Conkist Linker] Successfully configured manifest.json with Services and Smart Agents dependencies.");
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Conkist GDK", 
                        "Successfully added GDK Services and Smart Agents to the project manifest.\nUnity will now download and compile them.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Conkist GDK", 
                        "GDK Services and Smart Agents are already configured in this project.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Conkist Linker] Failed to configure project manifest: {ex.Message}");
                EditorUtility.DisplayDialog("Conkist GDK Error", 
                    $"Failed to configure project manifest: {ex.Message}", "OK");
            }
        }

        private static bool ConfigureProjectManifest(string manifestPath)
        {
            string jsonText = File.ReadAllText(manifestPath);
            bool modified = false;

            // 1. Add OpenUPM registry if missing
            if (!jsonText.Contains("package.openupm.com"))
            {
                // Simple JSON injection for scoped registries
                if (jsonText.Contains("\"scopedRegistries\""))
                {
                    int arrayStartIndex = jsonText.IndexOf("\"scopedRegistries\"");
                    int openBracketIndex = jsonText.IndexOf("[", arrayStartIndex);
                    if (openBracketIndex >= 0)
                    {
                        string newRegistry = "\n    {\n      \"name\": \"package.openupm.com\",\n      \"url\": \"https://package.openupm.com\",\n      \"scopes\": [\n        \"com.cysharp\",\n        \"jp.hadashikick.vcontainer\"\n      ]\n    },";
                        jsonText = jsonText.Insert(openBracketIndex + 1, newRegistry);
                        modified = true;
                    }
                }
                else
                {
                    int firstBraceIndex = jsonText.IndexOf('{');
                    if (firstBraceIndex >= 0)
                    {
                        string registryBlock = "\n  \"scopedRegistries\": [\n    {\n      \"name\": \"package.openupm.com\",\n      \"url\": \"https://package.openupm.com\",\n      \"scopes\": [\n        \"com.cysharp\",\n        \"jp.hadashikick.vcontainer\"\n      ]\n    }\n  ],";
                        jsonText = jsonText.Insert(firstBraceIndex + 1, registryBlock);
                        modified = true;
                    }
                }
            }

            // 2. Add me.conkist.gdk.services dependency if missing
            if (!jsonText.Contains("\"me.conkist.gdk.services\""))
            {
                int dependenciesIndex = jsonText.IndexOf("\"dependencies\"");
                int openBracketIndex = jsonText.IndexOf("{", dependenciesIndex);
                if (openBracketIndex >= 0)
                {
                    string newDep = $"\n    \"me.conkist.gdk.services\": \"{ServicesGitUrl}\",";
                    jsonText = jsonText.Insert(openBracketIndex + 1, newDep);
                    modified = true;
                }
            }

            // 3. Add me.conkist.gdk.smart-agents dependency if missing
            if (!jsonText.Contains("\"me.conkist.gdk.smart-agents\""))
            {
                int dependenciesIndex = jsonText.IndexOf("\"dependencies\"");
                int openBracketIndex = jsonText.IndexOf("{", dependenciesIndex);
                if (openBracketIndex >= 0)
                {
                    string newDep = $"\n    \"me.conkist.gdk.smart-agents\": \"{SmartAgentsGitUrl}\",";
                    jsonText = jsonText.Insert(openBracketIndex + 1, newDep);
                    modified = true;
                }
            }

            if (modified)
            {
                File.WriteAllText(manifestPath, jsonText);
                return true;
            }

            return false;
        }
    }
}
