using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.TopDownEngine
{
    [Serializable]
    public struct WeaponModelBindings
    {
        public GameObject WeaponModel;
        public int WeaponAnimationID;
    }
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Model Enabler")]
    public class WeaponModelEnabler : MonoBehaviour
    {
        [Tooltip("a list of model bindings. A binding is made of a gameobject, already present on the character, that will act as the visual representation of the weapon, and a name, that has to match the WeaponAnimationID of the actual Weapon")]
        public WeaponModelBindings[] Bindings;

        protected CharacterHandleWeapon _characterHandleWeapon;
        protected virtual void Awake()
        {
            _characterHandleWeapon = this.gameObject.GetComponent<CharacterHandleWeapon>();
        }
        protected virtual void Update()
        {
            if (Bindings.Length <= 0)
            {
                return;
            }

            if (_characterHandleWeapon == null)
            {
                return;
            }

            if (_characterHandleWeapon.CurrentWeapon == null)
            {
                return;
            }

            foreach (WeaponModelBindings binding in Bindings)
            {
                if (binding.WeaponAnimationID == _characterHandleWeapon.CurrentWeapon.WeaponAnimationID)
                {
                    binding.WeaponModel.SetActive(true);
                }
                else
                {
                    binding.WeaponModel.SetActive(false);
                }
            }
        }			
	}
}
