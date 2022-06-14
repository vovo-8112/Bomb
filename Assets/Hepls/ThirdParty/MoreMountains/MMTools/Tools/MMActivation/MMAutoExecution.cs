using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class MMAutoExecutionItem
    {
        public bool AutoExecuteOnAwake;
        public bool AutoExecuteOnEnable;
        public bool AutoExecuteOnDisable;
        public bool AutoExecuteOnStart;
        public bool AutoExecuteOnInstantiate;
        public UnityEvent Event;
    }
    public class MMAutoExecution : MonoBehaviour
    {
        public List<MMAutoExecutionItem> Events;
        protected virtual void Awake()
        {
            foreach (MMAutoExecutionItem item in Events)
            {
                if ((item.AutoExecuteOnAwake) && (item.Event != null))
                {
                    item.Event.Invoke();
                }
            }
        }
        protected virtual void Start()
        {
            foreach (MMAutoExecutionItem item in Events)
            {
                if ((item.AutoExecuteOnStart) && (item.Event != null))
                {
                    item.Event.Invoke();
                }
            }
        }
        protected virtual void OnEnable()
        {
            foreach (MMAutoExecutionItem item in Events)
            {
                if ((item.AutoExecuteOnEnable) && (item.Event != null))
                {
                    item.Event.Invoke();
                }
            }
        }
        protected virtual void OnDisable()
        {
            foreach (MMAutoExecutionItem item in Events)
            {
                if ((item.AutoExecuteOnDisable) && (item.Event != null))
                {
                    item.Event.Invoke();
                }
            }
        }
        protected virtual void OnInstantiate()
        {
            foreach (MMAutoExecutionItem item in Events)
            {
                if ((item.AutoExecuteOnInstantiate) && (item.Event != null))
                {
                    item.Event.Invoke();
                }
            }
        }
    }    
}

