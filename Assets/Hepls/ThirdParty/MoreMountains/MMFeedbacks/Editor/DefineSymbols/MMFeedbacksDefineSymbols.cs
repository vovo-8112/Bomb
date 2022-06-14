using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks {
#if UNITY_EDITOR
  [InitializeOnLoad]
  public class MMFeedbacksDefineSymbols {
    public static readonly string[] Symbols = new string[] {
      "MOREMOUNTAINS_FEEDBACKS"
    };
    static MMFeedbacksDefineSymbols() {
      string scriptingDefinesString =
        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
      List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
      scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
      PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
        string.Join(";", scriptingDefinesStringList.ToArray()));
    }
  }
#endif
}