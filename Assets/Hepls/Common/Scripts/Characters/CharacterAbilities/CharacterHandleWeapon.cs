using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using Photon.Pun;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Weapon")]
    public class CharacterHandleWeapon : CharacterAbility
    {
        [SerializeField]
        private PhotonView m_PhotonView;
        public override string HelpBoxText()
        {
            return
                "This component will allow your character to pickup and use weapons. What the weapon will do is defined in the Weapon classes. This just describes the behaviour of the 'hand' holding the weapon, not the weapon itself. Here you can set an initial weapon for your character to start with, allow weapon pickup, and specify a weapon attachment (a transform inside of your character, could be just an empty child gameobject, or a subpart of your model.";
        }

        [Header("Weapon")]
        [Tooltip("the initial weapon owned by the character")]
        public Weapon InitialWeapon;
        [Tooltip("if this is set to true, the character can pick up PickableWeapons")]
        public bool CanPickupWeapons = true;

        [Header("Feedbacks")]
        [Tooltip("a feedback that gets triggered at the character level everytime the weapon is used")]
        public MMFeedbacks WeaponUseFeedback;

        [Header("Binding")]
        [Tooltip("the position the weapon will be attached to. If left blank, will be this.transform.")]
        public Transform WeaponAttachment;
        [Tooltip("the position from which projectiles will be spawned (can be safely left empty)")]
        public Transform ProjectileSpawn;
        [Tooltip("if this is true this animator will be automatically bound to the weapon")]
        public bool AutomaticallyBindAnimator = true;
        [Tooltip("the ID of the AmmoDisplay this ability should update")]
        public int AmmoDisplayID = 0;
        [Tooltip("if this is true, IK will be automatically setup if possible")]
        public bool AutoIK = true;

        [Header("Input")]
        [Tooltip("if this is true you won't have to release your fire button to auto reload")]
        public bool ContinuousPress = false;
        [Tooltip("whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)")]
        public bool GettingHitInterruptsAttack = false;
        [Tooltip("whether or not pushing the secondary axis above its threshold should cause the weapon to shoot")]
        public bool UseSecondaryAxisThresholdToShoot = false;

        [Header("Buffering")]
        [Tooltip(
            "whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them")]
        public bool BufferInput;
        [MMCondition("BufferInput", true)]
        [Tooltip("if this is true, every new input will prolong the buffer")]
        public bool NewInputExtendsBuffer;
        [MMCondition("BufferInput", true)]
        [Tooltip("the maximum duration for the buffer, in seconds")]
        public float MaximumBufferDuration = 0.25f;
        [MMCondition("BufferInput", true)]
        [Tooltip("if this is true, and if this character is using GridMovement, then input will only be triggered when on a perfect tile")]
        public bool RequiresPerfectTile = false;

        [Header("Debug")]
        [MMReadOnly]
        [Tooltip("the weapon currently equipped by the Character")]
        public Weapon CurrentWeapon;
        public virtual int HandleWeaponID
        {
            get { return 1; }
        }
        public Animator CharacterAnimator { get; set; }
        public WeaponAim WeaponAimComponent
        {
            get { return _weaponAim; }
        }

        public delegate void OnWeaponChangeDelegate();
        public OnWeaponChangeDelegate OnWeaponChange;

        protected float _fireTimer = 0f;
        protected float _secondaryHorizontalMovement;
        protected float _secondaryVerticalMovement;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected WeaponIK _weaponIK;
        protected Transform _leftHandTarget = null;
        protected Transform _rightHandTarget = null;
        protected float _bufferEndsAt = 0f;
        protected bool _buffering = false;
        protected const string _weaponEquippedAnimationParameterName = "WeaponEquipped";
        protected const string _weaponEquippedIDAnimationParameterName = "WeaponEquippedID";
        protected int _weaponEquippedAnimationParameter;
        protected int _weaponEquippedIDAnimationParameter;
        protected CharacterGridMovement _characterGridMovement;
        protected List<WeaponModel> _weaponModels;
        protected override void PreInitialization()
        {
            base.PreInitialization();
            if (WeaponAttachment == null)
            {
                WeaponAttachment = transform;
            }
        }
        protected override void Initialization()
        {
            base.Initialization();
            Setup();
        }
        public virtual void Setup()
        {
            _character = this.gameObject.GetComponentInParent<Character>();
            _characterGridMovement = _character?.FindAbility<CharacterGridMovement>();
            _weaponModels = new List<WeaponModel>();

            foreach (WeaponModel model in _character.gameObject.GetComponentsInChildren<WeaponModel>())
            {
                _weaponModels.Add(model);
            }

            CharacterAnimator = _animator;
            if (WeaponAttachment == null)
            {
                WeaponAttachment = transform;
            }

            if ((_animator != null) && (AutoIK))
            {
                _weaponIK = _animator.GetComponent<WeaponIK>();
            }
            if (InitialWeapon != null)
            {
                ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);
            }
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            HandleFeedbacks();
            UpdateAmmoDisplay();
            HandleBuffer();
        }
        protected virtual void HandleFeedbacks()
        {
            if (CurrentWeapon != null)
            {
                if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
                {
                    WeaponUseFeedback?.PlayFeedbacks();
                }
            }
        }
        protected override void HandleInput()
        {
            if (!AbilityAuthorized || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }

            if ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
                || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown))
            {
                ShootStart();
            }

            if (CurrentWeapon != null)
            {
                bool buttonPressed = (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
                                     || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonPressed);

                if (ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
                {
                    ShootStart();
                }
            }

            if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                Reload();
            }

            if ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonUp))
            {
                ShootStop();
            }

            if (CurrentWeapon != null)
            {
                if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
                    && ((_inputManager.ShootAxis == MMInput.ButtonStates.Off) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.Off)))
                {
                    CurrentWeapon.WeaponInputStop();
                }
            }

            if (UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude))
            {
                ShootStart();
            }
        }
        protected virtual void HandleBuffer()
        {
            if (CurrentWeapon == null)
            {
                return;
            }
            if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle))
            {
                if (Time.time < _bufferEndsAt)
                {
                    ShootStart();
                }
                else
                {
                    _buffering = false;
                }
            }
        }
        public virtual void ShootStart()
        {
            if (!AbilityAuthorized || (CurrentWeapon == null) || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }
            if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
            {
                ExtendBuffer();
            }

            if (BufferInput && RequiresPerfectTile && (_characterGridMovement != null))
            {
                if (!_characterGridMovement.PerfectTile)
                {
                    ExtendBuffer();
                    return;
                }
                else
                {
                    _buffering = false;
                }
            }

            PlayAbilityStartFeedbacks();
            CurrentWeapon.WeaponInputStart();
        }

        protected virtual void ExtendBuffer()
        {
            if (!_buffering || NewInputExtendsBuffer)
            {
                _buffering = true;
                _bufferEndsAt = Time.time + MaximumBufferDuration;
            }
        }
        public virtual void ShootStop()
        {
            if (!AbilityAuthorized || (CurrentWeapon == null))
            {
                return;
            }

            if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
                || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
                || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
            {
                return;
            }

            if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
            {
                return;
            }

            ForceStop();
        }
        public virtual void ForceStop()
        {
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();

            if (CurrentWeapon != null)
            {
                CurrentWeapon.TurnWeaponOff();
            }
        }
        public virtual void Reload()
        {
            if (CurrentWeapon != null)
            {
                CurrentWeapon.InitiateReloadWeapon();
            }
        }
        public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            if (CurrentWeapon != null)
            {
                CurrentWeapon.TurnWeaponOff();

                if (!combo)
                {
                    ShootStop();

                    if (_weaponAim != null)
                    {
                        _weaponAim.RemoveReticle();
                    }

                    Destroy(CurrentWeapon.gameObject);
                }
            }

            if (newWeapon != null)
            {
                InstantiateWeapon(newWeapon, weaponID, combo);
            }
            else
            {
                CurrentWeapon = null;
            }

            if (OnWeaponChange != null)
            {
                OnWeaponChange();
            }
        }

        protected virtual void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            if (!combo)
            {
                CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset,
                    WeaponAttachment.transform.rotation);
            }

            CurrentWeapon.transform.parent = WeaponAttachment.transform;
            CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;
            CurrentWeapon.SetOwner(_character, this);
            CurrentWeapon.WeaponID = weaponID;
            _weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
            HandleWeaponIK();
            HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);
            if (m_PhotonView != null)
            {
                CurrentWeapon.GetPhotonView(m_PhotonView);
            }

            CurrentWeapon.Initialization();
            CurrentWeapon.InitializeComboWeapons();
            CurrentWeapon.InitializeAnimatorParameters();
            InitializeAnimatorParameters();
        }

        protected virtual void HandleWeaponIK()
        {
            if (_weaponIK != null)
            {
                _weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
            }

            _projectileWeapon = CurrentWeapon.gameObject.MMFGetComponentNoAlloc<ProjectileWeapon>();

            if (_projectileWeapon != null)
            {
                _projectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
            }
        }

        protected virtual void HandleWeaponModel(Weapon newWeapon, string weaponID, bool combo = false, Weapon weapon = null)
        {
            foreach (WeaponModel model in _weaponModels)
            {
                model.Hide();

                if (model.WeaponID == weaponID)
                {
                    model.Show();

                    if (model.UseIK)
                    {
                        _weaponIK.SetHandles(model.LeftHandHandle, model.RightHandHandle);
                    }

                    if (weapon != null)
                    {
                        if (model.BindFeedbacks)
                        {
                            weapon.WeaponStartMMFeedback = model.WeaponStartMMFeedback;
                            weapon.WeaponUsedMMFeedback = model.WeaponUsedMMFeedback;
                            weapon.WeaponStopMMFeedback = model.WeaponStopMMFeedback;
                            weapon.WeaponReloadMMFeedback = model.WeaponReloadMMFeedback;
                            weapon.WeaponReloadNeededMMFeedback = model.WeaponReloadNeededMMFeedback;
                        }

                        if (model.AddAnimator)
                        {
                            weapon.Animators.Add(model.TargetAnimator);
                        }

                        if (model.OverrideWeaponUseTransform)
                        {
                            weapon.WeaponUseTransform = model.WeaponUseTransform;
                        }
                    }
                }
            }
        }
        public override void Flip()
        {
        }
        public virtual void UpdateAmmoDisplay()
        {
            if ((GUIManager.Instance != null) && (_character.CharacterType == Character.CharacterTypes.Player))
            {
                if (CurrentWeapon == null)
                {
                    GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
                    return;
                }

                if (!CurrentWeapon.MagazineBased && (CurrentWeapon.WeaponAmmo == null))
                {
                    GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
                    return;
                }

                if (CurrentWeapon.WeaponAmmo == null)
                {
                    GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID);

                    GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, 0, 0, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize,
                        _character.PlayerID, AmmoDisplayID, false);

                    return;
                }
                else
                {
                    GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID);

                    GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased,
                        CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable + CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.WeaponAmmo.MaxAmmo,
                        CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, true);

                    return;
                }
            }
        }
        protected override void InitializeAnimatorParameters()
        {
            if (CurrentWeapon == null)
            {
                return;
            }

            RegisterAnimatorParameter(_weaponEquippedAnimationParameterName, AnimatorControllerParameterType.Bool, out _weaponEquippedAnimationParameter);
            RegisterAnimatorParameter(_weaponEquippedIDAnimationParameterName, AnimatorControllerParameterType.Int, out _weaponEquippedIDAnimationParameter);
        }
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _weaponEquippedAnimationParameter, (CurrentWeapon != null), _character._animatorParameters,
                _character.RunAnimatorSanityChecks);

            if (CurrentWeapon == null)
            {
                MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, -1, _character._animatorParameters,
                    _character.RunAnimatorSanityChecks);

                return;
            }
            else
            {
                MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, CurrentWeapon.WeaponAnimationID,
                    _character._animatorParameters, _character.RunAnimatorSanityChecks);
            }
        }

        protected override void OnHit()
        {
            base.OnHit();

            if (GettingHitInterruptsAttack && (CurrentWeapon != null))
            {
                CurrentWeapon.Interrupt();
            }
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            ShootStop();

            if (CurrentWeapon != null)
            {
                ChangeWeapon(null, "");
            }
        }

        protected override void OnRespawn()
        {
            base.OnRespawn();
            Setup();
        }
    }
}