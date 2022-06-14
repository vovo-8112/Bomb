using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{

    [AddComponentMenu("More Mountains/Tools/Activation/MMActivationOnStart")]
    public class MMActivationOnStart : MonoBehaviour
    {
        public enum Modes { Awake, Start }
        public Modes Mode = Modes.Start;
        public bool StateOnStart = true;
        public List<GameObject> TargetObjects;
        protected virtual void Awake()
        {
            if (Mode != Modes.Awake)
            {
                return;
            }
            SetState();
        }
        protected virtual void Start()
        {
            if (Mode != Modes.Start)
            {
                return;
            }
            SetState();
        }
        protected virtual void SetState()
        {
            foreach (GameObject obj in TargetObjects)
            {
                obj.SetActive(StateOnStart);
            }
        }
    }
}
