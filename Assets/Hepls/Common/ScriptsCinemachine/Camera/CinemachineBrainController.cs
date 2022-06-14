using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using Cinemachine;

namespace MoreMountains.TopDownEngine
{
    public enum MMCinemachineBrainEventTypes { ChangeBlendDuration }
    public struct MMCinemachineBrainEvent
    {
        public MMCinemachineBrainEventTypes EventType;
        public float Duration;

        public MMCinemachineBrainEvent(MMCinemachineBrainEventTypes eventType, float duration)
        {
            EventType = eventType;
            Duration = duration;
        }

        static MMCinemachineBrainEvent e;
        public static void Trigger(MMCinemachineBrainEventTypes eventType, float duration)
        {
            e.EventType = eventType;
            e.Duration = duration;
            MMEventManager.TriggerEvent(e);
        }
    }
    [RequireComponent(typeof(CinemachineBrain))]
    public class CinemachineBrainController : MonoBehaviour, MMEventListener<MMCinemachineBrainEvent>
    {
        protected CinemachineBrain _brain;
        protected virtual void Awake()
        {
            _brain = this.gameObject.GetComponent<CinemachineBrain>();
        }
        public virtual void SetDefaultBlendDuration(float newDuration)
        {
            _brain.m_DefaultBlend.m_Time = newDuration;
        }
        public virtual void OnMMEvent(MMCinemachineBrainEvent cinemachineBrainEvent)
        {
            switch (cinemachineBrainEvent.EventType)
            {
                case MMCinemachineBrainEventTypes.ChangeBlendDuration:
                    SetDefaultBlendDuration(cinemachineBrainEvent.Duration);
                    break;
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMCinemachineBrainEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMCinemachineBrainEvent>();
        }
    }
}
