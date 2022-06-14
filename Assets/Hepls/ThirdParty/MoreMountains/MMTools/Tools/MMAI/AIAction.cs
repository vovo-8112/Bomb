using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public abstract class AIAction : MonoBehaviour
    {
        public string Label;
        public abstract void PerformAction();
        public bool ActionInProgress { get; set; }
        protected AIBrain _brain;
        protected virtual void Awake()
        {
            _brain = this.gameObject.GetComponentInParent<AIBrain>();
        }
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {

        }
        public virtual void OnEnterState()
        {
            ActionInProgress = true;
        }
        public virtual void OnExitState()
        {
            ActionInProgress = false;
        }
    }
}