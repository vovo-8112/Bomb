using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Swap Brain")]
    public class AIActionSwapBrain : AIAction
    {
        [Tooltip("the brain to replace the Character's one with")]
        public AIBrain NewAIBrain;

        protected Character _character;
        protected override void Initialization()
        {
            base.Initialization();
            _character = this.gameObject.GetComponentInParent<Character>();
        }
        public override void PerformAction()
        {
            SwapBrain();
        }
        protected virtual void SwapBrain()
        {
            _character.CharacterBrain.gameObject.SetActive(false);
            _character.CharacterBrain.enabled = false;
            _character.CharacterBrain = NewAIBrain;
            NewAIBrain.gameObject.SetActive(true);
            NewAIBrain.enabled = true;
            NewAIBrain.ResetBrain();
        }
    }
}