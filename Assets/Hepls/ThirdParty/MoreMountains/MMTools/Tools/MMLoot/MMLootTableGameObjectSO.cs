using UnityEngine;

namespace  MoreMountains.Tools
{
    [CreateAssetMenu(fileName="LootDefinition",menuName="MoreMountains/Loot Definition")]
    public class MMLootTableGameObjectSO : ScriptableObject
    {
        public MMLootTableGameObject LootTable;
        public virtual GameObject GetLoot()
        {
            return LootTable.GetLoot()?.Loot;
        }
        public virtual void ComputeWeights()
        {
            LootTable.ComputeWeights();
        }
    }
}
