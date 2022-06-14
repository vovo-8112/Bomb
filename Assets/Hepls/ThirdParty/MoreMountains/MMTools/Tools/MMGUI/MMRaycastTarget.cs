using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/GUI/MMRaycastTarget")]
    public class MMRaycastTarget : Graphic
    {
        public override void SetVerticesDirty() { return; }
        public override void SetMaterialDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }
    }
}