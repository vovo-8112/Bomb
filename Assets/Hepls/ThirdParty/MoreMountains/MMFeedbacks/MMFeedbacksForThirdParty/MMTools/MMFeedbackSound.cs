using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Sound")]
    [FeedbackHelp("This feedback lets you play the specified AudioClip, either via event (you'll need something in your scene to catch a MMSfxEvent, for example a MMSoundManager), or cached (AudioSource gets created on init, and is then ready to be played), or on demand (instantiated on Play). For all these methods you can define a random volume between min/max boundaries (just set the same value in both fields if you don't want randomness), random pitch, and an optional AudioMixerGroup.")]
    public class MMFeedbackSound : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public enum PlayMethods { Event, Cached, OnDemand, Pool }

        [Header("Sound")]
        [Tooltip("the sound clip to play")]
        public AudioClip Sfx;

        [Header("Random Sound")]
        [Tooltip("an array to pick a random sfx from")]
        public AudioClip[] RandomSfx;

        [Header("Test")]
        [MMFInspectorButton("TestPlaySound")]
        public bool TestButton;
        [MMFInspectorButton("TestStopSound")]
        public bool TestStopButton;

        [Header("Method")]
        [Tooltip("the play method to use when playing the sound (event, cached or on demand)")]
        public PlayMethods PlayMethod = PlayMethods.Event;
        [Tooltip("the size of the pool when in Pool mode")]
        [MMFEnumCondition("PlayMethod", (int)PlayMethods.Pool)]
        public int PoolSize = 10;

        [Header("Volume")]
        [Tooltip("the minimum volume to play the sound at")]
        public float MinVolume = 1f;
        [Tooltip("the maximum volume to play the sound at")]
        public float MaxVolume = 1f;

        [Header("Pitch")]
        [Tooltip("the minimum pitch to play the sound at")]
        public float MinPitch = 1f;
        [Tooltip("the maximum pitch to play the sound at")]
        public float MaxPitch = 1f;

        [Header("Mixer")]
        [Tooltip("the audiomixer to play the sound with (optional)")]
        public AudioMixerGroup SfxAudioMixerGroup;
        public override float FeedbackDuration { get { return GetDuration(); } }

        protected AudioClip _randomClip;
        protected AudioSource _cachedAudioSource;
        protected AudioSource[] _pool;
        protected AudioSource _tempAudioSource;
        protected float _duration;
        protected AudioSource _editorAudioSource;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (PlayMethod == PlayMethods.Cached)
            {
                _cachedAudioSource = CreateAudioSource(owner, "CachedFeedbackAudioSource");
            }
            if (PlayMethod == PlayMethods.Pool)
            {
                _pool = new AudioSource[PoolSize];
                for (int i = 0; i < PoolSize; i++)
                {
                    _pool[i] = CreateAudioSource(owner, "PooledAudioSource"+i);
                }
            }
        }

        protected virtual AudioSource CreateAudioSource(GameObject owner, string audioSourceName)
        {
            GameObject temporaryAudioHost = new GameObject(audioSourceName);
            temporaryAudioHost.transform.position = owner.transform.position;
            temporaryAudioHost.transform.SetParent(owner.transform);
            _tempAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
            _tempAudioSource.playOnAwake = false;
            return _tempAudioSource; 
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            if (Active)
            {
                if (Sfx != null)
                {
                    _duration = Sfx.length;
                    PlaySound(Sfx, position, intensityMultiplier);
                    return;
                }

                if (RandomSfx.Length > 0)
                {
                    _randomClip = RandomSfx[Random.Range(0, RandomSfx.Length)];

                    if (_randomClip != null)
                    {
                        _duration = _randomClip.length;
                        PlaySound(_randomClip, position, intensityMultiplier);
                    }
                    
                }
            }
        }

        protected virtual float GetDuration()
        {
            if (Sfx != null)
            {
                return Sfx.length;
            }

            float longest = 0f;
            if ((RandomSfx != null) && (RandomSfx.Length > 0))
            {
                foreach (AudioClip clip in RandomSfx)
                {
                    if ((clip != null) && (clip.length > longest))
                    {
                        longest = clip.length;
                    }
                }

                return longest;
            }

            return 0f;
        }
        protected virtual void PlaySound(AudioClip sfx, Vector3 position, float intensity)
        {
            float volume = Random.Range(MinVolume, MaxVolume);
            
            if (!Timing.ConstantIntensity)
            {
                volume = volume * intensity;
            }
            
            float pitch = Random.Range(MinPitch, MaxPitch);

            int timeSamples = NormalPlayDirection ? 0 : sfx.samples - 1;
            
            if (!NormalPlayDirection)
            {
                pitch = -pitch;
            }
            
            if (PlayMethod == PlayMethods.Event)
            {
                MMSfxEvent.Trigger(sfx, SfxAudioMixerGroup, volume, pitch);
                return;
            }

            if (PlayMethod == PlayMethods.OnDemand)
            {
                GameObject temporaryAudioHost = new GameObject("TempAudio");
                temporaryAudioHost.transform.position = position;
                AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
                PlayAudioSource(audioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup);
                Destroy(temporaryAudioHost, sfx.length);
            }

            if (PlayMethod == PlayMethods.Cached)
            {
                PlayAudioSource(_cachedAudioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup);
            }

            if (PlayMethod == PlayMethods.Pool)
            {
                _tempAudioSource = GetAudioSourceFromPool();
                if (_tempAudioSource != null)
                {
                    PlayAudioSource(_tempAudioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup);
                }
            }
        }
        protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch, int timeSamples, AudioMixerGroup audioMixerGroup = null)
        {
            audioSource.clip = sfx;
            audioSource.timeSamples = timeSamples;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = false;
            if (audioMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = audioMixerGroup;
            }
            audioSource.Play(); 
        }
        protected virtual AudioSource GetAudioSourceFromPool()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                if (!_pool[i].isPlaying)
                {
                    return _pool[i];
                }
            }
            return null;
        }
        protected virtual async void TestPlaySound()
        {
            AudioClip tmpAudioClip = null;

            if (Sfx != null)
            {
                tmpAudioClip = Sfx;
            }

            if (RandomSfx.Length > 0)
            {
                tmpAudioClip = RandomSfx[Random.Range(0, RandomSfx.Length)];
            }

            if (tmpAudioClip == null)
            {
                Debug.LogError(Label + " on " + this.gameObject.name + " can't play in editor mode, you haven't set its Sfx.");
                return;
            }

            float volume = Random.Range(MinVolume, MaxVolume);
            float pitch = Random.Range(MinPitch, MaxPitch);
            GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
            temporaryAudioHost.transform.position = this.transform.position;
            _editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
            PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, 0);
            float length = 1000 * tmpAudioClip.length;
            await Task.Delay((int)length);
            DestroyImmediate(temporaryAudioHost);
        }
        protected virtual void TestStopSound()
        {
            if (_editorAudioSource != null)
            {
                _editorAudioSource.Stop();
            }            
        }
    }
}
