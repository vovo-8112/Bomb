using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class GameObjectExtensions
    {
        static List<Component> m_ComponentCache = new List<Component>();
		public static Component MMGetComponentNoAlloc(this GameObject @this, System.Type componentType)
        {
            @this.GetComponents(componentType, m_ComponentCache);
            Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component;
        }
        public static T MMGetComponentNoAlloc<T>(this GameObject @this) where T : Component
        {
            @this.GetComponents(typeof(T), m_ComponentCache);
            Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component as T;
        }
        public static T MMGetComponentAround<T>(this GameObject @this) where T : Component
        {
            T component = @this.GetComponentInChildren<T>(true);
            if (component == null)
            {
                component = @this.GetComponentInParent<T>();    
            }
            if (component == null)
            {
                component = @this.AddComponent<T>();    
            }
            return component;
        }
    }
}
