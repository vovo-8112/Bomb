using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(LineRenderer))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim Radius Circle")]
    public class WeaponAutoAimRadiusCircle : MMLineRendererCircle
    {
        [Header("Weapon Radius")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;
        protected override void Initialization()
        {
            base.Initialization();
            _line = gameObject.GetComponent<LineRenderer>();
            _line.enabled = false;
            
            if (TargetHandleWeaponAbility != null)
            {
                TargetHandleWeaponAbility.OnWeaponChange += OnWeaponChange;
            }
        }
        void OnWeaponChange()
        {
            WeaponAutoAim autoAim = TargetHandleWeaponAbility.CurrentWeapon.GetComponent<WeaponAutoAim>();
            _line.enabled = (autoAim != null);
            
            if (autoAim != null)
            {
                HorizontalRadius = autoAim.ScanRadius;
                VerticalRadius = autoAim.ScanRadius;
            }
            DrawCircle();
        }
        void OnDisable()
        {
            if (TargetHandleWeaponAbility != null)
            {
                TargetHandleWeaponAbility.OnWeaponChange -= OnWeaponChange;
            }
        }
    }
}