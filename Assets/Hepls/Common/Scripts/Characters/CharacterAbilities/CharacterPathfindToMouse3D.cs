using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Pathfind To Mouse")]
    [RequireComponent(typeof(CharacterPathfinder3D))]
    public class CharacterPathfindToMouse3D : CharacterAbility
    {
        [Header("Mouse")]
        [Tooltip("the index of the mouse button to read input on")]
        public int MouseButtonIndex = 1;

        [Header("OnClick")]
        [Tooltip("a feedback to play at the position of the click")]
        public MMFeedbacks OnClickFeedbacks; 
        
        public GameObject Destination { get; set; }

        protected CharacterPathfinder3D _characterPathfinder3D;
        protected Plane _playerPlane;
        protected bool _destinationSet = false;
        protected Camera _mainCamera;
        protected override void Initialization()
        {
            base.Initialization();
            _mainCamera = Camera.main;
            _characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
            _character.FindAbility<CharacterMovement>().ScriptDrivenInput = true;
            
            OnClickFeedbacks?.Initialization();
            _playerPlane = new Plane(Vector3.up, Vector3.zero);
            if (Destination == null)
            {
                Destination = new GameObject();
                Destination.name = this.name + "PathfindToMouseDestination";
            }
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            if (!AbilityAuthorized)
            {
                return;
            }
            DetectMouse();
        }
        protected virtual void DetectMouse()
        {
            if (Input.GetMouseButtonDown(MouseButtonIndex))
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
                    OnClickFeedbacks?.PlayFeedbacks(Destination.transform.position);
                }
            }
        }
    }
}
