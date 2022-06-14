using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class MultiplayerSplitCameraRig : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Header("Multiplayer Split Camera Rig")]
        [Tooltip("the list of camera controllers to bind to level manager players on load")]
        public List<CinemachineCameraController> CameraControllers;
        protected virtual void BindCameras()
        {
            int i = 0;
            foreach (Character character in LevelManager.Instance.Players)
            {
                CameraControllers[i].TargetCharacter = character;
                CameraControllers[i].FollowsAPlayer = true;
                CameraControllers[i].StartFollowing();
                i++;
            }
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == "CameraBound")
            {
                BindCameras();
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
