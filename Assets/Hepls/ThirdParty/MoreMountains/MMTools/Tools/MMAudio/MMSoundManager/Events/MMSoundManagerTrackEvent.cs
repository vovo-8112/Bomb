using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public enum MMSoundManagerTrackEventTypes
    {
        MuteTrack,
        UnmuteTrack,
        SetVolumeTrack,
        PlayTrack,
        PauseTrack,
        StopTrack,
        FreeTrack
    }
    public struct MMSoundManagerTrackEvent
    {
        public MMSoundManagerTrackEventTypes TrackEventType;
        public MMSoundManager.MMSoundManagerTracks Track;
        public float Volume;
        
        public MMSoundManagerTrackEvent(MMSoundManagerTrackEventTypes trackEventType, MMSoundManager.MMSoundManagerTracks track = MMSoundManager.MMSoundManagerTracks.Master, float volume = 1f)
        {
            TrackEventType = trackEventType;
            Track = track;
            Volume = volume;
        }

        static MMSoundManagerTrackEvent e;
        public static void Trigger(MMSoundManagerTrackEventTypes trackEventType, MMSoundManager.MMSoundManagerTracks track = MMSoundManager.MMSoundManagerTracks.Master, float volume = 1f)
        {
            e.TrackEventType = trackEventType;
            e.Track = track;
            e.Volume = volume;
            MMEventManager.TriggerEvent(e);
        }
    }
}

