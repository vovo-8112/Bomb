using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterAbilityNodeSwap")]
    public class CharacterAbilityNodeSwap : CharacterAbility
    {
        [Header("Ability Node Swap")]
        [Tooltip("a list of GameObjects that will replace this Character's set of ability nodes when the ability executes")]
        public List<GameObject> AdditionalAbilityNodes;
        protected override void HandleInput()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            
            if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                SwapAbilityNodes();
            }
        }
        public virtual void SwapAbilityNodes()
        {
            foreach (GameObject node in _character.AdditionalAbilityNodes)
            {
                node.gameObject.SetActive(false);
            }
            
            _character.AdditionalAbilityNodes = AdditionalAbilityNodes;

            foreach (GameObject node in _character.AdditionalAbilityNodes)
            {
                node.gameObject.SetActive(true);
            }

            _character.CacheAbilities();
        }
    }
}