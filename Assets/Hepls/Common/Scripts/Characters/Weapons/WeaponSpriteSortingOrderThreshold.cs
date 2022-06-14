using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(WeaponAim2D))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Sprite Sorting Order Threshold")]
    public class WeaponSpriteSortingOrderThreshold : MonoBehaviour
    {
        [Tooltip("the angle threshold at which to switch the sorting order")]
        public float Threshold = 0f;
        [Tooltip("the sorting order to apply when the weapon's rotation is below threshold")]
        public int BelowThresholdSortingOrder = 1;
        [Tooltip("the sorting order to apply when the weapon's rotation is above threshold")]
        public int AboveThresholdSortingOrder = -1;
        [Tooltip("the sprite whose sorting order we want to modify")]
        public SpriteRenderer Sprite;

        protected WeaponAim2D _weaponAim2D;
        protected virtual void Awake()
        {
            _weaponAim2D = this.gameObject.GetComponent<WeaponAim2D>();
        }
        protected virtual void Update()
        {
            if ((_weaponAim2D == null) || (Sprite == null)) 
            {
                return;
            }

            Sprite.sortingOrder = (_weaponAim2D.CurrentAngleRelative > Threshold) ? AboveThresholdSortingOrder : BelowThresholdSortingOrder;
        }
	}
}
