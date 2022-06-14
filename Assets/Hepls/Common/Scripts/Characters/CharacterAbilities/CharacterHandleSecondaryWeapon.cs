using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Secondary Weapon")]
    public class CharacterHandleSecondaryWeapon : CharacterHandleWeapon
    {
        public override int HandleWeaponID { get { return 2; } }
        protected override void HandleInput()
        {
            if (!AbilityAuthorized
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }
            if ((_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.SecondaryShootAxis == MMInput.ButtonStates.ButtonDown))
            {
                ShootStart();
            }

            if (CurrentWeapon != null)
            {
                bool buttonPressed =
                    (_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed) ||
                    (_inputManager.SecondaryShootAxis == MMInput.ButtonStates.ButtonPressed); 
                
                if (ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
                {
                    ShootStart();
                }
            }

            if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                Reload();
            }

            if ((_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.SecondaryShootAxis == MMInput.ButtonStates.ButtonUp))
            {
                ShootStop();
            }

            if (CurrentWeapon != null)
            {
                if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
                && ((_inputManager.SecondaryShootAxis == MMInput.ButtonStates.Off) && (_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.Off)))
                {
                    CurrentWeapon.WeaponInputStop();
                }
            }
        }
    }
}