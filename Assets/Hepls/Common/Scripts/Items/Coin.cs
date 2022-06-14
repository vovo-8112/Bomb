using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Items/Coin")]
	public class Coin : PickableItem
	{
		[Tooltip("The amount of points to add when collected")]
		public int PointsToAdd = 10;
		protected override void Pick(GameObject picker) 
		{
			TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsToAdd);
		}
	}
}