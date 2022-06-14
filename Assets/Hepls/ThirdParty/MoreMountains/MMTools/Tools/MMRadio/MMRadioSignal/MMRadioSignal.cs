using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class MMRadioSignalOnValueChange : UnityEvent<float> { }
    public abstract class MMRadioSignal : MonoBehaviour
    {
        public enum SignalModes { OneTime, Persistent, Driven }
        public enum TimeScales { Unscaled, Scaled }
        public virtual float Level { get { return CurrentLevel; } }
        public float TimescaleTime { get { return (TimeScale == TimeScales.Scaled) ? Time.time : Time.unscaledTime; } }
        public float TimescaleDeltaTime { get { return (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; } }

        [Header("Signal")]
        public SignalModes SignalMode = SignalModes.Persistent;
        public TimeScales TimeScale = TimeScales.Unscaled;
        public float Duration = 1f;
        public float GlobalMultiplier = 1f;
        [MMReadOnly]
        public float CurrentLevel = 0f;

        [Header("Play Settings")]
        [MMReadOnly]
        public bool Playing = false;
        [Range(0f, 1f)]
        public float DriverTime;
        public bool PlayOnStart = true;
        public MMRadioSignalOnValueChange OnValueChange;
        [Header(("Debug"))]
        [MMInspectorButton("StartShaking")] 
        public bool StartShakingButton;

        protected float _signalTime = 0f;
        protected float _shakeStartedTimestamp;
        protected float _levelLastFrame;
        protected virtual void Awake()
        {
            Initialization();
            if (PlayOnStart)
            {
                StartShaking();
            }
            this.enabled = PlayOnStart;
        }
        protected virtual void Initialization()
        {
            CurrentLevel = 0f;
        }
        public virtual void StartShaking()
        {
            if (Playing)
            {
                return;
            }
            else
            {
                this.enabled = true;
                _shakeStartedTimestamp = TimescaleTime;
                Playing = true;
                ShakeStarts();
            }
        }
        protected virtual void ShakeStarts()
        {

        }
        protected virtual void Update()
        {
            ProcessUpdate();

            if (SignalMode == SignalModes.Driven)
            {
                ProcessDrivenMode();
            }
            else if (SignalMode == SignalModes.Persistent)
            {
                _signalTime += TimescaleDeltaTime;
                if (_signalTime > Duration)
                {
                    _signalTime = 0f;
                }

                DriverTime = MMMaths.Remap(_signalTime, 0f, Duration, 0f, 1f);
            }
            else if (SignalMode == SignalModes.OneTime)
            {

            }

            if (Playing || (SignalMode == SignalModes.Driven))
            {
                Shake();
            }
                        
            if ((SignalMode == SignalModes.OneTime) && Playing && (TimescaleTime - _shakeStartedTimestamp > Duration))
            {
                ShakeComplete();
            }

            if ((_levelLastFrame != Level) && (OnValueChange != null))
            {
                OnValueChange.Invoke(Level);
            }

            _levelLastFrame = Level;
        }
        protected virtual void ProcessDrivenMode()
        {

        }
        protected virtual void ProcessUpdate()
        {

        }
        protected virtual void Shake()
        {

        }

        public virtual float GraphValue(float time)
        {
            return 0f;
        }
        protected virtual void ShakeComplete()
        {
            Playing = false;
            this.enabled = false;
        }
        protected virtual void OnEnable()
        {
            StartShaking();
        }
        protected virtual void OnDestroy()
        {

        }
        protected virtual void OnDisable()
        {
            if (Playing)
            {
                ShakeComplete();
            }
        }
        public virtual void Play()
        {
            this.enabled = true;
        }
        public virtual void Stop()
        {
            ShakeComplete();
        }
        public virtual float ApplyBias(float t, float bias)
        {
            if (bias == 0.5f)
            {
                return t;
            }
            
            bias = MMMaths.Remap(bias, 0f, 1f, 1f, 0f);
            
            float a = bias * 2.0f - 1.0f;

            if (a < 0)
            {
                t = 1 - Mathf.Pow(1.0f - t, Mathf.Max(1 + a, .01f));
            }
            else
            {
                t = Mathf.Pow(t, Mathf.Max(1 - a, .01f));
            }

            return t;
        }
    }
}
