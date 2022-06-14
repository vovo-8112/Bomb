using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Health/Health Auto Refill")]
    public class HealthAutoRefill : MonoBehaviour
    {
        public enum RefillModes { Linear, Bursts }

        [Header("Mode")]
        [Tooltip("the selected refill mode ")]
        public RefillModes RefillMode;

        [Header("Cooldown")]
        [Tooltip("how much time, in seconds, should pass before the refill kicks in")]
        public float CooldownAfterHit = 1f;
        
        [Header("Refill Settings")]
        [Tooltip("if this is true, health will refill itself when not at full health")]
        public bool RefillHealth = true;
        [MMEnumCondition("RefillMode", (int)RefillModes.Linear)]
        [Tooltip("the amount of health per second to restore when in linear mode")]
        public int HealthPerSecond;
        [MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
        [Tooltip("the amount of health to restore per burst when in burst mode")]
        public int HealthPerBurst = 5;
        [MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
        [Tooltip("the duration between two health bursts, in seconds")]
        public float DurationBetweenBursts = 2f;

        protected Health _health;
        protected float _lastHitTime = 0f;
        protected float _healthToGive = 0f;
        protected float _lastBurstTimestamp;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _health = this.gameObject.GetComponent<Health>();
        }
        protected virtual void Update()
        {
            ProcessRefillHealth();
        }
        protected virtual void ProcessRefillHealth()
        {
            if (!RefillHealth)
            {
                return;
            }

            if (Time.time - _lastHitTime < CooldownAfterHit)
            {
                return;
            }

            if (_health.CurrentHealth < _health.MaximumHealth)
            {
                switch (RefillMode)
                {
                    case RefillModes.Bursts:
                        if (Time.time - _lastBurstTimestamp > DurationBetweenBursts)
                        {
                            _health.GetHealth(HealthPerBurst, this.gameObject);
                            _lastBurstTimestamp = Time.time;
                        }
                        break;

                    case RefillModes.Linear:
                        _healthToGive += HealthPerSecond * Time.deltaTime;
                        if (_healthToGive > 1f)
                        {
                            int givenHealth = (int)_healthToGive;
                            _healthToGive -= givenHealth;
                            _health.GetHealth(givenHealth, this.gameObject);
                        }
                        break;
                }
            }
        }
        public virtual void OnHit()
        {
            _lastHitTime = Time.time;
        }
        protected virtual void OnEnable()
        {
            _health.OnHit += OnHit;
        }
        protected virtual void OnDisable()
        {
            _health.OnHit -= OnHit;
        }
    }
}
