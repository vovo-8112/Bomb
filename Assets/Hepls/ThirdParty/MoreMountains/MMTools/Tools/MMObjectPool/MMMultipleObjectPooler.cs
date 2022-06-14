using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{	
	[Serializable]
	public class MMMultipleObjectPoolerObject
	{
		public GameObject GameObjectToPool;
		public int PoolSize;
		public bool PoolCanExpand = true;
		public bool Enabled = true;
	}
	public enum MMPoolingMethods { OriginalOrder, OriginalOrderSequential, RandomBetweenObjects, RandomPoolSizeBased }
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMMultipleObjectPooler")]
    public class MMMultipleObjectPooler : MMObjectPooler
	{
		public List<MMMultipleObjectPoolerObject> Pool;
		[MMInformation("A MultipleObjectPooler is a reserve of objects, to be used by a Spawner. When asked, it will return an object from the pool (ideally an inactive one) chosen based on the pooling method you've chosen.\n- OriginalOrder will spawn objects in the order you've set them in the inspector (from top to bottom)\n- OriginalOrderSequential will do the same, but will empty each pool before moving to the next object\n- RandomBetweenObjects will pick one object from the pool, at random, but ignoring its pool size, each object has equal chances to get picked\n- PoolSizeBased randomly choses one object from the pool, based on its pool size probability (the larger the pool size, the higher the chances it'll get picked)'...",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public MMPoolingMethods PoolingMethod = MMPoolingMethods.RandomPoolSizeBased;
		[MMInformation("If you set CanPoolSameObjectTwice to false, the Pooler will try to prevent the same object from being pooled twice to avoid repetition. This will only affect random pooling methods, not ordered pooling.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public bool CanPoolSameObjectTwice=true;
		protected List<GameObject> _pooledGameObjects;
		protected List<GameObject> _pooledGameObjectsOriginalOrder;
		protected List<MMMultipleObjectPoolerObject> _randomizedPool;
		protected string _lastPooledObjectName;
		protected int _currentIndex=0;
		protected override string DetermineObjectPoolName()
		{
			return ("[MultipleObjectPooler] " + this.name);	
		}
		public override void FillObjectPool()
		{
            if ((Pool == null) || (Pool.Count == 0))
            {
                return;
            }

			CreateWaitingPool ();
			_pooledGameObjects = new List<GameObject>();
			_randomizedPool = new List<MMMultipleObjectPoolerObject>() ;
			for (int i = 0; i < Pool.Count; i++)
			{
				_randomizedPool.Add(Pool[i]);
			}
			_randomizedPool.MMShuffle();
			if (Pool.Count <= 1)
			{
				CanPoolSameObjectTwice=true;
			}

			bool stillObjectsToPool;
			int[] poolSizes;
			switch (PoolingMethod)
			{
				case MMPoolingMethods.OriginalOrder:

					stillObjectsToPool = true;
					_pooledGameObjectsOriginalOrder = new List<GameObject>();
					poolSizes = new int[Pool.Count];
					for (int i = 0; i < Pool.Count; i++)
					{
						poolSizes[i] = Pool[i].PoolSize;
					}
					while (stillObjectsToPool)
					{
						stillObjectsToPool = false;
						for (int i = 0; i < Pool.Count; i++)
						{
							if (poolSizes[i] > 0)
							{
								AddOneObjectToThePool(Pool[i].GameObjectToPool);
								poolSizes[i]--;
								stillObjectsToPool = true;
							}			            
						}
					}
					break;
				case MMPoolingMethods.OriginalOrderSequential:

					_pooledGameObjectsOriginalOrder = new List<GameObject>();
					foreach (MMMultipleObjectPoolerObject pooledGameObject in Pool)
					{
						for (int i = 0; i < pooledGameObject.PoolSize ; i++ )
						{
							AddOneObjectToThePool(pooledGameObject.GameObjectToPool);								
						}
					}				
					break;
				default:
					int k = 0;
					foreach (MMMultipleObjectPoolerObject pooledGameObject in Pool)
					{
						if (k > Pool.Count) { return; }
						for (int j = 0; j < Pool[k].PoolSize; j++)
						{
							AddOneObjectToThePool(pooledGameObject.GameObjectToPool);
						}
						k++;
					}
					break;
			}
			if ((PoolingMethod==MMPoolingMethods.OriginalOrder) || (PoolingMethod == MMPoolingMethods.OriginalOrderSequential))
			{
				foreach (GameObject pooledObject in _pooledGameObjects)
				{
					_pooledGameObjectsOriginalOrder.Add(pooledObject);					
				}
			}


		}
		protected virtual GameObject AddOneObjectToThePool(GameObject typeOfObject)
		{
			GameObject newGameObject = (GameObject)Instantiate(typeOfObject);
			newGameObject.gameObject.SetActive(false);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_waitingPool.transform);	
			}
			newGameObject.name=typeOfObject.name;
			_pooledGameObjects.Add(newGameObject);	
			return newGameObject;
		}
		public override GameObject GetPooledGameObject()
		{
			GameObject pooledGameObject;
			switch (PoolingMethod)
			{
				case MMPoolingMethods.OriginalOrder:
					pooledGameObject = GetPooledGameObjectOriginalOrder();
					break;
				case MMPoolingMethods.RandomPoolSizeBased:
					pooledGameObject =  GetPooledGameObjectPoolSizeBased();
					break;
				case MMPoolingMethods.RandomBetweenObjects:
					pooledGameObject =  GetPooledGameObjectRandomBetweenObjects();
					break;
				case MMPoolingMethods.OriginalOrderSequential:
					pooledGameObject =  GetPooledGameObjectOriginalOrder();
					break;
				default:
					pooledGameObject = null;
					break;
			}
			if (pooledGameObject!=null)
			{
				_lastPooledObjectName = pooledGameObject.name;
			}
			else
			{	
				_lastPooledObjectName="";
			}
			return pooledGameObject;
		}
		protected virtual GameObject GetPooledGameObjectOriginalOrder()
		{
			int newIndex;
			if (_currentIndex>=_pooledGameObjectsOriginalOrder.Count)
			{
				ResetCurrentIndex ();
			}

			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(_pooledGameObjects[_currentIndex].gameObject);

			if (_currentIndex >= _pooledGameObjects.Count) { return null; }
			if (!searchedObject.Enabled) { _currentIndex++; return null; }
			if (_pooledGameObjects[_currentIndex].gameObject.activeInHierarchy)
			{
				GameObject findObject = FindInactiveObject(_pooledGameObjects[_currentIndex].gameObject.name,_pooledGameObjects);
				if (findObject != null)
				{
					_currentIndex++;
					return findObject;
				}
				if (searchedObject.PoolCanExpand)
				{
					_currentIndex++;
					return AddOneObjectToThePool(searchedObject.GameObjectToPool);	
				}
				else
				{
					return null;					
				}
			}
			else
			{
				newIndex = _currentIndex;
				_currentIndex++;
				return _pooledGameObjects[newIndex]; 
			}
		}
		protected virtual GameObject GetPooledGameObjectPoolSizeBased()
		{
			int randomIndex = UnityEngine.Random.Range(0, _pooledGameObjects.Count);

			int overflowCounter=0;
			while (!PoolObjectEnabled(_pooledGameObjects[randomIndex]) && overflowCounter < _pooledGameObjects.Count)
			{
				randomIndex = UnityEngine.Random.Range(0, _pooledGameObjects.Count);
				overflowCounter++;
			}
			if (!PoolObjectEnabled(_pooledGameObjects[randomIndex]))
			{ 
				return null; 
			}
			overflowCounter = 0;
			while (!CanPoolSameObjectTwice 
				&& _pooledGameObjects[randomIndex].name == _lastPooledObjectName 
				&& overflowCounter < _pooledGameObjects.Count)
			{
				randomIndex = UnityEngine.Random.Range(0, _pooledGameObjects.Count);
				overflowCounter++;
			}
			if (_pooledGameObjects[randomIndex].gameObject.activeInHierarchy)
			{
				GameObject pulledObject = FindInactiveObject(_pooledGameObjects[randomIndex].gameObject.name,_pooledGameObjects);
				if (pulledObject!=null)
				{
					return pulledObject;
				}
				else
				{
					MMMultipleObjectPoolerObject searchedObject = GetPoolObject(_pooledGameObjects[randomIndex].gameObject);
					if (searchedObject==null)
					{
						return null; 
					}
					if (searchedObject.PoolCanExpand)
					{						
						return AddOneObjectToThePool(searchedObject.GameObjectToPool);						 	
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				return _pooledGameObjects[randomIndex];   
			}
		}
		protected virtual GameObject GetPooledGameObjectRandomBetweenObjects()
		{
			int randomIndex = UnityEngine.Random.Range(0, Pool.Count);

			int overflowCounter=0;
			while (!CanPoolSameObjectTwice && Pool[randomIndex].GameObjectToPool.name == _lastPooledObjectName && overflowCounter < _pooledGameObjects.Count )
			{
				randomIndex = UnityEngine.Random.Range(0, Pool.Count);
				overflowCounter++;
			}
			int originalRandomIndex = randomIndex+1;

			bool objectFound=false;
			overflowCounter=0;
			while (!objectFound 
				&& randomIndex != originalRandomIndex 
				&& overflowCounter < _pooledGameObjects.Count)
			{
				if (randomIndex >= Pool.Count)
				{
					randomIndex=0;
				}

				if (!Pool[randomIndex].Enabled)
				{
					randomIndex++;
					overflowCounter++;
					continue;
				}
				GameObject newGameObject = FindInactiveObject(Pool[randomIndex].GameObjectToPool.name, _pooledGameObjects);
				if (newGameObject!=null)
				{
					objectFound=true;
					return newGameObject;
				}
				else
				{
					if (Pool[randomIndex].PoolCanExpand)
					{
						return AddOneObjectToThePool(Pool[randomIndex].GameObjectToPool);	
					}
				}
				randomIndex++;
				overflowCounter++;
			}
			return null;
		}
		protected virtual GameObject GetPooledGameObjectOfType(string searchedName)
		{
			GameObject newObject = FindInactiveObject(searchedName,_pooledGameObjects);

			if (newObject!=null)
			{
				return newObject;
			}
			else
			{
				GameObject searchedObject = FindObject(searchedName,_pooledGameObjects);
				if (searchedObject == null) 
				{
					return null;
				}

				if (GetPoolObject(FindObject(searchedName,_pooledGameObjects)).PoolCanExpand)
				{
					GameObject newGameObject = (GameObject)Instantiate(searchedObject);
					_pooledGameObjects.Add(newGameObject);
					return newGameObject;
				}
			}
			return null;
		}
		protected virtual GameObject FindInactiveObject(string searchedName, List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].name.Equals(searchedName))
				{
					if (!list[i].gameObject.activeInHierarchy)
					{
						return list[i];
					}
				}            
			}
			return null;
		}

		protected virtual GameObject FindAnyInactiveObject(List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].gameObject.activeInHierarchy)
				{
					return list[i];
				}                        
			}
			return null;
		}
		protected virtual GameObject FindObject(string searchedName,List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].name.Equals(searchedName))
				{
					return list[i];
				}            
			}
			return null;
		}
		protected virtual MMMultipleObjectPoolerObject GetPoolObject(GameObject testedObject)
		{
			if (testedObject==null)
			{
				return null;
			}
			int i=0;
			foreach(MMMultipleObjectPoolerObject poolerObject in Pool)
			{
				if (testedObject.name.Equals(poolerObject.GameObjectToPool.name))
				{
					return (poolerObject);
				}
				i++;
			}
			return null;
		}

		protected virtual bool PoolObjectEnabled(GameObject testedObject)
		{
			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(testedObject);
			if (searchedObject != null)
			{
				return searchedObject.Enabled;
			}
			else
			{
				return false;
			}
		}

		public virtual void EnableObjects(string name,bool newStatus)
		{
			foreach(MMMultipleObjectPoolerObject poolerObject in Pool)
			{
				if (name.Equals(poolerObject.GameObjectToPool.name))
				{
					poolerObject.Enabled = newStatus;
				}
			}
		}

		public virtual void ResetCurrentIndex()
		{
			_currentIndex=0;
		}
	}
}