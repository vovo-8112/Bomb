using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Items/Stimpack")]
    public class Stimpack : PickableItem
    {
        [Tooltip("The amount of points to add when collected")]
        public int HealthToGive = 10;
        [Tooltip("if this is true, only player characters can pick this up")]
        public bool OnlyForPlayerCharacter = true;
        protected override void Pick(GameObject picker)
        {
            Character character = picker.gameObject.MMGetComponentNoAlloc<Character>();
            if (OnlyForPlayerCharacter && (character != null) && (_character.CharacterType != Character.CharacterTypes.Player))
            {
                return;
            }

            Health characterHealth = picker.gameObject.MMGetComponentNoAlloc<Health>();
            if (characterHealth != null)
            {
                characterHealth.GetHealth(HealthToGive, gameObject);
            }            
        }
    }
}