using System;
using System.Collections.Concurrent;
using UIForia.Graphics;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;

namespace UIForia.Rendering {

    public unsafe class ManagedTextInfo : IDisposable {

        internal TextInfo* textInfoPtr;
        
        internal DataList<TextMaterialSetup>.Shared materialSetup;
        internal DataList<TextVertexOverride>.Shared vertexOverrides;
        
        private TextVertexOverride* freeListPtr;
        
        internal int AllocateVertexId() {
            // if (freeListPtr == null) {
            //     freeListPtr = (TextVertexOverrideFreeList*) (vertexOverrides + freeListPtr)->next;
            // }
            // vertexOverrides
            vertexOverrides.Add(default);
            return vertexOverrides.size - 1;
        }
        
        internal static ConcurrentQueue<IntPtr> s_ReleaseQueue = new ConcurrentQueue<IntPtr>();
        internal LightList<TextEffect> textEffects;
        internal LightList<TextEffect> textEffectTable;
        public ManagedTextSpanInfo firstSpan;
        internal TextSystem textSystem;

        ~ManagedTextInfo() {
            Dispose();
        }

        public void Dispose() {
            if (textInfoPtr == null) {
                return;
            }

            s_ReleaseQueue.Enqueue((IntPtr) textInfoPtr);
        }

    }

}