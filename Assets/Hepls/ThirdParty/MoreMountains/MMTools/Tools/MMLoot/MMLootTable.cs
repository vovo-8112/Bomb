using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MoreMountains.Tools
{
    public class MMLootTable<T,V> where T:MMLoot<V>
    {
        [SerializeField]
        public List<T> ObjectsToLoot;
        [Header("Debug")]
        [MMReadOnly]
        public float WeightsTotal;
        
        protected float _maximumWeightSoFar = 0f;
        protected bool _weightsComputed = false;
        public virtual void ComputeWeights()
        {
            if (ObjectsToLoot == null)
            {
                return;
            }

            if (ObjectsToLoot.Count == 0)
            {
                return;
            }

            _maximumWeightSoFar = 0f;

            foreach(T lootDropItem in ObjectsToLoot)
            {
                if(lootDropItem.Weight >= 0f)
                {
                    lootDropItem.RangeFrom = _maximumWeightSoFar;
                    _maximumWeightSoFar += lootDropItem.Weight;	
                    lootDropItem.RangeTo = _maximumWeightSoFar;
                } 
                else 
                {
                    lootDropItem.Weight =  0f;						
                }
            }

            WeightsTotal = _maximumWeightSoFar;

            foreach(T lootDropItem in ObjectsToLoot)
            {
                lootDropItem.ChancePercentage = ((lootDropItem.Weight) / WeightsTotal) * 100;
            }

            _weightsComputed = true;
        }
        public virtual T GetLoot()
        {	
            if (ObjectsToLoot == null)
            {
                return null;
            }

            if (ObjectsToLoot.Count == 0)
            {
                return null;
            }

            if (!_weightsComputed)
            {
                ComputeWeights();
            }
            
            float index = Random.Range(0, WeightsTotal);
 
            foreach (T lootDropItem in ObjectsToLoot)
            {
                if ((index > lootDropItem.RangeFrom) && (index < lootDropItem.RangeTo))
                {
                    return lootDropItem;
                }
            }	
            
            return null;
        }
    }
    [System.Serializable]
    public class MMLootTableGameObject : MMLootTable<MMLootGameObject, GameObject> { }
    [System.Serializable]
    public class MMLootTableFloat : MMLootTable<MMLootFloat, float> { }
    [System.Serializable]
    public class MMLootTableString : MMLootTable<MMLootString, string> { } 
}
