using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    /// <summary>
    /// Enum representing standard automatic trigger events.
    /// </summary>
    public enum TriggerEvent
    {
        Start,
        OnEnable,
        OnDisable,
        Manual
    }

    /// <summary>
    /// Highly versatile AudioTrigger component under the KISS pattern.
    /// Spawns 2D or 3D spatial sound effects using an assigned AudioSource target,
    /// or falls back to the AudioManager's global pool.
    /// </summary>
    [AddComponentMenu("Conkist/Audio/AudioTrigger")]
    public class AudioTrigger : MonoBehaviour
    {
        [Header("Asset Sources")]
        [SerializeField] private AssetReference audioClipReference;

        private AudioClip _runtimeClip;

        /// <summary>
        /// Sets a runtime clip override for the trigger. Useful for manual playing and testing.
        /// </summary>
        public void SetRuntimeClip(AudioClip clip)
        {
            _runtimeClip = clip;
        }

        [Header("Trigger Options")]
        [SerializeField] private TriggerEvent triggerOn = TriggerEvent.Manual;
        [SerializeField] private bool is3D = false;
        [SerializeField] private AudioSource targetAudioSource;
        [SerializeField] private Transform targetTransform;

        [Header("Audio Variations")]
        [SerializeField] private bool applyPitchVariance = false;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;

        private void Start()
        {
            if (triggerOn == TriggerEvent.Start)
            {
                Trigger();
            }
        }

        private void OnEnable()
        {
            if (triggerOn == TriggerEvent.OnEnable)
            {
                Trigger();
            }
        }

        private void OnDisable()
        {
            if (triggerOn == TriggerEvent.OnDisable)
            {
                Trigger();
            }
        }

        /// <summary>
        /// Triggers the default asset (AudioClip Reference or runtime override) registered on the trigger.
        /// </summary>
        public void Trigger()
        {
            if (_runtimeClip != null)
            {
                Trigger(_runtimeClip);
            }
            else if (audioClipReference != null && audioClipReference.RuntimeKeyIsValid())
            {
                TriggerAddressable().Forget();
            }
        }

        /// <summary>
        /// Triggers playback of a custom AudioClip, overriding the default clip.
        /// </summary>
        public void Trigger(AudioClip customClip)
        {
            if (customClip == null) return;

            float pitch = GetPitch();

            if (targetAudioSource != null)
            {
                targetAudioSource.clip = customClip;
                targetAudioSource.pitch = pitch;
                targetAudioSource.spatialBlend = is3D ? 1f : 0f;
                if (is3D && targetTransform != null)
                {
                    targetAudioSource.transform.position = targetTransform.position;
                }

                // Adjust volume based on mixer exposure or fallback
                if (AudioManager.HasInstance)
                {
                    var am = AudioManager.Instance;
                    bool useMixer = am.IsMixerParameterExposed(am.SFXGroup);
                    targetAudioSource.volume = useMixer ? 1f : am.GetAdjustedSFXVolume();
                }

                targetAudioSource.Play();
            }
            else if (AudioManager.HasInstance)
            {
                Vector3 position = GetTargetPosition();
                if (is3D)
                {
                    AudioManager.Instance.PlaySFX3D(customClip, position, volumeScale: 1f, pitch: pitch);
                }
                else
                {
                    AudioManager.Instance.PlaySFX(customClip, volumeScale: 1f, pitch: pitch);
                }
            }
        }

        private async UniTaskVoid TriggerAddressable()
        {
            if (audioClipReference == null || !audioClipReference.RuntimeKeyIsValid()) return;

            try
            {
                AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(audioClipReference).ToUniTask();
                if (clip != null)
                {
                    Trigger(clip);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AudioTrigger] Failed to load addressable audio clip: {ex.Message}");
            }
        }

        private float GetPitch()
        {
            if (applyPitchVariance)
            {
                return Random.Range(minPitch, maxPitch);
            }
            return 1f;
        }

        private Vector3 GetTargetPosition()
        {
            if (targetTransform != null)
            {
                return targetTransform.position;
            }
            return transform.position;
        }
    }
}
