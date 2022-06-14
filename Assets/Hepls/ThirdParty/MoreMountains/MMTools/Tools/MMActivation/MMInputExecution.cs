using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class MMInputExecutionBinding
    {
        public KeyCode TargetKey = KeyCode.Space;
        public UnityEvent OnKeyDown;
        public UnityEvent OnKey;
        public UnityEvent OnKeyUp;
        public virtual void ProcessInput()
        {
            if (OnKey != null)
            {
                if (Input.GetKey(TargetKey))
                {
                    OnKey.Invoke();
                }
            }
            if (OnKeyDown != null)
            {
                if (Input.GetKeyDown(TargetKey))
                {
                    OnKeyDown.Invoke();
                }
            }
            if (OnKeyUp != null)
            {
                if (Input.GetKeyUp(TargetKey))
                {
                    OnKeyUp.Invoke();
                }
            }
        }
    }
    public class MMInputExecution : MonoBehaviour
    {
        [Header("Bindings")]
        public List<MMInputExecutionBinding> Bindings;
        protected virtual void Update()
        {
            HandleInput();
        }
        protected virtual void HandleInput()
        {
            foreach(MMInputExecutionBinding binding in Bindings)
            {
                binding.ProcessInput();
            }            
        }
    }
}
