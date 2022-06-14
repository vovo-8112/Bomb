using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMDoubleSpriteMask : MonoBehaviour, MMEventListener<MMSpriteMaskEvent>
    {
        [Header("Masks")]
        [Tooltip("the first sprite mask")]
        public MMSpriteMask Mask1;
        [Tooltip("the second sprite mask")]
        public MMSpriteMask Mask2;

        protected MMSpriteMask _currentMask;
        protected MMSpriteMask _dormantMask;
        protected virtual void Awake()
        {
            Mask1.gameObject.SetActive(true);
            Mask2.gameObject.SetActive(false);
            _currentMask = Mask1;
            _dormantMask = Mask2;
        }
        protected virtual void SwitchCurrentMask()
        {
            _currentMask = (_currentMask == Mask1) ? Mask2 : Mask1;
            _dormantMask = (_currentMask == Mask1) ? Mask2 : Mask1;
        }
        protected virtual IEnumerator DoubleMaskCo(MMSpriteMaskEvent spriteMaskEvent)
        {
            _dormantMask.transform.position = spriteMaskEvent.NewPosition;
            _dormantMask.transform.localScale = spriteMaskEvent.NewSize * _dormantMask.ScaleMultiplier;
            _dormantMask.gameObject.SetActive(true);
            yield return new WaitForSeconds(spriteMaskEvent.Duration);
            _currentMask.gameObject.SetActive(false);
            SwitchCurrentMask();
        }
        public virtual void OnMMEvent(MMSpriteMaskEvent spriteMaskEvent)
        {
            switch (spriteMaskEvent.EventType)
            {
                case MMSpriteMaskEvent.MMSpriteMaskEventTypes.DoubleMask:
                    StartCoroutine(DoubleMaskCo(spriteMaskEvent));
                    break;
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMSpriteMaskEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMSpriteMaskEvent>();
        }
    }
}
