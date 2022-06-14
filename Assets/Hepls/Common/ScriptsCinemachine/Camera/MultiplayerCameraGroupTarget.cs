using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Cinemachine;

namespace MoreMountains.TopDownEngine
{
    public class MultiplayerCameraGroupTarget : MonoBehaviour, MMEventListener<MMGameEvent>, MMEventListener<TopDownEngineEvent>
    {
        [Header("Multiplayer Camera Group Target")]
        [Tooltip("the virtual camera that will follow the group target")]
        public CinemachineVirtualCamera TargetCamera;

        protected CinemachineTargetGroup _targetGroup;
        protected virtual void Awake()
        {
            _targetGroup = this.gameObject.GetComponent<CinemachineTargetGroup>();
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == "Load")
            {
                if (_targetGroup == null)
                {
                    return;
                }

                int i = 0;
                _targetGroup.m_Targets = new CinemachineTargetGroup.Target[LevelManager.Instance.Players.Count];

                foreach (Character character in LevelManager.Instance.Players)
                {
                    CinemachineTargetGroup.Target target = new CinemachineTargetGroup.Target();
                    target.weight = 1;
                    target.radius = 0;
                    target.target = character.transform;

                    _targetGroup.m_Targets[i] = target;
                    i++;
                }

                TargetCamera.Follow = this.transform;
            }
        }

        public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
        {
            if (tdEvent.EventType == TopDownEngineEventTypes.PlayerDeath)
            {
                int i = 0;
                foreach (Character character in LevelManager.Instance.Players)
                {
                    if (character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
                    {
                        _targetGroup.m_Targets[i].weight = 0f;
                    }
                    i++;
                }
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<TopDownEngineEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}