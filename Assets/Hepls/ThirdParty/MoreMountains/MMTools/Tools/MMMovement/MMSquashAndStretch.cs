using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Movement/MMSquashAndStretch")]
    public class MMSquashAndStretch : MonoBehaviour
    {
        public enum Timescales { Regular, Unscaled }
        public enum Modes { Rigidbody, Rigidbody2D, Position }

        [MMInformation("This component will apply squash and stretch based on velocity (either position based or computed from a Rigidbody. It has to be put on an intermediary level in the hierarchy, between the logic (top level) and the model (bottom level).", MMInformationAttribute.InformationType.Info, false)]
        [Header("Velocity Detection")]
        public Modes Mode = Modes.Position;
        public Timescales Timescale = Timescales.Regular;


        [Header("Settings")]
        public float Intensity = 0.02f;
        public float MaximumVelocity = 1f;

        [Header("Rescale")]
        public Vector2 MinimumScale = new Vector2(0.5f, 0.5f);
        public Vector2 MaximumScale = new Vector2(2f, 2f);

        [Header("Squash")]
        public bool AutoSquashOnStop = false;
        public AnimationCurve SquashCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
        public float SquashVelocityThreshold = 0.1f;
        [MMVector("Min","Max")]
        public Vector2 SquashDuration = new Vector2(0.25f, 0.5f);
        [MMVector("Min", "Max")]
        public Vector2 SquashIntensity = new Vector2(0f, 1f);
        
        [Header("Debug")]
        [MMReadOnly]
        public Vector3 Velocity;
        [MMReadOnly]
        public float RemappedVelocity;
        [MMReadOnly]
        public float VelocityMagnitude;

        public float TimescaleTime { get { return (Timescale == Timescales.Regular) ? Time.time : Time.unscaledTime; } }
        public float TimescaleDeltaTime { get { return (Timescale == Timescales.Regular) ? Time.deltaTime : Time.unscaledDeltaTime; } }

        protected Rigidbody2D _rigidbody2D;
        protected Rigidbody _rigidbody;
        protected Transform _childTransform;
        protected Transform _parentTransform;
        protected Vector3 _direction;
        protected Vector3 _previousPosition;
        protected Vector3 _newLocalScale;
        protected Vector3 _initialScale;
        protected Quaternion _newRotation = Quaternion.identity;
        protected Quaternion _deltaRotation;
        protected float _squashStartedAt = 0f;
        protected bool _squashing = false;
        protected float _squashIntensity;
        protected float _squashDuration;

        protected bool _movementStarted = false;
        protected float _lastVelocity = 0f;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _initialScale = this.transform.localScale;

            _rigidbody = this.transform.parent.GetComponent<Rigidbody>();
            _rigidbody2D = this.transform.parent.GetComponent<Rigidbody2D>();

            _childTransform = this.transform.GetChild(0).transform;
            _parentTransform = this.transform.parent.GetComponent<Transform>();

            _previousPosition = _parentTransform.position;
        }
        protected virtual void LateUpdate()
        {
            SquashAndStretch();
        }
        protected virtual void SquashAndStretch()
        {
            if (TimescaleDeltaTime <= 0f)
            {
                return;
            }

            ComputeVelocityAndDirection();
            ComputeNewRotation();
            ComputeNewLocalScale();
            StorePreviousPosition();
        }
        protected virtual void ComputeVelocityAndDirection()
        {
            Velocity = Vector3.zero;

            switch (Mode)
            {
                case Modes.Rigidbody:
                    Velocity = _rigidbody.velocity;
                    break;

                case Modes.Rigidbody2D:
                    Velocity = _rigidbody2D.velocity;
                    break;

                case Modes.Position:
                    Velocity = (_previousPosition - _parentTransform.position) / TimescaleDeltaTime;
                    break;
            }

            VelocityMagnitude = Velocity.magnitude;
            RemappedVelocity = MMMaths.Remap(VelocityMagnitude, 0f, MaximumVelocity, 0f, 1f);
            _direction = Vector3.Normalize(Velocity);

            if (AutoSquashOnStop)
            {
                if (VelocityMagnitude > SquashVelocityThreshold)
                {
                    _movementStarted = true;
                    _lastVelocity = Mathf.Clamp(VelocityMagnitude, 0f, MaximumVelocity);
                }
                else if (_movementStarted)
                {
                    _movementStarted = false;
                    _squashing = true;
                    float duration = MMMaths.Remap(_lastVelocity, 0f, MaximumVelocity, SquashDuration.x, SquashDuration.y);
                    float intensity = MMMaths.Remap(_lastVelocity, 0f, MaximumVelocity, SquashIntensity.x, SquashIntensity.y);
                    Squash(duration, intensity);
                }
            }            
        }
        protected virtual void ComputeNewRotation()
        {
            if (VelocityMagnitude > 0.01f)
            {
                _newRotation = Quaternion.FromToRotation(Vector3.up, _direction);
            }
            _deltaRotation = _parentTransform.rotation;
            this.transform.rotation = _newRotation;
            _childTransform.rotation = _deltaRotation;
        }
        protected virtual void ComputeNewLocalScale()
        {
            if (_squashing)
            {
                float elapsed = MMMaths.Remap(TimescaleTime - _squashStartedAt, 0f, _squashDuration, 0f, 1f);
                _newLocalScale.x = _initialScale.x + SquashCurve.Evaluate(elapsed) * _squashIntensity;
                _newLocalScale.y = _initialScale.y - SquashCurve.Evaluate(elapsed) * _squashIntensity;
                _newLocalScale.z = _initialScale.z + SquashCurve.Evaluate(elapsed) * _squashIntensity;

                if (elapsed >= 1f)
                {
                    _squashing = false;
                }
            }
            else
            {
                _newLocalScale.x = Mathf.Clamp01(1f / (RemappedVelocity + 0.001f));
                _newLocalScale.y = RemappedVelocity;
                _newLocalScale.z = Mathf.Clamp01(1f / (RemappedVelocity + 0.001f));
                _newLocalScale = Vector3.Lerp(Vector3.one, _newLocalScale, VelocityMagnitude * Intensity);
            }            

            _newLocalScale.x = Mathf.Clamp(_newLocalScale.x, MinimumScale.x, MaximumScale.x);
            _newLocalScale.y = Mathf.Clamp(_newLocalScale.y, MinimumScale.y, MaximumScale.y);

            this.transform.localScale = _newLocalScale;
        }
        protected virtual void StorePreviousPosition()
        {
            _previousPosition = _parentTransform.position;
        }
        public virtual void Squash(float duration, float intensity)
        {
            _squashStartedAt = TimescaleTime;
            _squashing = true;
            _squashIntensity = intensity;
            _squashDuration = duration;
        }
    }
}
