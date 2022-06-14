using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class MMInspectorGroupAttribute : PropertyAttribute
    {
	    public string GroupName;
        public bool GroupAllFieldsUntilNextGroupAttribute;
        public int GroupColorIndex;

        public MMInspectorGroupAttribute(string groupName, bool groupAllFieldsUntilNextGroupAttribute = false, int groupColorIndex = 24)
        {
            if (groupColorIndex > 139) { groupColorIndex = 139; }

		    this.GroupName = groupName;
            this.GroupAllFieldsUntilNextGroupAttribute = groupAllFieldsUntilNextGroupAttribute;
            this.GroupColorIndex = groupColorIndex;
        }
    }
}