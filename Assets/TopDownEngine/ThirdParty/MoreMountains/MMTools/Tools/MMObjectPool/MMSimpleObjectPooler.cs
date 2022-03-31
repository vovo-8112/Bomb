using Photon.Pun;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A simple object pool outputting a single type of objects
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMSimpleObjectPooler")]
    public class MMSimpleObjectPooler : MMObjectPooler
    {
        /// the game object we'll instantiate 
        public GameObject GameObjectToPool;

        /// the number of objects we'll add to the pool
        public int PoolSize = 20;

        /// if true, the pool will automatically add objects to the itself if needed
        public bool PoolCanExpand = true;

        /// the actual object pool
        /// <summary>
        /// Fills the object pool with the gameobject type you've specified in the inspector
        /// </summary>
        public override void FillObjectPool()
        {
            if (GameObjectToPool == null)
            {
                return;
            }

            CreateWaitingPool();

            // we initialize the list we'll use to 

            int objectsToSpawn = PoolSize;

            // we add to the pool the specified number of objects
            for (int i = 0; i < objectsToSpawn; i++)
            {
                AddOneObjectToThePool();
            }
        }

        /// <summary>
        /// Determines the name of the object pool.
        /// </summary>
        /// <returns>The object pool name.</returns>
        protected override string DetermineObjectPoolName()
        {
            return ("[SimpleObjectPooler] " + this.name);
        }

        /// <summary>
        /// This method returns one inactive object from the pool
        /// </summary>
        /// <returns>The pooled game object.</returns>
        public override GameObject GetPooledGameObject()
        {
            // if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		
            if (PoolCanExpand)
            {
                return AddOneObjectToThePool();
            }

            // if the pool is empty and can't grow, we return nothing.
            return null;
        }

        /// <summary>
        /// Adds one object of the specified type (in the inspector) to the pool.
        /// </summary>
        /// <returns>The one object to the pool.</returns>
        protected virtual GameObject AddOneObjectToThePool()
        {
            if (GameObjectToPool == null)
            {
                Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any GameObjectToPool defined.", gameObject);
                return null;
            }

            var position = _waitingPool.transform.position;

            GameObject newGameObject = (GameObject)PhotonNetwork.Instantiate(GameObjectToPool.name,
                new Vector3(position.x, position.y + 0.5f, position.z), Quaternion.identity);

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