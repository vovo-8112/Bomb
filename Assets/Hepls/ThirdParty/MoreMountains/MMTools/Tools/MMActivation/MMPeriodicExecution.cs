using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    public class MMPeriodicExecution : MonoBehaviour
    {
        [MMVector("Min", "Max")]
        public Vector2 RandomIntervalDuration = new Vector2(1f, 3f);
        public UnityEvent OnRandomInterval;
        
        protected float _lastUpdateAt = 0f;
        protected float _currentInterval = 0f;
        protected virtual void Start()
        {
            DetermineNewInterval();
        }
        protected virtual void Update()
        {
            if (Time.time - _lastUpdateAt > _currentInterval)
            {
                OnRandomInterval?.Invoke();
                _lastUpdateAt = Time.time;
                DetermineNewInterval();
            }
        }
        protected virtual void DetermineNewInterval()
        {
            _currentInterval = Random.Range(RandomIntervalDuration.x, RandomIntervalDuration.y);
        }
    }
}
