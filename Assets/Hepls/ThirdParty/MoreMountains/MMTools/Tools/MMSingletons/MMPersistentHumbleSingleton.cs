using UnityEngine;
using System;

namespace MoreMountains.Tools
{
	public class MMPersistentHumbleSingleton<T> : MonoBehaviour	where T : Component
	{
		protected static T _instance;
		public float InitializationTime;
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T> ();
					if (_instance == null)
					{
						GameObject obj = new GameObject ();
						obj.hideFlags = HideFlags.HideAndDontSave;
						_instance = obj.AddComponent<T> ();
					}
				}
				return _instance;
			}
		}
		protected virtual void Awake ()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			InitializationTime=Time.time;

			DontDestroyOnLoad (this.gameObject);
			T[] check = FindObjectsOfType<T>();
			foreach (T searched in check)
			{
				if (searched!=this)
				{
					if (searched.GetComponent<MMPersistentHumbleSingleton<T>>().InitializationTime<InitializationTime)
					{
						Destroy (searched.gameObject);
					}
				}
			}

			if (_instance == null)
			{
				_instance = this as T;
			}
		}
	}
}
