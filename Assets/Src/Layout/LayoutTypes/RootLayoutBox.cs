using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class RootLayoutBox : LayoutBox {

        public RootLayoutBox(LayoutSystem2 layoutSystem)
            : base(layoutSystem, null) { }

        protected override Size RunContentSizeLayout() {
            throw new System.NotImplementedException();
        }


        public override void OnChildAdded(LayoutBox child) {
            children.Add(child);
        }

        public override void RunWidthLayout() {
            if (children == null || children.Count == 0) {
                return;
            }

            children[0].SetAllocatedXAndWidth(0, Mathf.Min(children[0].PreferredWidth, actualWidth));
        }

        public override void RunHeightLayout() {
            if (children == null || children.Count == 0) {
                return;
            }

            children[0].SetAllocatedYAndHeight(0, Mathf.Min(children[0].GetPreferredHeightForWidth(actualWidth), actualWidth));
        }

    }

}