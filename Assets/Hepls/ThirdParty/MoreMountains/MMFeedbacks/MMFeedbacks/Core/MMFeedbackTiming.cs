using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public enum TimescaleModes { Scaled, Unscaled }
    [System.Serializable]
    public class MMFeedbackTiming
    {
        public enum MMFeedbacksDirectionConditions { Always, OnlyWhenForwards, OnlyWhenBackwards };
        public enum PlayDirections { FollowMMFeedbacksDirection, OppositeMMFeedbacksDirection, AlwaysNormal, AlwaysRewind }

        [Header("Timescale")]
        [Tooltip("whether we're working on scaled or unscaled time")]
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        [Header("Delays")]
        [Tooltip("the initial delay to apply before playing the delay (in seconds)")]
        public float InitialDelay = 0f;
        [Tooltip("the cooldown duration mandatory between two plays")]
        public float CooldownDuration = 0f;
        [Tooltip("if this is true, holding pauses won't wait for this feedback to finish")]
        public bool ExcludeFromHoldingPauses = false;

        [Header("Stop")]
        [Tooltip("if this is true, this feedback will interrupt itself when Stop is called on its parent MMFeedbacks, otherwise it'll keep running")]
        public bool InterruptsOnStop = true;

        [Header("Repeat")]
        [Tooltip("the repeat mode, whether the feedback should be played once, multiple times, or forever")]
        public int NumberOfRepeats = 0;
        [Tooltip("if this is true, the feedback will be repeated forever")]
        public bool RepeatForever = false;
        [Tooltip("the delay (in seconds) between repeats")]
        public float DelayBetweenRepeats = 1f;

        [Header("Play Direction")]
        [Tooltip("this defines how this feedback should play when the host MMFeedbacks is played :" +
                 "- always (default) : this feedback will always play" +
                 "- OnlyWhenForwards : this feedback will only play if the host MMFeedbacks is played in the top to bottom direction (forwards)" +
                 "- OnlyWhenBackwards : this feedback will only play if the host MMFeedbacks is played in the bottom to top direction (backwards)")]
        public MMFeedbacksDirectionConditions MMFeedbacksDirectionCondition = MMFeedbacksDirectionConditions.Always;
        [Tooltip("this defines the way this feedback will play. It can play in its normal direction, or in rewind (a sound will play backwards," +
                 " an object normally scaling up will scale down, a curve will be evaluated from right to left, etc)" +
                 "- BasedOnMMFeedbacksDirection : will play normally when the host MMFeedbacks is played forwards, in rewind when it's played backwards" +
                 "- OppositeMMFeedbacksDirection : will play in rewind when the host MMFeedbacks is played forwards, and normally when played backwards" +
                 "- Always Normal : will always play normally, regardless of the direction of the host MMFeedbacks" +
                 "- Always Rewind : will always play in rewind, regardless of the direction of the host MMFeedbacks")]
        public PlayDirections PlayDirection = PlayDirections.FollowMMFeedbacksDirection;

        [Header("Intensity")]
        [Tooltip("if this is true, intensity will be constant, even if the parent MMFeedbacks is played at a lower intensity")]
        public bool ConstantIntensity = false;
        [Tooltip("if this is true, this feedback will only play if its intensity is higher or equal to IntensityIntervalMin and lower than IntensityIntervalMax")]
        public bool UseIntensityInterval = false;
        [Tooltip("the minimum intensity required for this feedback to play")]
        [MMFCondition("UseIntensityInterval", true)]
        public float IntensityIntervalMin = 0f;
        [Tooltip("the maximum intensity required for this feedback to play")]
        [MMFCondition("UseIntensityInterval", true)]
        public float IntensityIntervalMax = 0f;

        [Header("Sequence")]
        [Tooltip("A MMSequence to use to play these feedbacks on")]
        public MMSequence Sequence;
        [Tooltip("The MMSequence's TrackID to consider")]
        public int TrackID = 0;
        [Tooltip("whether or not to use the quantized version of the target sequence")]
        public bool Quantized = false;
        [Tooltip("if using the quantized version of the target sequence, the BPM to apply to the sequence when playing it")]
        [MMFCondition("Quantized", true)]
        public int TargetBPM = 120;
    }
}
