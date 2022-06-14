using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Activation/MMTimedActivation")]
    public class MMTimedActivation : MonoBehaviour
    {
        public enum TimedStatusChange { Enable, Disable, Destroy }
        public enum ActivationModes { Awake, Start, OnEnable, OnTriggerEnter, OnTriggerExit, OnTriggerEnter2D, OnTriggerExit2D, Script }
        public enum TriggerModes { None, Tag, Layer }
        public enum DelayModes { Time, Frames }

        [Header("Trigger Mode")]
        public ActivationModes ActivationMode = ActivationModes.Start;
        [MMEnumCondition("ActivationMode", (int)ActivationModes.OnTriggerEnter, (int)ActivationModes.OnTriggerExit)]
        public TriggerModes TriggerMode;
        [MMEnumCondition("TriggerMode", (int)TriggerModes.Layer)]
        public LayerMask TargetTriggerLayer;
        [MMEnumCondition("TriggerMode", (int)TriggerModes.Tag)]
        public string TargetTriggerTag;

        

        [Header("Delay")]
        public DelayModes DelayMode = DelayModes.Time;
        [MMEnumCondition("DelayMode", (int)DelayModes.Time)]
        public float TimeBeforeStateChange = 2;
        [MMEnumCondition("DelayMode", (int)DelayModes.Frames)]
        public int FrameCount = 1;

        [Header("Timed Activation")]
        public List<GameObject> TargetGameObjects;
        public List<MonoBehaviour> TargetBehaviours;
        public TimedStatusChange TimeDestructionMode = TimedStatusChange.Disable;

        [Header("Actions")]
        public UnityEvent TimedActions;
        protected virtual void Awake()
        {
            if (ActivationMode == ActivationModes.Awake)
            {
                StartChangeState();
            }
        }
        public virtual void TriggerSequence()
        {
            StartChangeState();
        }
        protected virtual void Start()
        {
            if (ActivationMode == ActivationModes.Start)
            {
                StartChangeState();
            }
        }
        protected virtual void OnEnable()
        {
            if (ActivationMode == ActivationModes.Start)
            {
                StartChangeState();
            }
        }
        protected virtual void OnTriggerEnter(Collider collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }
        protected virtual void OnTriggerExit(Collider collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }
        protected virtual void OnTriggerEnter2d(Collider2D collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }
        protected virtual void OnTriggerExit2d(Collider2D collider)
        {
            if ((ActivationMode == ActivationModes.OnTriggerEnter) && (CorrectTagOrLayer(collider.gameObject)))
            {
                StartChangeState();
            }
        }
        protected virtual bool CorrectTagOrLayer(GameObject target)
        {
            switch (TriggerMode)
            {
                case TriggerModes.None:
                    return true;
                case TriggerModes.Layer:
                    if (((1 << target.layer) & TargetTriggerLayer) != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case TriggerModes.Tag:
                    return (target.CompareTag(TargetTriggerTag));                    
            }
            return false;
        }
        protected virtual void StartChangeState()
        {
            StartCoroutine(TimedActivationSequence());
        }
        protected virtual IEnumerator TimedActivationSequence()
        {
            if (DelayMode == DelayModes.Time)
            {
                yield return MMCoroutine.WaitFor(TimeBeforeStateChange);
            }
            else
            {
                yield return StartCoroutine(MMCoroutine.WaitForFrames(FrameCount));
            }
            StateChange();
            Activate();
        }
        protected virtual void Activate()
        {
            if (TimedActions != null)
            {
                TimedActions.Invoke();
            }
        }
        protected virtual void StateChange()
        {
            foreach(GameObject targetGameObject in TargetGameObjects)
            {
                switch (TimeDestructionMode)
                {
                    case TimedStatusChange.Destroy:
                        Destroy(targetGameObject);
                        break;

                    case TimedStatusChange.Disable:
                        targetGameObject.SetActive(false);
                        break;

                    case TimedStatusChange.Enable:
                        targetGameObject.SetActive(true);
                        break;
                }
            }

            foreach (MonoBehaviour targetBehaviour in TargetBehaviours)
            {
                switch (TimeDestructionMode)
                {
                    case TimedStatusChange.Destroy:
                        Destroy(targetBehaviour);
                        break;

                    case TimedStatusChange.Disable:
                        targetBehaviour.enabled = false;
                        break;

                    case TimedStatusChange.Enable:
                        targetBehaviour.enabled = true;
                        break;
                }
            }
        }
    }
}
