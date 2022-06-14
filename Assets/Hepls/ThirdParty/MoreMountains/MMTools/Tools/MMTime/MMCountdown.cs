using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("More Mountains/Tools/Time/MMCountdown")]
    public class MMCountdown : MonoBehaviour
    {
        [Serializable]
        public class MMCountdownFloor
        {
            public float FloorValue;
            [MMReadOnly]
            public float LastChangedAt = 0f;
            public UnityEvent FloorEvent;
        }
        public enum MMCountdownDirections { Ascending, Descending }
        
        [Header("Debug")]
        [MMReadOnly]
        public float CurrentTime;
        [MMReadOnly]
        public MMCountdownDirections Direction;

        [Header("Countdown")]
        [MMInformation("You can define the bounds of the countdown (how much it should count down from, and to how much, the format it should be displayed in (standard Unity float ToString formatting).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public float CountdownFrom = 60f;
        public float CountdownTo = 0f;
        public bool Infinite = false;

        public enum FormatMethods { Explicit, Choices }

        [Header("Display")]
        public FormatMethods FormatMethod = FormatMethods.Choices;
        [MMEnumCondition("FormatMethod", (int)FormatMethods.Explicit)]
        public bool FloorValues = true;
        [MMEnumCondition("FormatMethod", (int)FormatMethods.Explicit)]
        public string Format = "00.00";
        [MMEnumCondition("FormatMethod", (int)FormatMethods.Choices)]
        public bool Hours = false;
        [MMEnumCondition("FormatMethod", (int)FormatMethods.Choices)]
        public bool Minutes = true;
        [MMEnumCondition("FormatMethod", (int)FormatMethods.Choices)]
        public bool Seconds = true;
        [MMEnumCondition("FormatMethod", (int)FormatMethods.Choices)]
        public bool Milliseconds = false;


        [Header("Settings")]
        [MMInformation("You can choose whether or not the countdown should automatically start on its Start, at what frequency (in seconds) it should refresh (0 means every frame), and the countdown's speed multiplier " +
            "(2 will be twice as fast, 0.5 half normal speed, etc). Floors are used to define and trigger events when certain floors are reached. For each floor, define a floor value (in seconds). Everytime this floor gets reached, the corresponding event will be triggered." +
            "Bind events here to trigger them when the countdown reaches its To destination, or every time it gets refreshed.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public bool AutoStart = true;
        public bool AutoReset = false;
        public bool PingPong = false;
        public float RefreshFrequency = 0.02f;
        public float CountdownSpeed = 1f;

        [Header("Floors")]
        public List<MMCountdownFloor> Floors;
        
        [Header("Events")]
        public UnityEvent CountdownCompleteEvent;
        public UnityEvent CountdownRefreshEvent;

        protected Text _text;
        protected float _lastRefreshAt;
        protected bool _countdowning = false;
        protected int _lastUnitValue = 0;
        protected virtual void Start()
        {
            _text = this.gameObject.GetComponent<Text>();
            Initialization();
        }

        protected virtual void Initialization()
        {
            _lastUnitValue = (int)CurrentTime;
            Direction = (CountdownFrom > CountdownTo) ? MMCountdownDirections.Descending : MMCountdownDirections.Ascending;

            CurrentTime = CountdownFrom;

            if (AutoStart)
            {
                StartCountdown();
            }
            foreach (MMCountdownFloor floor in Floors)
            {
                floor.LastChangedAt = CountdownFrom;
            }
        }
        protected virtual void Update()
        {
            if (!_countdowning)
            {
                return;
            }
            UpdateTime();
            UpdateText();
            CheckForFloors();
            CheckForEnd();
        }
        protected virtual void UpdateTime()
        {
            if (Direction == MMCountdownDirections.Descending)
            {
                CurrentTime -= Time.deltaTime * CountdownSpeed;
            }
            else
            {
                CurrentTime += Time.deltaTime * CountdownSpeed;
            }
        }
        protected virtual void UpdateText()
        {
            if (Time.time - _lastRefreshAt > RefreshFrequency)
            {
                if (_text != null)
                {
                    string newText = "";

                    if (FormatMethod == FormatMethods.Explicit)
                    {
                        if (FloorValues)
                        {
                            newText = Mathf.Floor(CurrentTime).ToString(Format);
                        }
                        else
                        {
                            newText = CurrentTime.ToString(Format);
                        }
                    }
                    else
                    {
                        newText = MMTime.FloatToTimeString(CurrentTime, Hours, Minutes, Seconds, Milliseconds);
                    }                    

                    _text.text = newText;
                }
                if (CountdownRefreshEvent != null)
                {
                    CountdownRefreshEvent.Invoke();
                }
                _lastRefreshAt = Time.time;
            }
        }
        protected virtual void CheckForEnd()
        {
            if (Infinite)
            {
                return;
            }

            bool endReached = (Direction == MMCountdownDirections.Ascending) ? (CurrentTime >= CountdownTo) : (CurrentTime <= CountdownTo);
            
            if (endReached)
            {
                if (CountdownCompleteEvent != null)
                {
                    CountdownCompleteEvent.Invoke();
                }
                if (PingPong)
                {
                    Direction = (Direction == MMCountdownDirections.Ascending) ? MMCountdownDirections.Descending : MMCountdownDirections.Ascending;
                    _countdowning = true;
                    float temp = CountdownFrom;
                    CountdownFrom = CountdownTo;
                    CountdownTo = temp;
                }
                else if (AutoReset)
                {
                    _countdowning = true;
                    CurrentTime = CountdownFrom;
                }
                else
                {
                    CurrentTime = CountdownTo;
                    _countdowning = false;
                }
            }
        }
        protected virtual void CheckForFloors()
        {
            foreach(MMCountdownFloor floor in Floors)
            {
                if (Mathf.Abs(CurrentTime - floor.LastChangedAt) >= floor.FloorValue)
                {
                    if (floor.FloorEvent != null)
                    {
                        floor.FloorEvent.Invoke();
                    }

                    if (Direction == MMCountdownDirections.Descending)
                    {
                        if (floor.LastChangedAt == CountdownFrom)
                        {                         
                            floor.LastChangedAt = CountdownFrom - floor.FloorValue;
                        }
                        else
                        {
                            floor.LastChangedAt = floor.LastChangedAt - floor.FloorValue;
                        }
                    }
                    else
                    {
                        if (floor.LastChangedAt == CountdownFrom)
                        {
                            floor.LastChangedAt = CountdownFrom + floor.FloorValue;
                        }
                        else
                        {
                            floor.LastChangedAt = floor.LastChangedAt + floor.FloorValue;
                        }
                    }                    
                }
            }
        }
        public virtual void StartCountdown()
        {
            _countdowning = true;
        }
        public virtual void StopCountdown()
        {
            _countdowning = false;
        }
        public virtual void ResetCountdown()
        {
            CurrentTime = CountdownFrom;
            Initialization();
        }
    }
}

