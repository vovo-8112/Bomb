using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMSoundSequencer")]
    public class MMSoundSequencer : MMSequencer
    {
        [Tooltip("the list of audio clips to play (one per track)")]
        public List<AudioClip> Sounds;

        protected List<AudioSource> _audioSources;
        protected override void Initialization()
        {
            base.Initialization();
            _audioSources = new List<AudioSource>();
            foreach(AudioClip sound in Sounds)
            {
                GameObject asGO = new GameObject();
                asGO.name = "AudioSource - " + sound.name;
                asGO.transform.SetParent(this.transform);
                AudioSource source = asGO.AddComponent<AudioSource>();
                source.loop = false;
                source.playOnAwake = false;
                source.clip = sound;
                source.volume = 1f;
                source.pitch = 1f;
                _audioSources.Add(source);
            }
        }
        protected override void OnBeat()
        {
            base.OnBeat();
            for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
            {
                if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
                {
                    if ((_audioSources.Count > i) && (_audioSources[i] != null))
                    {
                        _audioSources[i].Play();
                    }
                }
            }
        }
        public override void PlayTrackEvent(int index)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            base.PlayTrackEvent(index);            
            _audioSources[index].Play();
        }
        public override void EditorMaintenance()
        {
            base.EditorMaintenance();
            SetupSounds();
        }
        public virtual void SetupSounds()
        {
            if (Sequence == null)
            {
                return;
            }
            if (Sounds.Count < Sequence.SequenceTracks.Count)
            {
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    if (i >= Sounds.Count)
                    {
                        Sounds.Add(null);
                    }
                }
            }
            if (Sounds.Count > Sequence.SequenceTracks.Count)
            {
                Sounds.Clear();
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    Sounds.Add(null);
                }
            }
        }
    }    
}
