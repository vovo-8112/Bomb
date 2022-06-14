using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public enum MMSoundManagerSoundControlEventTypes
    {
        Pause,
        Resume,
        Stop,
        Free
    }
    public struct MMSoundManagerSoundControlEvent
    {
        public int SoundID;
        public MMSoundManagerSoundControlEventTypes MMSoundManagerSoundControlEventType;
        public AudioSource TargetSource;
        
        public MMSoundManagerSoundControlEvent(MMSoundManagerSoundControlEventTypes eventType, int soundID, AudioSource source = null)
        {
            SoundID = soundID;
            TargetSource = source;
            MMSoundManagerSoundControlEventType = eventType;
        }

        static MMSoundManagerSoundControlEvent e;
        public static void Trigger(MMSoundManagerSoundControlEventTypes eventType, int soundID, AudioSource source = null)
        {
            e.SoundID = soundID;
            e.TargetSource = source;
            e.MMSoundManagerSoundControlEventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
}

