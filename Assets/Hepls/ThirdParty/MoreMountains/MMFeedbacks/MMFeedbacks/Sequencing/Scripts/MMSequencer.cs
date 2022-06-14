using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMSequencer")]
    public class MMSequencer : MonoBehaviour
    {
        [Header("Sequence")]
        [Tooltip("the sequence to design on or to play")]
        public MMSequence Sequence;
        [Tooltip("the intended BPM for playback and design")]
        public int BPM = 160;
        [Tooltip("the number of notes in the sequence")]
        public int SequencerLength = 8;

        [Header("Playback")]
        [Tooltip("whether the sequence should loop or not when played back")]
        public bool Loop = true;
        [Tooltip("if this is true the sequence will play in random order")]
        public bool RandomSequence = false;
        [Tooltip("whether that sequencer should start playing on application start")]
        public bool PlayOnStart = false;
        
        [Header("Metronome")]
        [Tooltip("a sound to play every beat")]
        public AudioClip MetronomeSound;
        [Tooltip("the volume of the metronome sound")]
        [Range(0f, 1f)]
        public float MetronomeVolume = 0.2f;

        [Header("Events")]
        [Tooltip("a list of events to play every time an active beat is found on each track (one event per track)")]
        public List<UnityEvent> TrackEvents;

        [Header("Monitor")]
        [Tooltip("true if the sequencer is playing right now")]
        [MMFReadOnly]
        public bool Playing = false;
        [Tooltip("true if the sequencer has been played once")]
        [HideInInspector]
        public bool PlayedOnce = false;
        [Tooltip("true if a perfect beat was found this frame")]
        [MMFReadOnly]
        public bool BeatThisFrame = false;
        [Tooltip("the index of the last played bit (our position in the playing sequence)")]
        [MMFReadOnly]
        public int LastBeatIndex = 0;

        [HideInInspector]
        public int LastBPM = -1;
        [HideInInspector]
        public int LastTracksCount = -1;
        [HideInInspector]
        public int LastSequencerLength = -1;
        [HideInInspector]
        public MMSequence LastSequence;
        [HideInInspector]
        public int CurrentSequenceIndex = 0;
        [HideInInspector]
        public float LastBeatTimestamp = 0f;

        protected float _beatInterval;
        protected AudioSource _beatSoundAudiosource;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            Playing = false;
            if (MetronomeSound != null)
            {
                GameObject go = new GameObject();
                go.name = "BeatSoundAudioSource";
                go.transform.SetParent(this.transform);
                _beatSoundAudiosource = go.AddComponent<AudioSource>();
                _beatSoundAudiosource.clip = MetronomeSound;
                _beatSoundAudiosource.loop = false;
                _beatSoundAudiosource.playOnAwake = false;                
            }
            if (PlayOnStart)
            {
                PlaySequence();
            }
        }
        public virtual void ToggleSequence()
        {
            if (Playing)
            {
                StopSequence();
            }
            else
            {
                PlaySequence();
            }
        }
        public virtual void PlaySequence()
        {
            CurrentSequenceIndex = 0;
            Playing = true;
            LastBeatTimestamp = 0f;            
        }
        public virtual void StopSequence()
        {
            Playing = false;
        }
        public virtual void ClearSequence()
        {
            for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
            {
                for (int i = 0; i < SequencerLength; i++)
                {
                    Sequence.QuantizedSequence[trackIndex].Line[i].ID = -1;
                }
            }
        }
        protected virtual void Update()
        {
            HandleBeat();
        }
        protected virtual void HandleBeat()
        {
            BeatThisFrame = false;

            if (!Playing)
            {
                return;
            }

            if (CurrentSequenceIndex >= SequencerLength)
            {
                StopSequence();
                return;
            }

            _beatInterval = 60f / BPM;

            if ((Time.time - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
            {
                PlayBeat();
            }
        }
        public virtual void PlayBeat()
        {
            BeatThisFrame = true;
            LastBeatIndex = CurrentSequenceIndex;
            LastBeatTimestamp = Time.time;
            PlayedOnce = true;
            PlayMetronomeSound();
            OnBeat();

            for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
            {
                if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
                {
                    if (TrackEvents[i] != null)
                    {
                        TrackEvents[i].Invoke();
                    }
                }
            }
            CurrentSequenceIndex++;
            if ((CurrentSequenceIndex >= SequencerLength) && Loop)
            {
                CurrentSequenceIndex = 0;
            }
            if (RandomSequence)
            {
                CurrentSequenceIndex = UnityEngine.Random.Range(0, SequencerLength);
            }
        }
        protected virtual void OnBeat()
        {

        }
        public virtual void PlayTrackEvent(int index)
        {
            TrackEvents[index].Invoke();
        }
        public virtual void ToggleActive(int trackIndex)
        {
            Sequence.SequenceTracks[trackIndex].Active = !Sequence.SequenceTracks[trackIndex].Active;
        }
        public virtual void ToggleStep(int stepIndex)
        {
            bool active = (Sequence.QuantizedSequence[0].Line[stepIndex].ID != -1);

            for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
            {
                if (active)
                {
                    Sequence.QuantizedSequence[trackIndex].Line[stepIndex].ID = -1;
                }
                else
                {
                    Sequence.QuantizedSequence[trackIndex].Line[stepIndex].ID = Sequence.SequenceTracks[trackIndex].ID;
                }
            }
        }
        protected virtual void PlayMetronomeSound()
        {
            if (MetronomeSound != null)
            {
                _beatSoundAudiosource.volume = MetronomeVolume;
                _beatSoundAudiosource.Play();
            }
        }
        public virtual void IncrementLength()
        {
            if (Sequence == null)
            {
                return;
            }
            float beatDuration = 60f / BPM;
            SequencerLength++;
            Sequence.Length = SequencerLength * beatDuration;
            LastSequencerLength = SequencerLength;

            for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
            {
                MMSequenceNote newNote = new MMSequenceNote();
                newNote.ID = -1;
                newNote.Timestamp = Sequence.QuantizedSequence[trackIndex].Line.Count * beatDuration;
                Sequence.QuantizedSequence[trackIndex].Line.Add(newNote);
            }
        }
        public virtual void DecrementLength()
        {
            if (Sequence == null)
            {
                return;
            }
            float beatDuration = 60f / BPM;
            SequencerLength--;
            Sequence.Length = SequencerLength * beatDuration;
            LastSequencerLength = SequencerLength;

            for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
            {
                int removeIndex = Sequence.QuantizedSequence[trackIndex].Line.Count - 1;
                Sequence.QuantizedSequence[trackIndex].Line.RemoveAt(removeIndex);
            }
        }
        public virtual void UpdateTimestampsToMatchNewBPM()
        {
            if (Sequence == null)
            {
                return;
            }
            float beatDuration = 60f / BPM;

            Sequence.TargetBPM = BPM;
            Sequence.Length = SequencerLength * beatDuration;
            Sequence.EndSilenceDuration = beatDuration;
            Sequence.Quantized = true;

            for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
            {
                for (int i = 0; i < SequencerLength; i++)
                {
                    Sequence.QuantizedSequence[trackIndex].Line[i].Timestamp = i * beatDuration;
                }
            }
            LastBPM = BPM;
        }
        public virtual void ApplySequencerLengthToSequence()
        {
            if (Sequence == null)
            {
                return;
            }

            float beatDuration = 60f / BPM;

            Sequence.TargetBPM = BPM;
            Sequence.Length = SequencerLength * beatDuration;
            Sequence.EndSilenceDuration = beatDuration;
            Sequence.Quantized = true;
            
            if ((LastSequencerLength != SequencerLength) || (LastTracksCount != Sequence.SequenceTracks.Count))
            {
                Sequence.QuantizedSequence = new List<MMSequenceList>();

                for (int trackIndex = 0; trackIndex < Sequence.SequenceTracks.Count; trackIndex++)
                {
                    Sequence.QuantizedSequence.Add(new MMSequenceList());
                    Sequence.QuantizedSequence[trackIndex].Line = new List<MMSequenceNote>();
                    for (int i = 0; i < SequencerLength; i++)
                    {
                        MMSequenceNote note = new MMSequenceNote();
                        note.ID = -1;
                        note.Timestamp = i * beatDuration;
                        Sequence.QuantizedSequence[trackIndex].Line.Add(note);
                    }
                }                
            }
            
            LastTracksCount = Sequence.SequenceTracks.Count;
            LastSequencerLength = SequencerLength;
        }
        public virtual void EditorMaintenance()
        {
            SetupTrackEvents();
        }
        public virtual void SetupTrackEvents()
        {
            if (Sequence == null)
            {
                return;
            }
            if (TrackEvents.Count < Sequence.SequenceTracks.Count)
            {
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    if (i >= TrackEvents.Count)
                    {
                        TrackEvents.Add(new UnityEvent());
                    }
                }
            }
            if (TrackEvents.Count > Sequence.SequenceTracks.Count)
            {
                TrackEvents.Clear();
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    TrackEvents.Add(new UnityEvent());
                }
            }
        }
    }
}
