using UnityEngine;

namespace LevelLoader
{
    [CreateAssetMenu(fileName = "ConfigLevel", menuName = "Config/Levels/ConfigLevels")]
    public class LevelModel : ScriptableObject
    {
        public LevelDate LevelDate;
    }
}