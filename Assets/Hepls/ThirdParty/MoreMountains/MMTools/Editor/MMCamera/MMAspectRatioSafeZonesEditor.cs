using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    [CustomEditor(typeof(MMAspectRatioSafeZones), true)]
    [CanEditMultipleObjects]
    public class MMAspectRatioSafeZonesEditor : Editor
    {
        static MMAspectRatioSafeZones safeZones;
         void OnEnable()
         {
            SceneView.duringSceneGui -= OnSceneGUI;
            safeZones = (MMAspectRatioSafeZones)target;
            SceneView.duringSceneGui += OnSceneGUI;            
         }
         private static void OnSceneGUI(SceneView sceneView)
         {
             DrawFrameCenter(sceneView);
             DrawRatios(sceneView);
         }
        private static void DrawRatios(SceneView sceneView)
        {
            if (!safeZones.DrawRatios)
            {
                return;
            }

            Vector3 center = sceneView.pivot;

            float width = sceneView.position.width;
            float height = sceneView.position.height;

            Vector3 bottomLeft = new Vector3(center.x - width / 2f, center.y - height / 2f, 0f);
            Vector3 topRight = new Vector3(center.x + width / 2f, center.y + height / 2f, 0f);
            
            Vector3 topLeft = bottomLeft;
            topLeft.y = topRight.y;
            Vector3 bottomRight = topRight;
            bottomRight.y = bottomLeft.y;

            float size = safeZones.CameraSize;
            float spacing = 2f;
            Color dottedLineColor = Color.white;
            dottedLineColor.a = 0.4f;
            Handles.color = dottedLineColor;
            Handles.DrawDottedLine(new Vector3(topLeft.x, center.y + size, 0f), new Vector3(topRight.x, center.y + size, 0f), spacing);
            Handles.DrawDottedLine(new Vector3(topLeft.x, center.y - size, 0f), new Vector3(topRight.x, center.y - size, 0f), spacing);

            foreach (Ratio ratio in safeZones.Ratios)
            {
                if (ratio.DrawRatio)
                {
                    float aspectRatio = ratio.Size.x / ratio.Size.y;

                    Handles.color = ratio.RatioColor;
                    Vector3 ratioTopLeft =       new Vector3(center.x - size * aspectRatio, center.y + size, 0f);
                    Vector3 ratioTopRight =      new Vector3(center.x + size * aspectRatio, center.y + size, 0f);
                    Vector3 ratioBottomLeft =    new Vector3(center.x - size * aspectRatio, center.y - size, 0f);
                    Vector3 ratioBottomRight =   new Vector3(center.x + size * aspectRatio, center.y - size, 0f);
                    Vector3 ratioLabelPosition = ratioBottomLeft + 0.1f * Vector3.down + 0.1f * Vector3.right;
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = ratio.RatioColor;
                    style.fontSize = 8;
                    Handles.Label(ratioLabelPosition, ratio.Size.x + ":" + ratio.Size.y, style);
                    Vector3[] verts = new Vector3[] { ratioTopLeft, ratioTopRight, ratioBottomRight, ratioBottomLeft };
                    Handles.DrawSolidRectangleWithOutline(verts, new Color(0, 0, 0, 0), ratio.RatioColor);
                    Color zoneColor = ratio.RatioColor;
                    zoneColor.a = zoneColor.a * safeZones.UnsafeZonesOpacity;
                    verts = new Vector3[] { topLeft, topRight, new Vector3(topLeft.x, ratioTopLeft.y, 0f), new Vector3(topRight.x, ratioTopRight.y, 0f) };
                    Handles.DrawSolidRectangleWithOutline(verts, zoneColor, new Color(0, 0, 0, 0));
                    verts = new Vector3[] { bottomLeft, new Vector3(topLeft.x, ratioBottomLeft.y, 0f), new Vector3(topRight.x, ratioBottomRight.y, 0f), bottomRight };
                    Handles.DrawSolidRectangleWithOutline(verts, zoneColor, new Color(0, 0, 0, 0));
                    verts = new Vector3[] { new Vector3(topLeft.x, ratioTopLeft.y, 0f), ratioTopLeft, ratioBottomLeft, new Vector3(bottomLeft.x, ratioBottomLeft.y, 0f) };
                    Handles.DrawSolidRectangleWithOutline(verts, zoneColor, new Color(0, 0, 0, 0));
                    verts = new Vector3[] { new Vector3(topRight.x, ratioTopRight.y, 0f), new Vector3(bottomRight.x, ratioBottomRight.y, 0f), ratioBottomRight, ratioTopRight};
                    Handles.DrawSolidRectangleWithOutline(verts, zoneColor, new Color(0, 0, 0, 0));
                    Handles.DrawDottedLine(new Vector3(ratioBottomLeft.x, topLeft.y, 0f), new Vector3(ratioTopLeft.x, bottomLeft.y, 0f), spacing);
                    Handles.DrawDottedLine(new Vector3(ratioBottomRight.x, topLeft.y, 0f), new Vector3(ratioBottomRight.x, bottomLeft.y, 0f), spacing);
                }
            }
        }
        private static void DrawFrameCenter(SceneView sceneView)
        {
            if (!safeZones.DrawCenterCrosshair)
            {
                return;
            }

            Vector3 center = sceneView.pivot;
            float crossHairSize = safeZones.CenterCrosshairSize;

            float reticleSize = crossHairSize / 10f;

            Handles.color = safeZones.CenterCrosshairColor;

            Vector3 crosshairTopLeft = new Vector3(center.x - crossHairSize / 2f, center.y + crossHairSize / 2f, 0f);
            Vector3 crosshairTopRight = new Vector3(center.x + crossHairSize / 2f, center.y + crossHairSize / 2f, 0f);
            Vector3 crosshairBottomLeft = new Vector3(center.x - crossHairSize / 2f, center.y - crossHairSize / 2f, 0f);
            Vector3 crosshairBottomRight = new Vector3(center.x + crossHairSize / 2f, center.y - crossHairSize / 2f, 0f);
            Handles.DrawLine(new Vector3(center.x, center.y + crossHairSize / 2f, 0f), new Vector3(center.x, center.y - crossHairSize / 2f, 0f));
            Handles.DrawLine(new Vector3(center.x - crossHairSize / 2f, center.y, 0f), new Vector3(center.x + crossHairSize / 2f, center.y, 0f));
            Handles.DrawLine(crosshairTopLeft, new Vector3(crosshairTopLeft.x + reticleSize, crosshairTopLeft.y, 0f));
            Handles.DrawLine(crosshairTopLeft, new Vector3(crosshairTopLeft.x, crosshairTopLeft.y - reticleSize, 0f));
            Handles.DrawLine(crosshairTopRight, new Vector3(crosshairTopRight.x - reticleSize, crosshairTopRight.y, 0f));
            Handles.DrawLine(crosshairTopRight, new Vector3(crosshairTopRight.x, crosshairTopRight.y - reticleSize, 0f));
            Handles.DrawLine(crosshairBottomLeft, new Vector3(crosshairBottomLeft.x + reticleSize, crosshairBottomLeft.y, 0f));
            Handles.DrawLine(crosshairBottomLeft, new Vector3(crosshairBottomLeft.x, crosshairBottomLeft.y + reticleSize, 0f));
            Handles.DrawLine(crosshairBottomRight, new Vector3(crosshairBottomRight.x - reticleSize, crosshairBottomRight.y, 0f));
            Handles.DrawLine(crosshairBottomRight, new Vector3(crosshairBottomRight.x, crosshairBottomRight.y + reticleSize, 0f));
        }
    }
}
