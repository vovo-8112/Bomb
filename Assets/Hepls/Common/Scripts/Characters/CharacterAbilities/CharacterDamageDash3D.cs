using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Damage Dash 3D")]
    public class CharacterDamageDash3D : CharacterDash3D
    {
        [Header("Damage Dash")]
        [Tooltip("the DamageOnTouch object to activate when dashing (usually placed under the Character's model, will require a Collider2D of some form, set to trigger")]
        public DamageOnTouch TargetDamageOnTouch;
        protected override void Initialization()
        {
            base.Initialization();
            TargetDamageOnTouch?.gameObject.SetActive(false);
        }
        public override void DashStart()
        {
            base.DashStart();
            TargetDamageOnTouch?.gameObject.SetActive(true);
        }
        protected override void DashStop()
        {
            base.DashStop();
            TargetDamageOnTouch?.gameObject.SetActive(false);
        }
    }
}
