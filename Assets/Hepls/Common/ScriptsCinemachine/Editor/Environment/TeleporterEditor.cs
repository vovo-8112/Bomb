using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine {
  [CanEditMultipleObjects]
  [CustomEditor(typeof(Teleporter), true)]
  [InitializeOnLoad]
  public class TeleporterEditor : Editor {
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawHandles(Teleporter teleporter, GizmoType gizmoType) {
      Teleporter t = (teleporter as Teleporter);

      GUIStyle style = new GUIStyle();
      style.normal.textColor = Color.cyan;
      style.alignment = TextAnchor.UpperCenter;
      float verticalOffset = (t.transform.lossyScale.x > 0) ? 1f : 2f;
      Handles.Label(t.transform.position + Vector3.up * (2f + verticalOffset), t.name, style);
    }
  }
}