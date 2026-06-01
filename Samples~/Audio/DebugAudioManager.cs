using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    /// <summary>
    /// Debug utility that renders a self-contained IMGUI overlay on screen.
    /// Manually references both MusicController components in the scene and commands them via AudioManager.
    /// </summary>
    [AddComponentMenu("Conkist/Audio/DebugAudioManager")]
    public class DebugAudioManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MusicController musicControllerA;
        [SerializeField] private MusicController musicControllerB;

        private void Start()
        {
            if (musicControllerA == null)
            {
                musicControllerA = FindFirstObjectByType<MusicController>();
            }
            if (musicControllerB == null)
            {
                musicControllerB = FindFirstObjectByType<MusicController>();
            }
        }

        private void OnGUI()
        {
            // Set up a clean layout area in the top-left corner
            GUILayout.BeginArea(new Rect(10, 10, 280, 520));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Conkist GDK - Audio Debug Panel</b>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 });
            GUILayout.Space(10);

            if (AudioManager.HasInstance)
            {
                var am = AudioManager.Instance;

                // Sliders for Volumes
                GUILayout.Label($"Master Volume: {am.MasterVolume * 100f:F0}%");
                float master = GUILayout.HorizontalSlider(am.MasterVolume, 0f, 1f);
                if (Mathf.Abs(master - am.MasterVolume) > 0.01f) am.MasterVolume = master;

                GUILayout.Label($"Music Volume: {am.MusicVolume * 100f:F0}%");
                float music = GUILayout.HorizontalSlider(am.MusicVolume, 0f, 1f);
                if (Mathf.Abs(music - am.MusicVolume) > 0.01f) am.MusicVolume = music;

                GUILayout.Label($"SFX Volume: {am.SFXVolume * 100f:F0}%");
                float sfx = GUILayout.HorizontalSlider(am.SFXVolume, 0f, 1f);
                if (Mathf.Abs(sfx - am.SFXVolume) > 0.01f) am.SFXVolume = sfx;

                GUILayout.Label($"Voice Volume: {am.VoiceVolume * 100f:F0}%");
                float voice = GUILayout.HorizontalSlider(am.VoiceVolume, 0f, 1f);
                if (Mathf.Abs(voice - am.VoiceVolume) > 0.01f) am.VoiceVolume = voice;

                GUILayout.Space(15);
                GUILayout.Label("<b>Music Playback Controls</b>", new GUIStyle(GUI.skin.label) { fontSize = 11 });

                // Music Controller A
                if (musicControllerA != null)
                {
                    string playStateA = musicControllerA.IsPlaying ? "Playing" : (musicControllerA.IsPaused ? "Paused" : "Stopped");
                    GUILayout.Label($"<b>Music Controller A</b> (State: {playStateA})");
                    
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play A"))
                    {
                        am.PlayMusicController(musicControllerA);
                    }
                    if (GUILayout.Button("Pause A"))
                    {
                        am.PauseMusicController(musicControllerA);
                    }
                    if (GUILayout.Button("Stop A"))
                    {
                        am.StopMusicController(musicControllerA);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Restart A"))
                    {
                        musicControllerA.Restart();
                    }
                    if (GUILayout.Button("Next A"))
                    {
                        musicControllerA.Next();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play Track 0 (A)"))
                    {
                        musicControllerA.PlayTrack(0).Forget();
                    }
                    if (GUILayout.Button("Play Track 1 (A)"))
                    {
                        musicControllerA.PlayTrack(1).Forget();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("<color=yellow>MusicControllerA is not assigned</color>");
                }

                GUILayout.Space(10);

                // Music Controller B
                if (musicControllerB != null)
                {
                    string playStateB = musicControllerB.IsPlaying ? "Playing" : (musicControllerB.IsPaused ? "Paused" : "Stopped");
                    GUILayout.Label($"<b>Music Controller B</b> (State: {playStateB})");
                    
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play B"))
                    {
                        am.PlayMusicController(musicControllerB);
                    }
                    if (GUILayout.Button("Pause B"))
                    {
                        am.PauseMusicController(musicControllerB);
                    }
                    if (GUILayout.Button("Stop B"))
                    {
                        am.StopMusicController(musicControllerB);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Restart B"))
                    {
                        musicControllerB.Restart();
                    }
                    if (GUILayout.Button("Next B"))
                    {
                        musicControllerB.Next();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play Track 0 (B)"))
                    {
                        musicControllerB.PlayTrack(0).Forget();
                    }
                    if (GUILayout.Button("Play Track 1 (B)"))
                    {
                        musicControllerB.PlayTrack(1).Forget();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("<color=yellow>MusicControllerB is not assigned</color>");
                }

                GUILayout.Space(15);
                GUILayout.Label("<b>Global Playback Controls</b>", new GUIStyle(GUI.skin.label) { fontSize = 11 });

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Stop All"))
                {
                    am.StopAllMusic();
                }
                if (GUILayout.Button("Stop Current"))
                {
                    am.StopCurrentMusic();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                am.KeepLastMusicPlaying = GUILayout.Toggle(am.KeepLastMusicPlaying, " Keep Last Music Playing");
            }
            else
            {
                GUILayout.Label("<color=red>No AudioManager found in scene</color>");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
