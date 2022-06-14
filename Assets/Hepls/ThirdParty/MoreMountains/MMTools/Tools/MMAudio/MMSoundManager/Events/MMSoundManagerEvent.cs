using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public enum MMSoundManagerEventTypes
    {
        SaveSettings,
        LoadSettings,
        ResetSettings
    }
    public struct MMSoundManagerEvent
    {
        public MMSoundManagerEventTypes EventType;
        
        public MMSoundManagerEvent(MMSoundManagerEventTypes eventType)
        {
            EventType = eventType;
        }

        static MMSoundManagerEvent e;
        public static void Trigger(MMSoundManagerEventTypes eventType)
        {
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
}
