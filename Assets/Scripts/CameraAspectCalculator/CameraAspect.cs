using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraAspectCalculator
{
    public class CameraAspect : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private List<float> _fieldOfView;

        private float _aspect;

        private void Awake()
        {
            GetAspectRatio();
        }

        private void Update()
        {
            if (Math.Abs(_aspect - Screen.width / (float)Screen.height) > 0.1)
            {
                GetAspectRatio();
            }
        }

        private void GetAspectRatio()
        {
            _aspect = Screen.width / (float)Screen.height;

            if (_aspect >= 1.87) // iPhone X                  
            {
                _camera.fieldOfView = _fieldOfView[0];
            }
            else if (_aspect >= 1.74) // 16:9
            {
                _camera.fieldOfView = _fieldOfView[0];
            }
            else if (_aspect > 1.6) // 5:3
            {
                _camera.fieldOfView = _fieldOfView[1];
            }
            else if (Math.Abs(_aspect - 1.6) < Mathf.Epsilon) // 16:10
            {
                _camera.fieldOfView = _fieldOfView[2];
            }
            else if (_aspect >= 1.5) // 3:2
            {
                _camera.fieldOfView = _fieldOfView[2];
            }
            else //4:3
            {
                _camera.fieldOfView = _fieldOfView[3];
            }
        }
    }
}