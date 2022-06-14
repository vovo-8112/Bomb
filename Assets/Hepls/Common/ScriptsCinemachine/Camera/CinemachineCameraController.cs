using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using Cinemachine;

namespace MoreMountains.TopDownEngine
{
    public class CinemachineCameraController : MonoBehaviour, MMEventListener<MMCameraEvent>, MMEventListener<TopDownEngineEvent>
    {
        public bool FollowsPlayer { get; set; }
        [Tooltip("Whether or not this camera should follow a player")]
        public bool FollowsAPlayer = true;
        [Tooltip("Whether to confine this camera to the level bounds, as defined in the LevelManager")]
        public bool ConfineCameraToLevelBounds = true;
        [MMReadOnly]
        [Tooltip("the target character this camera should follow")]
        public Character TargetCharacter;

        protected CinemachineVirtualCamera _virtualCamera;
        protected CinemachineConfiner _confiner;
        protected virtual void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            _confiner = GetComponent<CinemachineConfiner>();
        }
        protected virtual void Start()
        {
            if ((_confiner != null) && ConfineCameraToLevelBounds)
            {
                _confiner.m_BoundingVolume = LevelManager.Instance.BoundsCollider;
            }
        }

        public virtual void SetTarget(Character character)
        {
            TargetCharacter = character;
        }
        public virtual void StartFollowing()
        {
            if (!FollowsAPlayer) { return; }
            FollowsPlayer = true;
            _virtualCamera.Follow = TargetCharacter.CameraTarget.transform;
            _virtualCamera.enabled = true;
        }
        public virtual void StopFollowing()
        {
            if (!FollowsAPlayer) { return; }
            FollowsPlayer = false;
            _virtualCamera.Follow = null;
            _virtualCamera.enabled = false;
        }

        public virtual void OnMMEvent(MMCameraEvent cameraEvent)
        {
            switch (cameraEvent.EventType)
            {
                case MMCameraEventTypes.SetTargetCharacter:
                    SetTarget(cameraEvent.TargetCharacter);
                    break;

                case MMCameraEventTypes.SetConfiner:                    
                    if (_confiner != null)
                    {
                        _confiner.m_BoundingVolume = cameraEvent.Bounds;
                    }
                    break;

                case MMCameraEventTypes.StartFollowing:
                    if (cameraEvent.TargetCharacter != null)
                    {
                        if (cameraEvent.TargetCharacter != TargetCharacter)
                        {
                            return;
                        }
                    }
                    StartFollowing();
                    break;

                case MMCameraEventTypes.StopFollowing:
                    if (cameraEvent.TargetCharacter != null)
                    {
                        if (cameraEvent.TargetCharacter != TargetCharacter)
                        {
                            return;
                        }
                    }
                    StopFollowing();
                    break;

                case MMCameraEventTypes.RefreshPosition:
                    StartCoroutine(RefreshPosition());
                    break;

                case MMCameraEventTypes.ResetPriorities:
                    _virtualCamera.Priority = 0;
                    break;
            }
        }

        protected virtual IEnumerator RefreshPosition()
        {
            _virtualCamera.enabled = false;
            yield return null;
            StartFollowing();
        }

        public virtual void OnMMEvent(TopDownEngineEvent topdownEngineEvent)
        {
            if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwitch)
            {
                SetTarget(LevelManager.Instance.Players[0]);
                StartFollowing();
            }

            if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwap)
            {
                SetTarget(LevelManager.Instance.Players[0]);
                MMCameraEvent.Trigger(MMCameraEventTypes.RefreshPosition);
            }
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMCameraEvent>();
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMCameraEvent>();
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}
