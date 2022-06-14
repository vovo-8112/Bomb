using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Feedbacks {
  [CreateAssetMenu(menuName = "MoreMountains/MMFeedbacks/Configuration", fileName = "MMFeedbacksConfiguration")]
  public class MMFeedbacksConfiguration : ScriptableObject {
    private static MMFeedbacksConfiguration _instance;
    private static bool _instantiated;
    public static MMFeedbacksConfiguration Instance {
      get {
        if (_instantiated) {
          return _instance;
        }

        string assetName = typeof(MMFeedbacksConfiguration).Name;

        MMFeedbacksConfiguration[] loadedAssets = Resources.LoadAll<MMFeedbacksConfiguration>("");

        if (loadedAssets.Length > 1) {
          Debug.LogError("Multiple " + assetName + "s were found in the project. There should only be one.");
        }

        if (loadedAssets.Length == 0) {
          _instance = CreateInstance<MMFeedbacksConfiguration>();
          Debug.LogError("No " + assetName + " found in the project, one was created at runtime, it won't persist.");
        } else {
          _instance = loadedAssets[0];
        }

        _instantiated = true;

        return _instance;
      }
    }

    [Header("Debug")]
    public MMFeedbacks _mmFeedbacks;

    [Header("Help settings")]
    public bool ShowInspectorTips = true;

    private void OnDestroy() {
      _instantiated = false;
    }
  }
}