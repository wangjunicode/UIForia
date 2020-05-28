using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Layout {

    internal class LayoutRunner2 {

        private UIElement rootElement;
        private LayoutSystem layoutSystem;
        private ElementSystem elementSystem;
        private UIView view;
        private ViewParameters viewParameters;
        private float lastDpi;

        public LayoutRunner2(UIView view, LayoutSystem layoutSystem) {
            this.view = view;
            this.layoutSystem = layoutSystem;
            this.elementSystem = layoutSystem.elementSystem;
        }
        
        public void RunLayout() {
            
            if (rootElement.isDisabled) {
                return;
            }

            viewParameters = new ViewParameters() {
                viewX = view.position.x,
                viewY = view.position.y,
                viewWidth = view.Viewport.width,
                viewHeight = view.Viewport.height,
                applicationWidth = layoutSystem.application.Width,
                applicationHeight = layoutSystem.application.Height
            };
            
            float currentDpi = view.application.DPIScaleFactor;
            
            if (currentDpi != lastDpi) {
                // InvalidateAll(rootElement);
            }
            
            
            
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct LayoutBoxUnion {

            [FieldOffset(0)] public LayoutType layoutType;
            [FieldOffset(4)] public FlexLayoutBoxBurst flex;
            [FieldOffset(4)] public FlexLayoutBoxBurst text;
            [FieldOffset(4)] public FlexLayoutBoxBurst grid;
            [FieldOffset(4)] public FlexLayoutBoxBurst stack;
            [FieldOffset(4)] public FlexLayoutBoxBurst scroll;
            [FieldOffset(4)] public FlexLayoutBoxBurst radial;
            [FieldOffset(4)] public FlexLayoutBoxBurst image;

        }

        private unsafe struct LayoutRun : IJob {

            public ElementId rootElementId;
            public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;
            public ElementTable<LayoutMetaData> layoutMetaTable;
            public DataList<LayoutBoxUnion>.Shared layoutBoxList;
            
            public void Execute() {
                // todo -- profile traversing into list and then running vs running via tree

                DataList<ElementId> stack = new DataList<ElementId>(128, Allocator.TempJob);

                ElementId ptr = layoutHierarchyTable[rootElementId].lastChildId;

                while (ptr != default) {
                    stack.Add(ptr);
                    ptr = layoutHierarchyTable[ptr].prevSiblingId;
                }

                while (stack.size != 0) {
                    ElementId current = stack[--stack.size];

                    LayoutBoxId boxId = layoutMetaTable[current].layoutBoxId;
                    
                    RunLayoutHorizontal(boxId);
                    
                    ElementId childPtr = layoutHierarchyTable[rootElementId].lastChildId;

                    while (childPtr != default) {
                        stack.Add(childPtr);
                        childPtr = layoutHierarchyTable[childPtr].prevSiblingId;
                    }


                }
                
                stack.Dispose();

            }

            private void RunLayoutHorizontal(LayoutBoxId layoutBoxId) {
                
                switch (layoutBoxId.layoutBoxType) {

                    case LayoutType.Flex: {
                        layoutBoxList[layoutBoxId.instanceId].flex.RunHorizontal();
                        break;
                    }

                    case LayoutType.Unset:
                        break;

                    case LayoutType.Grid:
                        break;

                    case LayoutType.Radial:
                        break;

                    case LayoutType.Stack:
                        break;
                    
                    case LayoutType.Text:
                        break;
                    
                }
            }

            private static void RunLayoutHorizontal(LayoutType layoutType, ElementId elementId) {
                
               
            } 
        }

    }

}