using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Conkist.GDK
{
    /// <summary>
    /// Decoupled component dedicated to managing background music.
    /// Manages track lists, playback states, and reacts to global volume changes.
    /// </summary>
    [AddComponentMenu("Conkist/Audio/MusicController")]
    public class MusicController : MonoBehaviour, EventListener<GameEvent>
    {
        [Header("Asset Sources")]
        [SerializeField] private AssetReference[] audioClipReferences;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicAudioSource; // Optional custom music source

        [Header("Trigger Options")]
        [SerializeField] private TriggerEvent triggerOn = TriggerEvent.Manual;

        [Header("Transition Settings")]
        [SerializeField] private bool blendTransition = false;
        [SerializeField] private float fadeDuration = 0.5f;

        private int _currentIndex = 0;
        private bool _isPaused = false;
        private AudioSource _localFallbackSource;
        private readonly Dictionary<int, AudioClip> _loadedClips = new Dictionary<int, AudioClip>();
        private bool _isLoading = false;

        public int CurrentIndex => _currentIndex;
        public bool IsPaused => _isPaused;
        public bool IsPlaying => GetActiveSource() != null && GetActiveSource().isPlaying;

        private void Start()
        {
            UpdateBGMVolumes();
            if (triggerOn == TriggerEvent.Start)
            {
                Play();
            }
        }

        private void OnEnable()
        {
            this.Subscribe<GameEvent>();
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.RegisterMusicController(this);
            }
            if (triggerOn == TriggerEvent.OnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            this.Unsubscribe<GameEvent>();
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.UnregisterMusicController(this);
            }
            if (triggerOn == TriggerEvent.OnDisable)
            {
                Play();
            }
        }

        /// <summary>
        /// Retrieves the currently active AudioSource.
        /// Falls back to AudioManager.Instance.DefaultMusicSource, or a local fallback.
        /// </summary>
        public AudioSource GetActiveSource()
        {
            if (musicAudioSource != null) return musicAudioSource;
            if (AudioManager.HasInstance) return AudioManager.Instance.DefaultMusicSource;

            if (_localFallbackSource == null)
            {
                GameObject go = new GameObject("LocalMusicSourceFallback");
                go.transform.SetParent(transform);
                _localFallbackSource = go.AddComponent<AudioSource>();
                _localFallbackSource.playOnAwake = false;
                _localFallbackSource.loop = true;
            }
            return _localFallbackSource;
        }

        private void UpdateBGMVolumes()
        {
            if (!AudioManager.HasInstance) return;

            var am = AudioManager.Instance;
            bool useMixer = am.IsMixerParameterExposed(am.MusicGroup);

            float targetVolume = useMixer ? 1f : am.GetAdjustedMusicVolume();
            AudioSource source = GetActiveSource();
            if (source != null)
            {
                source.volume = targetVolume;
            }
        }

        public void OnEventCallback(GameEvent eventData)
        {
            if (eventData.EventName == "VolumeChanged")
            {
                UpdateBGMVolumes();
            }
        }

        #region Music Control Functions

        /// <summary>
        /// Plays or resumes the background music.
        /// </summary>
        public void Play()
        {
            AudioSource source = GetActiveSource();
            if (source == null) return;

            if (_isPaused && source.clip != null)
            {
                source.Play();
                _isPaused = false;
                UpdateBGMVolumes();
            }
            else
            {
                PlayTrack(_currentIndex).Forget();
            }
        }

        /// <summary>
        /// Pauses the active music playback.
        /// </summary>
        public void Pause()
        {
            AudioSource source = GetActiveSource();
            if (source != null && source.isPlaying)
            {
                source.Pause();
                _isPaused = true;
            }
        }

        /// <summary>
        /// Stops the active music playback.
        /// </summary>
        public void Stop()
        {
            AudioSource source = GetActiveSource();
            if (source != null)
            {
                source.Stop();
            }
            _isPaused = false;
        }

        /// <summary>
        /// Plays the next track in the playlist, looping back to the start if necessary.
        /// </summary>
        public void Next()
        {
            if (audioClipReferences == null || audioClipReferences.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % audioClipReferences.Length;
            _isPaused = false;
            PlayTrack(_currentIndex).Forget();
        }

        /// <summary>
        /// Restarts the currently playing track from the beginning.
        /// </summary>
        public void Restart()
        {
            AudioSource source = GetActiveSource();
            if (source != null && source.clip != null)
            {
                source.time = 0;
                source.Play();
                _isPaused = false;
                UpdateBGMVolumes();
            }
            else
            {
                PlayTrack(_currentIndex).Forget();
            }
        }

        /// <summary>
        /// Plays a track at the specified index from the playlist.
        /// </summary>
        public async UniTask PlayTrack(int index)
        {
            if (audioClipReferences == null || audioClipReferences.Length == 0) return;
            if (index < 0 || index >= audioClipReferences.Length) return;

            _currentIndex = index;
            _isPaused = false;

            AudioSource source = GetActiveSource();
            if (source == null) return;

            AudioClip clip = null;
            if (!_loadedClips.TryGetValue(index, out clip) || clip == null)
            {
                AssetReference assetRef = audioClipReferences[index];
                if (assetRef == null || !assetRef.RuntimeKeyIsValid())
                {
                    Debug.LogWarning($"[MusicController] Invalid AssetReference at index {index}.");
                    return;
                }

                if (_isLoading) return; // Prevent concurrent loads
                _isLoading = true;

                try
                {
                    clip = await Addressables.LoadAssetAsync<AudioClip>(assetRef).ToUniTask();
                    _loadedClips[index] = clip;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[MusicController] Failed to load BGM track at index {index}: {ex.Message}");
                }
                finally
                {
                    _isLoading = false;
                }
            }

            if (clip != null)
            {
                if (blendTransition && source.isPlaying && source.clip != clip)
                {
                    FadeAndSwapTrack(source, clip).Forget();
                }
                else
                {
                    source.clip = clip;
                    source.Play();
                    UpdateBGMVolumes();
                }
            }
        }

        private async UniTaskVoid FadeAndSwapTrack(AudioSource source, AudioClip clip)
        {
            float halfDuration = fadeDuration * 0.5f;
            float elapsed = 0f;
            float startVol = source.volume;

            // Fade Out
            if (halfDuration > 0.001f)
            {
                while (elapsed < halfDuration)
                {
                    if (this == null || source == null) return;
                    elapsed += Time.unscaledDeltaTime;
                    source.volume = Mathf.Lerp(startVol, 0f, elapsed / halfDuration);
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            source.Stop();
            source.clip = clip;
            source.Play();

            // Fade In
            elapsed = 0f;
            float targetVolume = AudioManager.HasInstance && AudioManager.Instance.AudioMixer != null ? 1f : (AudioManager.HasInstance ? AudioManager.Instance.GetAdjustedMusicVolume() : 0.8f);

            if (halfDuration > 0.001f)
            {
                while (elapsed < halfDuration)
                {
                    if (this == null || source == null) return;
                    elapsed += Time.unscaledDeltaTime;
                    source.volume = Mathf.Lerp(0f, targetVolume, elapsed / halfDuration);
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }

            if (source != null)
            {
                source.volume = targetVolume;
            }
        }

        /// <summary>
        /// Legacy playback method. Re-routed to the new PlayTrack.
        /// </summary>
        [System.Obsolete("Use Play(), PlayTrack(index) or Next() instead.")]
        public void PlayBGM(AudioClip clip, bool loop = true, float fadeDuration = 1.0f)
        {
            AudioSource source = GetActiveSource();
            if (source != null)
            {
                source.clip = clip;
                source.loop = loop;
                source.Play();
                _isPaused = false;
                UpdateBGMVolumes();
            }
        }

        #endregion

        private void OnDestroy()
        {
            foreach (var clip in _loadedClips.Values)
            {
                if (clip != null)
                {
                    Addressables.Release(clip);
                }
            }
            _loadedClips.Clear();
        }
    }
}
