using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Camera/MMBillboard")]
    public class MMBillboard : MonoBehaviour
	{
		public Camera MainCamera { get; set; }
		public bool GrabMainCameraOnStart = true;
        public bool NestObject = true;
        public Vector3 OffsetDirection = Vector3.back;
        public Vector3 Up = Vector3.up;

		protected GameObject _parentContainer;
        private Transform _transform;
		protected virtual void Awake()
		{
            _transform = transform;

			if (GrabMainCameraOnStart == true)
			{
				GrabMainCamera ();
			}
		}

        private void Start()
        {
            if (NestObject)
            {
                NestThisObject();
            }                
        }
        protected virtual void NestThisObject()
		{
			_parentContainer = new GameObject();
			_parentContainer.name = "Parent"+transform.gameObject.name;
			_parentContainer.transform.position = transform.position;
			transform.parent = _parentContainer.transform;
		}
		protected virtual void GrabMainCamera()
		{
			MainCamera = Camera.main;
		}
		protected virtual void Update()
		{
            if (NestObject)
            {
                _parentContainer.transform.LookAt(_parentContainer.transform.position + MainCamera.transform.rotation * OffsetDirection, MainCamera.transform.rotation * Up);
            }                
            else
            {
                _transform.LookAt(_transform.position + MainCamera.transform.rotation * OffsetDirection, MainCamera.transform.rotation * Up);
            }
        }
	}
}
