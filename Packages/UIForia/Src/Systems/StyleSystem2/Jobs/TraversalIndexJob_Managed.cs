using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct TraversalIndexJob_Managed : IJob {

        /// <summary>
        /// freed internally
        /// </summary>
        public GCHandle rootElementHandle;
        
        /// <summary>
        /// output list, expects to already have proper size
        /// </summary>
        public UnmanagedList<ElementTraversalInfo> traversalInfo;
        
        public void Execute() {

            UIElement root = (UIElement) rootElementHandle.Target;
            
            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            stack.array[stack.size++] = root;
            int idx = 0;

            ushort ftbIndex = 0;
            ushort btfIndex = 0;

            while (stack.size != 0) {

                UIElement current = stack.array[--stack.size];

                traversalInfo.array[idx++] = new ElementTraversalInfo() {
                    depth = current.hierarchyDepth,
                    ftbIndex = ftbIndex++
                };

                current.ftbIndex = (ushort) (ftbIndex - 1);
                
                int childCount = current.ChildCount;

                stack.EnsureAdditionalCapacity(childCount);

                for (int i = childCount - 1; i >= 0; i--) {

                    UIElement child = current.children.array[i];

                    if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                        stack.array[stack.size++] = child;
                    }

                }

            }

            stack.array[stack.size++] = root;
            idx = 0;

            while (stack.size != 0) {

                UIElement current = stack.array[--stack.size];

                traversalInfo.array[idx++].btfIndex = btfIndex++;

                int childCount = current.ChildCount;
                current.btfIndex = (ushort) (btfIndex - 1);

                for (int i = 0; i < childCount; i++) {

                    UIElement child = current.children.array[i];

                    if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                        stack.array[stack.size++] = child;
                    }

                }

            }

            traversalInfo.size = idx;
            stack.Release();
            rootElementHandle.Free();
        }

    }

}