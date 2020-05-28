using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Systems;

namespace UIForia.Layout {

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct LayoutBoxUnion {

        [FieldOffset(0)] public LayoutBoxType layoutType;
        [FieldOffset(4)] public FlexLayoutBoxBurst flex;
        [FieldOffset(4)] public FlexLayoutBoxBurst text;
        [FieldOffset(4)] public FlexLayoutBoxBurst grid;
        [FieldOffset(4)] public FlexLayoutBoxBurst stack;
        [FieldOffset(4)] public FlexLayoutBoxBurst scroll;
        [FieldOffset(4)] public FlexLayoutBoxBurst radial;
        [FieldOffset(4)] public FlexLayoutBoxBurst image;

        public void Initialize(UIElement element) {

            this = default; // need to clear the memory here

            layoutType = GetLayoutBoxType(element);

            switch (layoutType) {
                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    FlexLayoutBoxBurst.Initialize_Managed(ref flex, element);
                    break;

                case LayoutBoxType.Grid:
                    break;

                case LayoutBoxType.Radial:
                    break;

                case LayoutBoxType.Stack:
                    break;

                case LayoutBoxType.Image:
                    break;

                case LayoutBoxType.ScrollView:
                    break;

                case LayoutBoxType.Text:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static LayoutBoxType GetLayoutBoxType(UIElement element) {

            switch (element) {

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

        public void OnChildrenChanged(ElementId elementId, LayoutSystem layoutSystem) {
            switch (layoutType) {

                case LayoutBoxType.Unset:
                case LayoutBoxType.Flex:
                    FlexLayoutBoxBurst.OnChildrenChanged_Managed(ref flex, elementId, layoutSystem);
                    break;

                case LayoutBoxType.Grid:
                    break;

                case LayoutBoxType.Radial:
                    break;

                case LayoutBoxType.Stack:
                    break;

                case LayoutBoxType.Image:
                    break;

                case LayoutBoxType.ScrollView:
                    break;

                case LayoutBoxType.Text:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
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

    }

}