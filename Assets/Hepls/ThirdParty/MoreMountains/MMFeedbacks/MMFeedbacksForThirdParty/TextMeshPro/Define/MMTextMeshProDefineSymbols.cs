using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class MMTextMeshProDefineSymbols
    {
        public static readonly string[] Symbols = new string[] 
        {
            "MOREMOUNTAINS_TEXTMESHPRO_INSTALLED"
        };
        static MMTextMeshProDefineSymbols()
        {
            string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup); 
            List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
            scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", scriptingDefinesStringList.ToArray()));
        }
    }
#endif
}