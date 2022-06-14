using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Spawn/GoToLevelEntryPoint")]
    public class GoToLevelEntryPoint : FinishLevel 
	{
        [Space(10)]
        [Header("Points of Entry")]
        [Tooltip("Whether or not to use entry points. If you don't, you'll simply move on to the next level")]
        public bool UseEntryPoints = false;
        [Tooltip("The index of the point of entry to move to in the next level")]
        public int PointOfEntryIndex;
        [Tooltip("The direction to face when moving to the next level")]
        public Character.FacingDirections FacingDirection;
		public override void GoToNextLevel()
		{
            if (UseEntryPoints)
            {
                GameManager.Instance.StorePointsOfEntry(LevelName, PointOfEntryIndex, FacingDirection);
            }
			
			base.GoToNextLevel ();
		}
	}
}
