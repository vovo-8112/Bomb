using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Tilemaps/MMTilemapBoolean")]
    public class MMTilemapBoolean : MonoBehaviour
    {
        public Tilemap TilemapToClean;

        [MMInspectorButton("BooleanClean")]
        public bool BooleanCleanButton;

        protected Tilemap _tilemap;
        protected virtual void BooleanClean()
        {
            if (TilemapToClean == null)
            {
                return;
            }

            _tilemap = this.gameObject.GetComponent<Tilemap>();
            foreach (Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
            {
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (_tilemap.HasTile(localPlace))
                {
                    if (TilemapToClean.HasTile(localPlace))
                    {
                        TilemapToClean.SetTile(localPlace, null);
                    }
                }                
            }
            _tilemap.RefreshAllTiles();            
        }
    }
}
