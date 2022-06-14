using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MoreMountains.Tools
{
    public abstract class AIDecision : MonoBehaviour
    {
        public abstract bool Decide();

        public string Label;
        public bool DecisionInProgress { get; set; }
        protected AIBrain _brain;
        protected virtual void Awake()
        {
            _brain = this.gameObject.GetComponentInParent<AIBrain>();
        }
        protected virtual void Start()
        {
            Initialization();
        }
        public virtual void Initialization()
        {

        }
        public virtual void OnEnterState()
        {
            DecisionInProgress = true;
        }
        public virtual void OnExitState()
        {
            DecisionInProgress = false;
        }
    }
}