using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(AudioListener))]
    public class MMAudioListener : MonoBehaviour
    {
        protected AudioListener _audioListener;
        protected AudioListener[] _otherListeners;
        protected virtual void OnEnable()
        {
            _audioListener = this.gameObject.GetComponent<AudioListener>();
            _otherListeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];

            foreach (AudioListener audioListener in _otherListeners)
            {
                if ((audioListener != null) && (audioListener != _audioListener) )
                {
                    audioListener.enabled = false;
                }    
            }
        }
    }    
}

