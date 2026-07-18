using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Conkist.GDK.Editor
{
    public class WebGLBuildPipeline
    {
        [MenuItem("Build/WebGL")]
        public static void Build()
        {

            Debug.Log("START BUILD");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            var report = BuildPipeline.BuildPlayer(
                new[] { UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).path },
                $"Build/WebGL",
                BuildTarget.WebGL,
                BuildOptions.None
                );
            Debug.Log("DONE");
            Debug.Log(report);
        }

        public static void ConsoleBuild()
        {

            Build();
            EditorApplication.Exit(1);
        }
    }
}
