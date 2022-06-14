using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMSpawnAroundTester : MonoBehaviour
    {
        public GameObject ObjectToInstantiate;
        public MMSpawnAroundProperties SpawnProperties;

        [Header("Debug")]
        public int DebugQuantity = 10000;
        [MMInspectorButton("DebugSpawn")]
        public bool DebugSpawnButton;

        [Header("Gizmos")]
        public bool DrawGizmos = false;
        public int GizmosQuantity = 1000;
        public float GizmosSize = 1f;
        
        protected GameObject _gameObject;
        public virtual void DebugSpawn()
        {
            for (int i = 0; i < DebugQuantity; i++)
            {
                Spawn();
            }
        }
        public virtual void Spawn()
        {
            _gameObject = Instantiate(ObjectToInstantiate);
            MMSpawnAround.ApplySpawnAroundProperties(_gameObject, SpawnProperties, this.transform.position);
        }
        protected virtual void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GizmosSize, Color.gray);    
            }
        }
    }
}
