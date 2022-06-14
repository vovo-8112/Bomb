using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionRandom")]
    public class AIDecisionRandom : AIDecision
    {
        [Header("Random")]
        [Tooltip("the total number to consider (in '5 out of 10', this would be 10)")]
        public int TotalChance = 10;
        [Tooltip("when rolling our dice, if the result is below the Odds, this decision will be true. In '5 out of 10', this would be 5.")]
        public int Odds = 4;

        protected Character _targetCharacter;
        public override bool Decide()
        {
            return EvaluateOdds();
        }
        protected virtual bool EvaluateOdds()
        {
            int dice = MMMaths.RollADice(TotalChance);
            bool result = (dice <= Odds);
            return result;
        }
    }
}
