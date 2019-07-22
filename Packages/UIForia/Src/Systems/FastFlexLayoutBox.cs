using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;

namespace UIForia.Systems {

    public class FastFlexLayoutBox : FastLayoutBox {

        private StructList<Item> itemList;
        private LayoutDirection direction;
        
        public FastFlexLayoutBox(UIElement element) : base(element) {
            this.itemList = new StructList<Item>();
        }

        public override void PerformLayout() {

            if (direction == LayoutDirection.Horizontal) {
                PerformLayoutHorizontal();
                Item[] items = itemList.array;
                FastLayoutBox child = firstChild;
                for (int i = 0; i < itemList.size; i++) {
                    ref Item item = ref items[i];
                    child.ApplyLayout(item.outputSize, default, default, default, default, default);
                    child = child.nextSibling;
                }
                
            }
            else {
                //    PerformLayoutVertical();
            }
            
            
            
        }

        private void PerformLayoutHorizontal() {
            
            Item[] items = itemList.array;
            FastLayoutBox child = firstChild;
            
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                
//                items[i].baseSize = child.GetSize(new BoxConstraint() {
//                    minWidth = 0,
//                    maxWidth = float.PositiveInfinity,
//                    minHeight = 0,
//                    maxHeight = float.PositiveInfinity
//                });

                child = child.nextSibling;
            }
        }

        public override void SetChildren(LightList<FastLayoutBox> children) {
            itemList.EnsureCapacity(children.size);
            itemList.size = children.size;
            Item[] items = itemList.array;
            
            for (int i = 0; i < children.size; i++) {
                ref Item item = ref items[i];
                
            }
        }
        
        public override void OnChildAdded(FastLayoutBox child, int index) {
                
        }
        
        public struct Item {

            public bool needsUpdate;
            public float mainAxisStart;
            public float crossAxisStart;
            public float mainSize;
            public float crossSize;
            public float minSize;
            public float maxSize;
            public int growFactor;
            public int shrinkFactor;
            public OffsetRect margin;
            public CrossAxisAlignment crossAxisAlignment;

        }

    }

}