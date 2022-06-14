using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Audio/MMPlaylistRemote")]
    public class MMPlaylistRemote : MonoBehaviour
    {
        public int TrackNumber = 0;

        [Header("Triggers")]
        public bool PlaySelectedTrackOnTriggerEnter = true;
        public bool PlaySelectedTrackOnTriggerExit = false;
        public string TriggerTag = "Player";

        [Header("Test")]
        [MMInspectorButton("Play")]
        public bool PlayButton;
        [MMInspectorButton("Pause")]
        public bool PauseButton;
        [MMInspectorButton("Stop")]
        public bool StopButton;
        [MMInspectorButton("PlayNextTrack")]
        public bool NextButton;
        [MMInspectorButton("PlaySelectedTrack")]
        public bool SelectedTrackButton;
        public virtual void Play()
        {
            MMPlaylistPlayEvent.Trigger();
        }
        public virtual void Pause()
        {
            MMPlaylistPauseEvent.Trigger();
        }
        public virtual void Stop()
        {
            MMPlaylistStopEvent.Trigger();
        }
        public virtual void PlayNextTrack()
        {
            MMPlaylistPlayNextEvent.Trigger();
        }
        public virtual void PlaySelectedTrack()
        {
            MMPlaylistPlayIndexEvent.Trigger(TrackNumber);
        }
        public virtual void PlayTrack(int trackIndex)
        {
            MMPlaylistPlayIndexEvent.Trigger(trackIndex);
        }
        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (PlaySelectedTrackOnTriggerEnter && (collider.CompareTag(TriggerTag)))
            {
                PlaySelectedTrack();
            }
        }
        protected virtual void OnTriggerExit(Collider collider)
        {
            if (PlaySelectedTrackOnTriggerExit && (collider.CompareTag(TriggerTag)))
            {
                PlaySelectedTrack();
            }
        }
    }
}
