using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Items/Pickable Action")]
    public class PickableAction : PickableItem
    {
        [Tooltip("the action(s) to trigger when picked")]
        public UnityEvent PickEvent;
        protected override void Pick(GameObject picker)
        {
            base.Pick(picker);
            if (PickEvent != null)
            {
                PickEvent.Invoke();
            }
        }
    }
}