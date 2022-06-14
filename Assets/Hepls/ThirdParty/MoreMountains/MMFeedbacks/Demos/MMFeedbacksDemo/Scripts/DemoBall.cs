using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    public class DemoBall : MonoBehaviour
    {
        public float LifeSpan = 2f;
        public MMFeedbacks DeathFeedback;
        protected virtual void Start()
        {
            StartCoroutine(ProgrammedDeath());
        }
        protected virtual IEnumerator ProgrammedDeath()
        {
            yield return MMCoroutine.WaitFor(LifeSpan);
            DeathFeedback?.PlayFeedbacks();
            this.gameObject.SetActive(false);
        }
    }
}