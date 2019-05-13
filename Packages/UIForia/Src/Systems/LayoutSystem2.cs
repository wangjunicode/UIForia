//
//using System.Collections.Generic;
//using UIForia.Elements;
//using UIForia.Layout;
//using UIForia.Layout.LayoutTypes;
//using UIForia.Util;
//using UnityEngine;
//
//namespace UIForia.Systems {
//
//    public class LayoutSystem2 : ILayoutSystem {
//
//        protected readonly IStyleSystem m_StyleSystem;
//        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
//        protected readonly LightList<TextLayoutBox> m_TextLayoutBoxes;
//
//        private Size m_ScreenSize;
//        private readonly LightList<LayoutSystem.ViewRect> m_Views;
//        private readonly LightList<UIElement> m_VisibleElementList;
//
//        private static readonly IComparer<UIElement> comparer = new UIElement.RenderLayerComparerAscending();
//
//        public LayoutSystem2(IStyleSystem styleSystem) {
//            this.m_StyleSystem = styleSystem;
//            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
//            this.m_Views = new LightList<LayoutSystem.ViewRect>();
//            this.m_VisibleElementList = new LightList<UIElement>();
//            this.m_TextLayoutBoxes = new LightList<TextLayoutBox>(64);
//            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
//        }
//        
//        public void OnReset() {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnUpdate() {
//            // for each view
//                // while(items requiring layout > 0) {
//                //     doLayout(list.removeFirst());
//                //     
//                // for each item loosely sorted by parent
//                // do matrix multiplication
//                // update query grid
//                
//        }
//
//        public void OnDestroy() {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnViewAdded(UIView view) {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnViewRemoved(UIView view) {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnElementEnabled(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnElementDisabled(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnElementDestroyed(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnElementCreated(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
//            throw new System.NotImplementedException();
//        }
//
//        public OffsetRect GetPaddingRect(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public OffsetRect GetMarginRect(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public OffsetRect GetBorderRect(UIElement element) {
//            throw new System.NotImplementedException();
//        }
//
//        public LayoutBox GetBoxForElement(UIElement itemElement) {
//            throw new System.NotImplementedException();
//        }
//
//        public LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null) {
//            throw new System.NotImplementedException();
//        }
//
//    }
//
//}