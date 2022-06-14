using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{	
	[AddComponentMenu("TopDown Engine/Environment/Teleporter")]
	public class Teleporter : ButtonActivated 
	{
        public enum CameraModes { DoNothing, TeleportCamera, CinemachinePriority }
        public enum TeleportationModes { Instant, Tween }
        public enum TimeModes { Unscaled, Scaled }

        [Header("Teleporter")]
        [Tooltip("if true, this won't teleport non player characters")]
        public bool OnlyAffectsPlayer = true;
        [Tooltip("the offset to apply when exiting this teleporter")]
        public Vector3 ExitOffset;
        [Tooltip("the selected teleportation mode ")]
        public TeleportationModes TeleportationMode = TeleportationModes.Instant;
        [MMEnumCondition("TeleportationMode", (int)TeleportationModes.Tween)]
        [Tooltip("the curve to apply to the teleportation tween")]
        public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInCubic;
        [Tooltip("whether or not to maintain the x value of the teleported object on exit")]
        public bool MaintainXEntryPositionOnExit = false;
        [Tooltip("whether or not to maintain the y value of the teleported object on exit")]
        public bool MaintainYEntryPositionOnExit = false;
        [Tooltip("whether or not to maintain the z value of the teleported object on exit")]
        public bool MaintainZEntryPositionOnExit = false;

        [Header("Destination")]
        [Tooltip("the teleporter's destination")]
        public Teleporter Destination;
        [Tooltip("if this is true, the teleported object will be put on the destination's ignore list, to prevent immediate re-entry. If your destination's offset is far enough from its center, you can set that to false")]
        public bool AddToDestinationIgnoreList = true;

        [Header("Rooms")]
        [Tooltip("the chosen camera mode")]
        public CameraModes CameraMode = CameraModes.TeleportCamera;
        [Tooltip("the room this teleporter belongs to")]
        public Room CurrentRoom;
        [Tooltip("the target room")]
        public Room TargetRoom;
        
        [Header("MMFader Transition")]
        [Tooltip("if this is true, a fade to black will occur when teleporting")]
        public bool TriggerFade = false;
        [MMCondition("TriggerFade", true)]
        [Tooltip("the ID of the fader to target")]
        public int FaderID = 0;
        [Tooltip("the curve to use to fade to black")]
        public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        [Tooltip("if this is true, fade events will ignore timescale")]
        public bool FadeIgnoresTimescale = false;

        [Header("Mask")]
        [Tooltip("whether or not we should ask to move a MMSpriteMask on activation")]
        public bool MoveMask = true;
        [MMCondition("MoveMask", true)]
        [Tooltip("the curve to move the mask along to")]
        public MMTween.MMTweenCurve MoveMaskCurve = MMTween.MMTweenCurve.EaseInCubic;
        [MMCondition("MoveMask", true)]
        [Tooltip("the method used to move the mask")]
        public MMSpriteMaskEvent.MMSpriteMaskEventTypes MoveMaskMethod = MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition;
        [MMCondition("MoveMask", true)]
        [Tooltip("the duration of the mask movement (usually the same as the DelayBetweenFades")]
        public float MoveMaskDuration = 0.2f;

        [Header("Freeze")]
        [Tooltip("whether or not time should be frozen during the transition")]
        public bool FreezeTime = false;
        [Tooltip("whether or not the character should be frozen (input blocked) for the duration of the transition")]
        public bool FreezeCharacter = true;

        [Header("Teleport Sequence")]
        [Tooltip("the timescale to use for the teleport sequence")]
        public TimeModes TimeMode = TimeModes.Unscaled;
        [Tooltip("the delay (in seconds) to apply before running the sequence")]
        public float InitialDelay = 0.1f;
        [Tooltip("the duration (in seconds) after the initial delay covering for the fade out of the scene")]
        public float FadeOutDuration = 0.2f;
        [Tooltip("the duration (in seconds) to wait for after the fade out and before the fade in")]
        public float DelayBetweenFades = 0.3f;
        [Tooltip("the duration (in seconds) after the initial delay covering for the fade in of the scene")]
        public float FadeInDuration = 0.2f;
        [Tooltip("the duration (in seconds) to apply after the fade in of the scene")]
        public float FinalDelay = 0.1f;

        public float LocalTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledTime : Time.time;
        public float LocalDeltaTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledDeltaTime : Time.deltaTime;

        protected Character _player;
        protected Character _characterTester;
        protected CharacterGridMovement _characterGridMovement;
	    protected List<Transform> _ignoreList;

        protected Vector3 _entryPosition;
        protected Vector3 _newPosition;
        protected virtual void Awake()
		{
            InitializeTeleporter();
        }
        protected virtual void InitializeTeleporter()
        {
            _ignoreList = new List<Transform>();
            if (CurrentRoom == null)
            {
                CurrentRoom = this.gameObject.GetComponentInParent<Room>();
            }
        }
	    protected override void TriggerEnter(GameObject collider)
        {
            if (_ignoreList.Contains(collider.transform))
			{
				return;
			}

            _characterTester = collider.GetComponent<Character>();

            if (_characterTester != null)
			{
                if (RequiresPlayerType)
                {
                    if (_characterTester.CharacterType != Character.CharacterTypes.Player)
                    {
                        return;
                    }
                }

				_player = _characterTester;
                _characterGridMovement = _player.GetComponent<CharacterGridMovement>();
			}
			if (OnlyAffectsPlayer || !AutoActivation)
            {
                base.TriggerEnter(collider);
			}
			else
            {
                Teleport(collider);
			}
		}
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
            }
            base.TriggerButtonAction();
            Teleport(_player.gameObject);
        }
		protected virtual void Teleport(GameObject collider)
		{
            _entryPosition = collider.transform.position;
            if (Destination != null)
            {
                StartCoroutine(TeleportSequence(collider));         
			}
		}
        protected virtual IEnumerator TeleportSequence(GameObject collider)
		{
            SequenceStart(collider);

            for (float timer = 0, duration = InitialDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }
            
            AfterInitialDelay(collider);

            for (float timer = 0, duration = FadeOutDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

            AfterFadeOut(collider);
            
            for (float timer = 0, duration = DelayBetweenFades; timer < duration; timer += LocalDeltaTime) { yield return null; }

            AfterDelayBetweenFades(collider);

            for (float timer = 0, duration = FadeInDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

            AfterFadeIn(collider);

            for (float timer = 0, duration = FinalDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }

            SequenceEnd(collider);
        }
        protected virtual void SequenceStart(GameObject collider)
        {
            ActivateZone();

            if (CameraMode == CameraModes.TeleportCamera)
            {
                MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
            }

            if (FreezeTime)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
            }

            if (FreezeCharacter && (_player != null))
            {
                _player.Freeze();
            }
        }
        protected virtual void AfterInitialDelay(GameObject collider)
        {            
            if (TriggerFade)
            {
                MMFadeInEvent.Trigger(FadeOutDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
            }
        }
        protected virtual void AfterFadeOut(GameObject collider)
        {            
            TeleportCollider(collider);

            if (AddToDestinationIgnoreList)
            {
                Destination.AddToIgnoreList(collider.transform);
            }            
            
            if (CameraMode == CameraModes.CinemachinePriority)
            {
                MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
                MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, DelayBetweenFades);
            }

            if (CurrentRoom != null)
            {
                CurrentRoom.PlayerExitsRoom();
            }
            
            if (TargetRoom != null)
            {
                TargetRoom.PlayerEntersRoom();
                TargetRoom.VirtualCamera.Priority = 10;
                MMSpriteMaskEvent.Trigger(MoveMaskMethod, (Vector2)TargetRoom.RoomColliderCenter, TargetRoom.RoomColliderSize, MoveMaskDuration, MoveMaskCurve);
            }
        }
        protected virtual void TeleportCollider(GameObject collider)
        {
            _newPosition = Destination.transform.position + Destination.ExitOffset;
            if (MaintainXEntryPositionOnExit)
            {
                _newPosition.x = _entryPosition.x;
            }
            if (MaintainYEntryPositionOnExit)
            {
                _newPosition.y = _entryPosition.y;
            }
            if (MaintainZEntryPositionOnExit)
            {
                _newPosition.z = _entryPosition.z;
            }

            switch (TeleportationMode)
            {
                case TeleportationModes.Instant:
                    collider.transform.position = _newPosition;
                    _ignoreList.Remove(collider.transform);
                    break;
                case TeleportationModes.Tween:
                    StartCoroutine(TeleportTweenCo(collider, collider.transform.position, _newPosition));
                    break;
            }
        }
        protected virtual IEnumerator TeleportTweenCo(GameObject collider, Vector3 origin, Vector3 destination)
        {
            float startedAt = LocalTime;
            while (LocalTime - startedAt < DelayBetweenFades)
            {
                float elapsedTime = LocalTime - startedAt;
                collider.transform.position = MMTween.Tween(elapsedTime, 0f, DelayBetweenFades, origin, destination, TweenCurve);
                yield return null;
            }
            _ignoreList.Remove(collider.transform);
        }
        protected virtual void AfterDelayBetweenFades(GameObject collider)
        {
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            if (TriggerFade)
            {
                MMFadeOutEvent.Trigger(FadeInDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
            }
        }
        protected virtual void AfterFadeIn(GameObject collider)
        {

        }
        protected virtual void SequenceEnd(GameObject collider)
        {
            if (FreezeCharacter && (_player != null))
            {
                _player.UnFreeze();
            }

            if (_characterGridMovement != null)
            {
                _characterGridMovement.SetCurrentWorldPositionAsNewPosition();
            }

            if (FreezeTime)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            }
        }
        public override void TriggerExitAction(GameObject collider)
        {
            if (_ignoreList.Contains(collider.transform))
            {
                _ignoreList.Remove(collider.transform);
            }
            base.TriggerExitAction(collider);
        }
        public virtual void AddToIgnoreList(Transform objectToIgnore)
		{
            if (!_ignoreList.Contains(objectToIgnore))
            {
                _ignoreList.Add(objectToIgnore);
            }            
        }
        protected virtual void OnDrawGizmos()
        {
            if (Destination != null)
            {
                MMDebug.DrawGizmoArrow(this.transform.position, (Destination.transform.position + Destination.ExitOffset) - this.transform.position, Color.cyan, 1f, 25f);
                MMDebug.DebugDrawCross(this.transform.position + ExitOffset, 0.5f, Color.yellow);
                MMDebug.DrawPoint(this.transform.position + ExitOffset, Color.yellow, 0.5f);
            }

            if (TargetRoom != null)
            {
                MMDebug.DrawGizmoArrow(this.transform.position, TargetRoom.transform.position - this.transform.position, MMColors.Pink, 1f, 25f);
            }
        }
	}
}