using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public class MMMiniObjectPooler : MonoBehaviour
    {
        public GameObject GameObjectToPool;
        public int PoolSize = 20;
        public bool PoolCanExpand = true;
        public bool MutualizeWaitingPools = false;
        public bool NestWaitingPool = true;
        protected GameObject _waitingPool = null;
        protected MMMiniObjectPool _objectPool;
        protected List<GameObject> _pooledGameObjects;
        protected virtual void Awake()
        {
            FillObjectPool();
        }
        protected virtual void CreateWaitingPool()
        {
            if (!NestWaitingPool)
            {
                return;
            }

            if (!MutualizeWaitingPools)
            {
                _waitingPool = new GameObject(DetermineObjectPoolName());
                _objectPool = _waitingPool.AddComponent<MMMiniObjectPool>();
                _objectPool.PooledGameObjects = new List<GameObject>();
                return;
            }
            else
            {
                GameObject waitingPool = GameObject.Find(DetermineObjectPoolName());
                if (waitingPool != null)
                {
                    _waitingPool = waitingPool;
                    _objectPool = _waitingPool.MMFGetComponentNoAlloc<MMMiniObjectPool>();
                }
                else
                {
                    _waitingPool = new GameObject(DetermineObjectPoolName());
                    _objectPool = _waitingPool.AddComponent<MMMiniObjectPool>();
                    _objectPool.PooledGameObjects = new List<GameObject>();
                }
            }
        }
        protected virtual string DetermineObjectPoolName()
        {
            return ("[MiniObjectPool] " + this.name);
        }
        public virtual void FillObjectPool()
        {
            if (GameObjectToPool == null)
            {
                return;
            }

            CreateWaitingPool();
            _pooledGameObjects = new List<GameObject>();

            int objectsToSpawn = PoolSize;

            if (_objectPool != null)
            {
                objectsToSpawn -= _objectPool.PooledGameObjects.Count;
                _pooledGameObjects = new List<GameObject>(_objectPool.PooledGameObjects);
            }
            for (int i = 0; i < objectsToSpawn; i++)
            {
                AddOneObjectToThePool();
            }
        }
        public virtual GameObject GetPooledGameObject()
        {
            for (int i = 0; i < _pooledGameObjects.Count; i++)
            {
                if (!_pooledGameObjects[i].gameObject.activeInHierarchy)
                {
                    return _pooledGameObjects[i];
                }
            }
            if (PoolCanExpand)
            {
                return AddOneObjectToThePool();
            }
            return null;
        }
		protected virtual GameObject AddOneObjectToThePool()
        {
            if (GameObjectToPool == null)
            {
                Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any GameObjectToPool defined.", gameObject);
                return null;
            }
            GameObject newGameObject = (GameObject)Instantiate(GameObjectToPool);
            newGameObject.gameObject.SetActive(false);
            if (NestWaitingPool)
            {
                newGameObject.transform.SetParent(_waitingPool.transform);
            }
            newGameObject.name = GameObjectToPool.name + "-" + _pooledGameObjects.Count;

            _pooledGameObjects.Add(newGameObject);

            _objectPool.PooledGameObjects.Add(newGameObject);

            return newGameObject;
        }
        public virtual void DestroyObjectPool()
        {
            if (_waitingPool != null)
            {
                Destroy(_waitingPool.gameObject);
            }
        }
    }


    public class MMMiniObjectPool : MonoBehaviour
    {
        [MMFReadOnly]
        public List<GameObject> PooledGameObjects;
    }
}