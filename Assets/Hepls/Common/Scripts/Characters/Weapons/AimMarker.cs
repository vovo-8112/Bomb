using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class AimMarker : MonoBehaviour
    {
        public enum MovementModes { Instant, Interpolate }

        [Header("Movement")]
        [Tooltip("The selected movement mode for this aim marker. Instant will move the marker instantly to its target, Interpolate will animate its position over time")]
        public MovementModes MovementMode;
        [Tooltip("an offset to apply to the position of the target (useful if you want, for example, the marker to appear above the target)")]
        public Vector3 Offset;
        [Tooltip("When in Interpolate mode, the duration of the movement animation")]
        [MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
        public float MovementDuration = 0.2f;
        [Tooltip("When in Interpolate mode, the curve to animate the movement on")]
        [MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
        public MMTween.MMTweenCurve MovementCurve = MMTween.MMTweenCurve.EaseInCubic;
        [Tooltip("When in Interpolate mode, the delay before the marker moves when changing target")]
        [MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
        public float MovementDelay = 0f;

        [Header("Feedbacks")]
        [Tooltip("A feedback to play when a target is found and we didn't have one already")]
        public MMFeedbacks FirstTargetFeedback;
        [Tooltip("a feedback to play when we already had a target and just found a new one")]
        public MMFeedbacks NewTargetAssignedFeedback;
        [Tooltip("a feedback to play when no more targets are found, and we just lost our last target")]
        public MMFeedbacks NoMoreTargetFeedback;

        protected Transform _target;
        protected Transform _targetLastFrame = null;
        protected WaitForSeconds _movementDelayWFS;
        protected float _lastTargetChangeAt = 0f;
        protected virtual void Awake()
        {
            FirstTargetFeedback?.Initialization(this.gameObject);
            NewTargetAssignedFeedback?.Initialization(this.gameObject);
            NoMoreTargetFeedback?.Initialization(this.gameObject);
            if (MovementDelay > 0f)
            {
                _movementDelayWFS = new WaitForSeconds(MovementDelay);
            }
        }
        protected virtual void Update()
        {
            HandleTargetChange();
            FollowTarget();
            _targetLastFrame = _target;
        }
        protected virtual void FollowTarget()
        {
            if (MovementMode == MovementModes.Instant)
            {
                this.transform.position = _target.transform.position + Offset;
            }
            else
            {
                if ((_target != null) && (Time.time - _lastTargetChangeAt > MovementDuration))
                {
                    this.transform.position = _target.transform.position + Offset;
                }
            }
        }
        public virtual void SetTarget(Transform newTarget)
        {
            _target = newTarget;

            if (newTarget == null)
            {
                return;
            }

            this.gameObject.SetActive(true);

            if (_targetLastFrame == null)
            {
                this.transform.position = _target.transform.position + Offset;
            }

            if (MovementMode == MovementModes.Instant)
            {
                this.transform.position = _target.transform.position + Offset;
            }
            else
            {
                MMTween.MoveTransform(this, this.transform, this.transform.position, _target.transform.position + Offset, _movementDelayWFS, MovementDelay, MovementDuration, MovementCurve);
            }
        }
        protected virtual void HandleTargetChange()
        {
            if (_target == _targetLastFrame)
            {
                return;
            }

            _lastTargetChangeAt = Time.time;

            if (_target == null)
            {
                NoMoreTargets();
                return;
            }

            if (_targetLastFrame == null)
            {
                FirstTargetFound();
                return;
            }

            if ((_targetLastFrame != null) && (_target != null))
            {
                NewTargetFound();
            }
        }
        protected virtual void NoMoreTargets()
        {
            NoMoreTargetFeedback?.PlayFeedbacks();
        }
        protected virtual void FirstTargetFound()
        {
            FirstTargetFeedback?.PlayFeedbacks();
        }
        protected virtual void NewTargetFound()
        {
            NewTargetAssignedFeedback?.PlayFeedbacks();
        }
        public virtual void Disable()
        {
            this.gameObject.SetActive(false);
        }
    }
}
