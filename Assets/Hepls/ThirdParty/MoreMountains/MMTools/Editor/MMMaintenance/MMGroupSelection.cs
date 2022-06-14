using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    public class MMGroupSelection 
    {
        [MenuItem("Tools/More Mountains/Group Selection %g")]
        public static void GroupSelection()
        {
            if (!Selection.activeTransform)
            {
                return;
            }

            GameObject groupObject = new GameObject();
            groupObject.name = "Group";

            Undo.RegisterCreatedObjectUndo(groupObject, "Group Selection");

            groupObject.transform.SetParent(Selection.activeTransform.parent, false);

            foreach (Transform selectedTransform in Selection.transforms)
            {
                Undo.SetTransformParent(selectedTransform, groupObject.transform, "Group Selection");
            }
            Selection.activeGameObject = groupObject;
        }
    }
}
