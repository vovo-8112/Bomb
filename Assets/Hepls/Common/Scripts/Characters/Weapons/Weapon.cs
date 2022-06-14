using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using Photon.Pun;

namespace MoreMountains.TopDownEngine
{
    [SelectionBase]
    public class Weapon : MMMonoBehaviour
    {
        [MMInspectorGroup("ID", true, 7)]
        [Tooltip("the name of the weapon, only used for debugging")]
        public string WeaponName;
        public enum TriggerModes
        {
            SemiAuto,
            Auto
        }
        public enum WeaponStates
        {
            WeaponIdle,
            WeaponStart,
            WeaponDelayBeforeUse,
            WeaponUse,
            WeaponDelayBetweenUses,
            WeaponStop,
            WeaponReloadNeeded,
            WeaponReloadStart,
            WeaponReload,
            WeaponReloadStop,
            WeaponInterrupted
        }
        [MMReadOnly]
        [Tooltip("whether or not the weapon is currently active")]
        public bool WeaponCurrentlyActive = true;

        [MMInspectorGroup("Use", true, 10)]
        [Tooltip("is this weapon on semi or full auto ?")]
        public TriggerModes TriggerMode = TriggerModes.Auto;
        [Tooltip("the delay before use, that will be applied for every shot")]
        public float DelayBeforeUse = 0f;
        [Tooltip(
            "whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)")]
        public bool DelayBeforeUseReleaseInterruption = true;
        [Tooltip("the time (in seconds) between two shots")]
        public float TimeBetweenUses = 1f;
        [Tooltip(
            "whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)")]
        public bool TimeBetweenUsesReleaseInterruption = true;

        [Header("Burst Mode")]
        [Tooltip("if this is true, the weapon will activate repeatedly for every shoot request")]
        public bool UseBurstMode = false;
        [Tooltip("the amount of 'shots' in a burst sequence")]
        public int BurstLength = 3;
        [Tooltip("the time between shots in a burst sequence (in seconds)")]
        public float BurstTimeBetweenShots = 0.1f;

        [MMInspectorGroup("Magazine", true, 11)]
        [Tooltip("whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool")]
        public bool MagazineBased = false;
        [Tooltip("the size of the magazine")]
        public int MagazineSize = 30;
        [Tooltip("if this is true, pressing the fire button when a reload is needed will reload the weapon. Otherwise you'll need to press the reload button")]
        public bool AutoReload;
        [Tooltip("the time it takes to reload the weapon")]
        public float ReloadTime = 2f;
        [Tooltip("the amount of ammo consumed everytime the weapon fires")]
        public int AmmoConsumedPerShot = 1;
        [Tooltip("if this is set to true, the weapon will auto destroy when there's no ammo left")]
        public bool AutoDestroyWhenEmpty;
        [Tooltip("the delay (in seconds) before weapon destruction if empty")]
        public float AutoDestroyWhenEmptyDelay = 1f;
        [MMReadOnly]
        [Tooltip("the current amount of ammo loaded inside the weapon")]
        public int CurrentAmmoLoaded = 0;

        [MMInspectorGroup("Position", true, 12)]
        [Tooltip("an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.")]
        public Vector3 WeaponAttachmentOffset = Vector3.zero;
        [Tooltip("should that weapon be flipped when the character flips?")]
        public bool FlipWeaponOnCharacterFlip = true;
        [Tooltip(
            "the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs")]
        public Vector3 RightFacingFlipValue = new Vector3(1, 1, 1);
        [Tooltip(
            "the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs")]
        public Vector3 LeftFacingFlipValue = new Vector3(-1, 1, 1);
        [Tooltip("a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)")]
        public Transform WeaponUseTransform;

        [MMInspectorGroup("IK", true, 13)]
        [Tooltip("the transform to which the character's left hand should be attached to")]
        public Transform LeftHandHandle;
        [Tooltip("the transform to which the character's right hand should be attached to")]
        public Transform RightHandHandle;

        [MMInspectorGroup("Movement", true, 14)]
        [Tooltip("if this is true, a multiplier will be applied to movement while the weapon is active")]
        public bool ModifyMovementWhileAttacking = false;
        [Tooltip("the multiplier to apply to movement while attacking")]
        public float MovementMultiplier = 0f;
        [Tooltip("if this is true all movement will be prevented (even flip) while the weapon is active")]
        public bool PreventAllMovementWhileInUse = false;
        [Tooltip("if this is true all aim will be prevented while the weapon is active")]
        public bool PreventAllAimWhileInUse = false;

        [MMInspectorGroup("Recoil", true, 15)]
        [Tooltip("the force to apply to push the character back when shooting")]
        public float RecoilForce = 0f;

        [MMInspectorGroup("Animation", true, 16)]
        [Tooltip("the other animators (other than the Character's) that you want to update every time this weapon gets used")]
        public List<Animator> Animators;
        [Tooltip(
            "If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.")]
        public bool PerformAnimatorSanityChecks = false;

        [MMInspectorGroup("Animation Parameters Names", true, 17)]
        [Tooltip("the ID of the weapon to pass to the animator")]
        public int WeaponAnimationID = 0;
        [Tooltip("the name of the weapon's idle animation parameter : this will be true all the time except when the weapon is being used")]
        public string IdleAnimationParameter;
        [Tooltip("the name of the weapon's start animation parameter : true at the frame where the weapon starts being used")]
        public string StartAnimationParameter;
        [Tooltip("the name of the weapon's delay before use animation parameter : true when the weapon has been activated but hasn't been used yet")]
        public string DelayBeforeUseAnimationParameter;
        [Tooltip("the name of the weapon's single use animation parameter : true at each frame the weapon activates (shoots)")]
        public string SingleUseAnimationParameter;
        [Tooltip("the name of the weapon's in use animation parameter : true at each frame the weapon has started firing but hasn't stopped yet")]
        public string UseAnimationParameter;
        [Tooltip("the name of the weapon's delay between each use animation parameter : true when the weapon is in use")]
        public string DelayBetweenUsesAnimationParameter;
        [Tooltip("the name of the weapon stop animation parameter : true after a shot and before the next one or the weapon's stop ")]
        public string StopAnimationParameter;
        [Tooltip("the name of the weapon reload start animation parameter")]
        public string ReloadStartAnimationParameter;
        [Tooltip("the name of the weapon reload animation parameter")]
        public string ReloadAnimationParameter;
        [Tooltip("the name of the weapon reload end animation parameter")]
        public string ReloadStopAnimationParameter;
        [Tooltip("the name of the weapon's angle animation parameter")]
        public string WeaponAngleAnimationParameter;
        [Tooltip("the name of the weapon's angle animation parameter, adjusted so it's always relative to the direction the character is currently facing")]
        public string WeaponAngleRelativeAnimationParameter;

        [MMInspectorGroup("Feedbacks", true, 18)]
        [Tooltip("the feedback to play when the weapon starts being used")]
        public MMFeedbacks WeaponStartMMFeedback;
        [Tooltip("the feedback to play while the weapon is in use")]
        public MMFeedbacks WeaponUsedMMFeedback;
        [Tooltip("the feedback to play when the weapon stops being used")]
        public MMFeedbacks WeaponStopMMFeedback;
        [Tooltip("the feedback to play when the weapon gets reloaded")]
        public MMFeedbacks WeaponReloadMMFeedback;
        [Tooltip("the feedback to play when the weapon gets reloaded")]
        public MMFeedbacks WeaponReloadNeededMMFeedback;

        [MMInspectorGroup("Settings", true, 19)]
        [Tooltip(
            "If this is true, the weapon will initialize itself on start, otherwise it'll have to be init manually, usually by the CharacterHandleWeapon class")]
        public bool InitializeOnStart = false;
        [Tooltip("whether or not this weapon can be interrupted")]
        public bool Interruptable = false;
        public string WeaponID { get; set; }
        public Character Owner { get; protected set; }
        public CharacterHandleWeapon CharacterHandleWeapon { get; set; }
        [MMReadOnly]
        [Tooltip("if true, the weapon is flipped right now")]
        public bool Flipped;
        public WeaponAmmo WeaponAmmo { get; protected set; }
        public MMStateMachine<WeaponStates> WeaponState;

        protected SpriteRenderer _spriteRenderer;
        protected WeaponAim _weaponAim;
        protected float _movementMultiplierStorage = 1f;
        protected Animator _ownerAnimator;
        protected float _delayBeforeUseCounter = 0f;
        protected float _delayBetweenUsesCounter = 0f;
        protected float _reloadingCounter = 0f;
        protected bool _triggerReleased = false;
        protected bool _reloading = false;
        protected ComboWeapon _comboWeapon;
        protected TopDownController _controller;
        protected CharacterMovement _characterMovement;
        protected Vector3 _weaponOffset;
        protected Vector3 _weaponAttachmentOffset;
        protected Transform _weaponAttachment;
        protected List<HashSet<int>> _animatorParameters;
        protected HashSet<int> _ownerAnimatorParameters;

        protected const string _aliveAnimationParameterName = "Alive";
        protected int _idleAnimationParameter;
        protected int _startAnimationParameter;
        protected int _delayBeforeUseAnimationParameter;
        protected int _singleUseAnimationParameter;
        protected int _useAnimationParameter;
        protected int _delayBetweenUsesAnimationParameter;
        protected int _stopAnimationParameter;
        protected int _reloadStartAnimationParameter;
        protected int _reloadAnimationParameter;
        protected int _reloadStopAnimationParameter;
        protected int _weaponAngleAnimationParameter;
        protected int _weaponAngleRelativeAnimationParameter;
        protected int _aliveAnimationParameter;
        protected int _comboInProgressAnimationParameter;

        protected PhotonView m_PhotonView;

        public void GetPhotonView(PhotonView photonView)
        {
            m_PhotonView = photonView;
        }
        protected virtual void Start()
        {
            if (InitializeOnStart)
            {
                Initialization();
            }
        }
        public virtual void Initialization()
        {
            Flipped = false;
            _spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            _comboWeapon = this.gameObject.GetComponent<ComboWeapon>();

            WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
            WeaponAmmo = GetComponent<WeaponAmmo>();
            _animatorParameters = new List<HashSet<int>>();
            _weaponAim = GetComponent<WeaponAim>();
            InitializeAnimatorParameters();

            if (WeaponAmmo == null)
            {
                CurrentAmmoLoaded = MagazineSize;
            }

            InitializeFeedbacks();
        }

        protected virtual void InitializeFeedbacks()
        {
            WeaponStartMMFeedback?.Initialization(this.gameObject);
            WeaponUsedMMFeedback?.Initialization(this.gameObject);
            WeaponStopMMFeedback?.Initialization(this.gameObject);
            WeaponReloadNeededMMFeedback?.Initialization(this.gameObject);
            WeaponReloadMMFeedback?.Initialization(this.gameObject);
        }
        public virtual void InitializeComboWeapons()
        {
            if (_comboWeapon != null)
            {
                _comboWeapon.Initialization();
            }
        }
        public virtual void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
        {
            Owner = newOwner;

            if (Owner != null)
            {
                CharacterHandleWeapon = handleWeapon;
                _characterMovement = Owner.GetComponent<Character>()?.FindAbility<CharacterMovement>();
                _controller = Owner.GetComponent<TopDownController>();

                if (CharacterHandleWeapon.AutomaticallyBindAnimator)
                {
                    if (CharacterHandleWeapon.CharacterAnimator != null)
                    {
                        _ownerAnimator = CharacterHandleWeapon.CharacterAnimator;
                    }

                    if (_ownerAnimator == null)
                    {
                        _ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Character>().CharacterAnimator;
                    }

                    if (_ownerAnimator == null)
                    {
                        _ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Animator>();
                    }
                }
            }
        }
        public virtual void WeaponInputStart()
        {
            if (_reloading)
            {
                return;
            }

            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                _triggerReleased = false;
                TurnWeaponOn();
            }
        }
        protected virtual void TurnWeaponOn()
        {
            TriggerWeaponStartFeedback();
            WeaponState.ChangeState(WeaponStates.WeaponStart);

            if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
            {
                _movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
                _characterMovement.MovementSpeedMultiplier = MovementMultiplier;
            }

            if (_comboWeapon != null)
            {
                _comboWeapon.WeaponStarted(this);
            }

            if (PreventAllMovementWhileInUse && (_characterMovement != null) && (_controller != null))
            {
                _characterMovement.SetMovement(Vector2.zero);
                _characterMovement.MovementForbidden = true;
            }

            if (PreventAllAimWhileInUse && (_weaponAim != null))
            {
                _weaponAim.enabled = false;
            }
        }
        protected virtual void Update()
        {
            FlipWeapon();
            ApplyOffset();
        }
        protected virtual void LateUpdate()
        {
            UpdateAnimator();
            ProcessWeaponState();
        }
        protected virtual void ProcessWeaponState()
        {
            if (WeaponState == null)
            {
                return;
            }

            switch (WeaponState.CurrentState)
            {
                case WeaponStates.WeaponIdle:
                    CaseWeaponIdle();
                    break;

                case WeaponStates.WeaponStart:
                    CaseWeaponStart();
                    break;

                case WeaponStates.WeaponDelayBeforeUse:
                    CaseWeaponDelayBeforeUse();
                    break;

                case WeaponStates.WeaponUse:
                    CaseWeaponUse();
                    break;

                case WeaponStates.WeaponDelayBetweenUses:
                    CaseWeaponDelayBetweenUses();
                    break;

                case WeaponStates.WeaponStop:
                    CaseWeaponStop();
                    break;

                case WeaponStates.WeaponReloadNeeded:
                    CaseWeaponReloadNeeded();
                    break;

                case WeaponStates.WeaponReloadStart:
                    CaseWeaponReloadStart();
                    break;

                case WeaponStates.WeaponReload:
                    CaseWeaponReload();
                    break;

                case WeaponStates.WeaponReloadStop:
                    CaseWeaponReloadStop();
                    break;

                case WeaponStates.WeaponInterrupted:
                    CaseWeaponInterrupted();
                    break;
            }
        }
        public virtual void CaseWeaponIdle()
        {
            ResetMovementMultiplier();
        }
        public virtual void CaseWeaponStart()
        {
            if (DelayBeforeUse > 0)
            {
                _delayBeforeUseCounter = DelayBeforeUse;
                WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
            }
            else
            {
                StartCoroutine(ShootRequestCo());
            }
        }
        public virtual void CaseWeaponDelayBeforeUse()
        {
            _delayBeforeUseCounter -= Time.deltaTime;

            if (_delayBeforeUseCounter <= 0)
            {
                StartCoroutine(ShootRequestCo());
            }
        }
        public virtual void CaseWeaponUse()
        {
            WeaponUse();
            _delayBetweenUsesCounter = TimeBetweenUses;
            WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
        }
        public virtual void CaseWeaponDelayBetweenUses()
        {
            _delayBetweenUsesCounter -= Time.deltaTime;

            if (_delayBetweenUsesCounter <= 0)
            {
                if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
                {
                    StartCoroutine(ShootRequestCo());
                }
                else
                {
                    TurnWeaponOff();
                }
            }
        }
        public virtual void CaseWeaponStop()
        {
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }
        public virtual void CaseWeaponReloadNeeded()
        {
            ReloadNeeded();
            ResetMovementMultiplier();
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }
        public virtual void CaseWeaponReloadStart()
        {
            ReloadWeapon();
            _reloadingCounter = ReloadTime;
            WeaponState.ChangeState(WeaponStates.WeaponReload);
        }
        public virtual void CaseWeaponReload()
        {
            ResetMovementMultiplier();
            _reloadingCounter -= Time.deltaTime;

            if (_reloadingCounter <= 0)
            {
                WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
            }
        }
        public virtual void CaseWeaponReloadStop()
        {
            _reloading = false;
            WeaponState.ChangeState(WeaponStates.WeaponIdle);

            if (WeaponAmmo == null)
            {
                CurrentAmmoLoaded = MagazineSize;
            }
        }
        public virtual void CaseWeaponInterrupted()
        {
            TurnWeaponOff();
            ResetMovementMultiplier();
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }
        public virtual void Interrupt()
        {
            if (Interruptable)
            {
                WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
            }
        }
        public virtual IEnumerator ShootRequestCo()
        {
            int remainingShots = UseBurstMode ? BurstLength : 1;
            float interval = UseBurstMode ? BurstTimeBetweenShots : 1;

            while (remainingShots > 0)
            {
                ShootRequest();
                remainingShots--;
                yield return MMCoroutine.WaitFor(interval);
            }
        }

        public virtual void ShootRequest()
        {
            if (_reloading)
            {
                return;
            }

            if (MagazineBased)
            {
                if (WeaponAmmo != null)
                {
                    if (WeaponAmmo.EnoughAmmoToFire())
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponUse);
                    }
                    else
                    {
                        if (AutoReload && MagazineBased)
                        {
                            InitiateReloadWeapon();
                        }
                        else
                        {
                            WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
                        }
                    }
                }
                else
                {
                    if (CurrentAmmoLoaded > 0)
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponUse);
                        CurrentAmmoLoaded -= AmmoConsumedPerShot;
                    }
                    else
                    {
                        if (AutoReload)
                        {
                            InitiateReloadWeapon();
                        }
                        else
                        {
                            WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
                        }
                    }
                }
            }
            else
            {
                if (WeaponAmmo != null)
                {
                    if (WeaponAmmo.EnoughAmmoToFire())
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponUse);
                    }
                    else
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
                    }
                }
                else
                {
                    WeaponState.ChangeState(WeaponStates.WeaponUse);
                }
            }
        }
        public virtual void WeaponUse()
        {
            if ((RecoilForce > 0f) && (_controller != null))
            {
                if (Owner != null)
                {
                    if (Owner.Orientation2D != null)
                    {
                        if (Flipped)
                        {
                            _controller.Impact(this.transform.right, RecoilForce);
                        }
                        else
                        {
                            _controller.Impact(-this.transform.right, RecoilForce);
                        }
                    }
                    else
                    {
                        _controller.Impact(-this.transform.forward, RecoilForce);
                    }
                }
            }

            TriggerWeaponUsedFeedback();
        }
        public virtual void WeaponInputStop()
        {
            if (_reloading)
            {
                return;
            }

            _triggerReleased = true;

            if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
            }
        }
        public virtual void TurnWeaponOff()
        {
            if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
            {
                return;
            }

            _triggerReleased = true;

            TriggerWeaponStopFeedback();
            WeaponState.ChangeState(WeaponStates.WeaponStop);
            ResetMovementMultiplier();

            if (_comboWeapon != null)
            {
                _comboWeapon.WeaponStopped(this);
            }

            if (PreventAllMovementWhileInUse && (_characterMovement != null))
            {
                _characterMovement.MovementForbidden = false;
            }

            if (PreventAllAimWhileInUse && (_weaponAim != null))
            {
                _weaponAim.enabled = true;
            }
        }

        protected virtual void ResetMovementMultiplier()
        {
            if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
            }
        }
        public virtual void ReloadNeeded()
        {
            TriggerWeaponReloadNeededFeedback();
        }
        public virtual void InitiateReloadWeapon()
        {
            if (_reloading || !MagazineBased)
            {
                return;
            }

            if (PreventAllMovementWhileInUse && (_characterMovement != null))
            {
                _characterMovement.MovementForbidden = false;
            }

            if (PreventAllAimWhileInUse && (_weaponAim != null))
            {
                _weaponAim.enabled = true;
            }

            WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
            _reloading = true;
        }
        protected virtual void ReloadWeapon()
        {
            if (MagazineBased)
            {
                TriggerWeaponReloadFeedback();
            }
        }
        public virtual void FlipWeapon()
        {
            if (Owner == null)
            {
                return;
            }

            if (Owner.Orientation2D == null)
            {
                return;
            }

            if (FlipWeaponOnCharacterFlip)
            {
                Flipped = !Owner.Orientation2D.IsFacingRight;

                if (_spriteRenderer != null)
                {
                    _spriteRenderer.flipX = Flipped;
                }
                else
                {
                    transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
                }
            }

            if (_comboWeapon != null)
            {
                _comboWeapon.FlipUnusedWeapons();
            }
        }
        public virtual IEnumerator WeaponDestruction()
        {
            yield return new WaitForSeconds(AutoDestroyWhenEmptyDelay);
            TurnWeaponOff();
            Destroy(this.gameObject);

            if (WeaponID != null)
            {
                List<int> weaponList = Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory
                    .InventoryContains(WeaponID);

                if (weaponList.Count > 0)
                {
                    Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.DestroyItem(weaponList[0]);
                }
            }
        }
        public virtual void ApplyOffset()
        {
            if (!WeaponCurrentlyActive)
            {
                return;
            }

            _weaponAttachmentOffset = WeaponAttachmentOffset;

            if (Owner == null)
            {
                return;
            }

            if (Owner.Orientation2D != null)
            {
                if (Flipped)
                {
                    _weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;
                }
                if (transform.parent != null)
                {
                    _weaponOffset = transform.parent.position + _weaponAttachmentOffset;
                    transform.position = _weaponOffset;
                }
            }
            else
            {
                if (transform.parent != null)
                {
                    _weaponOffset = _weaponAttachmentOffset;
                    transform.localPosition = _weaponOffset;
                }
            }
        }
        protected virtual void TriggerWeaponStartFeedback()
        {
            WeaponStartMMFeedback?.PlayFeedbacks(this.transform.position);
        }
        protected virtual void TriggerWeaponUsedFeedback()
        {
            WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
        }
        protected virtual void TriggerWeaponStopFeedback()
        {
            WeaponStopMMFeedback?.PlayFeedbacks(this.transform.position);
        }
        protected virtual void TriggerWeaponReloadNeededFeedback()
        {
            WeaponReloadNeededMMFeedback?.PlayFeedbacks(this.transform.position);
        }
        protected virtual void TriggerWeaponReloadFeedback()
        {
            WeaponReloadMMFeedback?.PlayFeedbacks(this.transform.position);
        }
        public virtual void InitializeAnimatorParameters()
        {
            if (Animators.Count > 0)
            {
                for (int i = 0; i < Animators.Count; i++)
                {
                    _animatorParameters.Add(new HashSet<int>());
                    AddParametersToAnimator(Animators[i], _animatorParameters[i]);

                    if (!PerformAnimatorSanityChecks)
                    {
                        Animators[i].logWarnings = false;
                    }
                }
            }

            if (_ownerAnimator != null)
            {
                _ownerAnimatorParameters = new HashSet<int>();
                AddParametersToAnimator(_ownerAnimator, _ownerAnimatorParameters);

                if (!PerformAnimatorSanityChecks)
                {
                    _ownerAnimator.logWarnings = false;
                }
            }
        }

        protected virtual void AddParametersToAnimator(Animator animator, HashSet<int> list)
        {
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleAnimationParameter, out _weaponAngleAnimationParameter,
                AnimatorControllerParameterType.Float, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleRelativeAnimationParameter, out _weaponAngleRelativeAnimationParameter,
                AnimatorControllerParameterType.Float, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, IdleAnimationParameter, out _idleAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StartAnimationParameter, out _startAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBeforeUseAnimationParameter, out _delayBeforeUseAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBetweenUsesAnimationParameter, out _delayBetweenUsesAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StopAnimationParameter, out _stopAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStartAnimationParameter, out _reloadStartAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStopAnimationParameter, out _reloadStopAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadAnimationParameter, out _reloadAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SingleUseAnimationParameter, out _singleUseAnimationParameter,
                AnimatorControllerParameterType.Bool, list);

            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, UseAnimationParameter, out _useAnimationParameter, AnimatorControllerParameterType.Bool,
                list);

            if (_comboWeapon != null)
            {
                MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, _comboWeapon.ComboInProgressAnimationParameter,
                    out _comboInProgressAnimationParameter, AnimatorControllerParameterType.Bool, list);
            }
        }
        public virtual void UpdateAnimator()
        {
            for (int i = 0; i < Animators.Count; i++)
            {
                UpdateAnimator(Animators[i], _animatorParameters[i]);
            }

            if ((_ownerAnimator != null) && (WeaponState != null) && (_ownerAnimatorParameters != null))
            {
                UpdateAnimator(_ownerAnimator, _ownerAnimatorParameters);
            }
        }

        protected virtual void UpdateAnimator(Animator animator, HashSet<int> list)
        {
            MMAnimatorExtensions.UpdateAnimatorBool(animator, _idleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), list,
                PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _startAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), list,
                PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBeforeUseAnimationParameter,
                (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), list, PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _useAnimationParameter,
                (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse
                                                                                      || WeaponState.CurrentState
                                                                                      == Weapon.WeaponStates.WeaponDelayBetweenUses), list,
                PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _singleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), list,
                PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBetweenUsesAnimationParameter,
                (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _stopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), list,
                PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStartAnimationParameter,
                (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), list, PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), list,
                PerformAnimatorSanityChecks);

            MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop),
                list, PerformAnimatorSanityChecks);

            if (Owner != null)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(animator, _aliveAnimationParameter,
                    (Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead), list, PerformAnimatorSanityChecks);
            }

            if (_weaponAim != null)
            {
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, _weaponAim.CurrentAngle, list, PerformAnimatorSanityChecks);

                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, _weaponAim.CurrentAngleRelative, list,
                    PerformAnimatorSanityChecks);
            }
            else
            {
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
            }

            if (_comboWeapon != null)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(animator, _comboInProgressAnimationParameter, _comboWeapon.ComboInProgress, list,
                    PerformAnimatorSanityChecks);
            }
        }
    }
}