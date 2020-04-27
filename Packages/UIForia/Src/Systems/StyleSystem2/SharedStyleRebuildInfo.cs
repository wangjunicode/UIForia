using System.Runtime.InteropServices;
using UIForia.Style;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia {

    [AssertSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SharedStyleRebuildInfo {

        public RangeInt location;
        public PagedListState* splitBufferBase;

        public PagedSplitBufferList<PropertyId, long> ResolvePropertyBuffer() {
            return new PagedSplitBufferList<PropertyId, long>(splitBufferBase);
        }
    }

}