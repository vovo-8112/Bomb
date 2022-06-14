using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [System.Serializable]
    [ExecuteAlways]
    public abstract class MMFeedback : MonoBehaviour
    {
        [Tooltip("whether or not this feedback is active")]
        public bool Active = true;
        [Tooltip("the name of this feedback to display in the inspector")]
        public string Label = "MMFeedback";
        [Tooltip("the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)")]
        [Range(0,100)]
        public float Chance = 100f;
        [Tooltip("a number of timing-related values (delay, repeat, etc)")]
        public MMFeedbackTiming Timing;
        public GameObject Owner { get; set; }
        [HideInInspector]
        public bool DebugActive = false;
        public virtual IEnumerator Pause { get { return null; } }
        public virtual bool HoldingPause { get { return false; } }
        public virtual bool LooperPause { get { return false; } }
        public virtual bool ScriptDrivenPause { get; set; }
        public virtual float ScriptDrivenPauseAutoResume { get; set; }
        public virtual bool LooperStart { get { return false; } }
        #if UNITY_EDITOR
        public virtual Color FeedbackColor { get { return Color.white;  } }
        #endif
        public virtual bool InCooldown { get { return (Timing.CooldownDuration > 0f) && (FeedbackTime - _lastPlayTimestamp < Timing.CooldownDuration); } }
        public float FeedbackTime 
        { 
            get 
            {
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    return Time.time;
                }
                else
                {
                    return Time.unscaledTime;
                }
            } 
        }
        public float FeedbackDeltaTime
        {
            get
            {
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    return Time.deltaTime;
                }
                else
                {
                    return Time.unscaledDeltaTime;
                }
            }
        }
        public float TotalDuration
        {
            get
            {
                float totalTime = 0f;

                if (Timing == null)
                {
                    return 0f;
                }
                
                if (Timing.InitialDelay != 0)
                {
                    totalTime += ApplyTimeMultiplier(Timing.InitialDelay);
                }
            
                totalTime += FeedbackDuration;

                if (Timing.NumberOfRepeats != 0)
                {
                    float delayBetweenRepeats = ApplyTimeMultiplier(Timing.DelayBetweenRepeats); 
                    
                    totalTime += Timing.NumberOfRepeats * (FeedbackDuration + delayBetweenRepeats);
                }

                return totalTime;
            }
        }
        public virtual float FeedbackStartedAt { get { return _lastPlayTimestamp; } }
        public virtual float FeedbackDuration { get { return 0f; } set { } }
        public virtual bool FeedbackPlaying { get { return ((FeedbackStartedAt > 0f) && (Time.time - FeedbackStartedAt < FeedbackDuration)); } }

        protected float _lastPlayTimestamp = -1f;
        protected int _playsLeft;
        protected bool _initialized = false;
        protected Coroutine _playCoroutine;
        protected Coroutine _infinitePlayCoroutine;
        protected Coroutine _sequenceCoroutine;
        protected Coroutine _repeatedPlayCoroutine;
        protected int _sequenceTrackID = 0;
        protected MMFeedbacks _hostMMFeedbacks;

        protected float _beatInterval;
        protected bool BeatThisFrame = false;
        protected int LastBeatIndex = 0;
        protected int CurrentSequenceIndex = 0;
        protected float LastBeatTimestamp = 0f;
        protected bool _isHostMMFeedbacksNotNull;

        protected virtual void OnEnable()
        {
            _hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
            _isHostMMFeedbacksNotNull = _hostMMFeedbacks != null;
        }
        public virtual void Initialization(GameObject owner)
        {
            _initialized = true;
            Owner = owner;
            _playsLeft = Timing.NumberOfRepeats + 1;
            _hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
            
            SetInitialDelay(Timing.InitialDelay);
            SetDelayBetweenRepeats(Timing.DelayBetweenRepeats);
            SetSequence(Timing.Sequence);

            CustomInitialization(owner);            
        }
        public virtual void Play(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (!_initialized)
            {
                Debug.LogWarning("The " + this + " feedback is being played without having been initialized. Call Initialization() first.");
            }
            if (InCooldown)
            {
                return;
            }

            if (Timing.InitialDelay > 0f) 
            {
                _playCoroutine = StartCoroutine(PlayCoroutine(position, feedbacksIntensity));
            }
            else
            {
                _lastPlayTimestamp = FeedbackTime;
                RegularPlay(position, feedbacksIntensity);
            }  
        }
        protected virtual IEnumerator PlayCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Timing.TimescaleMode == TimescaleModes.Scaled)
            {
                yield return MMFeedbacksCoroutine.WaitFor(Timing.InitialDelay);
            }
            else
            {
                yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.InitialDelay);
            }
            _lastPlayTimestamp = FeedbackTime;
            RegularPlay(position, feedbacksIntensity);
        }
        protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Chance == 0f)
            {
                return;
            }
            if (Chance != 100f)
            {
                float random = Random.Range(0f, 100f);
                if (random > Chance)
                {
                    return;
                }
            }

            if (Timing.UseIntensityInterval)
            {
                if ((feedbacksIntensity < Timing.IntensityIntervalMin) || (feedbacksIntensity >= Timing.IntensityIntervalMax))
                {
                    return;
                }
            }

            if (Timing.RepeatForever)
            {
                _infinitePlayCoroutine = StartCoroutine(InfinitePlay(position, feedbacksIntensity));
                return;
            }
            if (Timing.NumberOfRepeats > 0)
            {
                _repeatedPlayCoroutine = StartCoroutine(RepeatedPlay(position, feedbacksIntensity));
                return;
            }            
            if (Timing.Sequence == null)
            {
                CustomPlayFeedback(position, feedbacksIntensity);
            }
            else
            {
                _sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
            }
            
        }
        protected virtual IEnumerator InfinitePlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            while (true)
            {
                _lastPlayTimestamp = FeedbackTime;
                if (Timing.Sequence == null)
                {
                    CustomPlayFeedback(position, feedbacksIntensity);
                    if (Timing.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
                    }
                    else
                    {
                        yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
                    }
                }
                else
                {
                    _sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));

                    float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
                    if (Timing.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return MMFeedbacksCoroutine.WaitFor(delay);
                    }
                    else
                    {
                        yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
                    }
                }
            }
        }
        protected virtual IEnumerator RepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            while (_playsLeft > 0)
            {
                _lastPlayTimestamp = FeedbackTime;
                _playsLeft--;
                if (Timing.Sequence == null)
                {
                    CustomPlayFeedback(position, feedbacksIntensity);
                    
                    if (Timing.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
                    }
                    else
                    {
                        yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
                    }
                }
                else
                {
                    _sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
                    
                    float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
                    if (Timing.TimescaleMode == TimescaleModes.Scaled)
                    {
                        yield return MMFeedbacksCoroutine.WaitFor(delay);
                    }
                    else
                    {
                        yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
                    }
                }
            }
            _playsLeft = Timing.NumberOfRepeats + 1;
        }
        protected virtual IEnumerator SequenceCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            yield return null;
            float timeStartedAt = FeedbackTime;
            float lastFrame = FeedbackTime;

            BeatThisFrame = false;
            LastBeatIndex = 0;
            CurrentSequenceIndex = 0;
            LastBeatTimestamp = 0f;

            if (Timing.Quantized)
            {
                while (CurrentSequenceIndex < Timing.Sequence.QuantizedSequence[0].Line.Count)
                {
                    _beatInterval = 60f / Timing.TargetBPM;

                    if ((FeedbackTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
                    {
                        BeatThisFrame = true;
                        LastBeatIndex = CurrentSequenceIndex;
                        LastBeatTimestamp = FeedbackTime;

                        for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
                        {
                            if (Timing.Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID == Timing.TrackID)
                            {
                                CustomPlayFeedback(position, feedbacksIntensity);
                            }
                        }
                        CurrentSequenceIndex++;
                    }
                    yield return null;
                }
            }
            else
            {
                while (FeedbackTime - timeStartedAt < Timing.Sequence.Length)
                {
                    foreach (MMSequenceNote item in Timing.Sequence.OriginalSequence.Line)
                    {
                        if ((item.ID == Timing.TrackID) && (item.Timestamp >= lastFrame) && (item.Timestamp <= FeedbackTime - timeStartedAt))
                        {
                            CustomPlayFeedback(position, feedbacksIntensity);
                        }
                    }
                    lastFrame = FeedbackTime - timeStartedAt;
                    yield return null;
                }
            }
                    
        }
        public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (_playCoroutine != null) { StopCoroutine(_playCoroutine); }
            if (_infinitePlayCoroutine != null) { StopCoroutine(_infinitePlayCoroutine); }
            if (_repeatedPlayCoroutine != null) { StopCoroutine(_repeatedPlayCoroutine); }            
            if (_sequenceCoroutine != null) { StopCoroutine(_sequenceCoroutine);  }

            _lastPlayTimestamp = 0f;
            _playsLeft = Timing.NumberOfRepeats + 1;
            if (Timing.InterruptsOnStop)
            {
                CustomStopFeedback(position, feedbacksIntensity);    
            }
        }
        public virtual void ResetFeedback()
        {
            _playsLeft = Timing.NumberOfRepeats + 1;
            CustomReset();
        }
        public virtual void SetSequence(MMSequence newSequence)
        {
            Timing.Sequence = newSequence;
            if (Timing.Sequence != null)
            {
                for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
                {
                    if (Timing.Sequence.SequenceTracks[i].ID == Timing.TrackID)
                    {
                        _sequenceTrackID = i;
                    }
                }
            }
        }
        public virtual void SetDelayBetweenRepeats(float delay)
        {
            Timing.DelayBetweenRepeats = delay;
        }
        public virtual void SetInitialDelay(float delay)
        {
            Timing.InitialDelay = delay;
        }
        protected virtual float ApplyDirection(float normalizedTime)
        {
            return NormalPlayDirection ? normalizedTime : 1 - normalizedTime;
        }
        public virtual bool NormalPlayDirection
        {
            get
            {
                switch (Timing.PlayDirection)
                {
                    case MMFeedbackTiming.PlayDirections.FollowMMFeedbacksDirection:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                    case MMFeedbackTiming.PlayDirections.AlwaysNormal:
                        return true;
                    case MMFeedbackTiming.PlayDirections.AlwaysRewind:
                        return false;
                    case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
                        return !(_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                }
                return true;
            }
        }
        public virtual bool ShouldPlayInThisSequenceDirection
        {
            get
            {
                switch (Timing.MMFeedbacksDirectionCondition)
                {
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.Always:
                        return true;
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop);
                }
                return true;
            }
        }
        protected virtual float FinalNormalizedTime
        {
            get
            {
                return NormalPlayDirection ? 1f : 0f;
            }
        }
        protected virtual float ApplyTimeMultiplier(float duration)
        {
            if (_isHostMMFeedbacksNotNull)
            {
                return _hostMMFeedbacks.ApplyTimeMultiplier(duration);    
            }

            return duration;
        }
        protected virtual void CustomInitialization(GameObject owner) { }
        protected abstract void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f);
        protected virtual void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
        protected virtual void CustomReset() { }
    }   
}

