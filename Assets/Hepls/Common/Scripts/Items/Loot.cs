using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace  MoreMountains.TopDownEngine
{
    public class Loot : MonoBehaviour
    {
        public enum LootModes { Unique, LootTable, LootTableScriptableObject }

        [Header("Loot Mode")]
        [Tooltip("the selected loot mode : - unique : a simple object  - loot table : a LootTable specific to this Loot object - loot definition : a LootTable scriptable object (created by right click > Create > MoreMountains > TopDown Engine > Loot Definition. This loot definition can then be reused in other Loot objects.")]
        public LootModes LootMode = LootModes.Unique;
        [Tooltip("the object to loot, when in LootMode")]
        [MMEnumCondition("LootMode", (int) LootModes.Unique)]
        public GameObject GameObjectToLoot;
        [Tooltip("a loot table defining what objects to spawn")]
        [MMEnumCondition("LootMode", (int) LootModes.LootTable)]
        public MMLootTableGameObject LootTable;
        [Tooltip("a loot table scriptable object defining what objects to spawn")]
        [MMEnumCondition("LootMode", (int) LootModes.LootTableScriptableObject)]
        public MMLootTableGameObjectSO LootTableSO;

        [Header("Conditions")]
        [Tooltip("if this is true, loot will happen when this object dies")]
        public bool SpawnLootOnDeath = true;
        [Tooltip("if this is true, loot will happen when this object takes damage")]
        public bool SpawnLootOnDamage = false;
        
        [Header("Spawn")]
        [Tooltip("a delay (in seconds) to wait for before spawning loot")]
        public float Delay = 0f;
        [Tooltip("the minimum and maximum quantity of objects to spawn")]
        [MMVector("Min","Max")]
        public Vector2 Quantity = Vector2.one;
        [Tooltip("the position, rotation and scale objects should spawn at")]
        public MMSpawnAroundProperties SpawnProperties;
        [Tooltip("The maximum quantity of objects that can be looted from this object")] 
        public int MaximumQuantity = 100;
        [Tooltip("The remaining quantity of objects that can be looted from this Loot object, displayed for debug purposes")]
        [MMReadOnly]
        public int RemainingQuantity = 100;

        [Header("Collisions")]
        [Tooltip("Whether or not spawned objects should try and avoid obstacles")]
        public bool AvoidObstacles = false;
        public enum DimensionModes { TwoD, ThreeD}
        [Tooltip("whether collision detection should happen in 2D or 3D")]
        [MMCondition("AvoidObstacles", true)]
        public DimensionModes DimensionMode = DimensionModes.TwoD;
        [Tooltip("the layer mask containing layers the spawned objects shouldn't collide with")]
        [MMCondition("AvoidObstacles", true)]
        public LayerMask AvoidObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the radius around the object within which no obstacle should be found")]
        [MMCondition("AvoidObstacles", true)]
        public float AvoidRadius = 0.25f;
        [Tooltip("the amount of times the script should try finding another position for the loot if the last one was within an obstacle. More attempts : better results, higher cost")]
        [MMCondition("AvoidObstacles", true)]
        public int MaxAvoidAttempts = 5;
        
        [Header("Feedback")]
        [Tooltip("A MMFeedbacks to play when spawning loot. Only one feedback will play. If you want one per item, it's best to place it on the item itself, and have it play when the object gets instantiated.")]
        public MMFeedbacks LootFeedback;

        [Header("Debug")]
        [Tooltip("if this is true, gizmos will be drawn to show the shape within which loot will spawn")]
        public bool DrawGizmos = false;
        [Tooltip("the amount of gizmos to draw")]
        public int GizmosQuantity = 1000;
        [Tooltip("the color the gizmos should be drawn with")]
        public Color GizmosColor = MMColors.LightGray;
        [Tooltip("the size at which to draw the gizmos")]
        public float GimosSize = 1f;
        [Tooltip("a debug button used to trigger a loot")]
        [MMInspectorButton("SpawnLootDebug")] 
        public bool SpawnLootButton;

        protected Health _health;
        protected GameObject _objectToSpawn;
        protected GameObject _spawnedObject;
        protected Vector3 _raycastOrigin;
        protected RaycastHit2D _raycastHit2D;
        protected Collider[] _overlapBox;
        protected virtual void Awake()
        {
            _health = this.gameObject.GetComponent<Health>();
            InitializeLootTable();
            ResetRemainingQuantity();
        }
        public virtual void ResetRemainingQuantity()
        {
            RemainingQuantity = MaximumQuantity;
        }
        public virtual void InitializeLootTable()
        {
            switch (LootMode)
            {
                case LootModes.LootTableScriptableObject:
                    if (LootTableSO != null)
                    {
                        LootTableSO.ComputeWeights();
                    }
                    break;
                case LootModes.LootTable:
                    LootTable.ComputeWeights();
                    break;
            }
        }
        public virtual void SpawnLoot()
        {
            StartCoroutine(SpawnLootCo());
        }
        protected virtual void SpawnLootDebug()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("This debug button is only meant to be used while in Play Mode.");
                return;
            }

            SpawnLoot();
        }
        protected virtual IEnumerator SpawnLootCo()
        {
            yield return MMCoroutine.WaitFor(Delay);
            int randomQuantity = Random.Range((int)Quantity.x, (int)Quantity.y);
            for (int i = 0; i < randomQuantity; i++)
            {
                SpawnOneLoot();
            }
            LootFeedback?.PlayFeedbacks();
        }
        public virtual void SpawnOneLoot()
        {
            _objectToSpawn = GetObject();

            if (_objectToSpawn == null)
            {
                return;
            }

            if (RemainingQuantity <= 0)
            {
                return;
            }

            _spawnedObject = Instantiate(_objectToSpawn);

            if (AvoidObstacles)
            {
                bool placementOK = false;
                int amountOfAttempts = 0;
                while (!placementOK && (amountOfAttempts < MaxAvoidAttempts))
                {
                    MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);
                    
                    if (DimensionMode == DimensionModes.TwoD)
                    {
                        _raycastOrigin = _spawnedObject.transform.position;
                        _raycastHit2D = Physics2D.BoxCast(_raycastOrigin + Vector3.right * AvoidRadius, AvoidRadius * Vector2.one, 0f, Vector2.left, AvoidRadius, AvoidObstaclesLayerMask);
                        if (_raycastHit2D.collider == null)
                        {
                            placementOK = true;
                        }
                        else
                        {
                            amountOfAttempts++;
                        }
                    }
                    else
                    {
                        _raycastOrigin = _spawnedObject.transform.position;
                        _overlapBox = Physics.OverlapBox(_raycastOrigin, Vector3.one * AvoidRadius, Quaternion.identity, AvoidObstaclesLayerMask);
                        
                        if (_overlapBox.Length == 0)
                        {
                            placementOK = true;
                        }
                        else
                        {
                            amountOfAttempts++;
                        }
                    }
                }
            }
            else
            {
                MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);    
            }
            _spawnedObject.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);
            RemainingQuantity--;
        }
        protected virtual GameObject GetObject()
        {
            _objectToSpawn = null;
            switch (LootMode)
            {
                case LootModes.Unique:
                    _objectToSpawn = GameObjectToLoot;
                    break;
                case LootModes.LootTableScriptableObject:
                    if (LootTableSO == null)
                    {
                        _objectToSpawn = null;
                        break;
                    }
                    _objectToSpawn = LootTableSO.GetLoot();
                    break;
                case LootModes.LootTable:
                    _objectToSpawn = LootTable.GetLoot()?.Loot;
                    break;
            }

            return _objectToSpawn;
        }
        protected virtual void OnHit()
        {
            if (!SpawnLootOnDamage)
            {
                return;
            }

            SpawnLoot();
        }
        protected virtual void OnDeath()
        {
            if (!SpawnLootOnDeath)
            {
                return;
            }

            SpawnLoot();
        }
        protected virtual void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDeath += OnDeath;
                _health.OnHit += OnHit;
            }
        }
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
                _health.OnHit -= OnHit;
            }
        }
        protected virtual void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GimosSize, GizmosColor);    
            }
        }

    }
}
