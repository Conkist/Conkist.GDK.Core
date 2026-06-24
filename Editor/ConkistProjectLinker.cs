using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Conkist.GDK.Editor
{
    public static class ConkistProjectLinker
    {
        private const string CoreGitUrl = "https://github.com/Conkist/Conkist.GDK.Core.git";

        [MenuItem("Conkist/GDK/Configure Adjacent Projects")]
        public static void ConfigureAdjacentProjects()
        {
            // Current project directory (e.g., /Users/renato/Conkist/Conkist GDK)
            string currentProjDir = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            // Parent directory containing all projects (e.g., /Users/renato/Conkist)
            string parentDir = Path.GetFullPath(Path.Combine(currentProjDir, ".."));

            if (!Directory.Exists(parentDir))
            {
                Debug.LogError($"[Conkist Linker] Parent directory not found: {parentDir}");
                return;
            }

            string[] subDirectories = Directory.GetDirectories(parentDir);
            int updatedCount = 0;

            foreach (string dir in subDirectories)
            {
                // Skip the current project directory
                if (Path.GetFullPath(dir) == currentProjDir)
                    continue;

                string manifestPath = Path.Combine(dir, "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        if (ConfigureProjectManifest(manifestPath))
                        {
                            updatedCount++;
                            Debug.Log($"[Conkist Linker] Successfully configured manifest in: {dir}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Conkist Linker] Failed to configure project at {dir}: {ex.Message}");
                    }
                }
            }

            EditorUtility.DisplayDialog("Conkist Project Linker", 
                $"Adjacent projects configuration complete!\nUpdated projects: {updatedCount}", "OK");
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

            // 2. Add me.conkist.gdk.core dependency if missing
            if (!jsonText.Contains("\"me.conkist.gdk.core\""))
            {
                int dependenciesIndex = jsonText.IndexOf("\"dependencies\"");
                int openBracketIndex = jsonText.IndexOf("{", dependenciesIndex);
                if (openBracketIndex >= 0)
                {
                    string newDep = $"\n    \"me.conkist.gdk.core\": \"{CoreGitUrl}\",";
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
