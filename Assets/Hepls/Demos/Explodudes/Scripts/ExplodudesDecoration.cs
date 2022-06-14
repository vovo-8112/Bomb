using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ExplodudesDecoration : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Tooltip("the minimum force to apply to the background elements")]
        public Vector3 MinForce;
        [Tooltip("the maximum force to apply to the background elements")]
        public Vector3 MaxForce;
        [MMInspectorButton("Jump")]
        public bool JumpButton;

        protected Rigidbody _rigidbody;
        protected const string eventName = "Bomb";
        protected Vector3 _force;
        protected virtual void Start()
        {
            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == eventName)
            {
                Jump();
            }
        }
        public virtual void Jump()
        {
            _force.x = Random.Range(MinForce.x, MaxForce.x);
            _force.y = Random.Range(MinForce.y, MaxForce.y);
            _force.z = Random.Range(MinForce.z, MaxForce.z);
            _rigidbody.AddForce(_force, ForceMode.Impulse);
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
