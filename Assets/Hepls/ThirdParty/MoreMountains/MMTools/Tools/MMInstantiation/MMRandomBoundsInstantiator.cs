using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(Collider))]
    public class MMRandomBoundsInstantiator : MonoBehaviour
    {
        public enum StartModes { Awake, Start, None }
        public enum ScaleModes { Uniform, Vector3 }

        [Header("Random instantiation")]
        public StartModes StartMode = StartModes.Awake;
        public string InstantiatedObjectName = "RandomInstantiated";
        public bool ParentInstantiatedToThisObject = true;
        public bool DestroyPreviouslyInstantiatedObjects = true;

        [Header("Spawn")]
        public List<GameObject> RandomPool;
        [MMVector("Min", "Max")]
        public Vector2Int Quantity = new Vector2Int(1, 1);

        [Header("Scale")]
        public ScaleModes ScaleMode = ScaleModes.Uniform;
        [MMEnumCondition("ScaleMode", (int)ScaleModes.Uniform)]
        public float MinScale = 1f;
        [MMEnumCondition("ScaleMode", (int)ScaleModes.Uniform)]
        public float MaxScale = 1f;
        [MMEnumCondition("ScaleMode", (int)ScaleModes.Vector3)]
        public Vector3 MinVectorScale = Vector3.one;
        [MMEnumCondition("ScaleMode", (int)ScaleModes.Vector3)]
        public Vector3 MaxVectorScale = Vector3.one;

        [Header("Test")]
        [MMInspectorButton("Instantiate")]
        public bool InstantiateButton;

        protected Collider _collider;
        protected List<GameObject> _instantiatedGameObjects;
        protected Vector3 _newScale = Vector3.zero;
        protected virtual void Awake()
        {
            _collider = this.gameObject.GetComponent<Collider>();

            if (StartMode == StartModes.Awake)
            {
                Instantiate();
            }
        }
        protected virtual void Start()
        {
            if (StartMode == StartModes.Start)
            {
                Instantiate();
            }
        }
        protected virtual void Instantiate()
        {
            if (_instantiatedGameObjects == null)
            {
                _instantiatedGameObjects = new List<GameObject>();
            }
            if (DestroyPreviouslyInstantiatedObjects)
            {
                foreach(GameObject go in _instantiatedGameObjects)
                {
                    DestroyImmediate(go);
                }
                _instantiatedGameObjects.Clear();
            }

            int random = Random.Range(Quantity.x, Quantity.y);
            for (int i = 0; i < random; i++)
            {
                InstantiateRandomObject();
            }
        }
        public virtual void InstantiateRandomObject()
        {
            if (RandomPool.Count == 0)
            {
                return;
            }
            int randomIndex = Random.Range(0, RandomPool.Count);
            GameObject obj = Instantiate(RandomPool[randomIndex], this.transform.position, this.transform.rotation);
            obj.transform.position = MMBoundsExtensions.MMRandomPointInBounds(_collider.bounds);
            obj.transform.position = _collider.ClosestPoint(obj.transform.position);
            obj.name = InstantiatedObjectName;
            if (ParentInstantiatedToThisObject)
            {
                obj.transform.SetParent(this.transform);
            }
            switch (ScaleMode)
            {
                case ScaleModes.Uniform:
                    float newScale = Random.Range(MinScale, MaxScale);
                    obj.transform.localScale = Vector3.one * newScale;
                    break;
                case ScaleModes.Vector3:
                    _newScale = MMMaths.RandomVector3(MinVectorScale, MaxVectorScale);
                    obj.transform.localScale = _newScale;
                    break;
            }
            _instantiatedGameObjects.Add(obj);
        }
    }
}
