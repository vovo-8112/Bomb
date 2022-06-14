using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    [ExecuteAlways]
    [AddComponentMenu("More Mountains/Tools/Tilemaps/MMTilemapShadow")]
    [RequireComponent(typeof(Tilemap))]
    public class MMTilemapShadow : MonoBehaviour
    {
        public Tilemap ReferenceTilemap;
        
        [MMInspectorButton("UpdateShadows")]
        public bool UpdateShadowButton;

        protected Tilemap _tilemap;
        public virtual void UpdateShadows()
        {
            if (ReferenceTilemap == null)
            {
                return;
            }

            _tilemap = this.gameObject.GetComponent<Tilemap>();
           
            Copy(ReferenceTilemap, _tilemap);
        }
        public static void Copy(Tilemap source, Tilemap destination)
        {
            source.RefreshAllTiles();
            destination.RefreshAllTiles();
            
            List<Vector3Int> referenceTilemapPositions = new List<Vector3Int>();
            foreach (Vector3Int pos in source.cellBounds.allPositionsWithin)
            {
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (source.HasTile(localPlace))
                {
                    referenceTilemapPositions.Add(localPlace);
                }
            }
            Vector3Int[] positions = new Vector3Int[referenceTilemapPositions.Count];
            TileBase[] allTiles = new TileBase[referenceTilemapPositions.Count];
            int i = 0;
            foreach(Vector3Int tilePosition in referenceTilemapPositions)
            {
                positions[i] = tilePosition;
                allTiles[i] = source.GetTile(tilePosition);
                i++;
            }
            destination.ClearAllTiles();
            destination.RefreshAllTiles();
            destination.size = source.size;
            destination.origin = source.origin;
            destination.ResizeBounds();
            destination.SetTiles(positions, allTiles);
        }
		
	}
}
