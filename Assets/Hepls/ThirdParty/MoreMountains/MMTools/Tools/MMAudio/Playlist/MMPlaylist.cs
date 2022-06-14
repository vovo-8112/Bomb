using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{

    public struct MMPlaylistPlayEvent
    {
        public delegate void Delegate();
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger()
        {
            OnEvent?.Invoke();
        }
    }
    public struct MMPlaylistStopEvent
    {
        public delegate void Delegate();
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger()
        {
            OnEvent?.Invoke();
        }
    }
    public struct MMPlaylistPauseEvent
    {
        public delegate void Delegate();
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger()
        {
            OnEvent?.Invoke();
        }
    }
    public struct MMPlaylistPlayNextEvent
    {
        public delegate void Delegate();
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger()
        {
            OnEvent?.Invoke();
        }
    }

    public struct MMPlaylistPlayIndexEvent
    {
        public delegate void Delegate(int index);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(int index)
        {
            OnEvent?.Invoke(index);
        }
    }

    [System.Serializable]
    public class MMPlaylistSong
    {
        public AudioSource TargetAudioSource;
        [MMVector("Min", "Max")]
        public Vector2 Volume = new Vector2(0f, 1f);
        [MMVector("RMin", "RMax")]
        public Vector2 InitialDelay = Vector2.zero;
        [MMVector("RMin", "RMax")]
        public Vector2 CrossFadeDuration = new Vector2(2f, 2f);
        [MMVector("RMin", "RMax")]
        public Vector2 Pitch = Vector2.one;
        [Range(-1f, 1f)]
        public float StereoPan = 0f;
        [Range(0f, 1f)]
        public float SpatialBlend = 0f;
        public bool Loop = false;
        [MMReadOnly]
        public bool Playing = false;
        [MMReadOnly]
        public bool Fading = false;

        public virtual void Initialization()
        {
            this.Volume = new Vector2(0f, 1f);
            this.InitialDelay = Vector2.zero;
            this.CrossFadeDuration = Vector2.one;
            this.Pitch = Vector2.one;
            this.StereoPan = 0f;
            this.SpatialBlend = 0f;
            this.Loop = false;
        }
    }
    [AddComponentMenu("More Mountains/Tools/Audio/MMPlaylist")]
    public class MMPlaylist : MonoBehaviour
    {
        public enum PlaylistStates
        {
            Idle,
            Playing,
            Paused
        }

        [Header("Playlist Songs")]
        public List<MMPlaylistSong> Songs;

        [Header("Settings")]
        public bool RandomOrder = false;
        public bool Endless = true;
        public bool PlayOnStart = true;

        [Header("Status")]
        [MMReadOnly]
        public MMStateMachine<MMPlaylist.PlaylistStates> PlaylistState;
        [MMReadOnly]
        public int CurrentlyPlayingIndex = -1;
        [MMReadOnly]
        public string CurrentTrackName;

        [Header("Test")]
        [MMInspectorButton("Play")]
        public bool PlayButton;
        [MMInspectorButton("Pause")]
        public bool PauseButton;
        [MMInspectorButton("Stop")]
        public bool StopButton;
        [MMInspectorButton("PlayNextTrack")]
        public bool NextButton;

        protected int _songsPlayedSoFar = 0;
        protected int _songsPlayedThisCycle = 0;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _songsPlayedSoFar = 0;
            PlaylistState = new MMStateMachine<MMPlaylist.PlaylistStates>(this.gameObject, true);
            PlaylistState.ChangeState(PlaylistStates.Idle);
            if (Songs.Count == 0)
            {
                return;
            }
            if (PlayOnStart)
            {
                PlayFirstSong();
            }
        }
        protected virtual void PlayFirstSong()
        {
            _songsPlayedThisCycle = 0;
            CurrentlyPlayingIndex = -1;
            int newIndex = PickNextIndex();
            StartCoroutine(PlayTrack(newIndex));
        }
        protected virtual IEnumerator PlayTrack(int index)
        {
            if (Songs.Count == 0)
            {
                yield break;
            }
            if (!Endless && (_songsPlayedThisCycle > Songs.Count))
            {
                yield break;
            }
            if (PlaylistState.CurrentState == PlaylistStates.Playing)
            {
                StartCoroutine(Fade(CurrentlyPlayingIndex,
                     Random.Range(Songs[index].CrossFadeDuration.x, Songs[index].CrossFadeDuration.y),
                     Songs[CurrentlyPlayingIndex].Volume.y,
                     Songs[CurrentlyPlayingIndex].Volume.x,
                     true));
            }
            if (CurrentlyPlayingIndex >= 0)
            {
                foreach (MMPlaylistSong song in Songs)
                {
                    if (song != Songs[CurrentlyPlayingIndex])
                    {
                        song.Fading = false;
                    }
                }
            }
            yield return MMCoroutine.WaitFor(Random.Range(Songs[index].InitialDelay.x, Songs[index].InitialDelay.y));

            if (Songs[index].TargetAudioSource == null)
            {
                Debug.LogError(this.name + " : the playlist song you're trying to play is null");
                yield break;
            }

            Songs[index].TargetAudioSource.pitch = Random.Range(Songs[index].Pitch.x, Songs[index].Pitch.y);
            Songs[index].TargetAudioSource.panStereo = Songs[index].StereoPan;
            Songs[index].TargetAudioSource.spatialBlend = Songs[index].SpatialBlend;
            Songs[index].TargetAudioSource.loop = Songs[index].Loop;
            StartCoroutine(Fade(index,
                     Random.Range(Songs[index].CrossFadeDuration.x, Songs[index].CrossFadeDuration.y),
                     Songs[index].Volume.x,
                     Songs[index].Volume.y,
                     false));
            Songs[index].TargetAudioSource.Play();
            CurrentTrackName = Songs[index].TargetAudioSource.clip.name;
            PlaylistState.ChangeState(PlaylistStates.Playing);
            Songs[index].Playing = true;
            CurrentlyPlayingIndex = index;
            _songsPlayedSoFar++;
            _songsPlayedThisCycle++;
        }
        protected virtual IEnumerator Fade(int index, float duration, float initialVolume, float endVolume, bool stopAtTheEnd)
        {
            float startTimestamp = Time.time;
            float progress = 0f;
            Songs[index].Fading = true;

            while ((Time.time - startTimestamp < duration) && (Songs[index].Fading))
            {
                progress = MMMaths.Remap(Time.time - startTimestamp, 0f, duration, 0f, 1f);
                Songs[index].TargetAudioSource.volume = Mathf.Lerp(initialVolume, endVolume, progress);
                yield return null;
            }

            Songs[index].TargetAudioSource.volume = endVolume;

            if (stopAtTheEnd)
            {
                Songs[index].TargetAudioSource.Stop();
                Songs[index].Playing = false;
                Songs[index].Fading = false;
            }
        }
        protected virtual int PickNextIndex()
        {
            if (Songs.Count == 0)
            {
                return -1;
            }

            int newIndex = CurrentlyPlayingIndex;
            if (RandomOrder)
            {
                while (newIndex == CurrentlyPlayingIndex)
                {
                    newIndex = Random.Range(0, Songs.Count);
                }                
            }
            else
            {
                newIndex = (CurrentlyPlayingIndex + 1) % Songs.Count;
            }

            return newIndex;
        }
        public virtual void Play()
        {
            switch (PlaylistState.CurrentState)
            {
                case PlaylistStates.Idle:
                    PlayFirstSong();
                    break;

                case PlaylistStates.Paused:
                    Songs[CurrentlyPlayingIndex].TargetAudioSource.UnPause();
                    PlaylistState.ChangeState(PlaylistStates.Playing);
                    break;

                case PlaylistStates.Playing:
                    break;
            }
        }
        public virtual void Pause()
        {
            Songs[CurrentlyPlayingIndex].TargetAudioSource.Pause();
            PlaylistState.ChangeState(PlaylistStates.Paused);
        }
        public virtual void Stop()
        {
            Songs[CurrentlyPlayingIndex].TargetAudioSource.Stop();
            Songs[CurrentlyPlayingIndex].Playing = false;
            Songs[CurrentlyPlayingIndex].Fading = false;
            CurrentlyPlayingIndex = -1;
            PlaylistState.ChangeState(PlaylistStates.Idle);
        }
        public virtual void PlayNextTrack()
        {
            int newIndex = PickNextIndex();
            StartCoroutine(PlayTrack(newIndex));
        }

        protected virtual void OnPlayEvent()
        {
            Play();
        }

        protected virtual void OnPauseEvent()
        {
            Pause();
        }

        protected virtual void OnStopEvent()
        {
            Stop();
        }

        protected virtual void OnPlayNextEvent()
        {
            PlayNextTrack();
        }

        protected virtual void OnPlayIndexEvent(int index)
        {
            StartCoroutine(PlayTrack(index));
        }
		protected virtual void OnEnable()
        {
            MMPlaylistPauseEvent.Register(OnPauseEvent);
            MMPlaylistPlayEvent.Register(OnPlayEvent);
            MMPlaylistPlayNextEvent.Register(OnPlayNextEvent);
            MMPlaylistStopEvent.Register(OnStopEvent);
            MMPlaylistPlayIndexEvent.Register(OnPlayIndexEvent);
        }
        protected virtual void OnDisable()
        {
            MMPlaylistPauseEvent.Unregister(OnPauseEvent);
            MMPlaylistPlayEvent.Unregister(OnPlayEvent);
            MMPlaylistPlayNextEvent.Unregister(OnPlayNextEvent);
            MMPlaylistStopEvent.Unregister(OnStopEvent);
            MMPlaylistPlayIndexEvent.Unregister(OnPlayIndexEvent);
        }
        
        protected bool _firstDeserialization = true;
        protected int _listCount = 0;
        protected virtual void OnValidate()
        {
            if (_firstDeserialization)
            {
                if (Songs == null)
                {
                    _listCount = 0;
                    _firstDeserialization = false;
                }
                else
                {
                    _listCount = Songs.Count;
                    _firstDeserialization = false;
                }                
            }
            else
            {
                if (Songs.Count != _listCount)
                {
                    if (Songs.Count > _listCount)
                    {
                        foreach(MMPlaylistSong song in Songs)
                        {
                            song.Initialization();
                        }                            
                    }
                    _listCount = Songs.Count;
                }
            }
        }
    }
}
