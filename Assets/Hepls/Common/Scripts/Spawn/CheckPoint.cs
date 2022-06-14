using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public struct CheckPointEvent
    {
        public int Order;
        public CheckPointEvent(int order)
        {
            Order = order;
        }

        static CheckPointEvent e;
        public static void Trigger(int order)
        {
            e.Order = order;
            MMEventManager.TriggerEvent(e);
        }
    }
    [AddComponentMenu("TopDown Engine/Spawn/Checkpoint")]
	public class CheckPoint : MonoBehaviour 
	{
        [Header("Spawn")]
		[MMInformation("Add this script to a (preferrably empty) GameObject and it'll be added to the level's checkpoint list, allowing you to respawn from there. If you bind it to the LevelManager's starting point, that's where your character will spawn at the start of the level. And here you can decide whether the character should spawn facing left or right.",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("the facing direction the character should face when spawning from this checkpoint")]
		public Character.FacingDirections FacingDirection = Character.FacingDirections.East ;
		[Tooltip("whether or not this checkpoint should override any order and assign itself on entry")]
		public bool ForceAssignation = false;
		[Tooltip("the order of the checkpoint")]
		public int CheckPointOrder;
        
	    protected List<Respawnable> _listeners;
	    protected virtual void Awake () 
		{
			_listeners = new List<Respawnable>();
		}
		public virtual void SpawnPlayer(Character player)
		{
			player.RespawnAt(transform, FacingDirection);
			
			foreach(Respawnable listener in _listeners)
			{
				listener.OnPlayerRespawn(this,player);
			}
		}
		public virtual void AssignObjectToCheckPoint (Respawnable listener) 
		{
			_listeners.Add(listener);
		}
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
            TriggerEnter(collider.gameObject);            
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            TriggerEnter(collider.gameObject);
        }

        protected virtual void TriggerEnter(GameObject collider)
        {
            Character character = collider.GetComponent<Character>();

            if (character == null) { return; }
            if (character.CharacterType != Character.CharacterTypes.Player) { return; }
            if (LevelManager.Instance == null) { return; }
            LevelManager.Instance.SetCurrentCheckpoint(this);
            CheckPointEvent.Trigger(CheckPointOrder);
        }
        protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR

			if (LevelManager.Instance == null)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints == null)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints.Count == 0)
			{
				return;
			}

			for (int i=0; i < LevelManager.Instance.Checkpoints.Count; i++)
			{
				if ((i+1) < LevelManager.Instance.Checkpoints.Count)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(LevelManager.Instance.Checkpoints[i].transform.position,LevelManager.Instance.Checkpoints[i+1].transform.position);
				}
			}
			#endif
		}
	}
}