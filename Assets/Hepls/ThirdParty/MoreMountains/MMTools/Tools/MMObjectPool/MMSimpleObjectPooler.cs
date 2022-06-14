using Photon.Pun;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMSimpleObjectPooler")]
    public class MMSimpleObjectPooler : MMObjectPooler
    {
        public GameObject GameObjectToPool;
        public int PoolSize = 20;
        public bool PoolCanExpand = true;
        public override void FillObjectPool()
        {
            if (GameObjectToPool == null)
            {
                return;
            }

            CreateWaitingPool();

            int objectsToSpawn = PoolSize;
            for (int i = 0; i < objectsToSpawn; i++)
            {
                AddOneObjectToThePool();
            }
        }
        protected override string DetermineObjectPoolName()
        {
            return ("[SimpleObjectPooler] " + this.name);
        }
        public override GameObject GetPooledGameObject()
        {
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

            var position = _waitingPool.transform.position;

            GameObject newGameObject = (GameObject)PhotonNetwork.Instantiate(GameObjectToPool.name,
                position, Quaternion.identity);

            newGameObject.transform.SetParent(transform.parent.parent);

            newGameObject.gameObject.SetActive(false);

            if (NestWaitingPool)
            {
                newGameObject.transform.SetParent(_waitingPool.transform);
            }

            return newGameObject;
        }
    }
}