using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMMeshToPolygonCollider2D : MonoBehaviour
    {
        private static void GeneratePolygonCollider2D(MeshFilter meshFilter)
        {
            if (!ValidateMesh(meshFilter))
            {
                return;
            }
            PolygonCollider2D polygonCollider2D = InitializePolygonCollider2D(meshFilter);
            if (polygonCollider2D == null)
            {
                return;
            }

            Vector3[] vectors = MeshFilterToVectors(meshFilter);
            Vector2[] newPoints = VectorsToPoints(vectors);
            EditorUtility.SetDirty(polygonCollider2D);
            polygonCollider2D.SetPath(0, newPoints);
        }
        private static Vector2[] VectorsToPoints(Vector3[] vectors)
        {
            List<Vector2> newColliderVertices = new List<Vector2>();

            for (int i = 0; i < vectors.Length; i++)
            {
                newColliderVertices.Add(new Vector2(vectors[i].x, vectors[i].y));
            }

            Vector2[] newPoints = newColliderVertices.Distinct().ToArray();
            return newPoints;
        }
        private static Vector3[] MeshFilterToVectors(MeshFilter meshFilter)
        {
            List<Vector3> vertices = new List<Vector3>();
            meshFilter.sharedMesh.GetVertices(vertices);

            List<MMGeometry.MMEdge> boundaryPath = MMGeometry.GetEdges(meshFilter.sharedMesh.triangles).FindBoundary().SortEdges();

            Vector3[] vectors = new Vector3[boundaryPath.Count];
            for (int i = 0; i < boundaryPath.Count; i++)
            {
                vectors[i] = vertices[boundaryPath[i].Vertice1];
            }

            return vectors;
        }
        private static PolygonCollider2D InitializePolygonCollider2D(MeshFilter meshFilter)
        {
            PolygonCollider2D polygonCollider2D = meshFilter.GetComponent<PolygonCollider2D>();
            if (polygonCollider2D == null)
            {
                polygonCollider2D = meshFilter.gameObject.AddComponent<PolygonCollider2D>();
            }
            polygonCollider2D.pathCount = 1;
            return polygonCollider2D;
        }
        private static bool ValidateMesh(MeshFilter meshFilter)
        {
            if (meshFilter.sharedMesh == null)
            {
                Debug.LogWarning("[MMMeshToPolygonCollider2D] " 
                                + meshFilter.gameObject.name 
                                + " needs to have at least a mesh set on its mesh filter component.");
                return false;
            }
            return true;
        }
        [MenuItem("Tools/More Mountains/Collisions/Generate PolygonCollider2D", false, 601)]
        public static void GeneratePolygonCollider2DMenu()
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform == null)
            {
                Debug.LogWarning("[MMMeshToPolygonCollider2D] You need to select a gameobject first.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(activeTransform.gameObject.scene);
            MeshFilter[] meshFilters = activeTransform.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                GeneratePolygonCollider2D(meshFilter);                
            }
        }
    }
}
