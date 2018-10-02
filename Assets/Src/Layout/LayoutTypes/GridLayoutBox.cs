using Src.Systems;

namespace Src.Layout.LayoutTypes {

    public class GridLayoutBox : LayoutBox {

        public GridLayoutBox(LayoutSystem2 layoutSystem, UIElement element) 
            : base(layoutSystem, element) { }

        public override void RunLayout() {
        }

        protected override Size RunContentSizeLayout() {
            throw new System.NotImplementedException();
        }

        

    }

}