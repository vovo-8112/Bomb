using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.")]
    [FeedbackPath("Transform/Destination")]
    public class MMFeedbackDestinationTransform : MMFeedback
    {
        public enum TimeScales { Scaled, Unscaled }
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        #endif

        [Header("Target to animate")]
        [Tooltip("the target transform we want to animate properties on")]
        public Transform TargetTransform;
        
        [Header("Origin and destination")]
        [Tooltip("whether or not we want to force an origin transform. If not, the current position of the target transform will be used as origin instead")]
        public bool ForceOrigin = false;
        [Tooltip("the transform to use as origin in ForceOrigin mode")]
        [MMFCondition("ForceOrigin", true)] 
        public Transform Origin;
        [Tooltip("the destination transform whose properties we want to match")]
        public Transform Destination;
        
        [Header("Transition")]
        [Tooltip("the timescale to animate on")]
        public TimeScales TimeScale = TimeScales.Scaled;
        [Tooltip("whether or not we want to force transform properties at the end of the transition")]
        public bool ForceDestinationOnEnd = false;
        [Tooltip("a global curve to animate all properties on, unless dedicated ones are specified")]
        public AnimationCurve GlobalAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the duration of the transition, in seconds")]
        public float Duration = 0.2f;

        [Header("Axis Locks")]
        [Tooltip("whether or not to animate the X Position")]
        public bool AnimatePositionX = true;
        [Tooltip("whether or not to animate the Y Position")]
        public bool AnimatePositionY = true;
        [Tooltip("whether or not to animate the Z Position")]
        public bool AnimatePositionZ = true;
        [Tooltip("whether or not to animate the X rotation")]
        public bool AnimateRotationX = true;
        [Tooltip("whether or not to animate the Y rotation")]
        public bool AnimateRotationY = true;
        [Tooltip("whether or not to animate the Z rotation")]
        public bool AnimateRotationZ = true;
        [Tooltip("whether or not to animate the W rotation")]
        public bool AnimateRotationW = true;
        [Tooltip("whether or not to animate the X scale")]
        public bool AnimateScaleX = true;
        [Tooltip("whether or not to animate the Y scale")]
        public bool AnimateScaleY = true;
        [Tooltip("whether or not to animate the Z scale")]
        public bool AnimateScaleZ = true;

        [Header("Separate Curves")]
        [Tooltip("whether or not to use a separate animation curve to animate the position")]
        public bool SeparatePositionCurve = false;
        [Tooltip("the curve to use to animate the position on")]
        [MMFCondition("SeparatePositionCurve", true)]
        public AnimationCurve AnimatePositionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("whether or not to use a separate animation curve to animate the rotation")]
        public bool SeparateRotationCurve = false;
        [Tooltip("the curve to use to animate the rotation on")]
        [MMFCondition("SeparateRotationCurve", true)]
        public AnimationCurve AnimateRotationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("whether or not to use a separate animation curve to animate the scale")]
        public bool SeparateScaleCurve = false;
        [Tooltip("the curve to use to animate the scale on")]
        [MMFCondition("SeparateScaleCurve", true)]
        public AnimationCurve AnimateScaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        protected Coroutine _coroutine;
        protected Vector3 _newPosition;
        protected Quaternion _newRotation;
        protected Vector3 _newScale;
        protected Vector3 _pointAPosition;
        protected Vector3 _pointBPosition;
        protected Quaternion _pointARotation;
        protected Quaternion _pointBRotation;
        protected Vector3 _pointAScale;
        protected Vector3 _pointBScale;
        protected AnimationCurve _animationCurve;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetTransform != null))
            {
                _coroutine = StartCoroutine(AnimateToDestination());
            }
        }
        protected virtual IEnumerator AnimateToDestination()
        {
            _pointAPosition = ForceOrigin ? Origin.transform.position : TargetTransform.position;
            _pointBPosition = Destination.transform.position;

            if (!AnimatePositionX) { _pointAPosition.x = TargetTransform.position.x; _pointBPosition.x = _pointAPosition.x; }
            if (!AnimatePositionY) { _pointAPosition.y = TargetTransform.position.y; _pointBPosition.y = _pointAPosition.y; }
            if (!AnimatePositionZ) { _pointAPosition.z = TargetTransform.position.z; _pointBPosition.z = _pointAPosition.z; }
            
            _pointARotation = ForceOrigin ? Origin.transform.rotation : TargetTransform.rotation;
            _pointBRotation = Destination.transform.rotation;
            
            if (!AnimateRotationX) { _pointARotation.x = TargetTransform.rotation.x; _pointBRotation.x = _pointARotation.x; }
            if (!AnimateRotationY) { _pointARotation.y = TargetTransform.rotation.y; _pointBRotation.y = _pointARotation.y; }
            if (!AnimateRotationZ) { _pointARotation.z = TargetTransform.rotation.z; _pointBRotation.z = _pointARotation.z; }
            if (!AnimateRotationW) { _pointARotation.w = TargetTransform.rotation.w; _pointBRotation.w = _pointARotation.w; }

            _pointAScale = ForceOrigin ? Origin.transform.localScale : TargetTransform.localScale;
            _pointBScale = Destination.transform.localScale;
            
            if (!AnimateScaleX) { _pointAScale.x = TargetTransform.localScale.x; _pointBScale.x = _pointAScale.x; }
            if (!AnimateScaleY) { _pointAScale.y = TargetTransform.localScale.y; _pointBScale.y = _pointAScale.y; }
            if (!AnimateScaleZ) { _pointAScale.z = TargetTransform.localScale.z; _pointBScale.z = _pointAScale.z; }
            
            float journey = NormalPlayDirection ? 0f : Duration;
            while ((journey >= 0) && (journey <= Duration) && (Duration > 0))
            {
                float percent = Mathf.Clamp01(journey / Duration);

                _animationCurve = SeparatePositionCurve ? AnimatePositionCurve : GlobalAnimationCurve;
                _newPosition = Vector3.LerpUnclamped(_pointAPosition, _pointBPosition, _animationCurve.Evaluate(percent));
                
                _animationCurve = SeparateRotationCurve ? AnimateRotationCurve : GlobalAnimationCurve;
                _newRotation = Quaternion.LerpUnclamped(_pointARotation, _pointBRotation, _animationCurve.Evaluate(percent));
                
                _animationCurve = SeparateScaleCurve ? AnimateScaleCurve : GlobalAnimationCurve;
                _newScale = Vector3.LerpUnclamped(_pointAScale, _pointBScale, _animationCurve.Evaluate(percent));

                TargetTransform.position = _newPosition;
                TargetTransform.rotation = _newRotation;
                TargetTransform.localScale = _newScale;

                if (TimeScale == TimeScales.Scaled)
                {
                    journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                }
                else
                {
                    journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
                }
                yield return null;
            }
            if (ForceDestinationOnEnd)
            {
                if (NormalPlayDirection)
                {
                    TargetTransform.position = _pointBPosition;
                    TargetTransform.rotation = _pointBRotation;
                    TargetTransform.localScale = _pointBScale;
                }
                else
                {
                    TargetTransform.position = _pointAPosition;
                    TargetTransform.rotation = _pointARotation;
                    TargetTransform.localScale = _pointAScale;
                }    
            }
            
            _coroutine = null;
            yield break;
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);

            if (Active && (TargetTransform != null) && (_coroutine != null))
            {
                StopCoroutine(_coroutine);
            }
        }
    }    
}

