using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/MouseDrivenPathfinderAI3D")]
    public class MouseDrivenPathfinderAI3D : MonoBehaviour 
	{
        [Header("Testing")]
		[Tooltip("the camera we'll use to determine the destination from")]
        public Camera Cam;
		[Tooltip("a gameobject used to show the destination")]
        public GameObject Destination;

        protected CharacterPathfinder3D _characterPathfinder3D;
		protected Plane _playerPlane;
        protected bool _destinationSet = false;
        protected Camera _mainCamera;
        protected virtual void Awake()
        {
            _mainCamera = Camera.main;
            _characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
            _playerPlane = new Plane(Vector3.up, Vector3.zero);
        }
        protected virtual void Update()
        {
            DetectMouse();
        }
        protected virtual void DetectMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
                float distance;
                if (_playerPlane.Raycast(ray, out distance))
                {
                    Vector3 target = ray.GetPoint(distance);
                    Destination.transform.position = target;
                    _destinationSet = true;
                    _characterPathfinder3D.SetNewDestination(Destination.transform);
                }
            }
        }
    }
}
