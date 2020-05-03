using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(ConvertedStyleIdDebugView))]
    public unsafe struct ConvertedStyleId {

        public int styleSetId;
        public ushort newStyleCount;
        public ushort oldStyleCount;
        public StyleStatePair* pStyles;

        public StyleStatePair* pOldStyles {
            get => pStyles;
        }
        
        public StyleStatePair* pNewStyles {
            get => pStyles + oldStyleCount;
        }

    }
    
    public unsafe struct ConvertedStyleIdDebugView {

        public int styleSetId;

        public StyleStatePair[] newStyles;
        public StyleStatePair[] oldStyles;
        
        public ConvertedStyleIdDebugView(ConvertedStyleId target) {
            this.styleSetId = target.styleSetId;
            this.newStyles = new StyleStatePair[target.newStyleCount];
            this.oldStyles = new StyleStatePair[target.oldStyleCount];

            for (int i = 0; i < target.oldStyleCount; i++) {
                oldStyles[i] = target.pStyles[i];
            }

            for (int i = 0; i < target.newStyleCount; i++) {
                newStyles[i] = target.pStyles[target.oldStyleCount + i];
            }
        }

    }

    public interface IMergeableToList<T> where T : unmanaged {

        void Merge(UnmanagedList<T> output);

        int ItemCount();

    }

    public unsafe struct ConvertedStyleListDebugView {

        public UnmanagedPagedList<StyleStatePair> styles;
        public UnmanagedPagedList<ConvertedStyleId> ids;

        public ConvertedStyleListDebugView(ConvertedStyleList target) {
            this.styles = target.GetStyleList();
            this.ids = target.GetIdList();
        }

    }

    [DebuggerTypeProxy(typeof(ConvertedStyleListDebugView))]
    public unsafe struct ConvertedStyleList : IPerThreadCompatible, IMergeableToList<ConvertedStyleId> {

        private PagedListState* pIdRanges;
        private PagedListState* pStyles;

        public ConvertedStyleList(Allocator allocator) : this() {
            InitializeForThread(allocator);
        }

        public bool IsInitialized {
            get => pIdRanges != null && pStyles != null;
        }

        public UnmanagedPagedList<StyleStatePair> GetStyleList() {
            return new UnmanagedPagedList<StyleStatePair>(pStyles);
        }

        public UnmanagedPagedList<ConvertedStyleId> GetIdList() {
            return new UnmanagedPagedList<ConvertedStyleId>(pIdRanges);
        }

        public void InitializeForThread(Allocator allocator) {
            pIdRanges = new UnmanagedPagedList<ConvertedStyleId>(64, allocator).GetStatePointer();
            pStyles = new UnmanagedPagedList<StyleStatePair>(256, allocator).GetStatePointer();
        }

        public ConvertedStyleId this[int index] {
            get => GetIdList()[index];
        }

        public int size {
            get => GetIdList().totalItemCount;
        }

        public void Add(int styleDataId, StyleStatePair* buffer, int oldCount, int newCount) {
            pIdRanges->AddItem(new ConvertedStyleId() {
                styleSetId = styleDataId,
                oldStyleCount = (ushort) oldCount,
                newStyleCount = (ushort) newCount,
                pStyles = pStyles->AddRange<StyleStatePair>(buffer, oldCount + newCount)
            });
        }

        public void Dispose() {
            if (pIdRanges != null) {
                pIdRanges->Dispose();
            }

            if (pStyles != null) {
                pStyles->Dispose();
            }
        }

        public void Merge(UnmanagedList<ConvertedStyleId> output) {
            pIdRanges->FlattenToList<ConvertedStyleId>(output);
        }

        public int ItemCount() {
            return pIdRanges->totalItemCount;
        }

        public void Clear() {
            pIdRanges->Clear();
            pStyles->Clear();
        }

    }

}