﻿using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{

	public class MMTime : MonoBehaviour 
	{
        public static string FloatToTimeString(float t, bool displayHours = false, bool displayMinutes = true, bool displaySeconds = true, bool displayMilliseconds = false)
        {
            int intTime = (int)t;
            int hours = intTime / 3600;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            int milliseconds = Mathf.FloorToInt((t * 1000)%1000);
            
            if (displayHours && displayMinutes && displaySeconds && displayMilliseconds)
            {
                return string.Format("{0:00}:{1:00}:{2:00}.{3:D3}", hours, minutes, seconds, milliseconds);
            }
            if (!displayHours && displayMinutes && displaySeconds && displayMilliseconds)
            {
                return string.Format("{0:00}:{1:00}.{2:D3}", minutes, seconds, milliseconds);
            }
            if (!displayHours && !displayMinutes && displaySeconds && displayMilliseconds)
            {
                return string.Format("{0:D2}.{1:D3}", seconds, milliseconds);
            }
            if (!displayHours && !displayMinutes && displaySeconds && !displayMilliseconds)
            {
                return string.Format("{0:00}", seconds);
            }
            if (displayHours && displayMinutes && displaySeconds && !displayMilliseconds)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
            if (!displayHours && displayMinutes && displaySeconds && !displayMilliseconds)
            {
                return string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            return null;

        }
        public static float TimeStringToFloat(string timeInStringNotation)
	    {
			if (timeInStringNotation.Length!=12)
			{
				throw new Exception("The time in the TimeStringToFloat method must be specified using a hh:mm:ss:SSS syntax");
			}

			string[] timeStringArray = timeInStringNotation.Split(new string[] {":"},StringSplitOptions.None);

			float startTime=0f;
			float result;
			if (float.TryParse(timeStringArray[0], out result))
			{
				startTime+=result*3600f;
			}
			if (float.TryParse(timeStringArray[1], out result))
			{
				startTime+=result*60f;
			}
			if (float.TryParse(timeStringArray[2], out result))
			{
				startTime+=result;
			}
			if (float.TryParse(timeStringArray[3], out result))
			{
				startTime+=result/1000f;
			}

			return startTime;
	    }
	}
}
