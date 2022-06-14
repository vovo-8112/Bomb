using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [MMHiddenProperties("AbilityStopFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Swap")]
    public class CharacterSwap : CharacterAbility
    {
        [Header("Character Swap")]
        [Tooltip("the order in which this character should be picked ")]
        public int Order = 0;
        [Tooltip("the playerID to put back in the Character class once this character gets swapped")]
        public string PlayerID = "Player1";

        [Header("AI")]
        [Tooltip("if this is true, the AI Brain (if there's one on this character) will reset on swap")]
        public bool ResetAIBrainOnSwap = true;

        protected string _savedPlayerID;
        protected Character.CharacterTypes _savedCharacterType;
        protected override void Initialization()
        {
            base.Initialization();
            _savedCharacterType = _character.CharacterType;
            _savedPlayerID = _character.PlayerID;
        }
        public virtual void SwapToThisCharacter()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            PlayAbilityStartFeedbacks();
            _character.PlayerID = PlayerID;
            _character.CharacterType = Character.CharacterTypes.Player;
            _character.SetInputManager();
            if (_character.CharacterBrain != null)
            {
                _character.CharacterBrain.BrainActive = false;
            }
        }
        public virtual void ResetCharacterSwap()
        {
            _character.CharacterType = Character.CharacterTypes.AI;
            _character.PlayerID = _savedPlayerID;
            _character.SetInputManager(null);
            _characterMovement.SetHorizontalMovement(0f);
            _characterMovement.SetVerticalMovement(0f);
            _character.ResetInput();
            if (_character.CharacterBrain != null)
            {
                _character.CharacterBrain.BrainActive = true;
                if (ResetAIBrainOnSwap)
                {
                    _character.CharacterBrain.ResetBrain();    
                }
            }

        }
        public virtual bool Current()
        {
            return (_character.CharacterType == Character.CharacterTypes.Player);
        }
    }
}