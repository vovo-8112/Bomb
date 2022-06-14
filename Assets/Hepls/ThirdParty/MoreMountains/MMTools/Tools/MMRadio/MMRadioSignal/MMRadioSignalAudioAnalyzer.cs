using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMRadioSignalAudioAnalyzer : MMRadioSignal
    {
        [Header("Audio Analyzer")]
        public MMAudioAnalyzer TargetAnalyzer;
        public int BeatID;
        protected override void Shake()
        {
            base.Shake();
            CurrentLevel = TargetAnalyzer.Beats[BeatID].CurrentValue * GlobalMultiplier;
        }
    }
}
