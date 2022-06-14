using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    public class MMTilemapCleaner : MonoBehaviour
    {
        [MMInspectorButton("Clean")] 
        public bool CleanButton;
        [MMInspectorButton("CleanAllChildren")] 
        public bool CleanAllButton;
        
        protected Tilemap _tilemap;
        protected Tilemap[] _tilemaps;
        public virtual void Clean()
        {
            _tilemap = this.gameObject.GetComponent<Tilemap>();
            if (_tilemap != null)
            {
                _tilemap.ClearAllTiles();
            }
        }
        public virtual void CleanAllChildren()
        {
            _tilemaps = GetComponentsInChildren<Tilemap>();

            foreach (Tilemap tilemap in _tilemaps)
            {
                tilemap.ClearAllTiles();
            }
                
        }
    }    
}

