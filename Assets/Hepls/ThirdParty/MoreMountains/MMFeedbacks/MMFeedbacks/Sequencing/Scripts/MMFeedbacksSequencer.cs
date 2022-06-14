using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMFeedbacksSequencer")]
    public class MMFeedbacksSequencer : MMSequencer
    {
        [Tooltip("the list of audio clips to play (one per track)")]
        public List<MMFeedbacks> Feedbacks;
        protected override void OnBeat()
        {
            base.OnBeat();
            for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
            {
                if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
                {
                    if ((Feedbacks.Count > i) && (Feedbacks[i] != null))
                    {
                        Feedbacks[i].PlayFeedbacks();
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
            Feedbacks[index].PlayFeedbacks();
        }
        public override void EditorMaintenance()
        {
            base.EditorMaintenance();
            SetupFeedbacks();
        }
        public virtual void SetupFeedbacks()
        {
            if (Sequence == null)
            {
                return;
            }
            if (Feedbacks.Count < Sequence.SequenceTracks.Count)
            {
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    if (i >= Feedbacks.Count)
                    {
                        Feedbacks.Add(null);
                    }
                }
            }
            if (Feedbacks.Count > Sequence.SequenceTracks.Count)
            {
                Feedbacks.Clear();
                for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
                {
                    Feedbacks.Add(null);
                }
            }
        }
    }
}
