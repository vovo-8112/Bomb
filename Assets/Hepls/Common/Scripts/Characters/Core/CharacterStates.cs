using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	public class CharacterStates 
	{
		public enum CharacterConditions
		{
			Normal,
			ControlledMovement,
			Frozen,
			Paused,
			Dead
		}
		public enum MovementStates 
		{
			Null,
			Idle,
			Falling,
			Walking,
			Running,
			Crouching,
			Crawling, 
			Dashing,
			Jetpacking,
			Jumping,
			Pushing,
			DoubleJumping,
			Attacking,
            FallingDownHole
		}
	}
}