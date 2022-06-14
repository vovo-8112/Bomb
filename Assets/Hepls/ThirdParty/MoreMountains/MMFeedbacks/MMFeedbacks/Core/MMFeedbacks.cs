using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/MMFeedbacks")]
    [DisallowMultipleComponent]
    public class MMFeedbacks : MonoBehaviour
    {
        public enum Directions { TopToBottom, BottomToTop }
        public enum SafeModes { Nope, EditorOnly, RuntimeOnly, Full }
        public List<MMFeedback> Feedbacks = new List<MMFeedback>();
        public enum InitializationModes { Script, Awake, Start }
        [Tooltip("the chosen initialization modes. If you use Script, you'll have to initialize manually by calling the " +
                 "Initialization method and passing it an owner. Otherwise, you can have this component initialize " +
                 "itself at Awake or Start, and in this case the owner will be the MMFeedbacks itself")]
        public InitializationModes InitializationMode = InitializationModes.Start;
        [Tooltip("the selected safe mode")]
        public SafeModes SafeMode = SafeModes.Full;
        [Tooltip("the selected direction these feedbacks should play in")]
        public Directions Direction = Directions.TopToBottom;
        [Tooltip("whether or not this MMFeedbacks should invert its direction when all feedbacks have played")]
        public bool AutoChangeDirectionOnEnd = false;
        [Tooltip("whether or not to play this feedbacks automatically on Start")]
        public bool AutoPlayOnStart = false;
        [Tooltip("whether or not to play this feedbacks automatically on Enable")]
        public bool AutoPlayOnEnable = false;
        [Tooltip("a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)")]
        public float DurationMultiplier = 1f;
        [Tooltip("if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot")]
        public bool DisplayFullDurationDetails = false;
        [Tooltip("a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible")]
        public float CooldownDuration = 0f;
        [Tooltip("a duration, in seconds, to delay the start of this MMFeedbacks' contents play")]
        public float InitialDelay = 0f;
        [Tooltip("if this is true, you'll be able to trigger a new Play while this feedback is already playing, otherwise you won't be able to")]
        public bool CanPlayWhileAlreadyPlaying = true;
        [Tooltip("the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect." +
                 "Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.")]
        public float FeedbacksIntensity = 1f;
        [Tooltip("a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks")] 
        public MMFeedbacksEvents Events;
        [Tooltip("a global switch used to turn all feedbacks on or off globally")]
        public static bool GlobalMMFeedbacksActive = true;
        
        [HideInInspector]
        public bool DebugActive = false;
        public bool IsPlaying { get; protected set; }
        public bool InScriptDrivenPause { get; set; }
        public bool ContainsLoop { get; set; }
        public bool ShouldRevertOnNextPlay { get; set; }
        public float TotalDuration
        {
            get
            {
                float total = 0f;
                foreach (MMFeedback feedback in Feedbacks)
                {
                    if ((feedback != null) && (feedback.Active))
                    {
                        if (total < feedback.TotalDuration)
                        {
                            total = feedback.TotalDuration;    
                        }
                    }
                }
                return InitialDelay + total * DurationMultiplier;
            }
        }
        
        protected float _startTime = 0f;
        protected float _holdingMax = 0f;
        protected float _lastStartAt = 0f;
        protected bool _pauseFound = false;
        protected float _totalDuration = 0f;

        #region INITIALIZATION
        protected virtual void Awake()
        {
            if (AutoPlayOnEnable)
            {
                MMFeedbacksEnabler enabler = GetComponent<MMFeedbacksEnabler>(); 
                if (enabler == null)
                {
                    enabler = this.gameObject.AddComponent<MMFeedbacksEnabler>();
                }
                enabler.TargetMMFeedbacks = this;
            }
            
            if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
            {
                Initialization(this.gameObject);
            }
            CheckForLoops();
        }
        protected virtual void Start()
        {
            if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
            {
                Initialization(this.gameObject);
            }
            if (AutoPlayOnStart && Application.isPlaying)
            {
                PlayFeedbacks();
            }
            CheckForLoops();
        }
        protected virtual void OnEnable()
        {
            if (AutoPlayOnEnable && Application.isPlaying)
            {
                PlayFeedbacks();
            }
        }
        public virtual void Initialization()
        {
            Initialization(this.gameObject);
        }
        public virtual void Initialization(GameObject owner)
        {
            if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
            {
                AutoRepair();
            }

            IsPlaying = false;

            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    Feedbacks[i].Initialization(owner);
                }                
            }
        }

        #endregion

        #region PLAY
        public virtual void PlayFeedbacks()
        {
            PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
        }
        public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
        }
        public virtual void PlayFeedbacksInReverse()
        {
            PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
        }
        public virtual void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
        }
        public virtual void PlayFeedbacksOnlyIfReversed()
        {
            
            if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
                 || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
            {
                PlayFeedbacks();
            }
        }
        public virtual void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            
            if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
                 || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
            {
                PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            }
        }
        public virtual void PlayFeedbacksOnlyIfNormalDirection()
        {
            if (Direction == Directions.TopToBottom)
            {
                PlayFeedbacks();
            }
        }
        public virtual void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            if (Direction == Directions.TopToBottom)
            {
                PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            }
        }
        public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            while (IsPlaying)
            {
                yield return null;    
            }
        }

        #endregion

        #region SEQUENCE
        protected virtual void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            if (IsPlaying && !CanPlayWhileAlreadyPlaying)
            {
                return;
            }
            if (CooldownDuration > 0f)
            {
                if (Time.unscaledTime - _lastStartAt < CooldownDuration)
                {
                    return;
                }
            }
            if (!GlobalMMFeedbacksActive)
            {
                return;
            }

            if (!this.gameObject.activeInHierarchy)
            {
                return;
            }
            
            if (ShouldRevertOnNextPlay)
            {
                Revert();
                ShouldRevertOnNextPlay = false;
            }

            if (forceRevert)
            {
                Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
            }
            
            ResetFeedbacks();
            this.enabled = true;
            IsPlaying = true;
            _startTime = Time.unscaledTime;
            _lastStartAt = _startTime;
            _totalDuration = TotalDuration;
            
            if (InitialDelay > 0f)
            {
                StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
            }
            else
            {
                PreparePlay(position, feedbacksIntensity, forceRevert);
            }
        }

        protected virtual void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            Events.TriggerOnPlay(this);

            _holdingMax = 0f;
            _pauseFound = false;
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
                    {
                        _pauseFound = true;
                    }
                    if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
                    {
                        _pauseFound = true;
                    }    
                }
            }

            if (!_pauseFound)
            {
                PlayAllFeedbacks(position, feedbacksIntensity, forceRevert);
            }
            else
            {
                StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
            }
        }

        protected virtual void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (FeedbackCanPlay(Feedbacks[i]))
                {
                    Feedbacks[i].Play(position, feedbacksIntensity);
                }
            }
        }

        protected virtual IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            IsPlaying = true;
            yield return MMFeedbacksCoroutine.WaitFor(InitialDelay);
            PreparePlay(position, feedbacksIntensity, forceRevert);
        }
        
        protected virtual void Update()
        {
            if (IsPlaying)
            {
                if (!_pauseFound)
                {
                    if (Time.unscaledTime - _startTime >= _totalDuration)
                    {
                        IsPlaying = false;
                        Events.TriggerOnComplete(this);
                        ApplyAutoRevert();
                        this.enabled = false;
                    }    
                }
            }
            else
            {
                this.enabled = false;
            }
        }
        protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
        {
            IsPlaying = true;

            int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count-1;

            while ((i >= 0) && (i < Feedbacks.Count))
            {
                if (!IsPlaying)
                {
                    yield break;
                }

                if (Feedbacks[i] == null)
                {
                    yield break;
                }
                
                if (((Feedbacks[i].Active) && (Feedbacks[i].ScriptDrivenPause)) || InScriptDrivenPause)
                {
                    InScriptDrivenPause = true;

                    bool inAutoResume = (Feedbacks[i].ScriptDrivenPauseAutoResume > 0f); 
                    float scriptDrivenPauseStartedAt = Time.unscaledTime;
                    float autoResumeDuration = Feedbacks[i].ScriptDrivenPauseAutoResume;
                    
                    while (InScriptDrivenPause)
                    {
                        if (inAutoResume && (Time.unscaledTime - scriptDrivenPauseStartedAt > autoResumeDuration))
                        {
                            ResumeFeedbacks();
                        }
                        yield return null;
                    } 
                }
                if ((Feedbacks[i].Active)
                    && ((Feedbacks[i].HoldingPause == true) || (Feedbacks[i].LooperPause == true))
                    && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
                {
                    Events.TriggerOnPause(this);
                    while (Time.unscaledTime - _lastStartAt < _holdingMax)
                    {
                        yield return null;
                    }
                    _holdingMax = 0f;
                    _lastStartAt = Time.unscaledTime;
                }
                if (FeedbackCanPlay(Feedbacks[i]))
                {
                    Feedbacks[i].Play(position, feedbacksIntensity);
                }
                if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
                {
                    bool shouldPause = true;
                    if (Feedbacks[i].Chance < 100)
                    {
                        float random = Random.Range(0f, 100f);
                        if (random > Feedbacks[i].Chance)
                        {
                            shouldPause = false;
                        }
                    }

                    if (shouldPause)
                    {
                        yield return Feedbacks[i].Pause;
                        Events.TriggerOnResume(this);
                        _lastStartAt = Time.unscaledTime;
                        _holdingMax = 0f;
                    }
                }
                if (Feedbacks[i].Active)
                {
                    if ((Feedbacks[i].Pause == null) && (Feedbacks[i].ShouldPlayInThisSequenceDirection) && (!Feedbacks[i].Timing.ExcludeFromHoldingPauses))
                    {
                        float feedbackDuration = Feedbacks[i].TotalDuration;
                        _holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
                    }
                }
                if ((Feedbacks[i].LooperPause == true)
                    && (Feedbacks[i].Active)
                    && (Feedbacks[i].ShouldPlayInThisSequenceDirection)
                    && (((Feedbacks[i] as MMFeedbackLooper).NumberOfLoopsLeft > 0) || (Feedbacks[i] as MMFeedbackLooper).InInfiniteLoop))
                {
                    bool loopAtLastPause = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastPause;
                    bool loopAtLastLoopStart = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastLoopStart;

                    int newi = 0;

                    int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

                    while ((j >= 0) && (j <= Feedbacks.Count))
                    {
                        if (j == 0)
                        {
                            newi = j - 1;
                            break;
                        }
                        if (j == Feedbacks.Count)
                        {
                            newi = j ;
                            break;
                        }
                        if ((Feedbacks[j].Pause != null)
                            && (Feedbacks[j].FeedbackDuration > 0f)
                            && loopAtLastPause && (Feedbacks[j].Active))
                        {
                            newi = j;
                            break;
                        }
                        if ((Feedbacks[j].LooperStart == true)
                            && loopAtLastLoopStart
                            && (Feedbacks[j].Active))
                        {
                            newi = j;
                            break;
                        }

                        j += (Direction == Directions.TopToBottom) ? -1 : 1;
                    }
                    i = newi;
                }
                i += (Direction == Directions.TopToBottom) ? 1 : -1;
            }
            float unscaledTimeAtEnd = Time.unscaledTime;
            while (Time.unscaledTime - unscaledTimeAtEnd < _holdingMax)
            {
                yield return null;
            }
            IsPlaying = false;
            Events.TriggerOnComplete(this);
            ApplyAutoRevert();
        }

        #endregion

        #region STOP
        public virtual void StopFeedbacks()
        {
            StopFeedbacks(true);
        }
        public virtual void StopFeedbacks(bool stopAllFeedbacks = true)
        {
            StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
        }
        public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
        {
            if (stopAllFeedbacks)
            {
                for (int i = 0; i < Feedbacks.Count; i++)
                {
                    Feedbacks[i].Stop(position, feedbacksIntensity);
                }    
            }
            IsPlaying = false;
            StopAllCoroutines();
        }
        
        #endregion 

        #region CONTROLS
        public virtual void ResetFeedbacks()
        {
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
                {
                    Feedbacks[i].ResetFeedback();    
                }
            }
            IsPlaying = false;
        }
        public virtual void Revert()
        {
            Events.TriggerOnRevert(this);
            Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
        }
        public virtual void PauseFeedbacks()
        {
            Events.TriggerOnPause(this);
            InScriptDrivenPause = true;
        }
        public virtual void ResumeFeedbacks()
        {
            Events.TriggerOnResume(this);
            InScriptDrivenPause = false;
        }

        #endregion

        #region HELPERS
        protected virtual void CheckForLoops()
        {
            ContainsLoop = false;
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
                    {
                        ContainsLoop = true;
                        return;
                    }
                }                
            }
        }
        protected bool FeedbackCanPlay(MMFeedback feedback)
        {
            if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
            {
                return true;
            }
            else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
                     || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
            {
                return true;
            }
            return false;
        }
        protected virtual void ApplyAutoRevert()
        {
            if (AutoChangeDirectionOnEnd)
            {
                ShouldRevertOnNextPlay = true;
            }
        }
        public virtual float ApplyTimeMultiplier(float duration)
        {
            return duration * DurationMultiplier;
        }
        public virtual void AutoRepair()
        {
            List<Component> components = components = new List<Component>();
            components = this.gameObject.GetComponents<Component>().ToList();
            foreach (Component component in components)
            {
                if (component is MMFeedback)
                {
                    bool found = false;
                    for (int i = 0; i < Feedbacks.Count; i++)
                    {
                        if (Feedbacks[i] == (MMFeedback)component)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Feedbacks.Add((MMFeedback)component);
                    }
                }
            }
        } 

        #endregion 
        
        #region EVENTS
        protected virtual void OnDisable()
        {
        }
        protected virtual void OnValidate()
        {
            DurationMultiplier = Mathf.Clamp(DurationMultiplier, 0f, Single.MaxValue);
        }
        protected virtual void OnDestroy()
        {
            IsPlaying = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                foreach (MMFeedback feedback in Feedbacks)
                {
                    EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(feedback);
                    };                    
                }
            }
#endif
        }     
        
        #endregion EVENTS
    }
}
