using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    public class MMDebugMenuTestClass : MonoBehaviour
    {
        public string Label;

        private float multiplier;
        private void Start()
        {
            multiplier = Random.Range(0f, 50000f);
        }
        void Update()
        {
            float test = (Mathf.Sin(Time.time) + 2) * multiplier;
            MMDebug.DebugOnScreen(Label, test);
        }
    }
}
