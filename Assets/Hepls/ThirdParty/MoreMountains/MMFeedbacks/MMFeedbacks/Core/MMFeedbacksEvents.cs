using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace  MoreMountains.Feedbacks
{
    public struct MMFeedbacksEvent
    {
        public enum EventTypes { Play, Pause, Resume, Revert, Complete }
        
        public delegate void Delegate(MMFeedbacks source, EventTypes type);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(MMFeedbacks source, EventTypes type)
        {
            OnEvent?.Invoke(source, type);
        }
    }
    [Serializable]
    public class MMFeedbacksEvents
    {
        [Tooltip("whether or not this MMFeedbacks should fire MMFeedbacksEvents")] 
        public bool TriggerMMFeedbacksEvents = false;
        [Tooltip("whether or not this MMFeedbacks should fire Unity Events")] 
        public bool TriggerUnityEvents = true;
        [Tooltip("This event will fire every time this MMFeedbacks gets played")]
        public UnityEvent OnPlay;
        [Tooltip("This event will fire every time this MMFeedbacks starts a holding pause")]
        public UnityEvent OnPause;
        [Tooltip("This event will fire every time this MMFeedbacks resumes after a holding pause")]
        public UnityEvent OnResume;
        [Tooltip("This event will fire every time this MMFeedbacks reverts its play direction")]
        public UnityEvent OnRevert;
        [Tooltip("This event will fire every time this MMFeedbacks plays its last MMFeedback")]
        public UnityEvent OnComplete;

        public bool OnPlayIsNull { get; protected set; }
        public bool OnPauseIsNull { get; protected set; }
        public bool OnResumeIsNull { get; protected set; }
        public bool OnRevertIsNull { get; protected set; }
        public bool OnCompleteIsNull { get; protected set; }
        public virtual void Initialization()
        {
            OnPlayIsNull = OnPlay == null;
            OnPauseIsNull = OnPause == null;
            OnResumeIsNull = OnResume == null;
            OnRevertIsNull = OnRevert == null;
            OnCompleteIsNull = OnComplete == null;
        }
        public virtual void TriggerOnPlay(MMFeedbacks source)
        {
            if (!OnPlayIsNull && TriggerUnityEvents)
            {
                OnPlay.Invoke();
            }

            if (TriggerMMFeedbacksEvents)
            {
                MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Play);
            }
        }
        public virtual void TriggerOnPause(MMFeedbacks source)
        {
            if (!OnPauseIsNull && TriggerUnityEvents)
            {
                OnPause.Invoke();
            }

            if (TriggerMMFeedbacksEvents)
            {
                MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Pause);
            }
        }
        public virtual void TriggerOnResume(MMFeedbacks source)
        {
            if (!OnResumeIsNull && TriggerUnityEvents)
            {
                OnResume.Invoke();
            }

            if (TriggerMMFeedbacksEvents)
            {
                MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Resume);
            }
        }
        public virtual void TriggerOnRevert(MMFeedbacks source)
        {
            if (!OnRevertIsNull && TriggerUnityEvents)
            {
                OnRevert.Invoke();
            }

            if (TriggerMMFeedbacksEvents)
            {
                MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Revert);
            }
        }
        public virtual void TriggerOnComplete(MMFeedbacks source)
        {
            if (!OnCompleteIsNull && TriggerUnityEvents)
            {
                OnComplete.Invoke();
            }

            if (TriggerMMFeedbacksEvents)
            {
                MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Complete);
            }
        }
    }
   
}
