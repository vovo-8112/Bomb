using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [MMRequiresConstantRepaint]
    public class MMRadioBroadcaster : MMMonoBehaviour
    {
        [Header("Source")]
        public MMPropertyEmitter Emitter;

        [Header("Destinations")]
        public MMRadioReceiver[] Receivers;

        [Header("Channel Broadcasting")]
        public bool BroadcastOnChannel = true;
        [MMCondition("BroadcastOnChannel", true)]
        public int Channel = 0;
        [MMCondition("BroadcastOnChannel", true)]
        public bool OnlyBroadcastOnValueChange = true;
        public delegate void OnValueChangeDelegate();
        public OnValueChangeDelegate OnValueChange;

        protected float _levelLastFrame = 0f;
        protected virtual void Awake()
        {
            Emitter.Initialization(this.gameObject);
        }
        protected virtual void Update()
        {
            ProcessBroadcast();
        }
        protected virtual void ProcessBroadcast()
        {
            if (Emitter == null)
            {
                return;
            }

            float level = Emitter.GetLevel();

            if (level != _levelLastFrame)
            {
                OnValueChange?.Invoke();
                foreach (MMRadioReceiver receiver in Receivers)
                {
                    receiver?.SetLevel(level);
                }
                if (BroadcastOnChannel)
                {
                    MMRadioLevelEvent.Trigger(Channel, level);
                }
            }           

            _levelLastFrame = level;
        }
    }
    public struct MMRadioLevelEvent
    {
        public delegate void Delegate(int channel, float level);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(int channel, float level)
        {
            OnEvent?.Invoke(channel, level);
        }
    }
}
