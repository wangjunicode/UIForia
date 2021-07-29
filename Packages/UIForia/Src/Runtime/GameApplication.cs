using System;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {

    public struct EntryPoint {

        public Type type;
        public Func<UIForiaRuntimeSystem, UIElement, object, object> fn;

        public EntryPoint(Type type, Func<UIForiaRuntimeSystem, UIElement, object, object> fn) {
            this.type = type;
            this.fn = fn;
        }

    }

    internal struct ApplicationInfo {

        public StyleDatabase styleDatabase;
        public int initialElementCapacity;
        public EntryPoint[] entryPoints;
        public TemplateFor[] typeTemplates;
        public Func<UIForiaRuntimeSystem, UIElement, object, object> main => entryPoints[0].fn;

    }

    public struct TemplateFor {

        public Type type;
        public Func<UIForiaRuntimeSystem, UIElement, object, object> fn;

    }

    public class TestApplication : UIApplication { }

    public class UIEditorApplication : UIApplication {

        public void Render(CommandBuffer commandBuffer) {
            applicationLoop.RenderEditor(this, commandBuffer);
        }

        public bool HasFocusedTextInput() {
            return focusedTextInputElementId != default;
        }

    }

    public class GameApplication : UIApplication { }

}

// internal void DoEnableElement(ElementId enableRootId) {
//            throw new NotImplementedException();
//            int enableRootIndex = enableRootId.id & ElementId.k_IndexMask;
//            ref ElementMetaInfo rootMeta = ref metaTable[enableRootIndex];
//
//            // if element is not enabled (ie has a disabled ancestor or is not alive), no-op 
//            if ((rootMeta.flags & UIElementFlags.Alive) == 0) {
//                return;
//            }
//
//            // if parent is disabled we just flip the flag and return 
//            if ((rootMeta.flags & UIElementFlags.AncestorEnabled) == 0) {
//                metaTable[enableRootIndex].flags |= UIElementFlags.Enabled;
//                return;
//            }
//
//            StructStack<int> stack = StructStack<int>.Get();
//            stack.array[stack.size++] = enableRootIndex;
//
//            const UIElementFlags k_EnabledFlags = UIElementFlags.EnabledFlagSet;
//
//            rootMeta.flags |= k_EnabledFlags;
//
//            while (stack.size != 0) {
//
//                int currentIndex = stack.array[--stack.size];
//
//                ref ElementMetaInfo metaInfo = ref metaTable[currentIndex];
//
//                metaInfo.flags |= UIElementFlags.AncestorEnabled;
//
//                // if the element is itself disabled or destroyed, keep going
//                if ((metaInfo.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
//                    continue;
//                }
//
//                // register the flag set even if we get disabled via OnEnable, we just want to track that OnEnable was called at least once
//
//                metaInfo.flags |= UIElementFlags.InitThisFrame | UIElementFlags.HasBeenEnabled;
//
//                try {
//                    instanceTable[currentIndex].OnEnable();
//                }
//                catch (Exception e) {
//                    Debug.Log(e);
//                }
//
//                // only continue if calling enable didn't re-disable the element
//                if ((metaInfo.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
//                    continue;
//                }
//
//                int childCount = hierarchyInfoTable[currentIndex].childCount;
//                int ptr = hierarchyInfoTable[currentIndex].lastChildId.id & ElementId.k_IndexMask;
//
//                if (stack.size + childCount >= stack.array.Length) {
//                    Array.Resize(ref stack.array, stack.size + childCount + 64);
//                }
//
//                while (ptr != 0) {
//                    // inline stack push
//                    stack.array[stack.size++] = ptr;
//                    //ptr = hierarchyInfoTable[ptr].prevSiblingId.id & ElementId.k_IndexMask;
//                }
//
//            }
//
//            StructStack<int>.Release(ref stack);
//
//        }
//
//        public void DoDisableElement(ElementId disableRootId) {
//            throw new NotImplementedException();
//            // if element is already disabled or destroyed, no op
//
//            int disableRootIndex = disableRootId.id & ElementId.k_IndexMask;
//            ref ElementMetaInfo disableRootMeta = ref metaTable[disableRootIndex];
//
//            if ((disableRootMeta.flags & UIElementFlags.Alive) == 0 || (disableRootMeta.flags & UIElementFlags.Enabled) == 0) {
//                return;
//            }
//
//            if ((disableRootMeta.flags & UIElementFlags.AncestorEnabled) == 0) {
//                disableRootMeta.flags &= ~UIElementFlags.Enabled;
//                return;
//            }
//
//            // if element is now enabled we need to walk it's children
//            // and set enabled ancestor flags until we find a self-disabled child
//            StructStack<int> stack = StructStack<int>.Get();
//            stack.array[stack.size++] = disableRootIndex;
//
//            bool ancestorWasDisabled = (disableRootMeta.flags & UIElementFlags.AncestorEnabled) == 0;
//
//            // stack operations in the following code are inlined since this is a very hot path
//            while (stack.size != 0) {
//                // inline stack pop
//                int currentIndex = stack.array[--stack.size];
//
//                ref ElementMetaInfo metaInfo = ref metaTable[currentIndex];
//
//                // note -- root element gets this re-flipped if needed
//                metaInfo.flags &= ~(UIElementFlags.AncestorEnabled | UIElementFlags.InitThisFrame);
//
//                // if destroyed the whole subtree is also destroyed, do nothing.
//                // if already disabled the whole subtree is also disabled, do nothing.
//
//                if ((metaInfo.flags & UIElementFlags.Enabled) == 0) {
//                    continue;
//                }
//
//                if ((metaInfo.flags & UIElementFlags.HasBeenEnabled) != 0) {
//                    // todo -- profile not calling disable when it's not needed
//                    try {
//                        instanceTable[currentIndex].OnDisable();
//                    }
//                    catch (Exception e) {
//                        Debug.Log(e);
//                    }
//                }
//
//                // wipe out style state data 
//                styleStateTable[currentIndex] = default;
//
//                // if child is still disabled after OnDisable, traverse it's children
//                if ((metaInfo.flags & UIElementFlags.Enabled) != 0) {
//                    continue;
//                }
//
//                int childCount = hierarchyInfoTable[currentIndex].childCount;
//
//                if (stack.size + childCount >= stack.array.Length) {
//                    Array.Resize(ref stack.array, stack.size + childCount + 64);
//                }
//
//                int ptr = hierarchyInfoTable[currentIndex].lastChildId.id & ElementId.k_IndexMask;
//
//                while (ptr != 0) {
//                    // inline stack push
//                    stack.array[stack.size++] = ptr;
//                    ptr = hierarchyInfoTable[ptr].prevSiblingId.id & ElementId.k_IndexMask;
//                }
//            }
//
//            // was disabled in loop, need to reset it here
//            // set this after the loop so we dont have special cases inside it.
//            if (!ancestorWasDisabled) {
//                disableRootMeta.flags |= UIElementFlags.AncestorEnabled;
//            }
//
//            disableRootMeta.flags &= ~(UIElementFlags.InitThisFrame | UIElementFlags.Enabled);
//
//            StructStack<int>.Release(ref stack);
//
//            // inputSystem.BlurOnDisableOrDestroy();
//        }
//
//        internal void DestroyElement() {
//            throw new NotImplementedException();
//        }

// internal void InitializeElement(ElementId initRoot) {
//
//           ElementId parentId = hierarchyInfoTable[initRoot.id & ElementId.k_IndexMask].parentId;
//
//           int rootIndex = initRoot.id & ElementId.k_IndexMask;
//           int rootParentIndex = parentId.id & ElementId.k_IndexMask;
//
//           StructStack<ElementParentId> stack = StructStack<ElementParentId>.Get();
//
//           stack.Push(new ElementParentId() {
//               elementIndex = rootIndex,
//               parentIndex = rootParentIndex
//           });
//
//           while (stack.size > 0) {
//
//               ElementParentId current = stack.array[--stack.size];
//
//               ref ElementMetaInfo metaInfo = ref metaTable[current.elementIndex];
//               ref ElementMetaInfo parentMetaInfo = ref metaTable[current.parentIndex];
//               UIElement element = instanceTable[current.elementIndex];
//
//               if ((parentMetaInfo.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
//                   metaInfo.flags |= UIElementFlags.AncestorEnabled;
//               }
//               else {
//                   metaInfo.flags &= ~UIElementFlags.AncestorEnabled;
//               }
//
//               if (current.parentIndex == 0) {
//                   metaInfo.depth = (byte) 0;
//               }
//               else {
//                   metaInfo.depth = (byte) (parentMetaInfo.depth + 1);
//               }
//
//               // todo -- only invoke if we actually need to 
//               try {
//                   element.OnCreate(); // might have been disabled here!
//               }
//               catch (Exception e) {
//                   Debug.LogWarning(e);
//               }
//
//               ref HierarchyInfo hierarchyInfo = ref hierarchyInfoTable[current.elementIndex];
//
//               int childCount = hierarchyInfo.childCount;
//               int ptr = hierarchyInfo.firstChildId.id & ElementId.k_IndexMask;
//
//               if (stack.size + childCount >= stack.array.Length) {
//                   Array.Resize(ref stack.array, stack.size + childCount + 64);
//               }
//
//               while (ptr != 0) {
//                   stack.array[stack.size++] = new ElementParentId() {
//                       parentIndex = current.elementIndex,
//                       elementIndex = ptr
//                   };
//                   ptr = hierarchyInfoTable[ptr].nextSiblingId.id & ElementId.k_IndexMask;
//               }
//
//           }
//
//           StructStack<ElementParentId>.Release(ref stack);
//
//           bool rootEnabled = (metaTable[rootIndex].flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet;
//
//           if (!rootEnabled) {
//               return;
//           }
//
//           // cheat with flags here to turn element off, allowing the enable call to work properly
//           metaTable[rootIndex].flags &= ~UIElementFlags.Enabled;
//
//           DoEnableElement(initRoot);
//
//       }
//
//       internal void InitializeElement(UIElement initRoot) {
//           InitializeElement(initRoot.elementId);
//       }
//
//      