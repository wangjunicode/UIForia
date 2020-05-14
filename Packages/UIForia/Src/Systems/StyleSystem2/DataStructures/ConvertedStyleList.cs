using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(ConvertedStyleIdDebugView))]
    public unsafe struct ConvertedStyleId {

        public ElementId elementId;
        public ushort newStyleCount;
        public ushort oldStyleCount;
        public StyleStatePair* styles;

        public StyleStatePair* oldStyles {
            get => styles;
        }

        public StyleStatePair* newStyles {
            get => styles + oldStyleCount;
        }

    }

    public unsafe struct ConvertedStyleIdDebugView {

        public ElementId styleSetId;

        public StyleStatePair[] newStyles;
        public StyleStatePair[] oldStyles;

        public ConvertedStyleIdDebugView(ConvertedStyleId target) {
            this.styleSetId = target.elementId;
            this.newStyles = new StyleStatePair[target.newStyleCount];
            this.oldStyles = new StyleStatePair[target.oldStyleCount];

            for (int i = 0; i < target.oldStyleCount; i++) {
                oldStyles[i] = target.styles[i];
            }

            for (int i = 0; i < target.newStyleCount; i++) {
                newStyles[i] = target.styles[target.oldStyleCount + i];
            }
        }

    }

    public interface IMergeableToList<T> where T : unmanaged {

        void Merge(UnmanagedList<T> output);

        int ItemCount();

    }

    public unsafe struct ConvertedStyleListDebugView {

        public PagedList<StyleStatePair> styles;
        public PagedList<ConvertedStyleId> ids;

        public ConvertedStyleListDebugView(ConvertedStyleList target) {
            this.styles = target.GetStyleList();
            this.ids = target.GetIdList();
        }

    }

    [DebuggerTypeProxy(typeof(ConvertedStyleListDebugView))]
    public unsafe struct ConvertedStyleList : IPerThreadCompatible, IMergeableToList<ConvertedStyleId> {

        [NativeDisableUnsafePtrRestriction] private PagedListState* pIdRanges;
        [NativeDisableUnsafePtrRestriction] private PagedListState* pStyles;

        public ConvertedStyleList(Allocator allocator) : this() {
            InitializeForThread(allocator);
        }

        public bool IsInitialized {
            get => pIdRanges != null && pStyles != null;
        }

        public PagedList<StyleStatePair> GetStyleList() {
            return new PagedList<StyleStatePair>(pStyles);
        }

        public PagedList<ConvertedStyleId> GetIdList() {
            return new PagedList<ConvertedStyleId>(pIdRanges);
        }

        public void InitializeForThread(Allocator allocator) {
            pIdRanges = new PagedList<ConvertedStyleId>(64, allocator).GetStatePointer();
            pStyles = new PagedList<StyleStatePair>(256, allocator).GetStatePointer();
        }

        public ConvertedStyleId this[int index] {
            get => GetIdList()[index];
        }

        public int size {
            get => GetIdList().totalItemCount;
        }

        public void Add(ElementId elementId, StyleStatePair* buffer, int oldCount, int newCount) {
            pIdRanges->AddItem(new ConvertedStyleId() {
                elementId = elementId,
                oldStyleCount = (ushort) oldCount,
                newStyleCount = (ushort) newCount,
                styles = pStyles->AddRange<StyleStatePair>(buffer, oldCount + newCount)
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