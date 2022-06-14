using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Photon.Pun;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Core/Health")]
    public class Health : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("the model to disable (if set so)")]
        public GameObject Model;

        [Header("Status")]
        [MMReadOnly]
        [Tooltip("the current health of the character")]
        public int CurrentHealth;
        [MMReadOnly]
        [Tooltip("If this is true, this object can't take damage at this time")]
        public bool Invulnerable = false;

        [Header("Health")]
        [MMInformation("Add this component to an object and it'll have health, will be able to get damaged and potentially die.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the initial amount of health of the object")]
        public int InitialHealth = 10;
        [Tooltip("the maximum amount of health of the object")]
        public int MaximumHealth = 10;

        [Header("Damage")]
        [MMInformation(
            "Here you can specify an effect and a sound FX to instantiate when the object gets damaged, and also how long the object should flicker when hit (only works for sprites).",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("whether or not this Health object can be damaged")]
        public bool ImmuneToDamage = false;
        [Tooltip("whether or not this object is immune to damage knockback")]
        public bool ImmuneToKnockback = false;
        [Tooltip("the feedback to play when getting damage")]
        public MMFeedbacks DamageMMFeedbacks;
        [Tooltip(
            "if this is true, the damage value will be passed to the MMFeedbacks as its Intensity parameter, letting you trigger more intense feedbacks as damage increases")]
        public bool FeedbackIsProportionalToDamage = false;

        [Header("Death")]
        [MMInformation(
            "Here you can set an effect to instantiate when the object dies, a force to apply to it (topdown controller required), how many points to add to the game score, and where the character should respawn (for non-player characters only).",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("whether or not this object should get destroyed on death")]
        public bool DestroyOnDeath = true;
        [Tooltip("the time (in seconds) before the character is destroyed or disabled")]
        public float DelayBeforeDestruction = 0f;
        [Tooltip("the points the player gets when the object's health reaches zero")]
        public int PointsWhenDestroyed;
        [Tooltip(
            "if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)")]
        public bool RespawnAtInitialLocation = false;
        [Tooltip("if this is true, the controller will be disabled on death")]
        public bool DisableControllerOnDeath = true;
        [Tooltip("if this is true, the model will be disabled instantly on death (if a model has been set)")]
        public bool DisableModelOnDeath = true;
        [Tooltip("if this is true, collisions will be turned off when the character dies")]
        public bool DisableCollisionsOnDeath = true;
        [Tooltip("if this is true, collisions will also be turned off on child colliders when the character dies")]
        public bool DisableChildCollisionsOnDeath = false;
        [Tooltip("whether or not this object should change layer on death")]
        public bool ChangeLayerOnDeath = false;
        [Tooltip("whether or not this object should change layer on death")]
        public bool ChangeLayersRecursivelyOnDeath = false;
        [Tooltip("the layer we should move this character to on death")]
        public MMLayer LayerOnDeath;
        [Tooltip("the feedback to play when dying")]
        public MMFeedbacks DeathMMFeedbacks;
        [Tooltip("if this is true, color will be reset on revive")]
        public bool ResetColorOnRevive = true;
        [Tooltip("the name of the property on your renderer's shader that defines its color")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")]
        public bool UseMaterialPropertyBlocks = false;

        [Header("Shared Health")]
        [Tooltip("another Health component (usually on another character) towards which all health will be redirected")]
        public Health MasterHealth;

        [Header("Settings")]
        [Tooltip("if this is true, animator logs for the associated animator will be turned off to avoid potential spam")]
        public bool DisableAnimatorLogs = true;

        [Tooltip("PhotonView")]
        [SerializeField]
        private PhotonView _photonView;

        public int LastDamage { get; set; }
        public Vector3 LastDamageDirection { get; set; }
        public delegate void OnHitDelegate();

        public OnHitDelegate OnHit;
        public delegate void OnReviveDelegate();

        public OnReviveDelegate OnRevive;
        public delegate void OnDeathDelegate();

        public OnDeathDelegate OnDeath;

        protected Vector3 _initialPosition;
        protected Renderer _renderer;
        protected Character _character;
        protected TopDownController _controller;
        protected MMHealthBar _healthBar;
        protected Collider2D _collider2D;
        protected Collider _collider3D;
        protected CharacterController _characterController;
        protected bool _initialized = false;
        protected Color _initialColor;
        protected AutoRespawn _autoRespawn;
        protected Animator _animator;
        protected int _initialLayer;
        protected MaterialPropertyBlock _propertyBlock;
        protected bool _hasColorProperty = false;
        protected virtual void Awake()
        {
            Initialization();
            SetInitialHealth();
        }

        public virtual void SetInitialHealth()
        {
            if (MasterHealth == null)
            {
                SetHealth(InitialHealth);
            }
            else
            {
                CurrentHealth = MasterHealth.CurrentHealth;
            }
        }
        public virtual void Initialization()
        {
            _character = this.gameObject.GetComponent<Character>();

            if (Model != null)
            {
                Model.SetActive(true);
            }

            if (gameObject.MMGetComponentNoAlloc<Renderer>() != null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_character != null)
            {
                if (_character.CharacterModel != null)
                {
                    if (_character.CharacterModel.GetComponentInChildren<Renderer>() != null)
                    {
                        _renderer = _character.CharacterModel.GetComponentInChildren<Renderer>();
                    }
                }
            }

            if (_renderer != null)
            {
                if (UseMaterialPropertyBlocks && (_propertyBlock == null))
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }

                if (ResetColorOnRevive)
                {
                    if (UseMaterialPropertyBlocks)
                    {
                        if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
                        {
                            _hasColorProperty = true;
                            _initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
                        }
                    }
                    else
                    {
                        if (_renderer.material.HasProperty(ColorMaterialPropertyName))
                        {
                            _hasColorProperty = true;
                            _initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
                        }
                    }
                }
            }
            if (_character != null)
            {
                if (_character.CharacterAnimator != null)
                {
                    _animator = _character.CharacterAnimator;
                }
                else
                {
                    _animator = GetComponent<Animator>();
                }
            }
            else
            {
                _animator = GetComponent<Animator>();
            }

            if ((_animator != null) && DisableAnimatorLogs)
            {
                _animator.logWarnings = false;
            }

            _initialLayer = gameObject.layer;

            _autoRespawn = this.gameObject.GetComponent<AutoRespawn>();
            _healthBar = this.gameObject.GetComponent<MMHealthBar>();
            _controller = this.gameObject.GetComponent<TopDownController>();
            _characterController = this.gameObject.GetComponent<CharacterController>();
            _collider2D = this.gameObject.GetComponent<Collider2D>();
            _collider3D = this.gameObject.GetComponent<Collider>();

            DamageMMFeedbacks?.Initialization(this.gameObject);
            DeathMMFeedbacks?.Initialization(this.gameObject);

            StoreInitialPosition();
            _initialized = true;

            DamageEnabled();
        }
        public virtual void StoreInitialPosition()
        {
            _initialPosition = this.transform.position;
        }
        protected virtual void OnEnable()
        {
            SetInitialHealth();

            if (Model != null)
            {
                Model.SetActive(true);
            }

            DamageEnabled();
        }
        public virtual void Damage(int damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection)
        {
            if (Invulnerable || ImmuneToDamage)
            {
                return;
            }

            if (!this.enabled)
            {
                return;
            }
            if ((CurrentHealth <= 0) && (InitialHealth != 0))
            {
                return;
            }
            float previousHealth = CurrentHealth;

            if (MasterHealth != null)
            {
                previousHealth = MasterHealth.CurrentHealth;
                MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
            }
            else
            {
                SetHealth(CurrentHealth - damage);
            }

            LastDamage = damage;
            LastDamageDirection = damageDirection;

            if (OnHit != null)
            {
                OnHit();
            }
            if (invincibilityDuration > 0)
            {
                DamageDisabled();
                StartCoroutine(DamageEnabled(invincibilityDuration));
            }
            MMDamageTakenEvent.Trigger(_character, instigator, CurrentHealth, damage, previousHealth);

            if (_animator != null)
            {
                _animator.SetTrigger("Damage");
            }

            if (FeedbackIsProportionalToDamage)
            {
                DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
            }
            else
            {
                DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
            }
            UpdateHealthBar(true);
            if (MasterHealth != null)
            {
                if (MasterHealth.CurrentHealth <= 0)
                {
                    MasterHealth.CurrentHealth = 0;
                    MasterHealth.Kill();
                }
            }
            else
            {
                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;
                    Kill();
                }
            }
        }
        public virtual void Kill()
        {
            if (ImmuneToDamage)
            {
                return;
            }

            if (_character != null)
            {
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
                _character.Reset();

                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    if (_photonView.IsMine)
                    {
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
                    }
                    else
                    {
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelComplete, _character);
                    }
                }
            }

            SetHealth(0);
            DamageDisabled();

            DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);
            if (PointsWhenDestroyed != 0)
            {
                TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
            }

            if (_animator != null)
            {
                _animator.SetTrigger("Death");
            }
            if (DisableCollisionsOnDeath)
            {
                if (_collider2D != null)
                {
                    _collider2D.enabled = false;
                }

                if (_collider3D != null)
                {
                    _collider3D.enabled = false;
                }
                if (_controller != null)
                {
                    _controller.CollisionsOff();
                }

                if (DisableChildCollisionsOnDeath)
                {
                    foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                    {
                        collider.enabled = false;
                    }

                    foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }
            }

            if (ChangeLayerOnDeath)
            {
                gameObject.layer = LayerOnDeath.LayerIndex;

                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
                }
            }

            OnDeath?.Invoke();

            if (DisableControllerOnDeath && (_controller != null))
            {
                _controller.enabled = false;
            }

            if (DisableControllerOnDeath && (_characterController != null))
            {
                _characterController.enabled = false;
            }

            if (DisableModelOnDeath && (Model != null))
            {
                Model.SetActive(false);
            }

            if (DelayBeforeDestruction > 0f)
            {
                Invoke("DestroyObject", DelayBeforeDestruction);
            }
            else
            {
                DestroyObject();
            }
        }
        public virtual void Revive()
        {
            if (!_initialized)
            {
                return;
            }

            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }

            if (_collider3D != null)
            {
                _collider3D.enabled = true;
            }

            if (DisableChildCollisionsOnDeath)
            {
                foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = true;
                }

                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = true;
                }
            }

            if (ChangeLayerOnDeath)
            {
                gameObject.layer = _initialLayer;

                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(_initialLayer);
                }
            }

            if (_characterController != null)
            {
                _characterController.enabled = true;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
                _controller.CollisionsOn();
                _controller.Reset();
            }

            if (_character != null)
            {
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            }

            if (ResetColorOnRevive && (_renderer != null))
            {
                if (UseMaterialPropertyBlocks)
                {
                    _renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
                    _renderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    _renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
                }
            }

            if (RespawnAtInitialLocation)
            {
                transform.position = _initialPosition;
            }

            if (_healthBar != null)
            {
                _healthBar.Initialization();
            }

            Initialization();
            SetInitialHealth();
            OnRevive?.Invoke();
        }
        protected virtual void DestroyObject()
        {
            if (_autoRespawn == null)
            {
                if (DestroyOnDeath)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                _autoRespawn.Kill();
            }
        }
        public virtual void GetHealth(int health, GameObject instigator)
        {
            if (MasterHealth != null)
            {
                MasterHealth.SetHealth(Mathf.Min(CurrentHealth + health, MaximumHealth));
            }
            else
            {
                SetHealth(Mathf.Min(CurrentHealth + health, MaximumHealth));
            }

            UpdateHealthBar(true);
        }
        public virtual void ResetHealthToMaxHealth()
        {
            SetHealth(MaximumHealth);
        }
        public virtual void SetHealth(int newValue)
        {
            CurrentHealth = newValue;
            UpdateHealthBar(false);
        }
        public virtual void UpdateHealthBar(bool show)
        {
            if (_healthBar != null)
            {
                _healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
            }

            if (MasterHealth == null)
            {
                if (_character != null)
                {
                    if (_character.CharacterType == Character.CharacterTypes.Player)
                    {
                        if (GUIManager.Instance != null)
                        {
                            GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
                        }
                    }
                }
            }
        }
        public virtual void DamageDisabled()
        {
            Invulnerable = true;
        }
        public virtual void DamageEnabled()
        {
            Invulnerable = false;
        }
        public virtual IEnumerator DamageEnabled(float delay)
        {
            yield return new WaitForSeconds(delay);

            Invulnerable = false;
        }
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }
    }
}