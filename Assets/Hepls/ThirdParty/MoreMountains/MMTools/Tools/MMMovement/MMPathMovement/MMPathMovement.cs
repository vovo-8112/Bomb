using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Movement/MMPathMovement")]
    public class MMPathMovement : MonoBehaviour 
	{
		public enum PossibleAccelerationType
		{
			ConstantSpeed,
			EaseOut,
			AnimationCurve
		}
		public enum CycleOptions
		{
			BackAndForth,
			Loop,
			OnlyOnce,
            StopAtBounds,
            Random
		}
		public enum MovementDirection
		{
			Ascending,
			Descending
		}

        public enum UpdateModes
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

		[Header("Path")]
		[MMInformation("Here you can select the '<b>Cycle Option</b>'. Back and Forth will have your object follow the path until its end, and go back to the original point. If you select Loop, the path will be closed and the object will move along it until told otherwise. If you select Only Once, the object will move along the path from the first to the last point, and remain there forever.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public CycleOptions CycleOption;

		[MMInformation("Add points to the <b>Path</b> (set the size of the path first), then position the points using either the inspector or by moving the handles directly in scene view. For each path element you can specify a delay (in seconds). The order of the points will be the order the object follows.\nFor looping paths, you can then decide if the object will go through the points in the Path in Ascending (1, 2, 3...) or Descending (Last, Last-1, Last-2...) order.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public MovementDirection LoopInitialMovementDirection = MovementDirection.Ascending;
		public List<MMPathMovementElement> PathElements;

		[Header("Movement")]
		[MMInformation("Set the <b>speed</b> at which the path will be crawled, and if the movement should be constant or eased.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public float MovementSpeed = 1;
		public Vector3 CurrentSpeed { get; protected set; }
		public PossibleAccelerationType AccelerationType = PossibleAccelerationType.ConstantSpeed;
		public AnimationCurve Acceleration = new AnimationCurve(new Keyframe(0,1f),new Keyframe(1f,0f));
        public UpdateModes UpdateMode = UpdateModes.Update;

        [Header("Settings")]
		[MMInformation("The <b>MinDistanceToGoal</b> is used to check if we've (almost) reached a point in the Path. The 2 other settings here are for debug only, don't change them.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public float MinDistanceToGoal = .1f;
		protected Vector3 _originalTransformPosition;
		protected bool _originalTransformPositionStatus=false;
        public virtual bool CanMove { get; set; }
        
        protected bool _active=false;
	    protected IEnumerator<Vector3> _currentPoint;
		protected int _direction = 1;
		protected Vector3 _initialPosition;
	    protected Vector3 _finalPosition;
		protected Vector3 _previousPoint = Vector3.zero;
	    protected float _waiting=0;
	    protected int _currentIndex;
		protected float _distanceToNextPoint;
		protected bool _endReached = false;
	    protected virtual void Awake ()
		{
			Initialization ();
		}
        protected virtual void Start()
        {
            _originalTransformPosition = transform.position;
        }
        public virtual void ResetPath()
        {
            Initialization();
            CanMove = false;
            transform.position = _originalTransformPosition;
        }
        protected virtual void Initialization()
		{
			_active=true;
			_endReached = false;
            CanMove = true;
			if(PathElements == null || PathElements.Count < 1)
			{
				return;
			}
			if (LoopInitialMovementDirection == MovementDirection.Ascending)
			{
				_direction=1;
			}
			else
			{
				_direction=-1;
			}
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();
			if (!_originalTransformPositionStatus)
			{
				_originalTransformPositionStatus = true;
				_originalTransformPosition = transform.position;
			}
			transform.position = _originalTransformPosition + _currentPoint.Current;
		}

        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                ExecuteUpdate();
            }
        }

        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate)
            {
                ExecuteUpdate();
            }
        }

        protected virtual void Update()
        {
            if (UpdateMode == UpdateModes.Update)
            {
                ExecuteUpdate();
            }
        }
        protected virtual void PointReached()
        {

        }
        protected virtual void EndReached()
        {

        }
		protected virtual void ExecuteUpdate () 
		{
			if(PathElements == null 
				|| PathElements.Count < 1
				|| _endReached
				|| !CanMove
				)
			{
				return;
			}

			Move ();
		}
		protected virtual void Move()
		{
			_waiting -= Time.deltaTime;
			if (_waiting > 0)
			{
				CurrentSpeed = Vector3.zero;
				return;
			}
			_initialPosition=transform.position;
			MoveAlongThePath();
			_distanceToNextPoint = (transform.position - (_originalTransformPosition + _currentPoint.Current)).magnitude;
			if(_distanceToNextPoint < MinDistanceToGoal)
			{
				if (PathElements.Count > _currentIndex)
				{
					_waiting = PathElements[_currentIndex].Delay;				 
				}
                PointReached();
                _previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}
			_finalPosition = transform.position;
            if (Time.deltaTime != 0f)
            {
                CurrentSpeed = (_finalPosition - _initialPosition) / Time.deltaTime;
            }

            if (_endReached) 
			{
                EndReached();
                CurrentSpeed = Vector3.zero;
			}
		}
		public virtual void MoveAlongThePath()
		{
			switch (AccelerationType)
			{
				case PossibleAccelerationType.ConstantSpeed:
					transform.position = Vector3.MoveTowards (transform.position, _originalTransformPosition + _currentPoint.Current, Time.deltaTime * MovementSpeed);
					break;
				
				case PossibleAccelerationType.EaseOut:
					transform.position = Vector3.Lerp (transform.position, _originalTransformPosition + _currentPoint.Current, Time.deltaTime * MovementSpeed);
					break;

				case PossibleAccelerationType.AnimationCurve:
					float distanceBetweenPoints = Vector3.Distance (_previousPoint, _currentPoint.Current);

					if (distanceBetweenPoints <= 0)
					{
						return;
					}

					float remappedDistance = 1 - MMMaths.Remap (_distanceToNextPoint, 0f, distanceBetweenPoints, 0f, 1f);
					float speedFactor = Acceleration.Evaluate (remappedDistance);

					transform.position = Vector3.MoveTowards (transform.position, _originalTransformPosition + _currentPoint.Current, Time.deltaTime * MovementSpeed * speedFactor);
					break;
			}
		}
		public virtual IEnumerator<Vector3> GetPathEnumerator()
		{
			if(PathElements == null || PathElements.Count < 1)
			{
				yield break;
			}

			int index = 0;
			_currentIndex = index;
			while (true)
			{
				_currentIndex = index;
				yield return PathElements[index].PathElementPosition;
				
				if(PathElements.Count <= 1)
				{
					continue;
				}
                switch(CycleOption)
                {
                    case CycleOptions.Loop:
                        index = index + _direction;
                        if (index < 0)
                        {
                            index = PathElements.Count - 1;
                        }
                        else if (index > PathElements.Count - 1)
                        {
                            index = 0;
                        }
                        break;

                    case CycleOptions.BackAndForth:
                        if (index <= 0)
                        {
                            _direction = 1;
                        }
                        else if (index >= PathElements.Count - 1)
                        {
                            _direction = -1;
                        }
                        index = index + _direction;
                        break;

                    case CycleOptions.OnlyOnce:
                        if (index <= 0)
                        {
                            _direction = 1;
                        }
                        else if (index >= PathElements.Count - 1)
                        {
                            _direction = 0;
                            CurrentSpeed = Vector3.zero;
                            _endReached = true;
                        }
                        index = index + _direction;
                        break;
                    
                    case CycleOptions.Random:
	                    int newIndex = index;
	                    if (PathElements.Count > 1)
	                    {
		                    while (newIndex == index)
		                    {
			                    newIndex = Random.Range(0, PathElements.Count);
		                    }    
	                    }
	                    index = newIndex;
	                    break;

                    case CycleOptions.StopAtBounds:
                        if (index <= 0)
                        {
                            if (_direction == -1)
                            {
                                CurrentSpeed = Vector3.zero;
                                _endReached = true;
                            }
                            _direction = 1;
                        }
                        else if (index >= PathElements.Count - 1)
                        {
                            if (_direction == 1)
                            {
                                CurrentSpeed = Vector3.zero;
                                _endReached = true;
                            }
                            _direction = -1;
                        }
                        index = index + _direction;
                        break;
                }
            }
		}
		public virtual void ChangeDirection()
		{
			_direction = - _direction;
			_currentPoint.MoveNext();
		}
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			if (PathElements==null)
			{
				return;
			}

			if (PathElements.Count==0)
			{
				return;
			}
			if (_originalTransformPositionStatus==false)
			{
		    	_originalTransformPosition=transform.position;
				_originalTransformPositionStatus=true;
			}
			if (transform.hasChanged && _active==false)
			{
				_originalTransformPosition=transform.position;
			}
			for (int i=0;i<PathElements.Count;i++)
			{
				MMDebug.DrawGizmoPoint(_originalTransformPosition+PathElements[i].PathElementPosition,0.2f,Color.green);
				if ((i+1)<PathElements.Count)
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[i].PathElementPosition,_originalTransformPosition+PathElements[i+1].PathElementPosition);
				}
				if ( (i == PathElements.Count-1) && (CycleOption == CycleOptions.Loop) )
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[0].PathElementPosition,_originalTransformPosition+PathElements[i].PathElementPosition);
				}
			}
			if (Application.isPlaying)
			{
				MMDebug.DrawGizmoPoint(_originalTransformPosition + _currentPoint.Current,0.2f,Color.blue);
				MMDebug.DrawGizmoPoint(_originalTransformPosition + _previousPoint,0.2f,Color.red);
			}
			#endif


		}
		public virtual void UpdateOriginalTransformPosition(Vector3 newOriginalTransformPosition)
		{
			_originalTransformPosition = newOriginalTransformPosition;
		}
		public virtual Vector3 GetOriginalTransformPosition()
		{
			return _originalTransformPosition;
		}
		public virtual void SetOriginalTransformPositionStatus(bool status)
		{
			_originalTransformPositionStatus = status;
		}
		public virtual bool GetOriginalTransformPositionStatus()
		{
			return _originalTransformPositionStatus ;
		}
	}
}