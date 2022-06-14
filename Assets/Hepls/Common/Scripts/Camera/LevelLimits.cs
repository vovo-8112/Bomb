using UnityEngine;
using System.Collections;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Camera/LevelLimits")]
    public class LevelLimits : MonoBehaviour
	{
		[Tooltip("Left x coordinate")]
		public float LeftLimit;
		[Tooltip("Right x coordinate")]
		public float RightLimit;
		[Tooltip("Bottom y coordinate ")]
		public float BottomLimit;
		[Tooltip("Top y coordinate")]
		public float TopLimit;

        protected BoxCollider2D _collider;
	    protected virtual void Awake()
	    {
	        _collider = GetComponent<BoxCollider2D>();

	        LeftLimit = _collider.bounds.min.x;
	        RightLimit = _collider.bounds.max.x;
	        BottomLimit = _collider.bounds.min.y;
	        TopLimit = _collider.bounds.max.y;
	    }
	}
}