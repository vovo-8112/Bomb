using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [Serializable]
    public class SoundSettings
    {
        public bool MusicOn = true;
        public bool SfxOn = true;
    }
    [System.Obsolete("This SoundManager is now obsolete, and has been replaced by the bigger, better, faster MMSoundManager. It will be removed definitely in an upcoming update. You should remove this one from this scene, and add a MMSoundManager in its place.")]
    public class SoundManager : MMPersistentSingleton<SoundManager>, MMEventListener<TopDownEngineEvent>, MMEventListener<MMGameEvent>
    {
        [Header("Settings")]
        [Tooltip("the current sound settings ")]
        public SoundSettings Settings;

        [Header("Music")]
        [Range(0, 1)]
        [Tooltip("the music volume")]
        public float MusicVolume = 0.3f;

        [Header("Sound Effects")]
        [Range(0, 1)]
        [Tooltip("the sound fx volume")]
        public float SfxVolume = 1f;

        [Header("Pause")]
        [Tooltip("whether or not Sfx should be muted when the game is paused")]
        public bool MuteSfxOnPause = true;

        public bool IsMusicOn { get { return Settings.MusicOn; } internal set { Settings.MusicOn = value; } }
        public bool IsSfxOn { get { return Settings.SfxOn; } internal set { Settings.SfxOn = value; } }

        protected const string _saveFolderName = "TopDownEngine/";
        protected const string _saveFileName = "sound.settings";
        protected AudioSource _backgroundMusic;
        protected List<AudioSource> _loopingSounds = new List<AudioSource>();
        public virtual void PlayBackgroundMusic(AudioSource musicAudioSource, bool loop = true)
        {
            if (_backgroundMusic != null)
            {
                _backgroundMusic.Stop();
            }
            _backgroundMusic = musicAudioSource;
            _backgroundMusic.volume = MusicVolume;
            _backgroundMusic.loop = loop;
            if (!Settings.MusicOn)
            {
                return;
            }
            _backgroundMusic.Play();
        }
        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, bool loop = false)
        {
            if (!Settings.SfxOn)
                return null;
            GameObject temporaryAudioHost = new GameObject("TempAudio");
            temporaryAudioHost.transform.position = location;
            AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
            audioSource.clip = sfx;
            audioSource.volume = SfxVolume;
            audioSource.loop = loop;
            audioSource.Play();

            if (!loop)
            {
                Destroy(temporaryAudioHost, sfx.length);
            }
            else
            {
                _loopingSounds.Add(audioSource);
            }
            return audioSource;
        }
        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, float pitch, float pan, float spatialBlend = 0.0f, float volumeMultiplier = 1.0f, bool loop = false,
            AudioSource reuseSource = null, AudioMixerGroup audioGroup = null)
        {
            if (!Settings.SfxOn || !sfx)
            {
                return null;
            }

            var audioSource = reuseSource;
            GameObject temporaryAudioHost = null;

            if (audioSource == null)
            {
                temporaryAudioHost = new GameObject("TempAudio");
                var newAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
                audioSource = newAudioSource;
            }
            audioSource.transform.position = location;

            audioSource.time = 0.0f;
            audioSource.clip = sfx;

            audioSource.pitch = pitch;
            audioSource.spatialBlend = spatialBlend;
            audioSource.panStereo = pan;
            audioSource.volume = SfxVolume * volumeMultiplier;
            audioSource.loop = loop;
            if (audioGroup)
                audioSource.outputAudioMixerGroup = audioGroup;
            audioSource.Play();

            if (!loop && !reuseSource)
            {
                Destroy(temporaryAudioHost, sfx.length);
            }

            if (loop)
            {
                _loopingSounds.Add(audioSource);
            }
            return audioSource;
        }
        public virtual void StopLoopingSound(AudioSource source)
        {
            if (source != null)
            {
                _loopingSounds.Remove(source);
                Destroy(source.gameObject);
            }
        }
		protected virtual void SetMusic(bool status)
        {
            Settings.MusicOn = status;
            SaveSoundSettings();
            if (status)
            {
                UnmuteBackgroundMusic();
            }
            else
            {
                MuteBackgroundMusic();
            }
        }
		protected virtual void SetSfx(bool status)
        {
            Settings.SfxOn = status;
            SaveSoundSettings();
        }
		public virtual void MusicOn() { SetMusic(true); }
		public virtual void MusicOff() { SetMusic(false); }
		public virtual void SfxOn() { SetSfx(true); }
		public virtual void SfxOff() { SetSfx(false); }
		protected virtual void SaveSoundSettings()
        {
            MMSaveLoadManager.Save(Settings, _saveFileName, _saveFolderName);
        }
		protected virtual void LoadSoundSettings()
        {
            SoundSettings settings = (SoundSettings)MMSaveLoadManager.Load(typeof(SoundSettings), _saveFileName, _saveFolderName);
            if (settings != null)
            {
                Settings = settings;
            }
        }
		protected virtual void ResetSoundSettings()
        {
            MMSaveLoadManager.DeleteSave(_saveFileName, _saveFolderName);
        }
        public virtual void StopAllLoopingSounds()
        {
            foreach (AudioSource source in _loopingSounds)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
        }
        protected virtual void MuteAllSfx()
        {
            foreach (AudioSource source in _loopingSounds)
            {
                if (source != null)
                {
                    source.mute = true;
                }
            }
        }
		protected virtual void UnmuteAllSfx()
        {
            foreach (AudioSource source in _loopingSounds)
            {
                if (source != null)
                {
                    source.mute = false;
                }
            }
        }
        public virtual void UnmuteBackgroundMusic()
        {
            if (_backgroundMusic != null)
            {
                _backgroundMusic.mute = false;
            }
        }
        public virtual void MuteBackgroundMusic()
        {
            if (_backgroundMusic != null)
            {
                _backgroundMusic.mute = true;
            }
        }

        public bool IsBackgroundMusicInScene()
        {
            return _backgroundMusic != null;
        }

        public bool IsBackgroundMusicPlaying()
        {
            return _backgroundMusic != null && _backgroundMusic.isPlaying;
        }

        public virtual void PauseBackgroundMusic()
        {
            if (_backgroundMusic != null)
                _backgroundMusic.Pause();
        }

        public virtual void ResumeBackgroundMusic()
        {
            if (_backgroundMusic != null)
                _backgroundMusic.Play();
        }

        public virtual void StopBackgroundMusic()
        {
            if (_backgroundMusic != null)
            {
                _backgroundMusic.Stop();
                _backgroundMusic = null;
            }
        }
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            if (engineEvent.EventType == TopDownEngineEventTypes.Pause)
            {
                if (MuteSfxOnPause)
                {
                    MuteAllSfx();
                }
            }
            if (engineEvent.EventType == TopDownEngineEventTypes.UnPause)
            {
                if (MuteSfxOnPause)
                {
                    UnmuteAllSfx();
                }
            }
        }
        public virtual void OnMMSfxEvent(AudioClip clipToPlay, AudioMixerGroup audioGroup = null, float volume = 1f, float pitch = 1f)
        {
            PlaySound(clipToPlay, this.transform.position, pitch, 0.0f, 0.0f, volume, false, audioGroup: audioGroup);
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (MuteSfxOnPause)
            {
                if (gameEvent.EventName == "inventoryOpens")
                {
                    MuteAllSfx();
                }
                if (gameEvent.EventName == "inventoryCloses")
                {
                    UnmuteAllSfx();
                }
            }
        }
        protected virtual void OnEnable()
        {
            MMSfxEvent.Register(OnMMSfxEvent);
            this.MMEventStartListening<TopDownEngineEvent>();
            this.MMEventStartListening<MMGameEvent>();
            LoadSoundSettings();
            _loopingSounds = new List<AudioSource>();
        }
		protected virtual void OnDisable()
        {
            if (_enabled)
            {
                MMSfxEvent.Unregister(OnMMSfxEvent);
                this.MMEventStopListening<TopDownEngineEvent>();
                this.MMEventStopListening<MMGameEvent>();
            }
        }
    }
}