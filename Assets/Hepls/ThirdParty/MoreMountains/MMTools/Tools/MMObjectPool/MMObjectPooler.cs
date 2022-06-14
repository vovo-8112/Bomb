using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public abstract class MMObjectPooler : MonoBehaviour
	{
		public static MMObjectPooler Instance;
		public bool MutualizeWaitingPools = false;
		public bool NestWaitingPool = true;
		protected GameObject _waitingPool = null;
        protected MMObjectPool _objectPool;
	    protected virtual void Awake()
	    {
			Instance = this;
			FillObjectPool();
	    }
		protected virtual void CreateWaitingPool()
		{
			if (!MutualizeWaitingPools)
			{
				_waitingPool = new GameObject(DetermineObjectPoolName());
                _objectPool = _waitingPool.AddComponent<MMObjectPool>();
                _objectPool.PooledGameObjects = new List<GameObject>();
                return;
			}
			else
			{
				GameObject waitingPool = GameObject.Find (DetermineObjectPoolName ());
				if (waitingPool != null)
                {
                    _waitingPool = waitingPool;
                    _objectPool = _waitingPool.MMGetComponentNoAlloc<MMObjectPool>();
                }
				else
				{
					_waitingPool = new GameObject(DetermineObjectPoolName());
                    _objectPool = _waitingPool.AddComponent<MMObjectPool>();
                    _objectPool.PooledGameObjects = new List<GameObject>();
                }
			}
		}
		protected virtual string DetermineObjectPoolName()
		{
			return ("[ObjectPooler] " + this.name);	
		}
	    public virtual void FillObjectPool()
	    {
	        return ;
	    }
		public virtual GameObject GetPooledGameObject()
	    {
	        return null;
	    }
        public virtual void DestroyObjectPool()
        {
            if (_waitingPool != null)
            {
                Destroy(_waitingPool.gameObject);
            }
        }
    }
}