using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{
    [Serializable]
    public struct MMSoundManagerPlayOptions
    {
        public MMSoundManager.MMSoundManagerTracks MmSoundManagerTrack;
        public Vector3 Location;
        public bool Loop;
        public float Volume;
        public int ID;
        public bool Fade;
        public float FadeInitialVolume;
        public float FadeDuration;
        public MMTweenType FadeTween;
        public bool Persistent;
        public AudioSource RecycleAudioSource;
        public AudioMixerGroup AudioGroup;
        public float Pitch;
        public float PanStereo;
        public float SpatialBlend;
        public bool SoloSingleTrack;
        public bool SoloAllTracks;
        public bool AutoUnSoloOnEnd;
        public bool BypassEffects;
        public bool BypassListenerEffects;
        public bool BypassReverbZones;
        public int Priority;
        public float ReverbZoneMix;
        public float DopplerLevel;
        public int Spread;
        public AudioRolloffMode RolloffMode;
        public float MinDistance;
        public float MaxDistance;
        public static MMSoundManagerPlayOptions Default
        {
            get
            {
                MMSoundManagerPlayOptions defaultOptions = new MMSoundManagerPlayOptions();
                defaultOptions.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
                defaultOptions.Location = Vector3.zero;
                defaultOptions.Loop = false;
                defaultOptions.Volume = 1.0f;
                defaultOptions.ID = 0;
                defaultOptions.Fade = false;
                defaultOptions.FadeInitialVolume = 0f;
                defaultOptions.FadeDuration = 1f;
                defaultOptions.FadeTween = null;
                defaultOptions.Persistent = false;
                defaultOptions.RecycleAudioSource = null;
                defaultOptions.AudioGroup = null;
                defaultOptions.Pitch = 1f;
                defaultOptions.PanStereo = 0f;
                defaultOptions.SpatialBlend = 0.0f;
                defaultOptions.SoloSingleTrack = false;
                defaultOptions.SoloAllTracks = false;
                defaultOptions.AutoUnSoloOnEnd = false;
                defaultOptions.BypassEffects = false;
                defaultOptions.BypassListenerEffects = false;
                defaultOptions.BypassReverbZones = false;
                defaultOptions.Priority = 128;
                defaultOptions.ReverbZoneMix = 1f;
                defaultOptions.DopplerLevel = 1f;
                defaultOptions.Spread = 0;
                defaultOptions.RolloffMode = AudioRolloffMode.Logarithmic;
                defaultOptions.MinDistance = 1f;
                defaultOptions.MaxDistance = 500f;
                return defaultOptions;
            }
        }
    }

}
