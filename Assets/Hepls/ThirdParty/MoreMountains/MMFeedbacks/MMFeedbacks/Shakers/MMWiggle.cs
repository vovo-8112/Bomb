using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Feedbacks
{
    public enum WiggleTypes { None, Random, PingPong, Noise, Curve }
    [Serializable]
    public class WiggleProperties
    {
        [Header("Status")]
        public bool WigglePermitted = true;

        [Header("Type")]
        public WiggleTypes WiggleType = WiggleTypes.Random;
        public bool UseUnscaledTime = false;
        public bool StartWigglingAutomatically = true;
        public bool SmoothPingPong = true;

        [Header("Speed")]
        public bool UseSpeedCurve = false;
        public AnimationCurve SpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Frequency")]
        public float FrequencyMin = 0f;
        public float FrequencyMax = 1f;

        [Header("Amplitude")]
        public Vector3 AmplitudeMin = Vector3.zero;
        public Vector3 AmplitudeMax = Vector3.one;
        public bool RelativeAmplitude = true;

        [Header("Curve")]
        public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public Vector3 RemapCurveZeroMin = Vector3.zero;
        public Vector3 RemapCurveZeroMax = Vector3.zero;
        public Vector3 RemapCurveOneMin = Vector3.one;
        public Vector3 RemapCurveOneMax = Vector3.one;
        public bool RelativeCurveAmplitude = true;
        public bool CurvePingPong = false;

        [Header("Pause")]
        public float PauseMin = 0f;
        public float PauseMax = 0f;

        [Header("Limited Time")]
        public bool LimitedTime = false;
        public float LimitedTimeTotal;
        public AnimationCurve LimitedTimeFalloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        public bool LimitedTimeResetValue = true;
        [MMFReadOnly]
        public float LimitedTimeLeft;        

        [Header("Noise Frequency")]
        public Vector3 NoiseFrequencyMin = Vector3.zero;
        public Vector3 NoiseFrequencyMax = Vector3.one;

        [Header("Noise Shift")]
        public Vector3 NoiseShiftMin = Vector3.zero;
        public Vector3 NoiseShiftMax = Vector3.zero;
        public float GetDeltaTime()
        {
            return UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
        public float GetTime()
        {
            return UseUnscaledTime ? Time.unscaledTime : Time.time;
        }
    }
    public struct InternalWiggleProperties
    {
        public Vector3 returnVector;
        public Vector3 newValue;
        public Vector3 initialValue;
        public Vector3 startValue;
        public float timeSinceLastChange ;
        public float randomFrequency;
        public Vector3 randomNoiseFrequency;
        public Vector3 randomAmplitude;
        public Vector3 randomNoiseShift;
        public float timeSinceLastPause;
        public float pauseDuration;
        public float noiseElapsedTime;
        public Vector3 limitedTimeValueSave;
        public Vector3 remapZero;
        public Vector3 remapOne;
        public float curveDirection;
    }
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMWiggle")]
    public class MMWiggle : MonoBehaviour 
    {
        public enum UpdateModes { Update, FixedUpdate, LateUpdate }
        [Tooltip("the selected update mode")]
        public UpdateModes UpdateMode = UpdateModes.Update;
        [Tooltip("whether or not position wiggle is active")]
        public bool PositionActive = false;
        [Tooltip("whether or not rotation wiggle is active")]
        public bool RotationActive = false;
        [Tooltip("whether or not scale wiggle is active")]
        public bool ScaleActive = false;
        [Tooltip("all public info related to position wiggling")]
        public WiggleProperties PositionWiggleProperties;
        [Tooltip("all public info related to rotation wiggling")]
        public WiggleProperties RotationWiggleProperties;
        [Tooltip("all public info related to scale wiggling")]
        public WiggleProperties ScaleWiggleProperties;
        [Tooltip("a debug duration used in conjunction with the debug buttons")]
        public float DebugWiggleDuration = 2f;

        protected InternalWiggleProperties _positionInternalProperties;
        protected InternalWiggleProperties _rotationInternalProperties;
        protected InternalWiggleProperties _scaleInternalProperties;

        public virtual void WigglePosition(float duration)
        {
            WiggleValue(ref PositionWiggleProperties, ref _positionInternalProperties, duration);
        }

        public virtual void WiggleRotation(float duration)
        {
            WiggleValue(ref RotationWiggleProperties, ref _rotationInternalProperties, duration);
        }

        public virtual void WiggleScale(float duration)
        {
            WiggleValue(ref ScaleWiggleProperties, ref _scaleInternalProperties, duration);
        }

        protected virtual void WiggleValue(ref WiggleProperties property, ref InternalWiggleProperties internalProperties, float duration)
        {
            InitializeRandomValues(ref property, ref internalProperties);
            internalProperties.limitedTimeValueSave = internalProperties.initialValue;
            property.LimitedTime = true;
            property.LimitedTimeLeft = duration;
            property.LimitedTimeTotal = duration;
            property.WigglePermitted = true;
        }
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _positionInternalProperties.initialValue = transform.localPosition;
            _positionInternalProperties.startValue = this.transform.localPosition;

            _rotationInternalProperties.initialValue = transform.localEulerAngles;
            _rotationInternalProperties.startValue = this.transform.localEulerAngles;

            _scaleInternalProperties.initialValue = transform.localScale;
            _scaleInternalProperties.startValue = this.transform.localScale;

            InitializeRandomValues(ref PositionWiggleProperties, ref _positionInternalProperties);
            InitializeRandomValues(ref RotationWiggleProperties, ref _rotationInternalProperties);
            InitializeRandomValues(ref ScaleWiggleProperties, ref _scaleInternalProperties);
        }
        protected virtual void InitializeRandomValues(ref WiggleProperties properties, ref InternalWiggleProperties internalProperties)
        {
            internalProperties.newValue = this.transform.localPosition;
            internalProperties.timeSinceLastChange = 0;
            internalProperties.returnVector = Vector3.zero;
            internalProperties.randomFrequency = UnityEngine.Random.Range(properties.FrequencyMin, properties.FrequencyMax);
            internalProperties.randomNoiseFrequency = Vector3.zero;
            internalProperties.randomAmplitude = Vector3.zero;
            internalProperties.timeSinceLastPause = 0;
            internalProperties.pauseDuration = 0;
            internalProperties.noiseElapsedTime = 0;
            internalProperties.curveDirection = 1f;
            properties.LimitedTimeLeft = properties.LimitedTimeTotal;

            RandomizeVector3(ref internalProperties.randomAmplitude, properties.AmplitudeMin, properties.AmplitudeMax);
            RandomizeVector3(ref internalProperties.randomNoiseFrequency, properties.NoiseFrequencyMin, properties.NoiseFrequencyMax);
            RandomizeVector3(ref internalProperties.randomNoiseShift, properties.NoiseShiftMin, properties.NoiseShiftMax);

            RandomizeVector3(ref internalProperties.remapZero, properties.RemapCurveZeroMin, properties.RemapCurveZeroMax);
            RandomizeVector3(ref internalProperties.remapOne, properties.RemapCurveOneMin, properties.RemapCurveOneMax);

            internalProperties.newValue = DetermineNewValue(properties, internalProperties.newValue, internalProperties.initialValue, ref internalProperties.startValue, 
                ref internalProperties.randomAmplitude, ref internalProperties.randomFrequency, ref internalProperties.pauseDuration);
        }
        protected virtual void Update()
        {
            if (UpdateMode == UpdateModes.Update)
            {
                ProcessUpdate();
            }
        }
        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate)
            {
                ProcessUpdate();
            }
        }
        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                ProcessUpdate();
            }
        }
        protected virtual void ProcessUpdate()
        {
            _positionInternalProperties.returnVector = transform.localPosition;
            if (UpdateValue(PositionActive, PositionWiggleProperties, ref _positionInternalProperties))
            {
                transform.localPosition = ApplyFalloff(_positionInternalProperties.returnVector, PositionWiggleProperties);
            }

            _rotationInternalProperties.returnVector = transform.localEulerAngles;
            if (UpdateValue(RotationActive, RotationWiggleProperties, ref _rotationInternalProperties))
            {
                transform.localEulerAngles = ApplyFalloff(_rotationInternalProperties.returnVector, RotationWiggleProperties);
            }

            _scaleInternalProperties.returnVector = transform.localScale;
            if (UpdateValue(ScaleActive, ScaleWiggleProperties, ref _scaleInternalProperties))
            {
                transform.localScale = ApplyFalloff(_scaleInternalProperties.returnVector, ScaleWiggleProperties);
            }
        }
        protected virtual bool UpdateValue(bool valueActive, WiggleProperties properties, ref InternalWiggleProperties internalProperties)
        {
            if (!valueActive) { return false; }
            if (!properties.WigglePermitted) { return false;  }
            if ((properties.LimitedTime) && (properties.LimitedTimeTotal > 0f))
            {
                float timeSave = properties.LimitedTimeLeft;
                properties.LimitedTimeLeft -= properties.GetDeltaTime();
                if (properties.LimitedTimeLeft <= 0)
                {
                    if (timeSave > 0f)
                    {
                        if (properties.LimitedTimeResetValue)
                        {
                            internalProperties.returnVector = internalProperties.limitedTimeValueSave;
                            properties.LimitedTimeLeft = 0;
                            properties.WigglePermitted = false;
                            return true;
                        }
                    }                    
                    return false;
                }
            }

            switch (properties.WiggleType)
            {
                case WiggleTypes.PingPong:
                    return MoveVector3TowardsTarget(ref internalProperties.returnVector, properties, ref internalProperties.startValue, internalProperties.initialValue, 
                        ref internalProperties.newValue, ref internalProperties.timeSinceLastPause, 
                        ref internalProperties.timeSinceLastChange, ref internalProperties.randomAmplitude, 
                        ref internalProperties.randomFrequency, 
                        ref internalProperties.pauseDuration, internalProperties.randomFrequency);
                    

                case WiggleTypes.Random:
                    return MoveVector3TowardsTarget(ref internalProperties.returnVector, properties, ref internalProperties.startValue, internalProperties.initialValue, 
                        ref internalProperties.newValue, ref internalProperties.timeSinceLastPause, 
                        ref internalProperties.timeSinceLastChange, ref internalProperties.randomAmplitude, 
                        ref internalProperties.randomFrequency, 
                        ref internalProperties.pauseDuration, internalProperties.randomFrequency);

                case WiggleTypes.Noise:
                    internalProperties.returnVector = AnimateNoiseValue(ref internalProperties, properties);                    
                    return true;

                case WiggleTypes.Curve:
                    internalProperties.returnVector = AnimateCurveValue(ref internalProperties, properties);
                    return true;
            }
            return false;
        }
        protected Vector3 ApplyFalloff(Vector3 newValue, WiggleProperties properties)
        {
            if ((properties.LimitedTime) && (properties.LimitedTimeTotal > 0f))
            {
                float curveProgress = (properties.LimitedTimeTotal - properties.LimitedTimeLeft) / properties.LimitedTimeTotal;
                float curvePercent = properties.LimitedTimeFalloff.Evaluate(curveProgress);
                newValue = newValue * curvePercent;
            }
            return newValue;
        }
        protected virtual Vector3 AnimateNoiseValue(ref InternalWiggleProperties internalProperties, WiggleProperties properties)
        {
            internalProperties.noiseElapsedTime += properties.GetDeltaTime();

            internalProperties.newValue.x = (Mathf.PerlinNoise(internalProperties.randomNoiseFrequency.x * internalProperties.noiseElapsedTime, internalProperties.randomNoiseShift.x) * 2.0f - 1.0f) * internalProperties.randomAmplitude.x;
            internalProperties.newValue.y = (Mathf.PerlinNoise(internalProperties.randomNoiseFrequency.y * internalProperties.noiseElapsedTime, internalProperties.randomNoiseShift.y) * 2.0f - 1.0f) * internalProperties.randomAmplitude.y;
            internalProperties.newValue.z = (Mathf.PerlinNoise(internalProperties.randomNoiseFrequency.z * internalProperties.noiseElapsedTime, internalProperties.randomNoiseShift.z) * 2.0f - 1.0f) * internalProperties.randomAmplitude.z;

            if (properties.RelativeAmplitude)
            {
                internalProperties.newValue += internalProperties.initialValue;
            }

            return internalProperties.newValue;
        }
        protected virtual Vector3 AnimateCurveValue(ref InternalWiggleProperties internalProperties, WiggleProperties properties)
        {
            internalProperties.timeSinceLastPause += properties.GetDeltaTime();
            internalProperties.timeSinceLastChange += properties.GetDeltaTime();
            if (internalProperties.timeSinceLastPause < internalProperties.pauseDuration)
            {
                float curveProgress = (internalProperties.curveDirection == 1f) ? 1f : 0f;

                EvaluateCurve(properties.Curve, curveProgress, internalProperties.remapZero, internalProperties.remapOne, ref internalProperties.newValue);
                if (properties.RelativeCurveAmplitude)
                {
                    internalProperties.newValue += internalProperties.initialValue;
                }
            }
            if (internalProperties.timeSinceLastPause == internalProperties.timeSinceLastChange)
            {
                internalProperties.timeSinceLastChange = 0f;
            }
            if (internalProperties.randomFrequency > 0)
            {
                float curveProgress = (internalProperties.timeSinceLastChange) / internalProperties.randomFrequency;
                if (internalProperties.curveDirection < 0f)
                {
                    curveProgress = 1 - curveProgress;
                }

                EvaluateCurve(properties.Curve, curveProgress, internalProperties.remapZero, internalProperties.remapOne, ref internalProperties.newValue);
                
                if (internalProperties.timeSinceLastChange > internalProperties.randomFrequency)
                {
                    internalProperties.timeSinceLastChange = 0f;
                    internalProperties.timeSinceLastPause = 0f;
                    if (properties.CurvePingPong)
                    {
                        internalProperties.curveDirection = -internalProperties.curveDirection;
                    }                    

                    RandomizeFloat(ref internalProperties.randomFrequency, properties.FrequencyMin, properties.FrequencyMax);
                }
            }

            if (properties.RelativeCurveAmplitude)
            {
                internalProperties.newValue = internalProperties.initialValue + internalProperties.newValue;
            }

            return internalProperties.newValue;
        }

        protected virtual void EvaluateCurve(AnimationCurve curve, float percent, Vector3 remapMin, Vector3 remapMax, ref Vector3 returnValue)
        {
            returnValue.x = MMFeedbacksHelpers.Remap(curve.Evaluate(percent), 0f, 1f, remapMin.x, remapMax.x);
            returnValue.y = MMFeedbacksHelpers.Remap(curve.Evaluate(percent), 0f, 1f, remapMin.y, remapMax.y);
            returnValue.z = MMFeedbacksHelpers.Remap(curve.Evaluate(percent), 0f, 1f, remapMin.z, remapMax.z);
        }
        protected virtual bool MoveVector3TowardsTarget(ref Vector3 movedValue, WiggleProperties properties, ref Vector3 startValue, Vector3 initialValue, 
            ref Vector3 destinationValue, ref float timeSinceLastPause, ref float timeSinceLastValueChange, 
            ref Vector3 randomAmplitude, ref float randomFrequency,
            ref float pauseDuration, float frequency)
        {
            timeSinceLastPause += properties.GetDeltaTime();
            timeSinceLastValueChange += properties.GetDeltaTime();
            if (timeSinceLastPause < pauseDuration)
            {
                return false;
            }
            if (timeSinceLastPause == timeSinceLastValueChange)
            {
                timeSinceLastValueChange = 0f;
            }
            if (frequency > 0)
            {
                float curveProgress = (timeSinceLastValueChange) / frequency;

                if (!properties.UseSpeedCurve)
                {
                    movedValue = Vector3.Lerp(startValue, destinationValue, curveProgress);
                }
                else
                {
                    float curvePercent = properties.SpeedCurve.Evaluate(curveProgress);
                    movedValue = Vector3.LerpUnclamped(startValue, destinationValue, curvePercent);
                }


                if (timeSinceLastValueChange > frequency)
                {
                    timeSinceLastValueChange = 0f;
                    timeSinceLastPause = 0f;
                    movedValue = destinationValue;
                    destinationValue = DetermineNewValue(properties, movedValue, initialValue, ref startValue, 
                        ref randomAmplitude, ref randomFrequency, ref pauseDuration);
                }
            }
            return true;
        }
        protected virtual Vector3 DetermineNewValue(WiggleProperties properties, Vector3 newValue, Vector3 initialValue, ref Vector3 startValue, 
            ref Vector3 randomAmplitude, ref float randomFrequency, ref float pauseDuration)
        {
            switch (properties.WiggleType)
            {
                case WiggleTypes.PingPong:

                    if (properties.RelativeAmplitude)
                    {
                        if (newValue == properties.AmplitudeMin + initialValue)
                        {
                            newValue = properties.AmplitudeMax;
                            startValue = properties.AmplitudeMin;
                        }
                        else
                        {
                            newValue = properties.AmplitudeMin;
                            startValue = properties.AmplitudeMax;
                        }
                        startValue += initialValue;
                        newValue += initialValue;
                    }
                    else
                    {
                        newValue = (newValue == properties.AmplitudeMin) ? properties.AmplitudeMax : properties.AmplitudeMin;
                        startValue = (newValue == properties.AmplitudeMin) ? properties.AmplitudeMax : properties.AmplitudeMin;
                    }                    
                    RandomizeFloat(ref randomFrequency, properties.FrequencyMin, properties.FrequencyMax);
                    RandomizeFloat(ref pauseDuration, properties.PauseMin, properties.PauseMax);
                    return newValue;

                case WiggleTypes.Random:
                    startValue = newValue;
                    RandomizeFloat(ref randomFrequency, properties.FrequencyMin, properties.FrequencyMax);
                    RandomizeVector3(ref randomAmplitude, properties.AmplitudeMin, properties.AmplitudeMax);
                    RandomizeFloat(ref pauseDuration, properties.PauseMin, properties.PauseMax);
                    newValue = randomAmplitude;
                    if (properties.RelativeAmplitude)
                    {
                        newValue += initialValue;
                    }
                    return newValue;
            }
            return Vector3.zero;            
        }
        protected virtual float RandomizeFloat(ref float randomizedFloat, float floatMin, float floatMax)
        {
            randomizedFloat = UnityEngine.Random.Range(floatMin, floatMax);
            return randomizedFloat;
        }
        protected virtual Vector3 RandomizeVector3(ref Vector3 randomizedVector, Vector3 vectorMin, Vector3 vectorMax)
        {
            randomizedVector.x = UnityEngine.Random.Range(vectorMin.x, vectorMax.x);
            randomizedVector.y = UnityEngine.Random.Range(vectorMin.y, vectorMax.y);
            randomizedVector.z = UnityEngine.Random.Range(vectorMin.z, vectorMax.z);
            return randomizedVector;
        }
    }
}