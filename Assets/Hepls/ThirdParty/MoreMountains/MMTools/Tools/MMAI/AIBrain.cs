using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/AI/AIBrain")]
    public class AIBrain : MonoBehaviour
    {
        public List<AIState> States;
        public bool BrainActive = true;
        public AIState CurrentState { get; protected set; }
        [MMReadOnly]
        public float TimeInThisState;
        [MMReadOnly]
        public Transform Target;
        [MMReadOnly]
        public Vector3 _lastKnownTargetPosition = Vector3.zero;

        [Header("Frequencies")]
        public float ActionsFrequency = 0f;
        public float DecisionFrequency = 0f;
        public bool RandomizeFrequencies = false;
        [MMVector("min","max")]
        public Vector2 RandomActionFrequency = new Vector2(0.5f, 1f);
        [MMVector("min","max")]
        public Vector2 RandomDecisionFrequency = new Vector2(0.5f, 1f);

        protected AIDecision[] _decisions;
        protected float _lastActionsUpdate = 0f;
        protected float _lastDecisionsUpdate = 0f;
        protected AIState _initialState;

        public virtual AIAction[] GetAttachedActions()
        {
            AIAction[] actions = this.gameObject.GetComponentsInChildren<AIAction>();
            return actions;
        }

        public virtual AIDecision[] GetAttachedDecisions()
        {
            AIDecision[] decisions = this.gameObject.GetComponentsInChildren<AIDecision>();
            return decisions;
        }
        protected virtual void Awake()
        {
            foreach (AIState state in States)
            {
                state.SetBrain(this);
            }
            _decisions = GetAttachedDecisions();
            if (RandomizeFrequencies)
            {
                ActionsFrequency = Random.Range(RandomActionFrequency.x, RandomActionFrequency.y);
                DecisionFrequency = Random.Range(RandomDecisionFrequency.x, RandomDecisionFrequency.y);
            }
        }
        protected virtual void Start()
        {
            ResetBrain();
        }
        protected virtual void Update()
        {
            if (!BrainActive || (CurrentState == null) || (Time.timeScale == 0f))
            {
                return;
            }

            if (Time.time - _lastActionsUpdate > ActionsFrequency)
            {
                CurrentState.PerformActions();
                _lastActionsUpdate = Time.time;
            }
            
            if (Time.time - _lastDecisionsUpdate > DecisionFrequency)
            {
                CurrentState.EvaluateTransitions();
                _lastDecisionsUpdate = Time.time;
            }
            
            TimeInThisState += Time.deltaTime;

            StoreLastKnownPosition();
        }
        public virtual void TransitionToState(string newStateName)
        {
            if (CurrentState == null)
            {
                CurrentState = FindState(newStateName);
                if (CurrentState != null)
                {
                    CurrentState.EnterState();
                }
                return;
            }
            if (newStateName != CurrentState.StateName)
            {
                CurrentState.ExitState();
                OnExitState();

                CurrentState = FindState(newStateName);
                if (CurrentState != null)
                {
                    CurrentState.EnterState();
                }                
            }
        }
        protected virtual void OnExitState()
        {
            TimeInThisState = 0f;
        }
        protected virtual void InitializeDecisions()
        {
            if (_decisions == null)
            {
                _decisions = GetAttachedDecisions();
            }
            foreach(AIDecision decision in _decisions)
            {
                decision.Initialization();
            }
        }
        protected AIState FindState(string stateName)
        {
            foreach (AIState state in States)
            {
                if (state.StateName == stateName)
                {
                    return state;
                }
            }
            if (stateName != "")
            {
                Debug.LogError("You're trying to transition to state '" + stateName + "' in " + this.gameObject.name + "'s AI Brain, but no state of this name exists. Make sure your states are named properly, and that your transitions states match existing states.");
            }            
            return null;
        }
        protected virtual void StoreLastKnownPosition()
        {
            if (Target != null)
            {
                _lastKnownTargetPosition = Target.transform.position;
            }
        }
        public virtual void ResetBrain()
        {
            InitializeDecisions();

            if (CurrentState != null)
            {
                CurrentState.ExitState();
                OnExitState();
            }
            
            if (States.Count > 0)
            {
                CurrentState = States[0];
                CurrentState?.EnterState();
            }  
        }
    }
}
