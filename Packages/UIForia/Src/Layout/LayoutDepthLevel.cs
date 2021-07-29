using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Burst;
using UnityEngine;

namespace UIForia.Layout {

    [DebuggerTypeProxy(typeof(LayoutDepthLevelDebuggerView))]
    internal unsafe struct LayoutDepthLevel {

        public RangeInt nodeRange;
        public RangeInt ignoredRange;

#if DEBUG
        [NoAlias] public LayoutTree* tree;
#endif

    }

    internal sealed unsafe class LayoutDepthLevelDebuggerView {

        public LayoutNode[] Normal;
        public LayoutNode[] Ignored;

        public LayoutDepthLevelDebuggerView(LayoutDepthLevel level) {
#if UNITY_EDITOR
            Normal = TypedUnsafe.ToArray(level.tree->nodeList.array, level.nodeRange.start, level.nodeRange.length);
            Ignored = TypedUnsafe.ToArray(level.tree->nodeList.array, level.ignoredRange.start, level.ignoredRange.length);
#endif
        }

    }

}