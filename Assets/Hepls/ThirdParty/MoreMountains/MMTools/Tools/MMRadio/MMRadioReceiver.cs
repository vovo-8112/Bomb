using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [MMRequiresConstantRepaint]
    public class MMRadioReceiver : MMMonoBehaviour
    {
        [Header("Target")]
        public MMPropertyReceiver Receiver;

        [Header("Channel")]
        public bool CanListen = true;
        [MMCondition("CanListen", true)]
        public int Channel = 0;

        [Header("Modifiers")]
        public bool RandomizeLevel = false;
        [MMCondition("RandomizeLevel", true)]
        public float MinRandomLevelMultiplier = 0f;
        [MMCondition("RandomizeLevel", true)]
        public float MaxRandomLevelMultiplier = 1f;

        protected bool _listeningToEvents = false;
        protected float _randomLevelMultiplier = 1f;
        protected float _lastLevel;
        protected virtual void Awake()
        {
            Receiver.Initialization(this.gameObject);

            if (!_listeningToEvents && CanListen)
            {
                StartListening();
            }

            GenerateRandomLevelMultiplier();
        }

        public virtual void GenerateRandomLevelMultiplier()
        {
            if (RandomizeLevel)
            {
                _randomLevelMultiplier = Random.Range(MinRandomLevelMultiplier, MaxRandomLevelMultiplier);
            }
        }
        public virtual void SetLevel(float newLevel)
        {
            Receiver.SetLevel(newLevel);
        }
        protected virtual void OnRadioLevelEvent(int channel, float level)
        {
            if (channel != Channel)
            {
                return;
            }
            if (RandomizeLevel)
            {
                level *= _randomLevelMultiplier;
            }
            SetLevel(level);
        }
        protected virtual void OnDestroy()
        {
            _listeningToEvents = false;
            StopListening();
        }
        public virtual void StartListening()
        {
            _listeningToEvents = true;
            MMRadioLevelEvent.Register(OnRadioLevelEvent);
        }
        public virtual void StopListening()
        {
            _listeningToEvents = false;
            MMRadioLevelEvent.Unregister(OnRadioLevelEvent);
        }
    }
}
