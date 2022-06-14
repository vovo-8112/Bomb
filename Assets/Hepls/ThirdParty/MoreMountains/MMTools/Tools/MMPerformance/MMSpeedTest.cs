using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace MoreMountains.Tools
{
    public struct MMSpeedTestItem
    {
        public string TestID;
        public Stopwatch Timer;
        public MMSpeedTestItem(string testID)
        {
            TestID = testID;
            Timer = Stopwatch.StartNew();
        }
    }
    public static class MMSpeedTest 
    {
        private static readonly Dictionary<string, MMSpeedTestItem> _speedTests = new Dictionary<string, MMSpeedTestItem>();
        public static void StartTest(string testID)
        {
            if (_speedTests.ContainsKey(testID))
            {
                _speedTests.Remove(testID);
            }

            MMSpeedTestItem item = new MMSpeedTestItem(testID);
            _speedTests.Add(testID, item);
        }
        public static void EndTest(string testID)
        {
            if (!_speedTests.ContainsKey(testID))
            {
                return;
            }

            _speedTests[testID].Timer.Stop();
            float elapsedTime = _speedTests[testID].Timer.ElapsedMilliseconds / 1000f;
            _speedTests.Remove(testID);

            UnityEngine.Debug.Log("<color=red>MMSpeedTest</color> [Test "+testID+"] test duration : "+elapsedTime+"s");
        }        
    }
}
