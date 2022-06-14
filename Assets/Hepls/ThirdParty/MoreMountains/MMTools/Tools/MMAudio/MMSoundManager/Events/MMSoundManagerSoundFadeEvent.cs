using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public struct MMSoundManagerSoundFadeEvent
    {
        public int SoundID;
        public float FadeDuration;
        public float FinalVolume;
        public MMTweenType FadeTween;
        
        public MMSoundManagerSoundFadeEvent(int soundID, float fadeDuration, float finalVolume, MMTweenType fadeTween)
        {
            SoundID = soundID;
            FadeDuration = fadeDuration;
            FinalVolume = finalVolume;
            FadeTween = fadeTween;
        }

        static MMSoundManagerSoundFadeEvent e;
        public static void Trigger(int soundID, float fadeDuration, float finalVolume, MMTweenType fadeTween)
        {
            e.SoundID = soundID;
            e.FadeDuration = fadeDuration;
            e.FinalVolume = finalVolume;
            e.FadeTween = fadeTween;
            MMEventManager.TriggerEvent(e);
        }
    }
}

