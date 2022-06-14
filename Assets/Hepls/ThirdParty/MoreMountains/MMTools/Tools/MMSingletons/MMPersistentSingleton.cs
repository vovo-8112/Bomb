using UnityEngine;

namespace MoreMountains.Tools
{
	public class MMPersistentSingleton<T> : MonoBehaviour	where T : Component
	{
		[Header("Persistent Singleton")]
		[Tooltip("if this is true, this singleton will auto detach if it finds itself parented on awake")]
		public bool AutomaticallyUnparentOnAwake = true;
		
		public static bool HasInstance => _instance != null;
		public static T Current => _instance;
		
		protected static T _instance;
		protected bool _enabled;
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

			if (AutomaticallyUnparentOnAwake)
			{
				this.transform.SetParent(null);
			}

            if (_instance == null)
			{
				_instance = this as T;
				DontDestroyOnLoad (transform.gameObject);
				_enabled = true;
			}
			else
			{
				if(this != _instance)
				{
					Destroy(this.gameObject);
				}
			}
		}
	}
}
