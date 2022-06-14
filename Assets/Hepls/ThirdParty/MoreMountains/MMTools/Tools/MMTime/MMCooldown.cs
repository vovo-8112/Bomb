using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [System.Serializable]    
    public class MMCooldown 
    {
        public enum CooldownStates { Idle, Consuming, PauseOnEmpty, Refilling }
        public bool Unlimited = false;
        public float ConsumptionDuration = 2f;
        public float PauseOnEmptyDuration = 1f;
        public float RefillDuration = 1f;
        public bool CanInterruptRefill = true;
        [MMReadOnly]
        public CooldownStates CooldownState = CooldownStates.Idle;
        [MMReadOnly]
        public float CurrentDurationLeft;

        protected WaitForSeconds _pauseOnEmptyWFS;
        protected float _emptyReachedTimestamp = 0f;
        public virtual void Initialization()
        {
            _pauseOnEmptyWFS = new WaitForSeconds(PauseOnEmptyDuration);
            CurrentDurationLeft = ConsumptionDuration;
            CooldownState = CooldownStates.Idle;
            _emptyReachedTimestamp = 0f;
        }
        public virtual void Start()
        {
            if (Ready())
            {
                CooldownState = CooldownStates.Consuming;
            }
        }

        public virtual bool Ready()
        {
            if (Unlimited)
            {
                return true;
            }
            if (CooldownState == CooldownStates.Idle)
            {
                return true;
            }
            if ((CooldownState == CooldownStates.Refilling) && (CanInterruptRefill))
            {
                return true;
            }
            return false;
        }
        public virtual void Stop()
        {
            if (CooldownState == CooldownStates.Consuming)
            {
                CooldownState = CooldownStates.PauseOnEmpty;
            }
        }
        public virtual void Update()
        {
            if (Unlimited)
            {
                return;
            }

            switch (CooldownState)
            {
                case CooldownStates.Idle:
                    break;

                case CooldownStates.Consuming:
                    CurrentDurationLeft = CurrentDurationLeft - Time.deltaTime;
                    if (CurrentDurationLeft <= 0f)
                    {
                        CurrentDurationLeft = 0f;
                        _emptyReachedTimestamp = Time.time;
                        CooldownState = CooldownStates.PauseOnEmpty;
                    }
                    break;

                case CooldownStates.PauseOnEmpty:
                    if (Time.time - _emptyReachedTimestamp >= PauseOnEmptyDuration)
                    {
                        CooldownState = CooldownStates.Refilling;
                    }
                    break;

                case CooldownStates.Refilling:
                    CurrentDurationLeft += (RefillDuration * Time.deltaTime) / RefillDuration;
                    if (CurrentDurationLeft >= RefillDuration)
                    {
                        CurrentDurationLeft = ConsumptionDuration;
                        CooldownState = CooldownStates.Idle;
                    }
                    break;
            }
        }
	}
}
