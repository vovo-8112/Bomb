using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class AITransition 
    {
        public AIDecision Decision;
        public string TrueState;
        public string FalseState;
    }
}
