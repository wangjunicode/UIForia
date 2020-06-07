using System;
using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

namespace UIForia.Layout {

    internal unsafe class LayoutContext : IDisposable {

        internal UIElement rootElement;
        internal UIView view;
        internal float lastDpi;
        internal BurstLayoutRunner* runner;

        internal DataList<ElementId>.Shared parentList;
        internal DataList<ElementId>.Shared elementList;
        internal List_TextLineInfo* lineBuffer;
        public DataList<ElementId>.Shared ignoredList;

        public LayoutContext(UIView view, LayoutSystem layoutSystem) {
            this.view = view;
            this.rootElement = view.dummyRoot;
            this.runner = TypedUnsafe.Malloc<BurstLayoutRunner>(Allocator.Persistent);
            *runner = new BurstLayoutRunner();
            this.elementList = new DataList<ElementId>.Shared(32, Allocator.Persistent);
            this.parentList = new DataList<ElementId>.Shared(32, Allocator.Persistent);
            this.ignoredList = new DataList<ElementId>.Shared(8, Allocator.Persistent);

            lineBuffer = TypedUnsafe.Malloc<List_TextLineInfo>(Allocator.Persistent);
            *lineBuffer = new List_TextLineInfo(16, Allocator.Persistent);

            layoutSystem.worldMatrices[rootElement.id] = float4x4.identity;

            layoutSystem.transformInfoTable[rootElement.id] = new TransformInfo() {
                scaleX = 1,
                scaleY = 1
            };

            layoutSystem.layoutBoxTable[rootElement.id].Initialize(layoutSystem, rootElement);
            layoutSystem.layoutHierarchyTable[rootElement.id] = new LayoutHierarchyInfo() {
                behavior = LayoutBehavior.Normal
            };

            layoutSystem.clipInfoTable[rootElement.id] = new ClipInfo() {
                overflow = Overflow.Visible,
                clipBehavior = ClipBehavior.Normal,
                visibility = Visibility.Visible,
                clipBounds = ClipBounds.BorderBox,
                clipperIndex = 1,
                pointerEvents = PointerEvents.Normal
            };

        }

        public void Dispose() {
            if (lineBuffer != null) {
                lineBuffer->Dispose();
                TypedUnsafe.Dispose(lineBuffer, Allocator.Persistent);
            }

            ignoredList.Dispose();
            elementList.Dispose();
            parentList.Dispose();
            TypedUnsafe.Dispose(runner, Allocator.Persistent);
        }

        public void RemoveFromIgnoredList(ElementId elementId) {
            // this is safe because the ignored list gets sorted in FlattenLayoutTree
            for (int i = 0; i < ignoredList.size; i++) {
                if (ignoredList[i] == elementId) {
                    ignoredList[i] = ignoredList[--ignoredList.size];
                    return;
                }
            }
        }

        public void AddToIgnoredList(ElementId elementId) {
            ignoredList.Add(elementId);
        }

    }

}