using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ExplodudesBomb : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("the model of the bomb")]
        public Transform BombModel;
        [Tooltip("the particle system used for the northbound explosion")]
        public ParticleSystem DirectedExplosionN;
        [Tooltip("the particle system used for the southbound explosion")]
        public ParticleSystem DirectedExplosionS;
        [Tooltip("the particle system used for the eastbound explosion")]
        public ParticleSystem DirectedExplosionE;
        [Tooltip("the particle system used for the westbound explosion")]
        public ParticleSystem DirectedExplosionW;

        [Header("Raycasts")]
        [Tooltip("the offset to apply to the base of the obstacle detecting raycast")]
        public Vector3 RaycastOffset = Vector3.zero;
        [Tooltip("the max distance of the raycast (should be bigger than the grid)")]
        public float MaximumRaycastDistance = 50f;
        [Tooltip("the layers to consider as obstacles to the bomb's fire")]
        public LayerMask ObstaclesMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the layers to apply damage to")]
        public LayerMask DamageLayerMask;
        [Tooltip("a small offset to apply to the raycasts")]
        public float SkinWidth = 0.01f;

        [Header("Bomb")]
        [Tooltip("the delay (in seconds) before the bomb's explosion")]
        public float BombDelayBeforeExplosion = 3f;
        [Tooltip("the duration (in seconds) for which the bomb is active")]
        public float BombExplosionActiveDuration = 0.5f;
        [Tooltip("a delay after the bomb has exploded and before it gets destroyed(in seconds)")]
        public float BombAdditionalDelayBeforeDestruction = 1.5f;
        [Tooltip("the damage applied by the bomb to anything with a Health component")]
        public int BombDamage = 10;
        [Tooltip("the distance the bomb affects")]
        public int BombDistanceInGridUnits = 3;

        [Header("Feedbacks")]
        [Tooltip("the feedbacks to play when the bomb explodes")]
        public MMFeedbacks ExplosionFeedbacks;

        [Header("Owner")]
        [MMReadOnly]
        [Tooltip("the owner of the bomb")]
        public GameObject Owner;

        protected BoxCollider _boxCollider;
        protected WaitForSeconds _bombDuration;
        protected WaitForSeconds _explosionDuration;
        protected WaitForSeconds _additionalDelayBeforeDestruction;

        protected RaycastHit _raycastNorth;
        protected RaycastHit _raycastSouth;
        protected RaycastHit _raycastEast;
        protected RaycastHit _raycastWest;

        protected float _obstacleNorthDistance = 0f;
        protected float _obstacleEastDistance = 0f;
        protected float _obstacleWestDistance = 0f;
        protected float _obstacleSouthDistance = 0f;

        protected DamageOnTouch _damageAreaEast;
        protected DamageOnTouch _damageAreaWest;
        protected DamageOnTouch _damageAreaNorth;
        protected DamageOnTouch _damageAreaSouth;
        protected DamageOnTouch _damageAreaCenter;

        protected Vector3 _damageAreaPosition;
        protected Vector3 _damageAreaSize;

        protected Coroutine _delayBeforeExplosionCoroutine;
        protected ExplodudesBomb _otherBomb;
        protected bool _exploded = false;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _bombDuration = new WaitForSeconds(BombDelayBeforeExplosion);
            _explosionDuration = new WaitForSeconds(BombExplosionActiveDuration);
            _additionalDelayBeforeDestruction = new WaitForSeconds(BombAdditionalDelayBeforeDestruction);
            BombModel.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            _boxCollider = this.gameObject.MMGetComponentNoAlloc<BoxCollider>();
            _boxCollider.isTrigger = true;
            _damageAreaEast = CreateDamageArea("East");
            _damageAreaWest = CreateDamageArea("West");
            _damageAreaSouth = CreateDamageArea("South");
            _damageAreaNorth = CreateDamageArea("North");
            _damageAreaCenter = CreateDamageArea("Center");
            _damageAreaSize.x = GridManager.Instance.GridUnitSize / 2f;
            _damageAreaSize.y = GridManager.Instance.GridUnitSize / 2f;
            _damageAreaSize.z = GridManager.Instance.GridUnitSize / 2f;

            _damageAreaPosition = this.transform.position + Vector3.up * GridManager.Instance.GridUnitSize / 2f;

            _damageAreaCenter.gameObject.transform.position = _damageAreaPosition;
            _damageAreaCenter.gameObject.MMFGetComponentNoAlloc<BoxCollider>().size = _damageAreaSize;
        }
        protected virtual void ResetBomb()
        {
            _exploded = false;
            _boxCollider.enabled = true;
            _boxCollider.isTrigger = true;
            BombModel.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            BombModel.gameObject.SetActive(true);
            _delayBeforeExplosionCoroutine = StartCoroutine(DelayBeforeExplosionCoroutine());
        }
        protected virtual DamageOnTouch CreateDamageArea(string name)
        {
            GameObject damageAreaGameObject = new GameObject();
            damageAreaGameObject.SetActive(false);
            damageAreaGameObject.transform.SetParent(this.transform);
            damageAreaGameObject.name = "ExplodudesBombDamageArea" + name;
            damageAreaGameObject.layer = LayerMask.NameToLayer("Enemies");

            DamageOnTouch damageOnTouch = damageAreaGameObject.AddComponent<DamageOnTouch>();
            damageOnTouch.DamageCaused = BombDamage;
            damageOnTouch.TargetLayerMask = DamageLayerMask;
            damageOnTouch.DamageTakenEveryTime = 0;
            damageOnTouch.InvincibilityDuration = 0f;
            damageOnTouch.DamageTakenEveryTime = 10;

            BoxCollider colllider = damageAreaGameObject.AddComponent<BoxCollider>();
            colllider.isTrigger = true;

            return damageOnTouch;
        }
        protected virtual IEnumerator DelayBeforeExplosionCoroutine()
        {
            yield return _bombDuration;

            Detonate();
        }
        public virtual void Detonate()
        {
            if (_exploded)
            {
                return;
            }

            StartCoroutine(DetonateCoroutine());
        }
        protected virtual IEnumerator DetonateCoroutine()
        {
            _exploded = true;
            _boxCollider.enabled = false;
            StopCoroutine(_delayBeforeExplosionCoroutine);
            CastRays();
            DirectedExplosion(_raycastEast, _damageAreaEast, DirectedExplosionE, 90f);
            DirectedExplosion(_raycastWest, _damageAreaWest, DirectedExplosionW, 90f);
            DirectedExplosion(_raycastNorth, _damageAreaNorth, DirectedExplosionN, 0f);
            DirectedExplosion(_raycastSouth, _damageAreaSouth, DirectedExplosionS, 0f);
            _damageAreaCenter.gameObject.SetActive(true);
            ExplosionFeedbacks?.PlayFeedbacks();
            BombModel.gameObject.SetActive(false);

            yield return _explosionDuration;

            _damageAreaEast.gameObject.SetActive(false);
            _damageAreaWest.gameObject.SetActive(false);
            _damageAreaNorth.gameObject.SetActive(false);
            _damageAreaSouth.gameObject.SetActive(false);
            _damageAreaCenter.gameObject.SetActive(false);

            yield return _additionalDelayBeforeDestruction;

            this.gameObject.SetActive(false);
            Destroy(gameObject);
        }
        protected virtual void DirectedExplosion(RaycastHit hit, DamageOnTouch damageArea, ParticleSystem explosion, float angle)
        {
            float hitDistance = hit.distance;
            if (hit.collider.gameObject.MMFGetComponentNoAlloc<Health>() != null)
            {
                hitDistance += GridManager.Instance.GridUnitSize;
            }
            _otherBomb = hit.collider.gameObject.MMFGetComponentNoAlloc<ExplodudesBomb>();

            if ((_otherBomb != null) && (hitDistance <= BombDistanceInGridUnits))
            {
                hitDistance += GridManager.Instance.GridUnitSize;
                _otherBomb.Detonate();
            }
            if (hitDistance <= GridManager.Instance.GridUnitSize / 2f)
            {
                return;
            }
            float explosionLength;
            float adjustedDistance = hitDistance - GridManager.Instance.GridUnitSize / 2f;
            float maxExplosionLength = BombDistanceInGridUnits * GridManager.Instance.GridUnitSize;
            explosionLength = Mathf.Min(adjustedDistance, maxExplosionLength);
            explosionLength -= GridManager.Instance.GridUnitSize / 2f;
            _damageAreaSize.x = GridManager.Instance.GridUnitSize / 2f;
            _damageAreaSize.y = GridManager.Instance.GridUnitSize / 2f;
            _damageAreaSize.z = explosionLength;

            _damageAreaPosition = this.transform.position
                                  + (hit.point - (this.transform.position + RaycastOffset)).normalized
                                  * (explosionLength / 2f + GridManager.Instance.GridUnitSize / 2f) + Vector3.up * GridManager.Instance.GridUnitSize / 2f;

            damageArea.gameObject.transform.position = _damageAreaPosition;
            damageArea.gameObject.transform.LookAt(this.transform.position + Vector3.up * (GridManager.Instance.GridUnitSize / 2f));
            damageArea.gameObject.SetActive(true);
            damageArea.gameObject.MMFGetComponentNoAlloc<BoxCollider>().size = _damageAreaSize;
            explosion.gameObject.SetActive(true);
            explosion.transform.position = _damageAreaPosition;
            ParticleSystem.ShapeModule shape = explosion.shape;
            shape.scale = new Vector3(0.1f, 0.1f, explosionLength);
            shape.rotation = new Vector3(0f, angle, 0f);
            MMGameEvent.Trigger("Bomb");
        }
        protected virtual void CastRays()
        {
            float boxWidth = (_boxCollider.bounds.size.x / 2f) + SkinWidth;
            boxWidth = 0f;

            _raycastEast = MMDebug.Raycast3D(this.transform.position + Vector3.right * boxWidth + RaycastOffset, Vector3.right, MaximumRaycastDistance,
                ObstaclesMask, Color.red, true);

            if (_raycastEast.collider != null)
            {
                _obstacleEastDistance = _raycastEast.distance;
            }
            else
            {
                _obstacleEastDistance = 0f;
            }

            _raycastNorth = MMDebug.Raycast3D(this.transform.position + Vector3.forward * boxWidth + RaycastOffset, Vector3.forward, MaximumRaycastDistance,
                ObstaclesMask, Color.red, true);

            if (_raycastNorth.collider != null)
            {
                _obstacleNorthDistance = _raycastNorth.distance;
            }
            else
            {
                _obstacleNorthDistance = 0f;
            }

            _raycastSouth = MMDebug.Raycast3D(this.transform.position + Vector3.back * boxWidth + RaycastOffset, Vector3.back, MaximumRaycastDistance,
                ObstaclesMask, Color.red, true);

            if (_raycastSouth.collider != null)
            {
                _obstacleSouthDistance = _raycastSouth.distance;
            }
            else
            {
                _obstacleSouthDistance = 0f;
            }

            _raycastWest = MMDebug.Raycast3D(this.transform.position + Vector3.left * boxWidth + RaycastOffset, Vector3.left, MaximumRaycastDistance,
                ObstaclesMask, Color.red, true);

            if (_raycastWest.collider != null)
            {
                _obstacleWestDistance = _raycastWest.distance;
            }
            else
            {
                _obstacleWestDistance = 0f;
            }
        }
        public virtual void OnEnable()
        {
            ResetBomb();
        }
        protected virtual void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject == Owner)
            {
                _boxCollider.isTrigger = false;
            }
        }
    }
}