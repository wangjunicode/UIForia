using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class RootLayoutBox : LayoutBox {


        public RootLayoutBox(LayoutSystem layoutSystem)
            : base(layoutSystem, null) {}

        public override void RunLayout() {
            if (children == null || children.Count == 0) {
                return;
            }
            
            actualWidth = layoutSystem.ViewportRect.width;
            actualHeight = layoutSystem.ViewportRect.height;
            
            children[0].SetAllocatedXAndWidth(0, Mathf.Min(children[0].GetWidths().clampedSize, actualWidth));
            children[0].SetAllocatedYAndHeight(0, Mathf.Min(children[0].GetHeights(actualWidth).clampedSize, actualHeight));

        }

    }

}