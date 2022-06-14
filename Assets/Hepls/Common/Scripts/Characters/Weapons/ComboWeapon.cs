using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Weapons/Combo Weapon")]
    public class ComboWeapon : MonoBehaviour
    {
        [Header("Combo")]
        [Tooltip("whether or not the combo can be dropped if enough time passes between two consecutive attacks")]
        public bool DroppableCombo = true;
        [Tooltip("the delay after which the combo drops")]
        public float DropComboDelay = 0.5f;

        [Header("Animation")]
        [Tooltip("the name of the animation parameter to update when a combo is in progress.")]
        public string ComboInProgressAnimationParameter = "ComboInProgress";

        [Header("Debug")]
        [MMReadOnly]
        [Tooltip("the list of weapons, set automatically by the class")]
        public Weapon[] Weapons;
        [MMReadOnly]
        [Tooltip("the reference to the weapon's Owner")]
        public CharacterHandleWeapon OwnerCharacterHandleWeapon;
        [MMReadOnly]
        [Tooltip("the time spent since the last weapon stopped")]
        public float TimeSinceLastWeaponStopped;
        public bool ComboInProgress
        {
            get
            {
                bool comboInProgress = false;
                foreach (Weapon weapon in Weapons)
                {
                    if (weapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle)
                    {
                        comboInProgress = true;
                    }
                }
                return comboInProgress;
            }
        }

        protected int _currentWeaponIndex = 0;
        protected bool _countdownActive = false;
        protected virtual void Start()
        {
            Initialization();
        }
        public virtual void Initialization()
        {
            Weapons = GetComponents<Weapon>();
            InitializeUnusedWeapons();
        }
        protected virtual void Update()
        {
            ResetCombo();
        }
        public virtual void ResetCombo()
        {
            if (Weapons.Length > 1)
            {
                if (_countdownActive && DroppableCombo)
                {
                    TimeSinceLastWeaponStopped += Time.deltaTime;
                    if (TimeSinceLastWeaponStopped > DropComboDelay)
                    {
                        _countdownActive = false;
                        
                        _currentWeaponIndex = 0;
                        OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[_currentWeaponIndex];
                        OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[_currentWeaponIndex], Weapons[_currentWeaponIndex].WeaponName, true);
                    }
                }
            }
        }
        public virtual void WeaponStarted(Weapon weaponThatStarted)
        {
            _countdownActive = false;
        }
        public virtual void WeaponStopped(Weapon weaponThatStopped)
        {
            OwnerCharacterHandleWeapon = Weapons[_currentWeaponIndex].CharacterHandleWeapon;
            
            int newIndex = 0;
            if (OwnerCharacterHandleWeapon != null)
            {
                if (Weapons.Length > 1)
                {
                    if (_currentWeaponIndex < Weapons.Length-1)
                    {
                        newIndex = _currentWeaponIndex + 1;
                    }
                    else
                    {
                        newIndex = 0;
                    }

                    _countdownActive = true;
                    TimeSinceLastWeaponStopped = 0f;

                    _currentWeaponIndex = newIndex;
                    OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[newIndex];
                    OwnerCharacterHandleWeapon.CurrentWeapon.WeaponCurrentlyActive = false;
                    OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[newIndex], Weapons[newIndex].WeaponName, true);
                    OwnerCharacterHandleWeapon.CurrentWeapon.WeaponCurrentlyActive = true;
                }
            }
        }
        public virtual void FlipUnusedWeapons()
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (i != _currentWeaponIndex)
                {
                    Weapons[i].Flipped = !Weapons[i].Flipped;
                }                
            }
        }
        protected virtual void InitializeUnusedWeapons()
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (i != _currentWeaponIndex)
                {
                    Weapons[i].SetOwner(Weapons[_currentWeaponIndex].Owner, Weapons[_currentWeaponIndex].CharacterHandleWeapon);
                    Weapons[i].Initialization();
                    Weapons[i].WeaponCurrentlyActive = false;
                }
            }
        }
    }
}
