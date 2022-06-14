using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you trigger a fade event.")]
    [FeedbackPath("Camera/Fade")]
    public class MMFeedbackFade : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif
        public enum FadeTypes { FadeIn, FadeOut, Custom }
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

        [Header("Fade")]
        [Tooltip("the type of fade we want to use when this feedback gets played")]
        public FadeTypes FadeType;
        [Tooltip("the ID of the fader(s) to pilot")]
        public int ID = 0;
        [Tooltip("the duration (in seconds) of the fade")]
        public float Duration = 1f;
        [Tooltip("the curve to use for this fade")]
        public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        [Tooltip("whether or not this fade should ignore timescale")]
        public bool IgnoreTimeScale = true;

        [Header("Custom")]
        [Tooltip("the target alpha we're aiming for with this fade")]
        public float TargetAlpha;

        [Header("Position")]
        [Tooltip("the chosen way to position the object")]
        public PositionModes PositionMode = PositionModes.FeedbackPosition;
        [Tooltip("the transform at which to instantiate the object")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
        public Transform TargetTransform;
        [Tooltip("the transform at which to instantiate the object")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
        public Vector3 TargetPosition;
        [Tooltip("the position offset at which to instantiate the vfx object")]
        public Vector3 PositionOffset;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }

        protected Vector3 _position;
        protected FadeTypes _fadeType;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                _position = GetPosition(position);
                _fadeType = FadeType;
                if (!NormalPlayDirection)
                {
                    if (FadeType == FadeTypes.FadeIn)
                    {
                        _fadeType = FadeTypes.FadeOut;
                    }
                    else if (FadeType == FadeTypes.FadeOut)
                    {
                        _fadeType = FadeTypes.FadeIn;
                    }
                }
                switch (_fadeType)
                {
                    case FadeTypes.Custom:
                        MMFadeEvent.Trigger(FeedbackDuration, TargetAlpha, Curve, ID, IgnoreTimeScale, _position);
                        break;
                    case FadeTypes.FadeIn:
                        MMFadeInEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
                        break;
                    case FadeTypes.FadeOut:
                        MMFadeOutEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
                        break;
                }
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            MMFadeStopEvent.Trigger(ID);
        }
        protected virtual Vector3 GetPosition(Vector3 position)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.position + PositionOffset;
                case PositionModes.Transform:
                    return TargetTransform.position + PositionOffset;
                case PositionModes.WorldPosition:
                    return TargetPosition + PositionOffset;
                case PositionModes.Script:
                    return position + PositionOffset;
                default:
                    return position + PositionOffset;
            }
        }
    }
}
