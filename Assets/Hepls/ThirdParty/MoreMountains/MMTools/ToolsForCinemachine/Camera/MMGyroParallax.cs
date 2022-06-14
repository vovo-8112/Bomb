﻿using Cinemachine;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    [AddComponentMenu("More Mountains/Tools/Cinemachine/MMGyroCam")]
    public class MMGyroCam
    {
        public CinemachineVirtualCamera Cam;
        public Transform LookAt;
        public Transform RotationCenter;
        public Vector2 MinRotation = new Vector2(-2f, -2f);
        public Vector2 MaxRotation = new Vector2(2f, 2f);
        public Transform AnimatedPosition;
        [MMReadOnly]
        public Vector3 InitialAngles;
        [MMReadOnly]
        public Vector3 InitialPosition;
    }
    public class MMGyroParallax : MMGyroscope
    {

        [Header("Cameras")]
        public List<MMGyroCam> Cams;
        
        protected Vector3 _newAngles;
        protected override void Start()
        {
            base.Start();
            Initialization();
        }
        public virtual void Initialization()
        {
            foreach (MMGyroCam cam in Cams)
            {
                cam.InitialAngles = cam.Cam.transform.localEulerAngles;
                cam.InitialPosition = cam.Cam.transform.position;
            }
        }
        protected override void Update()
        {
            base.Update();
            MoveCameras();
        }
        protected virtual void MoveCameras()
        {
            foreach (MMGyroCam cam in Cams)
            {
                float newX = 0f;
                float newY = 0f;

                var gyroGravity = LerpedCalibratedGyroscopeGravity;
                if (gyroGravity.x > 0)
                {
                    newX = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.x, 0.5f, 0, cam.MinRotation.x, 0);
                }
                if (gyroGravity.x < 0)
                {
                    newX = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.x, 0, -.5f, 0, cam.MaxRotation.x);
                }
                if (gyroGravity.y > 0)
                {
                    newY = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.y, 0.5f, 0, cam.MinRotation.y, 0f);
                }
                if (gyroGravity.y < 0)
                {
                    newY = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.y, 0f, -0.5f, 0f, cam.MaxRotation.y);
                }

                var camTransform = cam.Cam.transform;

                if (cam.AnimatedPosition != null)
                {
                    _newAngles = cam.AnimatedPosition.localEulerAngles;
                    _newAngles.x += newX;
                    _newAngles.z += newY;

                    camTransform.position = cam.AnimatedPosition.position;
                    camTransform.localEulerAngles = cam.AnimatedPosition.localEulerAngles;
                }
                else
                {
                    _newAngles = cam.InitialAngles;
                    _newAngles.x += newX;
                    _newAngles.z += newY;
                    
                    camTransform.position = cam.InitialPosition;
                    camTransform.localEulerAngles = cam.InitialAngles;
                }

                var rotationTransform = cam.RotationCenter.transform;
                camTransform.RotateAround(rotationTransform.position, rotationTransform.up, newX);
                camTransform.RotateAround(rotationTransform.position, rotationTransform.right, newY);

                if (cam.Cam.LookAt == null)
                {
                    if (cam.LookAt != null)
                    {
                        camTransform.LookAt(cam.LookAt);
                    }
                    else
                    {
                        camTransform.LookAt(cam.RotationCenter);
                    }
                }
            }
        }
    }
}
