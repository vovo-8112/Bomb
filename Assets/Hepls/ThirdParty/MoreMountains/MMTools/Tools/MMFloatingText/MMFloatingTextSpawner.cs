using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = System.Random;

namespace MoreMountains.Tools
{
    #region Events
    public struct MMFloatingTextSpawnEvent
    {
        public delegate void Delegate(int channel, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(int channel, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
        {
            OnEvent?.Invoke(channel, spawnPosition, value, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient, useUnscaledTime);
        } 
    }
    #endregion
    public class MMFloatingTextSpawner : MMMonoBehaviour
    {
        public  enum PoolerModes { Simple, Multiple }
        public enum AlignmentModes { Fixed, MatchInitialDirection, MatchMovementDirection }

        [MMInspectorGroup("General Settings", true, 10)]
        [Tooltip("the channel to listen for events on. this will have to be matched in the feedbacks trying to command this spawner")]
        public int Channel = 0;
        [Tooltip("whether or not this spawner can spawn at this time")]
        public bool CanSpawn = true;
        [Tooltip("whether or not this spawner should spawn objects on unscaled time")]
        public bool UseUnscaledTime = false;
        
        [MMInspectorGroup("Pooler", true, 24)]
        [Tooltip("the selected pooler mode (single prefab or multiple ones)")]
        public PoolerModes PoolerMode = PoolerModes.Simple;
        [Tooltip("the prefab to spawn (ignored if in multiple mode)")]
        public MMFloatingText PooledSimpleMMFloatingText;
        [Tooltip("the prefabs to spawn (ignored if in simple mode)")]
        public List<MMFloatingText> PooledMultipleMMFloatingText;
        [Tooltip("the amount of objects to pool to avoid having to instantiate them at runtime. Should be bigger than the max amount of texts you plan on having on screen at any given moment")]
        public int PoolSize = 20;
        [Tooltip("whether or not to nest the waiting pools")]
        public bool NestWaitingPool = true;
        [Tooltip("whether or not to mutualize the waiting pools")]
        public bool MutualizeWaitingPools = true;
        [Tooltip("whether or not the text pool can expand if the pool is empty")]
        public bool PoolCanExpand = true;

        [MMInspectorGroup("Spawn Settings", true, 14)]
        [Tooltip("the random min and max lifetime duration for the spawned texts (in seconds)")]
        [MMVector("Min", "Max")] 
        public Vector2 Lifetime = Vector2.one;
        
        [Header("Spawn Position Offset")]
        [Tooltip("the random min position at which to spawn the text, relative to its intended spawn position")]
        public Vector3 SpawnOffsetMin = Vector3.zero;
        [Tooltip("the random max position at which to spawn the text, relative to its intended spawn position")]
        public Vector3 SpawnOffsetMax = Vector3.zero;

        [MMInspectorGroup("Animate Position", true, 15)] 
        
        [Header("Movement")]
        [Tooltip("whether or not to animate the movement of spawned texts")]
        public bool AnimateMovement = true;
        [Tooltip("whether or not to animate the X movement of spawned texts")]
        public bool AnimateX = false;
        [Tooltip("the value to which the x movement curve's zero should be remapped to")]
        [MMCondition("AnimateX", true)] 
        public Vector2 RemapXZero = Vector2.zero;
        [Tooltip("the value to which the x movement curve's one should be remapped to")]
        [MMCondition("AnimateX", true)] 
        public Vector2 RemapXOne = Vector2.one;
        [Tooltip("the curve on which to animate the x movement")]
        [MMCondition("AnimateX", true)]
        public AnimationCurve AnimateXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [Tooltip("whether or not to animate the Y movement of spawned texts")]
        public bool AnimateY = true;
        [Tooltip("the value to which the y movement curve's zero should be remapped to")]
        [MMCondition("AnimateY", true)] 
        public Vector2 RemapYZero = Vector2.zero;
        [Tooltip("the value to which the y movement curve's one should be remapped to")]
        [MMCondition("AnimateY", true)]
        public Vector2 RemapYOne = new Vector2(5f, 5f);
        [Tooltip("the curve on which to animate the y movement")]
        [MMCondition("AnimateY", true)]
        public AnimationCurve AnimateYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [Tooltip("whether or not to animate the Z movement of spawned texts")]
        public bool AnimateZ = false;
        [Tooltip("the value to which the z movement curve's zero should be remapped to")]
        [MMCondition("AnimateZ", true)] 
        public Vector2 RemapZZero = Vector2.zero;
        [Tooltip("the value to which the z movement curve's one should be remapped to")]
        [MMCondition("AnimateZ", true)] 
        public Vector2 RemapZOne = Vector2.one;
        [Tooltip("the curve on which to animate the z movement")]
        [MMCondition("AnimateZ", true)]
        public AnimationCurve AnimateZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        
        [MMInspectorGroup("Facing Directions", true, 16)]
        
        [Header("Alignment")]
        [Tooltip("the selected alignment mode (whether the spawned text should have a fixed alignment, orient to match the initial spawn direction, or its movement curve)")]
        public AlignmentModes AlignmentMode = AlignmentModes.Fixed;
        [Tooltip("when in fixed mode, the direction in which to keep the spawned texts")]
        [MMEnumCondition("AlignmentMode", (int)AlignmentModes.Fixed)]
        public Vector3 FixedAlignment = Vector3.up;

        [Header("Billboard")]
        [Tooltip("whether or not spawned texts should always face the camera")]
        public bool AlwaysFaceCamera;
        [Tooltip("whether or not this spawner should automatically grab the main camera on start")]
        [MMCondition("AlwaysFaceCamera", true)]
        public bool AutoGrabMainCameraOnStart = true;
        [Tooltip("if not in auto grab mode, the camera to use for billboards")]
        [MMCondition("AlwaysFaceCamera", true)]
        public Camera TargetCamera;
                
        [MMInspectorGroup("Animate Scale", true, 46)]
        [Tooltip("whether or not to animate the scale of spawned texts")]
        public bool AnimateScale = true;
        [Tooltip("the value to which the scale curve's zero should be remapped to")]
        [MMCondition("AnimateScale", true)]
        public Vector2 RemapScaleZero = Vector2.zero;
        [Tooltip("the value to which the scale curve's one should be remapped to")]
        [MMCondition("AnimateScale", true)]
        public Vector2 RemapScaleOne = Vector2.one;
        [Tooltip("the curve on which to animate the scale")]
        [MMCondition("AnimateScale", true)]
        public AnimationCurve AnimateScaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f));
        
        [MMInspectorGroup("Animate Color", true, 55)]
        [Tooltip("whether or not to animate the spawned text's color over time")]
        public bool AnimateColor = false;
        [Tooltip("the gradient over which to animate the spawned text's color over time")]
        [GradientUsage(true)]
        public Gradient AnimateColorGradient = new Gradient();

        [MMInspectorGroup("Animate Opacity", true, 45)]
        [Tooltip("whether or not to animate the opacity of the spawned texts")]
        public bool AnimateOpacity = true;
        [Tooltip("the value to which the opacity curve's zero should be remapped to")]
        [MMCondition("AnimateOpacity", true)]
        public Vector2 RemapOpacityZero = Vector2.zero;
        [Tooltip("the value to which the opacity curve's one should be remapped to")]
        [MMCondition("AnimateOpacity", true)]
        public Vector2 RemapOpacityOne = Vector2.one;
        [Tooltip("the curve on which to animate the opacity")]
        [MMCondition("AnimateOpacity", true)]
        public AnimationCurve AnimateOpacityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f));

        [MMInspectorGroup("Intensity Multipliers", true, 45)]
        [Tooltip("whether or not the intensity multiplier should impact lifetime")]
        public bool IntensityImpactsLifetime = false;
        [Tooltip("when getting an intensity multiplier, the value by which to multiply the lifetime")]
        [MMCondition("IntensityImpactsLifetime", true)]
        public float IntensityLifetimeMultiplier = 1f;
        [Tooltip("whether or not the intensity multiplier should impact movement")]
        public bool IntensityImpactsMovement = false;
        [Tooltip("when getting an intensity multiplier, the value by which to multiply the movement values")]
        [MMCondition("IntensityImpactsMovement", true)]
        public float IntensityMovementMultiplier = 1f;
        [Tooltip("whether or not the intensity multiplier should impact scale")]
        public bool IntensityImpactsScale = false;
        [Tooltip("when getting an intensity multiplier, the value by which to multiply the scale values")]
        [MMCondition("IntensityImpactsScale", true)]
        public float IntensityScaleMultiplier = 1f;

        [MMInspectorGroup("Debug", true, 12)]
        [Tooltip("a random value to display when pressing the TestSpawnOne button")]
        public Vector2Int DebugRandomValue = new Vector2Int(100, 500);
        [Tooltip("the min and max bounds within which to pick a value to output when pressing the TestSpawnMany button")]
        [MMVector("Min", "Max")] 
        public Vector2 DebugInterval = new Vector2(0.3f, 0.5f);
        [Tooltip("a button used to test the spawn of one text")]
        [MMInspectorButton("TestSpawnOne")]
        public bool TestSpawnOneBtn;
        [Tooltip("a button used to start/stop the spawn of texts at regular intervals")]
        [MMInspectorButton("TestSpawnMany")]
        public bool TestSpawnManyBtn;
        
        protected MMObjectPooler _pooler;
        protected MMFloatingText _floatingText;
        protected Coroutine _testSpawnCoroutine;
        
        protected float _lifetime;
        protected float _speed;
        protected Vector3 _spawnOffset;
        protected Vector3 _direction;
        protected Gradient _colorGradient;
        protected bool _animateColor;

        #region Initialization
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            InstantiateObjectPool();
            GrabMainCamera();
        }
        protected virtual void InstantiateObjectPool()
        {
            if (_pooler == null)
            {
                if (PoolerMode == PoolerModes.Simple)
                {
                    InstantiateSimplePool();
                }
                else
                {
                    InstantiateMultiplePool();
                }
            }
        }
        protected virtual void InstantiateSimplePool()
        {
            if (PooledSimpleMMFloatingText == null)
            {
                Debug.LogError(this.name + " : no PooledSimpleMMFloatingText prefab has been set.");
                return;
            }
            GameObject newPooler = new GameObject();
            newPooler.name = PooledSimpleMMFloatingText.name + "_Pooler";
            newPooler.transform.SetParent(this.transform);
            MMSimpleObjectPooler simplePooler = newPooler.AddComponent<MMSimpleObjectPooler>();
            simplePooler.PoolSize = PoolSize;
            simplePooler.GameObjectToPool = PooledSimpleMMFloatingText.gameObject;
            simplePooler.NestWaitingPool = NestWaitingPool;
            simplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
            simplePooler.PoolCanExpand = PoolCanExpand;
            simplePooler.FillObjectPool();
            _pooler = simplePooler;
        }
        protected virtual void InstantiateMultiplePool()
        {
            GameObject newPooler = new GameObject();
            newPooler.name = this.name + "_Pooler";
            newPooler.transform.SetParent(this.transform);
            MMMultipleObjectPooler multiplePooler = newPooler.AddComponent<MMMultipleObjectPooler>();
            multiplePooler.Pool = new List<MMMultipleObjectPoolerObject>();
            foreach (MMFloatingText obj in PooledMultipleMMFloatingText)
            {
                MMMultipleObjectPoolerObject item = new MMMultipleObjectPoolerObject();
                item.GameObjectToPool = obj.gameObject;
                item.PoolCanExpand = PoolCanExpand;
                item.PoolSize = PoolSize;
                item.Enabled = true;
                multiplePooler.Pool.Add(item);
            }
            multiplePooler.NestWaitingPool = NestWaitingPool;
            multiplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
            multiplePooler.FillObjectPool();
            _pooler = multiplePooler;
        }
        protected virtual void GrabMainCamera()
        {
            if (AutoGrabMainCameraOnStart)
            {
                TargetCamera = Camera.main;
            }
        }

        #endregion
        protected virtual void Spawn(string value, Vector3 position, Vector3 direction, float intensity = 1f,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null)
        {
            if (!CanSpawn)
            {
                return;
            }

            _direction = (direction != Vector3.zero) ? direction + this.transform.up : this.transform.up;

            this.transform.position = position;

            GameObject nextGameObject = _pooler.GetPooledGameObject();

            float lifetimeMultiplier = IntensityImpactsLifetime ? intensity * IntensityLifetimeMultiplier : 1f;
            float movementMultiplier = IntensityImpactsMovement ? intensity * IntensityMovementMultiplier : 1f;
            float scaleMultiplier = IntensityImpactsScale ? intensity * IntensityScaleMultiplier : 1f;

            _lifetime = UnityEngine.Random.Range(Lifetime.x, Lifetime.y) * lifetimeMultiplier;
            _spawnOffset = MMMaths.RandomVector3(SpawnOffsetMin, SpawnOffsetMax);
            _animateColor = AnimateColor;
            _colorGradient = AnimateColorGradient;

            float remapXZero = UnityEngine.Random.Range(RemapXZero.x, RemapXZero.y);
            float remapXOne = UnityEngine.Random.Range(RemapXOne.x, RemapXOne.y) * movementMultiplier;
            float remapYZero = UnityEngine.Random.Range(RemapYZero.x, RemapYZero.y);
            float remapYOne = UnityEngine.Random.Range(RemapYOne.x, RemapYOne.y) * movementMultiplier;
            float remapZZero = UnityEngine.Random.Range(RemapZZero.x, RemapZZero.y);
            float remapZOne = UnityEngine.Random.Range(RemapZOne.x, RemapZOne.y) * movementMultiplier;
            float remapOpacityZero = UnityEngine.Random.Range(RemapOpacityZero.x, RemapOpacityZero.y);
            float remapOpacityOne = UnityEngine.Random.Range(RemapOpacityOne.x, RemapOpacityOne.y);
            float remapScaleZero = UnityEngine.Random.Range(RemapScaleZero.x, RemapOpacityZero.y);
            float remapScaleOne = UnityEngine.Random.Range(RemapScaleOne.x, RemapScaleOne.y) * scaleMultiplier;

            if (forceLifetime)
            {
                _lifetime = lifetime;
            }

            if (forceColor)
            {
                _animateColor = true;
                _colorGradient = animateColorGradient;
            }
            if (nextGameObject==null) { return; }
            nextGameObject.gameObject.SetActive(true);
            nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
            nextGameObject.transform.position = this.transform.position + _spawnOffset;

            _floatingText = nextGameObject.MMGetComponentNoAlloc<MMFloatingText>();
            _floatingText.SetUseUnscaledTime(UseUnscaledTime, true);
            _floatingText.ResetPosition();
            _floatingText.SetProperties(value, _lifetime, _direction, AnimateMovement, 
                AlignmentMode, FixedAlignment, AlwaysFaceCamera, TargetCamera,
                AnimateX, AnimateXCurve, remapXZero, remapXOne,
                AnimateY, AnimateYCurve, remapYZero, remapYOne,
                AnimateZ, AnimateZCurve, remapZZero, remapZOne,
                AnimateOpacity, AnimateOpacityCurve, remapOpacityZero, remapOpacityOne,
                AnimateScale, AnimateScaleCurve, remapScaleZero, remapScaleOne,
                _animateColor, _colorGradient);            
        }
        public virtual void OnMMFloatingTextSpawnEvent(int channel, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
            bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
        {
            if (channel != Channel)
            {
                return;
            }

            UseUnscaledTime = useUnscaledTime;
            Spawn(value, spawnPosition, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient);
        }
        protected virtual void OnEnable()
        {
            MMFloatingTextSpawnEvent.Register(OnMMFloatingTextSpawnEvent);
        }
        protected virtual void OnDisable()
        {
            MMFloatingTextSpawnEvent.Unregister(OnMMFloatingTextSpawnEvent);
        }

        #region TestMethods
        protected virtual void TestSpawnOne()
        {
            string test = UnityEngine.Random.Range(DebugRandomValue.x, DebugRandomValue.y).ToString();
            Spawn(test, this.transform.position, Vector3.zero);
        }
        protected virtual void TestSpawnMany()
        {
            if (_testSpawnCoroutine == null)
            {
                _testSpawnCoroutine = StartCoroutine(TestSpawnManyCo());    
            }
            else
            {
                StopCoroutine(_testSpawnCoroutine);
                _testSpawnCoroutine = null;
            }
        }
        protected virtual IEnumerator TestSpawnManyCo()
        {
            float lastSpawnAt = Time.time;
            float interval = UnityEngine.Random.Range(DebugInterval.x, DebugInterval.y);
            while (true)
            {
                if (Time.time - lastSpawnAt > interval)
                {
                    TestSpawnOne();
                    lastSpawnAt = Time.time;
                    interval = UnityEngine.Random.Range(DebugInterval.x, DebugInterval.y);
                }
                yield return null;
            }
        }
        
        #endregion
    }
}
