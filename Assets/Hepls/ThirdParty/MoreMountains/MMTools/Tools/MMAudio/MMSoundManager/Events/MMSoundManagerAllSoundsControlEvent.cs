using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public enum MMSoundManagerAllSoundsControlEventTypes
    {
        Pause, Play, Stop, Free, FreeAllButPersistent, FreeAllLooping
    }
    public struct MMSoundManagerAllSoundsControlEvent
    {
        public MMSoundManagerAllSoundsControlEventTypes EventType;
        
        public MMSoundManagerAllSoundsControlEvent(MMSoundManagerAllSoundsControlEventTypes eventType)
        {
            EventType = eventType;
        }

        static MMSoundManagerAllSoundsControlEvent e;
        public static void Trigger(MMSoundManagerAllSoundsControlEventTypes eventType)
        {
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
}

