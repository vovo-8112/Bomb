using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    public class MMRadioSignalGenerator : MMRadioSignal
    {
        public bool AnimatedPreview = false;
        public bool BackAndForth = false;
        [MMCondition("BackAndForth", true)]
        public float BackAndForthMirrorPoint = 0.5f;
        public MMRadioSignalGeneratorItemList SignalList;
        [MMVector("Min", "Max")]
        public Vector2 Clamps = new Vector2(0f, 1f);
        [Range(0f, 1f)]
        public float Bias = 0.5f;
        void Reset()
        {
            SignalList = new MMRadioSignalGeneratorItemList(){
             new MMRadioSignalGeneratorItem()
         };
        }
        public virtual float Evaluate(float time)
        {
            float level = 1f;
            if (SignalList.Count <= 0)
            {
                return level;
            }

            time = ApplyBias(time, Bias);
            
            for (int i = 0; i < SignalList.Count; i++)
            {
                if (SignalList[i].Active)
                {
                    float newLevel = MMSignal.GetValueNormalized(time,
                                                             SignalList[i].SignalType, SignalList[i].Phase,
                                                             SignalList[i].Amplitude, SignalList[i].Frequency, SignalList[i].Offset,
                                                             SignalList[i].Invert, SignalList[i].Curve, SignalList[i].TweenCurve,
                                                             true, Clamps.x, Clamps.y, BackAndForth, BackAndForthMirrorPoint);
                    
                    level = (SignalList[i].Mode == MMRadioSignalGeneratorItem.GeneratorItemModes.Multiply) ? level * newLevel : level + newLevel;
                    
                    
                }
            }
            CurrentLevel *= GlobalMultiplier;
            
            CurrentLevel = Mathf.Clamp(CurrentLevel, Clamps.x, Clamps.y);
            return level;
        }
        protected override void Shake()
        {
            base.Shake();

            if (!Playing)
            {
                return;
            }

            if (SignalMode == SignalModes.OneTime)
            {
                float elapsedTime = TimescaleTime - _shakeStartedTimestamp;
                CurrentLevel = Evaluate(MMMaths.Remap(elapsedTime, 0f, Duration, 0f, 1f));   
            }
            else
            {
                CurrentLevel = Evaluate(DriverTime);
            }
        }
        protected override void ShakeComplete()
        {
            base.ShakeComplete();
            CurrentLevel = Evaluate(1f);
        }
        public override float GraphValue(float time)
        {
            time = ApplyBias(time, Bias);
            return Evaluate(time);
        }
    }
    [System.Serializable]
    public class MMRadioSignalGeneratorItem
    {
        public enum GeneratorItemModes { Multiply, Additive }
        public bool Active = true;
        public MMSignal.SignalType SignalType = MMSignal.SignalType.Sine;
        [MMEnumCondition("SignalType", (int)MMSignal.SignalType.AnimationCurve)]
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [MMEnumCondition("SignalType", (int)MMSignal.SignalType.MMTween)]
        public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInOutQuartic;
        public GeneratorItemModes Mode = GeneratorItemModes.Multiply;
        [Range(-1f, 1f)]
        public float Phase = 0f;
        [Range(0f, 10f)]
        public float Frequency = 5f;
        [Range(0f, 1f)]
        public float Amplitude = 1f;
        [Range(-1f, 1f)]
        public float Offset = 0f;
        public bool Invert = false;
    }
    [System.Serializable]
    public class MMRadioSignalGeneratorItemList : MMReorderableArray<MMRadioSignalGeneratorItem>
    {
    }
}

