using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.InventoryEngine
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class InventoryEngineDefineSymbols
    {
        public static readonly string[] Symbols = new string[]
        {
         "MOREMOUNTAINS_INVENTORYENGINE"
        };
        static InventoryEngineDefineSymbols()
        {
            string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
            scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", scriptingDefinesStringList.ToArray()));
        }
    }
#endif
}
