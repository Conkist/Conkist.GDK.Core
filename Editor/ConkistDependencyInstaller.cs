using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Conkist.GDK.Editor
{
    [InitializeOnLoad]
    public static class ConkistDependencyInstaller
    {
        private struct DependencyData
        {
            public string Name;
            public string Source;

            public DependencyData(string name, string source)
            {
                Name = name;
                Source = source;
            }
        }

        private static readonly DependencyData[] RequiredDependencies = new[]
        {
            new DependencyData("com.cysharp.unitask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11"),
            new DependencyData("jp.hadashikick.vcontainer", "jp.hadashikick.vcontainer"),
            new DependencyData("com.unity.nuget.newtonsoft-json", "com.unity.nuget.newtonsoft-json"),
            new DependencyData("com.unity.addressables", "com.unity.addressables"),
            new DependencyData("com.unity.entities", "com.unity.entities"),
            new DependencyData("com.unity.learn.iet-framework", "com.unity.learn.iet-framework")
        };

        private static Queue<DependencyData> _installQueue;
        private static AddRequest _currentRequest;

        static ConkistDependencyInstaller()
        {
            // Register OpenUPM programmatically first to ensure clean VContainer resolution
            EnsureOpenUPMRegistry();

            // Trigger check and sequential installation
            CheckAndInstall();
        }

        private static void EnsureOpenUPMRegistry()
        {
            string manifestPath = "Packages/manifest.json";
            if (!File.Exists(manifestPath)) return;

            try
            {
                string text = File.ReadAllText(manifestPath);

                // If OpenUPM is already present, do nothing
                if (text.Contains("package.openupm.com")) return;

                Debug.Log("[Conkist GDK] Programmatically adding OpenUPM scoped registry to manifest.json...");

                if (text.Contains("\"scopedRegistries\""))
                {
                    // Scoped registries array already exists. Insert our registry object into the list.
                    int arrayStartIndex = text.IndexOf("\"scopedRegistries\"");
                    int openBracketIndex = text.IndexOf("[", arrayStartIndex);
                    if (openBracketIndex >= 0)
                    {
                        string newRegistry = "\n    {\n      \"name\": \"package.openupm.com\",\n      \"url\": \"https://package.openupm.com\",\n      \"scopes\": [\n        \"jp.hadashikick.vcontainer\"\n      ]\n    },";
                        text = text.Insert(openBracketIndex + 1, newRegistry);
                    }
                }
                else
                {
                    // Scoped registries array does not exist. Insert a brand new one right after the first '{'
                    int firstBraceIndex = text.IndexOf('{');
                    if (firstBraceIndex >= 0)
                    {
                        string registryBlock = "\n  \"scopedRegistries\": [\n    {\n      \"name\": \"package.openupm.com\",\n      \"url\": \"https://package.openupm.com\",\n      \"scopes\": [\n        \"jp.hadashikick.vcontainer\"\n      ]\n    }\n  ],";
                        text = text.Insert(firstBraceIndex + 1, registryBlock);
                    }
                }

                File.WriteAllText(manifestPath, text);
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError("[Conkist GDK] Failed to configure OpenUPM scoped registry programmatically: " + ex.Message);
            }
        }

        private static void CheckAndInstall()
        {
            ListRequest request = Client.List(true);
            while (!request.IsCompleted)
            {
                // Synchronous wait during initial import/compilation load
            }

            if (request.Status == StatusCode.Success)
            {
                HashSet<string> installedPackages = new HashSet<string>();
                foreach (var package in request.Result)
                {
                    installedPackages.Add(package.name);
                }

                _installQueue = new Queue<DependencyData>();

                foreach (var dep in RequiredDependencies)
                {
                    if (!installedPackages.Contains(dep.Name))
                    {
                        _installQueue.Enqueue(dep);
                    }
                }

                if (_installQueue.Count > 0)
                {
                    Debug.Log($"[Conkist GDK] Found {_installQueue.Count} missing dependencies. Starting sequential installation...");
                    ProcessNextInstallation();
                }
                else
                {
                    UpdateGlobalDefineSymbols(installedPackages);
                }
            }
        }

        private static void ProcessNextInstallation()
        {
            if (_installQueue == null || _installQueue.Count == 0)
            {
                Debug.Log("[Conkist GDK] All missing dependencies have been successfully processed.");
                UpdateGlobalDefineSymbols(null);
                return;
            }

            var nextDep = _installQueue.Dequeue();
            Debug.Log($"[Conkist GDK] Installing '{nextDep.Name}' from source '{nextDep.Source}'...");

            _currentRequest = Client.Add(nextDep.Source);
            EditorApplication.update += MonitorProgress;
        }

        private static void MonitorProgress()
        {
            if (_currentRequest != null && _currentRequest.IsCompleted)
            {
                EditorApplication.update -= MonitorProgress;

                if (_currentRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[Conkist GDK] Successfully installed dependency: {_currentRequest.Result.name}");
                }
                else
                {
                    Debug.LogError($"[Conkist GDK] Failed to install dependency: {_currentRequest.Error.message}");
                }

                _currentRequest = null;

                // Process the next package in queue
                ProcessNextInstallation();
            }
        }

        private static void UpdateGlobalDefineSymbols(HashSet<string> installedPackages = null)
        {
            if (installedPackages == null)
            {
                ListRequest request = Client.List(true);
                while (!request.IsCompleted) { }
                if (request.Status == StatusCode.Success)
                {
                    installedPackages = new HashSet<string>();
                    foreach (var package in request.Result)
                    {
                        installedPackages.Add(package.name);
                    }
                }
            }

            if (installedPackages == null) return;

            bool hasUniTask = installedPackages.Contains("com.cysharp.unitask");
            bool hasVContainer = installedPackages.Contains("jp.hadashikick.vcontainer");

            SetSymbolState("CONKIST_UNITASK", hasUniTask);
            SetSymbolState("CONKIST_VCONTAINER", hasVContainer);
        }

        private static void SetSymbolState(string symbol, bool active)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup == BuildTargetGroup.Unknown) return;

#if UNITY_2021_2_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif

            List<string> symbols = new List<string>(defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            bool modified = false;

            if (active)
            {
                if (!symbols.Contains(symbol))
                {
                    symbols.Add(symbol);
                    modified = true;
                }
            }
            else
            {
                if (symbols.Contains(symbol))
                {
                    symbols.Remove(symbol);
                    modified = true;
                }
            }

            if (modified)
            {
                string newDefines = string.Join(";", symbols);
#if UNITY_2021_2_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
#endif
                Debug.Log($"[Conkist GDK] Scripting define symbol '{symbol}' was {(active ? "enabled" : "disabled")} globally.");
            }
        }
    }
}
