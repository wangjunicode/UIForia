using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Layout {

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct LayoutBoxUnion : IDisposable {

        [FieldOffset(0)] public LayoutBoxType layoutType;
        [FieldOffset(4)] public FlexLayoutBoxBurst flex;
        [FieldOffset(4)] public TextLayoutBoxBurst text;
        [FieldOffset(4)] public GridLayoutBoxBurst grid;
        [FieldOffset(4)] public StackLayoutBoxBurst stack;
        [FieldOffset(4)] public ScrollLayoutBoxBurst scroll;
        [FieldOffset(4)] public FlexLayoutBoxBurst radial;
        [FieldOffset(4)] public FlexLayoutBoxBurst image;
        [FieldOffset(4)] public RootLayoutBoxBurst root;

        public void Initialize(LayoutBoxType layoutType, LayoutSystem layoutSystem, UIElement element) {

            this = default; // need to clear the memory here

            this.layoutType = layoutType;

            switch (layoutType) {
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    flex.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.Grid:
                    grid.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.Radial:
                    radial.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.Stack:
                    stack.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.Image:
                    image.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.Text:
                    text.OnInitialize(layoutSystem, element);
                    break;

                case LayoutBoxType.Root:
                    root.OnInitialize(layoutSystem, element);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static LayoutBoxType GetLayoutBoxType(UIElement element) {

            switch (element) {

                case UIViewRootElement _: return LayoutBoxType.Root;
                case UITextElement _: return LayoutBoxType.Text;
                case ScrollView _: return LayoutBoxType.ScrollView;
                case UIImageElement _: return LayoutBoxType.Image;

                default:
                    switch (element.style.LayoutType) {
                        default:
                        case LayoutType.Unset:
                        case LayoutType.Flex:
                            return LayoutBoxType.Flex;

                        case LayoutType.Grid: return LayoutBoxType.Grid;
                        case LayoutType.Radial: return LayoutBoxType.Radial;
                        case LayoutType.Stack: return LayoutBoxType.Stack;

                    }

            }

        }

        public static LayoutBoxType GetLayoutBoxTypeForScrollView(UIElement element) {

            switch (element.style.LayoutType) {
                default:
                case LayoutType.Unset:
                case LayoutType.Flex: return LayoutBoxType.Flex;

                case LayoutType.Grid: return LayoutBoxType.Grid;
                case LayoutType.Radial: return LayoutBoxType.Radial;
                case LayoutType.Stack: return LayoutBoxType.Stack;
            }

        }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            switch (layoutType) {

                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    return flex.GetActualContentWidth(ref runner);

                case LayoutBoxType.Grid:
                    return grid.GetActualContentWidth(ref runner);

                case LayoutBoxType.Radial:
                    return radial.GetActualContentWidth(ref runner);

                case LayoutBoxType.Stack:
                    return stack.GetActualContentWidth(ref runner);

                case LayoutBoxType.Image:
                    return image.GetActualContentWidth(ref runner);

                case LayoutBoxType.ScrollView:
                    return 0; // should never get called, would recurse

                case LayoutBoxType.Text:
                    return text.GetActualContentWidth(ref runner);

                case LayoutBoxType.Root:
                    return root.GetActualContentWidth(ref runner);

                default:
                    return 0;
            }
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            switch (layoutType) {

                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    return flex.GetActualContentHeight(ref runner);

                case LayoutBoxType.Grid:
                    return grid.GetActualContentHeight(ref runner);

                case LayoutBoxType.Radial:
                    return radial.GetActualContentHeight(ref runner);

                case LayoutBoxType.Stack:
                    return stack.GetActualContentHeight(ref runner);

                case LayoutBoxType.Image:
                    return image.GetActualContentHeight(ref runner);

                case LayoutBoxType.ScrollView:
                    return 0; // should never get called, would recurse

                case LayoutBoxType.Text:
                    return text.GetActualContentHeight(ref runner);

                case LayoutBoxType.Root:
                    return root.GetActualContentHeight(ref runner);

                default:
                    return 0;
            }
        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            switch (layoutType) {

                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    return flex.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Grid:
                    return grid.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Radial:
                    return radial.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Stack:
                    return stack.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Image:
                    return image.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.ScrollView:
                    return 0; // should never get called, would recurse

                case LayoutBoxType.Text:
                    return text.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Root:
                    return root.ResolveAutoWidth(ref runner, elementId, measurement, blockSize);

                default:
                    return 0;
            }
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            switch (layoutType) {

                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    return flex.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Grid:
                    return grid.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Radial:
                    return radial.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Stack:
                    return stack.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Image:
                    return image.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.ScrollView:
                    return 0; // should never get called, would recurse

                case LayoutBoxType.Text:
                    return text.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                case LayoutBoxType.Root:
                    return root.ResolveAutoHeight(ref runner, elementId, measurement, blockSize);

                default:
                    return 0;
            }
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            switch (layoutType) {

                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    flex.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.Grid:
                    grid.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.Radial:
                    radial.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.Stack:
                    stack.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.Image:
                    image.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.Text:
                    text.OnChildrenChanged(layoutSystem);
                    break;

                case LayoutBoxType.Root:
                    root.OnChildrenChanged(layoutSystem);
                    break;
            }

        }

        public void RunLayoutHorizontal(BurstLayoutRunner* runner) {
            switch (layoutType) {

                default:
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    flex.RunHorizontal(runner);
                    break;

                case LayoutBoxType.Grid:
                    grid.RunHorizontal(runner);
                    break;

                case LayoutBoxType.Radial:
                    radial.RunHorizontal(runner);
                    break;

                case LayoutBoxType.Stack:
                    stack.RunHorizontal(runner);
                    break;

                case LayoutBoxType.Image:
                    image.RunHorizontal(runner);
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.RunHorizontal(runner);
                    break;

                case LayoutBoxType.Text:
                    text.RunHorizontal(runner);
                    break;

                case LayoutBoxType.Root:
                    root.RunHorizontal(runner);
                    break;
            }
        }

        public void RunLayoutVertical(BurstLayoutRunner* runner) {

            switch (layoutType) {

                default:
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    flex.RunVertical(runner);
                    break;

                case LayoutBoxType.Grid:
                    grid.RunVertical(runner);
                    break;

                case LayoutBoxType.Radial:
                    radial.RunVertical(runner);
                    break;

                case LayoutBoxType.Stack:
                    stack.RunVertical(runner);
                    break;

                case LayoutBoxType.Image:
                    image.RunVertical(runner);
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.RunVertical(runner);
                    break;

                case LayoutBoxType.Text:
                    text.RunVertical(runner);
                    break;

                case LayoutBoxType.Root:
                    root.RunVertical(runner);
                    break;
            }

        }

        public float ComputeContentWidth(ref BurstLayoutRunner runner, BlockSize blockSize) {
            switch (layoutType) {

                default:
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    return flex.ComputeContentWidth(ref runner, blockSize);

                case LayoutBoxType.Grid:
                    return grid.ComputeContentWidth(ref runner, blockSize);

                case LayoutBoxType.Radial:
                    return radial.ComputeContentWidth(ref runner, blockSize);

                case LayoutBoxType.Stack:
                    return stack.ComputeContentWidth(ref runner, blockSize);

                case LayoutBoxType.Image:
                    return image.ComputeContentWidth(ref runner, blockSize);

                case LayoutBoxType.ScrollView:
                    return scroll.ComputeContentWidth(ref runner, blockSize);

                case LayoutBoxType.Text:
                    return text.ComputeContentWidth(ref runner, blockSize);
            }

        }

        public float ComputeContentHeight(ref BurstLayoutRunner runner, BlockSize blockSize) {
            switch (layoutType) {

                default:
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    return flex.ComputeContentHeight(ref runner, blockSize);

                case LayoutBoxType.Grid:
                    return grid.ComputeContentHeight(ref runner, blockSize);

                case LayoutBoxType.Radial:
                    return radial.ComputeContentHeight(ref runner, blockSize);

                case LayoutBoxType.Stack:
                    return stack.ComputeContentHeight(ref runner, blockSize);

                case LayoutBoxType.Image:
                    return image.ComputeContentHeight(ref runner, blockSize);

                case LayoutBoxType.ScrollView:
                    return scroll.ComputeContentHeight(ref runner, blockSize);

                case LayoutBoxType.Text:
                    return text.ComputeContentHeight(ref runner, blockSize);
            }
        }

        public void Dispose() {
            switch (layoutType) {

                default:
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    flex.Dispose();
                    break;

                case LayoutBoxType.Grid:
                    grid.Dispose();
                    break;

                case LayoutBoxType.Radial:
                    radial.Dispose();
                    break;

                case LayoutBoxType.Stack:
                    stack.Dispose();
                    break;

                case LayoutBoxType.Image:
                    image.Dispose();
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.Dispose();
                    break;

                case LayoutBoxType.Text:
                    text.Dispose();
                    break;
            }

            this = default;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) {
            switch (layoutType) {
                default:
                case LayoutBoxType.Unset:
                    break;

                case LayoutBoxType.Flex:
                    flex.OnStylePropertiesChanged(layoutSystem, element, properties, propertyCount);
                    break;

                case LayoutBoxType.Grid:
                    grid.OnStylePropertiesChanged(layoutSystem, element, properties, propertyCount);
                    break;

                case LayoutBoxType.Radial:
                    break;

                case LayoutBoxType.Stack:
                    stack.OnStylePropertiesChanged(layoutSystem, element, properties, propertyCount);
                    break;

                case LayoutBoxType.Image:
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.OnStylePropertiesChanged(layoutSystem, element, properties, propertyCount);
                    break;

                case LayoutBoxType.Text:
                    break;

                case LayoutBoxType.Root:
                    break;
            }
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId elementId, StyleProperty[] properties, int propertyCount) {
            switch (layoutType) {
                default:
                case LayoutBoxType.Unset:
                    break;

                case LayoutBoxType.Flex:
                    flex.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.Grid:
                    grid.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.Radial:
                    radial.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.Stack:
                    stack.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.Image:
                    image.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.ScrollView:
                    scroll.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.Text:
                    text.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;

                case LayoutBoxType.Root:
                    root.OnChildStyleChanged(layoutSystem, elementId, properties, propertyCount);
                    break;
            }
        }

    }

}