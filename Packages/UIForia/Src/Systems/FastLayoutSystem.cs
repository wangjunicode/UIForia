using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class FastFlexLayoutBox : FastLayoutBox {

        private StructList<Item> itemList;
        
        public FastFlexLayoutBox(UIElement element) : base(element) {
            this.itemList = new StructList<Item>();    
        }

        
        public override void PerformLayout() {
            
            Item[] items = itemList.array;
            FastLayoutBox child = firstChild;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                
                if (item.needsUpdate) {
                        
                }

//                items[i].baseSize = child.GetSize(new BoxConstraint() {
//                    minWidth = 0,
//                    maxWidth = float.PositiveInfinity,
//                    minHeight = 0,
//                    maxHeight = float.PositiveInfinity
//                });
                
                child = child.nextSibling;
            }
            
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

    [Flags]
    public enum LayoutRenderFlag {

        NeedsLayout

    }

    public struct OffsetBox { }

    public struct MeasurementSet { }

    public struct BoxConstraint {

        public bool isTight;
        public float minWidth;
        public float maxWidth;
        public float minHeight;
        public float maxHeight;

    }


    public class FastLayoutSystem : ILayoutSystem {

        public LightList<FastLayoutBox> nodesNeedingLayout;
        private readonly Dictionary<int, LayoutBoxPool> layoutBoxPoolMap;

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;

        public FastLayoutSystem() {
            this.nodesNeedingLayout = new LightList<FastLayoutBox>(32);
            this.layoutBoxPoolMap = new Dictionary<int, LayoutBoxPool>();
            this.layoutBoxPoolMap[(int) LayoutType.Flex] = new LayoutBoxPool<FlexLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Grid] = new LayoutBoxPool<GridLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Radial] = new LayoutBoxPool<RadialLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Fixed] = new LayoutBoxPool<FixedLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Flow] = new LayoutBoxPool<FlowLayoutBox>();
            this.layoutBoxPoolMap[TextLayoutPoolKey] = new LayoutBoxPool<TextLayoutBox>();
            this.layoutBoxPoolMap[ImageLayoutPoolKey] = new LayoutBoxPool<ImageLayoutBox>();
        }
        
        public void OnReset() {
            
        }

        public void OnUpdate() {
            
            nodesNeedingLayout.Sort((a, b) => a.depth - b.depth);
            
            for (int i = 0; i < nodesNeedingLayout.size; i++) {
                if((nodesNeedingLayout[i].flags & LayoutRenderFlag.NeedsLayout) != 0) {
                    nodesNeedingLayout[i].Layout();
                }
            }
            
            nodesNeedingLayout.QuickClear();
            
        }

        public void OnDestroy() {
            
        }

        public void OnViewAdded(UIView view) {
            
        }

        public void OnViewRemoved(UIView view) {
            
        }

        public void OnElementEnabled(UIElement element) {
            
        }

        public void OnElementDisabled(UIElement element) {
            
        }

        public void OnElementDestroyed(UIElement element) {
                 
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
            
        }

        public void OnElementCreated(UIElement element) {
            element.layoutBox = CreateLayoutBox(element);
        }

        private FastLayoutBox CreateLayoutBox(UIElement element) {
            FastLayoutBox retn = null;
            if ((element is UITextElement)) {
                TextLayoutBox textLayout = (TextLayoutBox) layoutBoxPoolMap[TextLayoutPoolKey].Get(element);
                // m_TextLayoutBoxes.Add(textLayout);
                //retn = textLayout;
            }
            else if ((element is UIImageElement)) {
                //retn = layoutBoxPoolMap[ImageLayoutPoolKey].Get(element);
            }
            else {
                switch (element.style.LayoutType) {
                    case LayoutType.Flex:
                        retn = new FastFlexLayoutBox(element);//layoutBoxPoolMap[(int) LayoutType.Flex].Get(element);
                        break;

                    case LayoutType.Flow:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Flow].Get(element);
                        break;

                    case LayoutType.Fixed:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Fixed].Get(element);
                        break;

                    case LayoutType.Grid:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Grid].Get(element);
                        break;

                    case LayoutType.Radial:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Radial].Get(element);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return retn;
        }

        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
            return null;
        }

        public OffsetRect GetPaddingRect(UIElement element) {
            return default;
        }

        public OffsetRect GetMarginRect(UIElement element) {
            return default;
        }

        public OffsetRect GetBorderRect(UIElement element) {
            return default;
        }

        public LayoutBox GetBoxForElement(UIElement itemElement) {
            return default;
        }

        public LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null) {
            return default;
        }

    }

}