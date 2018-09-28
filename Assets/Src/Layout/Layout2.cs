//using System;
//using System.Collections.Generic;
//using Rendering;
//using Src.Systems;
//
//namespace Src.Layout {
//
//    public abstract class Layout2 {
//
//        public abstract void Run(LayoutBox box, LayoutUpdateType layoutUpdateType);
//        
//        public virtual float GetContentPreferredWidth(LayoutBox box) {
//            return 0;
//        }
//
//        public virtual float GetContentMinWidth(LayoutBox box) {
//            return 0;
//        }
//
//        public virtual float GetContentMaxWidth(LayoutBox box) {
//            return 0;
//        }
//        
//        public virtual float GetContentPreferredHeight(LayoutBox box, float width) {
//            return 0;
//        }
//
//        public virtual float GetContentMinHeight(LayoutBox box, float width) {
//            return 0;
//        }
//
//        public virtual float GetContentMaxHeight(LayoutBox box, float width) {
//            return 0;
//        }
//
//
//    }
//
//    public class RootLayout : Layout2 {
//
//        public override void Run(LayoutBox box, LayoutUpdateType layoutUpdateType) {
//            box.firstChild.UpdateWidthValues();
//            box.firstChild.UpdateHeightValuesUsingWidth(box.firstChild.preferredWidth);
//            box.firstChild.SetRectFromParentLayout(0, 0, box.firstChild.preferredWidth, box.firstChild.preferredHeight);
//        }
//
//    }
//
//    public class GridLayout2 : Layout2 {
//
//        public override void Run(LayoutBox box, LayoutUpdateType layoutUpdateType) {
//        }
//
//    }
//
//    public class FixedLayout2 : Layout2 {
//
//        public override void Run(LayoutBox box, LayoutUpdateType layoutUpdateType) {
//            
//        }
//
//    }
//
//    
//    public struct LayoutUpdate {
//
//        public int elementId;
//        
//
//    }
//
//    public enum LayoutChangeType {
//
//        ChildAdded,
//        ChildRemoved,
//        ChildMoved,
//        ChildEnabled,
//        ChildDisabled,
//        ChildHidden,
//        ChildShown,
//        BorderChanged,
//        MinWidth,
//        MinHeight,
//        MaxWidth,
//        MaxHeight,
//        PrefWidth,
//        PrefHeight,
//        ContentBoxWidth,
//        ContentBoxHeight,
//        LayoutParameter,
//        TransformChanged,
//        Overflow
//        
//    }
//    
//    // layout boxes NEVER set their own allocated size
//    // when box preferred size changes, all we can do is tell the parent layout box
//    // parent layout box will guarantee allocated size is between min & max (if defined)
//    // child may overflow as needed
//    
//    // width is always computed first
//    // height can depend on width but not vice versa
//    
//    // example: text element gets more text
//    // space for that text might not change parent layout
//    // but child's own size will take this into account
//    // parent -> sets allocated size
//    // child -> sets own size
//    // overflow -> own size > allocated size for a given axis
//    
////    public class FlexColumnLayoutBox : LayoutBox2 {
////
////        // change element, change type, new value, old value
////        // FlushUpdates(ChildrenAdded, ChildrenRemoved, ChildrenMoved, StyleChanges)
////        public void FlushLayoutUpdates() {
////            // full layout:
////                // wrap mode changes
////                // if actual width changed
////                // if grew && no flexed items -> re-distribute space
////                // if grew && flexed items -> flex if possible
////                // if shrunk && no flexed items -> recompute overflow
////                // if shrunk && has shrink items -> 
////                // if height changed && has stretched item -> stretch
////                // 
////        }
////
////        public void OnPositionChanged() { }
////        
////        public void OnAllocatedSizeChanged() { }
////
////        public void OnChildPreferredWidthChanged() {
////            // if (stretched) && actual width of child within min / max -> all good
////            // if shrunk && item can shrink 
////            // updateType.RedistributeWidth = true;
////            // updateWidthsFromTrack(trackForChild)
////        }
////                
////    }
//
//}