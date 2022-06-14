using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMPositionRecorder : MonoBehaviour
    {
        public enum Modes { Framecount, Time }

        [Header("Recording Settings")]
        public int NumberOfPositionsToRecord = 100;
        public Modes Mode = Modes.Framecount;
        [MMEnumCondition("Mode", (int)Modes.Framecount)]
        public int FrameInterval = 0;
        [MMEnumCondition("Mode", (int) Modes.Time)]
        public float TimeInterval = 0.02f;
        public bool RecordOnTimescaleZero = false;
        
        [Header("Debug")]
        public Vector3[] Positions;
        [MMReadOnly]
        public int FrameCounter;

        protected int _frameCountLastRecord = 0;
        protected float _timeLastRecord = 0f;
        protected virtual void Awake()
        {
            Positions = new Vector3[NumberOfPositionsToRecord];
            for (int i = 0; i < Positions.Length; i++)
            {
                Positions[i] = this.transform.position;    
            }
        }
        protected virtual void Update()
        {
            if (!RecordOnTimescaleZero && Time.timeScale == 0f)
            {
                return;
            }
            StorePositions();
        }
        protected virtual void StorePositions()
        {
            FrameCounter = Time.frameCount;

            if (Mode == Modes.Framecount)
            {
                if (FrameCounter - _frameCountLastRecord < FrameInterval)
                {
                    return;
                }

                _frameCountLastRecord = FrameCounter;
            }
            else
            {
                if (Time.time - _timeLastRecord < TimeInterval)
                {
                    return;
                }

                _timeLastRecord = Time.time;
            }
            Positions[0] = this.transform.position;
            Array.Copy(Positions, 0, Positions, 1, Positions.Length - 1);
        }
    }
}
