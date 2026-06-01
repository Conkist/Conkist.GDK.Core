using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Conkist.GDK
{
    /// <summary>
    /// Central audio manager utilizing an AudioSource Object Pool.
    /// Supports simultaneous overlapping 2D sound effects, 3D spatial sounds at specific coordinates,
    /// and global volume mixing control.
    /// </summary>
    [AddComponentMenu("Conkist/Managers/AudioManager")]
    public class AudioManager : SingletonBehaviour<AudioManager>
    {

        private const string MASTER_VOLUME_KEY = "Conkist_MasterVolume";
        private const string MUSIC_VOLUME_KEY = "Conkist_MusicVolume";
        private const string SFX_VOLUME_KEY = "Conkist_SFXVolume";
        private const string VOICE_VOLUME_KEY = "Conkist_VoiceVolume";

        [Header("Object Pool Settings")]
        [SerializeField] private int initialPoolSize = 8;

        [Header("Audio Mixer Integration")]
        [SerializeField] private UnityEngine.Audio.AudioMixer audioMixer;

        [Header("Mixer Groups")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup masterGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup musicGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup voiceGroup;

        private readonly List<AudioSource> _sfxPool = new List<AudioSource>();
        private readonly List<string> _loadedAddressableKeys = new List<string>();

        public UnityEngine.Audio.AudioMixer AudioMixer => audioMixer;
        public UnityEngine.Audio.AudioMixerGroup MasterGroup => masterGroup;
        public UnityEngine.Audio.AudioMixerGroup MusicGroup => musicGroup;
        public UnityEngine.Audio.AudioMixerGroup SFXGroup => sfxGroup;
        public UnityEngine.Audio.AudioMixerGroup VoiceGroup => voiceGroup;

        public float MasterVolume
        {
            get => GetVolume(masterGroup, MASTER_VOLUME_KEY);
            set { SetVolume(masterGroup, MASTER_VOLUME_KEY, value); UpdateVolumes(); }
        }

        public float MusicVolume
        {
            get => GetVolume(musicGroup, MUSIC_VOLUME_KEY);
            set { SetVolume(musicGroup, MUSIC_VOLUME_KEY, value); UpdateVolumes(); }
        }

        public float SFXVolume
        {
            get => GetVolume(sfxGroup, SFX_VOLUME_KEY);
            set { SetVolume(sfxGroup, SFX_VOLUME_KEY, value); UpdateVolumes(); }
        }

        public float VoiceVolume
        {
            get => GetVolume(voiceGroup, VOICE_VOLUME_KEY);
            set { SetVolume(voiceGroup, VOICE_VOLUME_KEY, value); UpdateVolumes(); }
        }
        [Header("Music Management Settings")]
        [SerializeField] private bool keepLastMusicPlaying = false;

        private readonly List<MusicController> _musicControllers = new List<MusicController>();

        public bool KeepLastMusicPlaying
        {
            get => keepLastMusicPlaying;
            set => keepLastMusicPlaying = value;
        }

        public int RegisteredMusicControllerCount => _musicControllers.Count;

        public void RegisterMusicController(MusicController controller)
        {
            if (!_musicControllers.Contains(controller))
            {
                _musicControllers.Add(controller);
            }
        }

        public void UnregisterMusicController(MusicController controller)
        {
            if (_musicControllers.Contains(controller))
            {
                _musicControllers.Remove(controller);
            }
        }

        public void PlayMusicController(int index)
        {
            if (index >= 0 && index < _musicControllers.Count)
            {
                if (!keepLastMusicPlaying)
                {
                    StopAllMusicExcept(index);
                }
                _musicControllers[index].Play();
            }
        }

        public void PlayMusicController(MusicController controller)
        {
            if (controller == null) return;
            int index = _musicControllers.IndexOf(controller);
            if (index >= 0)
            {
                PlayMusicController(index);
            }
            else
            {
                controller.Play();
            }
        }

        public void PauseMusicController(int index)
        {
            if (index >= 0 && index < _musicControllers.Count)
            {
                _musicControllers[index].Pause();
            }
        }

        public void PauseMusicController(MusicController controller)
        {
            if (controller == null) return;
            int index = _musicControllers.IndexOf(controller);
            if (index >= 0)
            {
                PauseMusicController(index);
            }
            else
            {
                controller.Pause();
            }
        }

        public void StopMusicController(int index)
        {
            if (index >= 0 && index < _musicControllers.Count)
            {
                _musicControllers[index].Stop();
            }
        }

        public void StopMusicController(MusicController controller)
        {
            if (controller == null) return;
            int index = _musicControllers.IndexOf(controller);
            if (index >= 0)
            {
                StopMusicController(index);
            }
            else
            {
                controller.Stop();
            }
        }

        public void StopAllMusic()
        {
            for (int i = 0; i < _musicControllers.Count; i++)
            {
                _musicControllers[i].Stop();
            }
        }

        public void StopCurrentMusic()
        {
            for (int i = 0; i < _musicControllers.Count; i++)
            {
                if (_musicControllers[i].IsPlaying)
                {
                    _musicControllers[i].Stop();
                }
            }
        }

        private void StopAllMusicExcept(int activeIndex)
        {
            for (int i = 0; i < _musicControllers.Count; i++)
            {
                if (i != activeIndex)
                {
                    _musicControllers[i].Stop();
                }
            }
        }
        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            InitializePool();
            LoadSettings();
        }

        private void InitializePool()
        {
            Transform sfxParent = transform.Find("SFX");
            if (sfxParent == null)
            {
                GameObject sfxChild = new GameObject("SFX");
                sfxChild.transform.SetParent(transform);
                sfxParent = sfxChild.transform;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewPooledSource(sfxParent);
            }
        }

        private AudioSource CreateNewPooledSource(Transform parent = null)
        {
            if (parent == null)
            {
                Transform sfxParent = transform.Find("SFX");
                if (sfxParent == null)
                {
                    GameObject sfxChild = new GameObject("SFX");
                    sfxChild.transform.SetParent(transform);
                    sfxParent = sfxChild.transform;
                }
                parent = sfxParent;
            }

            GameObject sfxObj = new GameObject($"Pooled_SFX_Source_{_sfxPool.Count}");
            sfxObj.transform.SetParent(parent);
            
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            if (sfxGroup != null)
            {
                source.outputAudioMixerGroup = sfxGroup;
            }
            
            _sfxPool.Add(source);
            return source;
        }

        /// <summary>
        /// Retrieves an inactive AudioSource from the pool or dynamically instantiates a new one if all are busy.
        /// </summary>
        public AudioSource GetPooledSFXSource()
        {
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                if (!_sfxPool[i].isPlaying)
                {
                    return _sfxPool[i];
                }
            }

            // Pool exhausted, create new source dynamically
            return CreateNewPooledSource();
        }

        /// <summary>
        /// Updates the volume of all active pooled sources.
        /// </summary>
        public void UpdateVolumes()
        {
            if (!IsMixerParameterExposed(sfxGroup))
            {
                float currentSFXVol = GetAdjustedSFXVolume();
                for (int i = 0; i < _sfxPool.Count; i++)
                {
                    if (_sfxPool[i].isPlaying)
                    {
                        _sfxPool[i].volume = currentSFXVol;
                    }
                }
            }

            // Trigger global event notifying other controllers (like MusicController) to update BGM volumes
            EventManager.TriggerEvent(new GameEvent("VolumeChanged"));
        }

        private float LinearToDecibels(float linear)
        {
            return Mathf.Log10(Mathf.Max(0.0001f, linear)) * 20f;
        }

        private float DecibelsToLinear(float db)
        {
            return Mathf.Pow(10f, db / 20f);
        }

        public float GetAdjustedMusicVolume() => MusicVolume * MasterVolume;
        public float GetAdjustedSFXVolume() => SFXVolume * MasterVolume;

        /// <summary>
        /// Checks if a parameter corresponding to the AudioMixerGroup name is exposed on the AudioMixer.
        /// </summary>
        public bool IsMixerParameterExposed(UnityEngine.Audio.AudioMixerGroup group)
        {
            if (audioMixer == null || group == null) return false;
            float temp;
            return audioMixer.GetFloat(group.name, out temp) || audioMixer.GetFloat(group.name + "Volume", out temp);
        }

        private AudioSource _defaultMusicSource;

        /// <summary>
        /// Lazily initializes and retrieves the default AudioSource for background music.
        /// </summary>
        public AudioSource DefaultMusicSource
        {
            get
            {
                if (_defaultMusicSource == null)
                {
                    Transform musicParent = transform.Find("Music");
                    if (musicParent == null)
                    {
                        GameObject musicChild = new GameObject("Music");
                        musicChild.transform.SetParent(transform);
                        musicParent = musicChild.transform;
                    }

                    Transform sourceTrans = musicParent.Find("DefaultMusicSource");
                    if (sourceTrans != null)
                    {
                        _defaultMusicSource = sourceTrans.GetComponent<AudioSource>();
                    }

                    if (_defaultMusicSource == null)
                    {
                        GameObject musicObj = new GameObject("DefaultMusicSource");
                        musicObj.transform.SetParent(musicParent);
                        _defaultMusicSource = musicObj.AddComponent<AudioSource>();
                    }

                    _defaultMusicSource.playOnAwake = false;
                    _defaultMusicSource.loop = true;
                    if (musicGroup != null)
                    {
                        _defaultMusicSource.outputAudioMixerGroup = musicGroup;
                    }
                }
                return _defaultMusicSource;
            }
        }

        #region Play SFX (2D and 3D)

        /// <summary>
        /// Plays a one-shot 2D SFX using a pooled AudioSource.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetPooledSFXSource();
            source.transform.position = transform.position;
            source.spatialBlend = 0f; // 2D Sound
            source.clip = clip;
            source.volume = volumeScale * (IsMixerParameterExposed(sfxGroup) ? 1f : GetAdjustedSFXVolume());
            source.pitch = pitch;
            source.Play();
        }

        /// <summary>
        /// Plays a one-shot 3D spatial SFX at a specific target location using a pooled AudioSource.
        /// </summary>
        public void PlaySFX3D(AudioClip clip, Vector3 position, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetPooledSFXSource();
            source.transform.position = position;
            source.spatialBlend = 1f; // Full 3D Spatial sound
            source.clip = clip;
            source.volume = volumeScale * (IsMixerParameterExposed(sfxGroup) ? 1f : GetAdjustedSFXVolume());
            source.pitch = pitch;
            source.Play();
        }

        /// <summary>
        /// Plays a one-shot 2D SFX using an Addressables key.
        /// </summary>
        public async UniTask PlaySFX(string address, float volumeScale = 1f, float pitch = 1f)
        {
            if (string.IsNullOrEmpty(address)) return;

            try
            {
                AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(address);
                if (clip != null)
                {
                    TrackAddressableKey(address);
                    PlaySFX(clip, volumeScale, pitch);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AudioManager] Failed to load 2D SFX at key '{address}': {ex.Message}");
            }
        }

        /// <summary>
        /// Plays a one-shot 3D SFX at a specific target location using an Addressables key.
        /// </summary>
        public async UniTask PlaySFX3D(string address, Vector3 position, float volumeScale = 1f, float pitch = 1f)
        {
            if (string.IsNullOrEmpty(address)) return;

            try
            {
                AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(address);
                if (clip != null)
                {
                    TrackAddressableKey(address);
                    PlaySFX3D(clip, position, volumeScale, pitch);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AudioManager] Failed to load 3D SFX at key '{address}': {ex.Message}");
            }
        }

        private void TrackAddressableKey(string key)
        {
            if (!_loadedAddressableKeys.Contains(key))
            {
                _loadedAddressableKeys.Add(key);
            }
        }

        #endregion

        #region Save / Load Settings

        private float GetVolume(UnityEngine.Audio.AudioMixerGroup group, string prefsKey)
        {
            float saved = PlayerPrefs.GetFloat(prefsKey, 0.8f);
            if (audioMixer != null && group != null)
            {
                float db;
                if (audioMixer.GetFloat(group.name, out db) || audioMixer.GetFloat(group.name + "Volume", out db))
                {
                    return DecibelsToLinear(db);
                }
            }
            return saved;
        }

        private void SetVolume(UnityEngine.Audio.AudioMixerGroup group, string prefsKey, float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(prefsKey, value);
            PlayerPrefs.Save();

            if (audioMixer != null && group != null)
            {
                float db = LinearToDecibels(value);
                
                // Safely check parameter existence with GetFloat (which doesn't print warnings) before calling SetFloat
                float temp;
                if (audioMixer.GetFloat(group.name, out temp))
                {
                    audioMixer.SetFloat(group.name, db);
                }
                if (audioMixer.GetFloat(group.name + "Volume", out temp))
                {
                    audioMixer.SetFloat(group.name + "Volume", db);
                }
            }

            EventManager.TriggerEvent(new GameEvent("VolumeChanged"));
        }

        private void LoadSettings()
        {
            SetVolume(masterGroup, MASTER_VOLUME_KEY, PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f));
            SetVolume(musicGroup, MUSIC_VOLUME_KEY, PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f));
            SetVolume(sfxGroup, SFX_VOLUME_KEY, PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f));
            SetVolume(voiceGroup, VOICE_VOLUME_KEY, PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 0.8f));
            UpdateVolumes();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (Instance == this)
            {
                foreach (var key in _loadedAddressableKeys)
                {
                    Addressables.Release(key);
                }
                _loadedAddressableKeys.Clear();
            }
        }

        #endregion
    }
}
