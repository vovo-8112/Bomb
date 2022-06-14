using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ProximityManager : MonoBehaviour, MMEventListener<TopDownEngineEvent>
    {
        [Header("Target")]
        [Tooltip("whether or not to automatically grab the player from the LevelManager once the scene loads")]
        public bool AutomaticallySetPlayerAsTarget = true;
        [Tooltip("the target to detect proximity with")]
        public Transform ProximityTarget;

        [Header("EnableDisable")]
        [Tooltip("whether or not to automatically grab all ProximityManaged objects in the scene")]
        public bool AutomaticallyGrabControlledObjects = true;
        [Tooltip("the list of objects to check proximity with")]
        public List<ProximityManaged> ControlledObjects;
        
        [Header("Tick")]
        [Tooltip("the frequency, in seconds, at which to evaluate distances and enable/disable stuff")]
        public float EvaluationFrequency = 0.5f;

        protected float _lastEvaluationAt = 0f;
        protected virtual void Start()
        {
            GrabControlledObjects();
        }
        protected virtual void GrabControlledObjects()
        {
            if (AutomaticallyGrabControlledObjects)
            {
                var items = FindObjectsOfType<ProximityManaged>();
                foreach(ProximityManaged managed in items)
                {
                    ControlledObjects.Add(managed);
                }
            }
        }
        public virtual void AddControlledObject(ProximityManaged newObject)
        {
            ControlledObjects.Add(newObject);
        }
        protected virtual void SetPlayerAsTarget()
        {
            if (AutomaticallySetPlayerAsTarget)
            {
                ProximityTarget = LevelManager.Instance.Players[0].transform;
            }            
        }
        protected virtual void Update()
        {
            EvaluateDistance();
        }
        protected virtual void EvaluateDistance()
        {
            if (ProximityTarget == null)
            {
                return;
            }
            
            if (Time.time - _lastEvaluationAt > EvaluationFrequency)
            {
                _lastEvaluationAt = Time.time;
            }
            else
            {
                return;
            }
            foreach(ProximityManaged proxy in ControlledObjects)
            {
                float distance = Vector3.Distance(proxy.transform.position, ProximityTarget.position);
                if (proxy.gameObject.activeInHierarchy && (distance > proxy.DisableDistance))
                {
                    proxy.gameObject.SetActive(false);
                    proxy.DisabledByManager = true;
                }
                if (!proxy.gameObject.activeInHierarchy && proxy.DisabledByManager && (distance < proxy.EnableDistance))
                {
                    proxy.gameObject.SetActive(true);
                    proxy.DisabledByManager = false;
                }
            }
        }
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            if ((engineEvent.EventType == TopDownEngineEventTypes.LevelStart)
                || (engineEvent.EventType == TopDownEngineEventTypes.CharacterSwap))
            {
                SetPlayerAsTarget();
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}