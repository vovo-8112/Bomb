using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace  MoreMountains.Tools
{
    public class MMViewportEdgeTeleporter : MonoBehaviour
    {
        [Header("Camera")]
        public bool AutoGrabMainCamera;
        public Camera MainCamera;

        [Header("Viewport Bounds")]
        [MMVector("X","Y")]
        public Vector2 ViewportOrigin = new Vector2(0f, 0f);
        [MMVector("W","H")]
        public Vector2 ViewportDimensions = new Vector2(1f, 1f);
        
        [Header("Teleport Bounds")]
        [MMVector("X","Y")]
        public Vector2 TeleportOrigin = new Vector2(0f, 0f);
        [MMVector("W","H")]
        public Vector2 TeleportDimensions = new Vector2(1f, 1f);

        [Header("Events")]
        public UnityEvent OnTeleport;
        
        protected Vector3 _viewportPosition;
        protected Vector3 _newViewportPosition;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (AutoGrabMainCamera)
            {
                MainCamera = Camera.main;
            }
        }
        public virtual void SetCamera(Camera newCamera)
        {
            MainCamera = newCamera;
        }
        protected virtual void Update()
        {
            DetectEdges();
        }
        protected virtual void DetectEdges()
        {
            _viewportPosition = MainCamera.WorldToViewportPoint(this.transform.position);
            
            bool teleport = false;
            
            if (_viewportPosition.x < ViewportOrigin.x) 
            {
                _newViewportPosition.x = TeleportDimensions.x;
                _newViewportPosition.y = _viewportPosition.y;
                _newViewportPosition.z = _viewportPosition.z;
                teleport = true;
            }
            else if (_viewportPosition.x >= ViewportDimensions.x) 
            {
                _newViewportPosition.x = TeleportOrigin.x;
                _newViewportPosition.y = _viewportPosition.y;
                _newViewportPosition.z = _viewportPosition.z;
                teleport = true;
            }
            if (_viewportPosition.y < ViewportOrigin.y) 
            {
                _newViewportPosition.x = _viewportPosition.x;
                _newViewportPosition.y = TeleportDimensions.y;
                _newViewportPosition.z = _viewportPosition.z;
                teleport = true;
            }
            else if (_viewportPosition.y >= ViewportDimensions.y) 
            {
                _newViewportPosition.x = _viewportPosition.x;
                _newViewportPosition.y = TeleportOrigin.y;
                _newViewportPosition.z = _viewportPosition.z;
                teleport = true;
            }

            if (teleport)
            {
                OnTeleport?.Invoke();
                this.transform.position = MainCamera.ViewportToWorldPoint(_newViewportPosition);
            }
        }
    }    
}

