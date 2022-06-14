using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMRandomInstantiator : MonoBehaviour
    {
        public enum StartModes { Awake, Start, None }

        [Header("Random instantiation")]
        public StartModes StartMode = StartModes.Awake;
        public string InstantiatedObjectName = "RandomInstantiated";
        public bool ParentInstantiatedToThisObject = true;
        public bool DestroyPreviouslyInstantiatedObject = true;
        public List<GameObject> RandomPool;

        [Header("Test")]
        [MMInspectorButton("InstantiateRandomObject")]
        public bool InstantiateButton;

        protected GameObject _instantiatedGameObject;
        protected virtual void Awake()
        {
            if (StartMode == StartModes.Awake)
            {
                InstantiateRandomObject();
            }
        }
        protected virtual void Start()
        {
            if (StartMode == StartModes.Start)
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
            if (DestroyPreviouslyInstantiatedObject)
            {
                if (_instantiatedGameObject != null)
                {
                    DestroyImmediate(_instantiatedGameObject);
                }
            }
            int randomIndex = Random.Range(0, RandomPool.Count);
            _instantiatedGameObject = Instantiate(RandomPool[randomIndex], this.transform.position, this.transform.rotation);
            _instantiatedGameObject.name = InstantiatedObjectName;
            if (ParentInstantiatedToThisObject)
            {
                _instantiatedGameObject.transform.SetParent(this.transform);
            }
        }
    }
}
