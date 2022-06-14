using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMAudioSourceSequencer")]
    public class MMAudioSourceSequencer : MMSequencer
    {
        [Tooltip("the list of audio sources to play (one per track)")]
        public List<AudioSource> AudioSources;
        protected override void OnBeat()
        {
            base.OnBeat();
            for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
            {
                if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
                {
                    if ((AudioSources.Count > i) && (AudioSources[i] != null))
                    {
                        AudioSources[i].Play();
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
            AudioSources[index].Play();
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
            if (AudioSources.Count < Sequence.SequenceTracks.Count)
            {
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    if (i >= AudioSources.Count)
                    {
                        AudioSources.Add(null);
                    }
                }
            }
            if (AudioSources.Count > Sequence.SequenceTracks.Count)
            {
                AudioSources.Clear();
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    AudioSources.Add(null);
                }
            }
        }
    }    
}
