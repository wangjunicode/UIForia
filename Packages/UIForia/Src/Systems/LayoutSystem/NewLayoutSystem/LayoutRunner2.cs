using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Layout {

    internal unsafe class LayoutRunner2 : IDisposable {

        private UIElement rootElement;
        private LayoutSystem layoutSystem;
        private ElementSystem elementSystem;
        private UIView view;
        private ViewParameters viewParameters;
        private float lastDpi;
        public BurstLayoutRunner* runner;
        
        public LayoutRunner2(UIView view, LayoutSystem layoutSystem) {
            this.view = view;
            this.rootElement = view.RootElement;
            this.layoutSystem = layoutSystem;
            this.elementSystem = layoutSystem.elementSystem;
            this.runner = TypedUnsafe.Malloc<BurstLayoutRunner>(1, Allocator.Persistent);
        }

        // todo -- return job to run 
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

            new LayoutRun() {
                rootElementId = rootElement.id,
                layoutBoxTable = layoutSystem.elementSystem.layoutBoxTable,
                layoutMetaTable = layoutSystem.elementSystem.layoutMetaDataTable,
                layoutHierarchyTable = layoutSystem.elementSystem.layoutHierarchyTable
            }.Run();

        }

        private unsafe struct LayoutRun : IJob {

            public ElementId rootElementId;
            public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;
            public ElementTable<LayoutMetaData> layoutMetaTable;
            public ElementTable<LayoutBoxUnion> layoutBoxTable;
            public BurstLayoutRunner* runner;

            public void Execute() {
                // todo -- profile traversing into list and then running vs running via tree

                DataList<ElementId> stack = new DataList<ElementId>(256, Allocator.TempJob);

                ElementId ptr = layoutHierarchyTable[rootElementId].lastChildId;

                while (ptr != default) {
                    stack.Add(ptr);
                    ptr = layoutHierarchyTable[ptr].prevSiblingId;
                }

                while (stack.size != 0) {
                    ElementId current = stack[--stack.size];

                    // will need flags probably
                    layoutBoxTable[current].RunLayoutHorizontal(runner);

                    ElementId childPtr = layoutHierarchyTable[current].lastChildId;

                    while (childPtr != default) {
                        stack.Add(childPtr);
                        childPtr = layoutHierarchyTable[childPtr].prevSiblingId;
                    }

                }

                stack.Dispose();

            }

        }

        public void Dispose() {
            TypedUnsafe.Dispose(runner, Allocator.Persistent);
        }

    }

}