using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Cinemachine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    public class Room : MonoBehaviour, MMEventListener<TopDownEngineEvent>
    {
        public enum Modes { TwoD, ThreeD }
        public Vector3 RoomColliderCenter
        {
            get
            {
                if (_roomCollider2D != null)
                {
                    return _roomCollider2D.bounds.center + (Vector3)_roomCollider2D.offset;
                }
                else
                {
                    return _roomCollider.bounds.center;
                }
            }
        }
        
        public Vector3 RoomColliderSize
        {
            get
            {
                if (_roomCollider2D != null)
                {
                    return _roomCollider2D.bounds.size;
                }
                else
                {
                    return _roomCollider.bounds.size;
                }
            }
        }

        public Bounds RoomBounds
        {
            get
            {
                if (_roomCollider2D != null)
                {
                    return _roomCollider2D.bounds;
                }
                else
                {
                    return _roomCollider.bounds;
                }
            }
        }

        [Header("Mode")]
        [Tooltip("whether this room is intended to work in 2D or 3D mode")]
        public Modes Mode = Modes.TwoD;

        [Header("Camera")]
        [Tooltip("the virtual camera associated to this room")]
        public CinemachineVirtualCamera VirtualCamera;
        [Tooltip("the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room")]
        public BoxCollider Confiner;
        [Tooltip("the confiner component of the virtual camera")]
        public CinemachineConfiner CinemachineCameraConfiner;
        [Tooltip("whether or not the confiner should be auto resized on start to match the camera's size and ratio")]
        public bool ResizeConfinerAutomatically = true;
        [Tooltip("whether or not this Room should look at the level's start position and declare itself the current room on start or not")]
        public bool AutoDetectFirstRoomOnStart = true;
        [MMEnumCondition("Mode", (int)Modes.TwoD)]
        [Tooltip("the depth of the room (used to resize the z value of the confiner")]
        public float RoomDepth = 100f;

        [Header("State")]
        [Tooltip("whether this room is the current room or not")]
        public bool CurrentRoom = false;
        [Tooltip("whether this room has already been visited or not")]
        public bool RoomVisited = false;

        [Header("Actions")]
        [Tooltip("the event to trigger when the player enters the room for the first time")]
        public UnityEvent OnPlayerEntersRoomForTheFirstTime;
        [Tooltip("the event to trigger everytime the player enters the room")]
        public UnityEvent OnPlayerEntersRoom;
        [Tooltip("the event to trigger everytime the player exits the room")]
        public UnityEvent OnPlayerExitsRoom;

        [Header("Activation")]
        [Tooltip("a list of gameobjects to enable when entering the room, and disable when exiting it")]
        public List<GameObject> ActivationList;

        protected Collider _roomCollider;
        protected Collider2D _roomCollider2D;
        protected Camera _mainCamera;
        protected Vector2 _cameraSize;
        protected bool _initialized = false;
        protected virtual void Awake()
        {
            VirtualCamera.Priority = 0;
        }
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (_initialized)
            {
                return;
            }
            _roomCollider = this.gameObject.GetComponent<Collider>();
            _roomCollider2D = this.gameObject.GetComponent<Collider2D>();
            _mainCamera = Camera.main;          
            StartCoroutine(ResizeConfiner());
            _initialized = true;
        }
        protected virtual IEnumerator ResizeConfiner()
        {
            if ((VirtualCamera == null) || (Confiner == null) || !ResizeConfinerAutomatically)
            {
                yield break;
            }
            yield return null;
            yield return null;

            Confiner.transform.position = RoomColliderCenter;
            Vector3 size = RoomColliderSize;

            switch (Mode)
            {
                case Modes.TwoD:
                    size.z = RoomDepth;
                    Confiner.size = size;
                    _cameraSize.y = 2 * _mainCamera.orthographicSize;
                    _cameraSize.x = _cameraSize.y * _mainCamera.aspect;

                    Vector3 newSize = Confiner.size;

                    if (Confiner.size.x < _cameraSize.x)
                    {
                        newSize.x = _cameraSize.x;
                    }
                    if (Confiner.size.y < _cameraSize.y)
                    {
                        newSize.y = _cameraSize.y;
                    }

                    Confiner.size = newSize;
                    break;
                case Modes.ThreeD:
                    Confiner.size = size;
                    break;
            }
            
            CinemachineCameraConfiner.InvalidatePathCache();
        }
        protected virtual void HandleLevelStartDetection()
        {
            if (!_initialized)
            {
                Initialization();
            }

            if (AutoDetectFirstRoomOnStart)
            {
                if (LevelManager.Instance != null)
                {
                    if (RoomBounds.Contains(LevelManager.Instance.Players[0].transform.position))
                    {
                        MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
                        MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, 0f);

                        MMSpriteMaskEvent.Trigger(MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition,
                            RoomColliderCenter,
                            RoomColliderSize,
                            0f, MMTween.MMTweenCurve.LinearTween);

                        PlayerEntersRoom();
                        VirtualCamera.Priority = 10;
                        VirtualCamera.enabled = true;
                    }
                    else
                    {
                        VirtualCamera.Priority = 0;
                        VirtualCamera.enabled = false;
                    }
                }
            }
        }
        public virtual void PlayerEntersRoom()
        {
            CurrentRoom = true;
            if (RoomVisited)
            {
                OnPlayerEntersRoom?.Invoke();
            }
            else
            {
                RoomVisited = true;
                OnPlayerEntersRoomForTheFirstTime?.Invoke();
            }  
            foreach(GameObject go in ActivationList)
            {
                go.SetActive(true);
            }
        }
        public virtual void PlayerExitsRoom()
        {
            CurrentRoom = false;
            OnPlayerExitsRoom?.Invoke();
            foreach (GameObject go in ActivationList)
            {
                go.SetActive(false);
            }
        }
        public virtual void OnMMEvent(TopDownEngineEvent topDownEngineEvent)
        {
            if ((topDownEngineEvent.EventType == TopDownEngineEventTypes.RespawnComplete)
                || (topDownEngineEvent.EventType == TopDownEngineEventTypes.LevelStart))
            {
                HandleLevelStartDetection();
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}
