using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will instantiate the specified ParticleSystem at the specified position on Start or on Play, optionally nesting them.")]
    [FeedbackPath("Particles/Particles Instantiation")]
    public class MMFeedbackParticlesInstantiation : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
        #endif
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }
        public enum Modes { Cached, OnDemand }

        [Header("Particles Instantiation")]
        [Tooltip("whether the particle system should be cached or created on demand the first time")]
        public Modes Mode = Modes.Cached;
        [Tooltip("if this is false, a brand new particle system will be created every time")]
        [MMFEnumCondition("Mode", (int)Modes.OnDemand)]
        public bool CachedRecycle = true;
        [Tooltip("the particle system to spawn")]
        public ParticleSystem ParticlesPrefab;
        [Tooltip("the possible random particle systems")]
        public List<ParticleSystem> RandomParticlePrefabs;

        [Header("Position")]
        [Tooltip("the selected position mode")]
        public PositionModes PositionMode = PositionModes.FeedbackPosition;
        [Tooltip("the position at which to spawn this particle system")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
        public Transform InstantiateParticlesPosition;
        [Tooltip("the world position to move to when in WorldPosition mode")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
        public Vector3 TargetWorldPosition;
        [Tooltip("an offset to apply to the instantiation position")]
        public Vector3 Offset;
        [Tooltip("whether or not the particle system should be nested in hierarchy or floating on its own")]
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform, (int)PositionModes.FeedbackPosition)]
        public bool NestParticles = true;
        [Tooltip("whether or not to also apply rotation")]
        public bool ApplyRotation = false;
        [Tooltip("whether or not to also apply scale")]
        public bool ApplyScale = false;

        protected ParticleSystem _instantiatedParticleSystem;
        protected List<ParticleSystem> _instantiatedRandomParticleSystems;
        protected override void CustomInitialization(GameObject owner)
        {
            if (Active)
            {
                if (Mode == Modes.Cached)
                {
                    InstantiateParticleSystem();
                }
            }
        }
        protected virtual void InstantiateParticleSystem()
        {
            if ((Mode == Modes.OnDemand) && (!CachedRecycle))
            {
            }
            else
            {
                if (_instantiatedParticleSystem != null)
                {
                    Destroy(_instantiatedParticleSystem.gameObject);
                }
            }

            if (RandomParticlePrefabs.Count > 0)
            {
                if (Mode == Modes.Cached)
                {
                    _instantiatedRandomParticleSystems = new List<ParticleSystem>();
                    foreach(ParticleSystem system in RandomParticlePrefabs)
                    {
                        ParticleSystem newSystem = GameObject.Instantiate(system) as ParticleSystem;
                        _instantiatedRandomParticleSystems.Add(newSystem);
                    }
                }
                else
                {
                    int random = Random.Range(0, RandomParticlePrefabs.Count);
                    _instantiatedParticleSystem = GameObject.Instantiate(RandomParticlePrefabs[random]) as ParticleSystem;
                }
            }
            else
            {
                _instantiatedParticleSystem = GameObject.Instantiate(ParticlesPrefab) as ParticleSystem;
            }

            if (_instantiatedParticleSystem != null)
            {
                PositionParticleSystem(_instantiatedParticleSystem);
            }

            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    PositionParticleSystem(system);
                }
            }
        }

        protected virtual void PositionParticleSystem(ParticleSystem system)
        {
            if (InstantiateParticlesPosition == null)
            {
                if (Owner != null)
                {
                    InstantiateParticlesPosition = Owner.transform;
                }
            }

            if (system != null)
            {
                system.Stop();
            }

            if (NestParticles)
            {
                if (PositionMode == PositionModes.FeedbackPosition)
                {
                    system.transform.SetParent(this.transform);
                }
                if (PositionMode == PositionModes.Transform)
                {
                    system.transform.SetParent(InstantiateParticlesPosition);
                }
            }

            system.transform.position = GetPosition(this.transform.position);
            if (ApplyRotation)
            {
                system.transform.rotation = GetRotation(this.transform);    
            }

            if (ApplyScale)
            {
                system.transform.localScale = GetScale(this.transform);    
            }
            
            system.Clear();
        }
        protected virtual Quaternion GetRotation(Transform target)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.rotation;
                case PositionModes.Transform:
                    return InstantiateParticlesPosition.rotation;
                case PositionModes.WorldPosition:
                    return Quaternion.identity;
                case PositionModes.Script:
                    return this.transform.rotation;
                default:
                    return this.transform.rotation;
            }
        }
        protected virtual Vector3 GetScale(Transform target)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.localScale;
                case PositionModes.Transform:
                    return InstantiateParticlesPosition.localScale;
                case PositionModes.WorldPosition:
                    return this.transform.localScale;
                case PositionModes.Script:
                    return this.transform.localScale;
                default:
                    return this.transform.localScale;
            }
        }
        protected virtual Vector3 GetPosition(Vector3 position)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.position + Offset;
                case PositionModes.Transform:
                    return InstantiateParticlesPosition.position + Offset;
                case PositionModes.WorldPosition:
                    return TargetWorldPosition + Offset;
                case PositionModes.Script:
                    return position + Offset;
                default:
                    return position + Offset;
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (Mode == Modes.OnDemand)
            {
                InstantiateParticleSystem();
            }

            if (_instantiatedParticleSystem != null)
            {
                _instantiatedParticleSystem.Stop();
                _instantiatedParticleSystem.transform.position = GetPosition(position);
                _instantiatedParticleSystem.Play();
            }

            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    system.Stop();
                    system.transform.position = GetPosition(position);
                }
                int random = Random.Range(0, _instantiatedRandomParticleSystems.Count);
                _instantiatedRandomParticleSystems[random].Play();
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }
            if (_instantiatedParticleSystem != null)
            {
                _instantiatedParticleSystem?.Stop();
            }    
            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach(ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    system.Stop();
                }
            }
        }
        protected override void CustomReset()
        {
            base.CustomReset();

            if (!Active)
            {
                return;
            }

            if (InCooldown)
            {
                return;
            }

            if (_instantiatedParticleSystem != null)
            {
                _instantiatedParticleSystem?.Stop();
            }
            if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
            {
                foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
                {
                    system.Stop();
                }
            }
        }
    }
}
