using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class RootLayoutBox : LayoutBox {

        public RootLayoutBox(LayoutSystem2 layoutSystem) 
            : base(layoutSystem, null) { }

        public override void RunLayout() {
            
            children[0].SetAllocatedRect(
                0, 0, 
                Mathf.Min(children[0].PreferredWidth, actualWidth), 
                Mathf.Min(children[0].PreferredHeight, actualHeight)
            );
        }

        protected override Size RunContentSizeLayout() {
            throw new System.NotImplementedException();
        }


        public override void OnChildAddedChild(LayoutBox child) {
            children.Add(child);
        }

    }

}