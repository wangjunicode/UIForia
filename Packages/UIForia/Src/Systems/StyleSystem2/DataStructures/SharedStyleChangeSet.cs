using System;
using System.Collections.Generic;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct SharedStyleChangeSet : IDisposable {

        public PagedList<StyleId> styleIds;
        public UnmanagedList<SharedStyleChangeEntry> entries;

        public SharedStyleChangeSet(uint idPageSize, int changeSetDefaultSize, Allocator allocator) {
            this.entries = new UnmanagedList<SharedStyleChangeEntry>(changeSetDefaultSize, allocator);
            this.styleIds = new PagedList<StyleId>(idPageSize, allocator);
        }

        public int Size {
            get => entries.size;
        }

        public void InitializeSharedStyles(ElementId styleDataId, StyleState2 state, params StyleId[] newStyles) {
            fixed (StyleId* styles = newStyles) {
                InitializeSharedStyles(styleDataId, styles, newStyles.Length, state);
            }

        }

        public void InitializeSharedStyles(ElementId styleDataId, StyleId* newStyleBuffer, int newStyleCount, StyleState2 state) {
            state |= StyleState2.Normal;

            StyleId* ptr = styleIds.Reserve(newStyleCount);

            SharedStyleChangeEntry changeSetData = new SharedStyleChangeEntry(styleDataId, (StyleState2Byte) state) {
                oldStyleCount = 0,
                newStyleCount = (byte) newStyleCount,
                pStyles = ptr
            };

            for (int i = 0; i < newStyleCount; i++) {
                ptr[i] = newStyleBuffer[i];
            }

            entries.Add(changeSetData);

        }

        // this is just for testing
        public void SetSharedStyles(ElementId styleDataId, ref StyleSetData styleData, params StyleId[] newStyles) {
            StyleId* styles = stackalloc StyleId[newStyles.Length];
            for (int i = 0; i < newStyles.Length; i++) {
                styles[i] = newStyles[i];
            }

            SetSharedStyles(styleDataId, ref styleData, styles, newStyles.Length);
        }

        public void SetSharedStyles(ElementId styleDataId, ref StyleSetData styleData, IList<StyleId> newStyles) {
            StyleId* styles = stackalloc StyleId[newStyles.Count];
            for (int i = 0; i < newStyles.Count; i++) {
                styles[i] = newStyles[i];
            }

            SetSharedStyles(styleDataId, ref styleData, styles, newStyles.Count);
        }

        public void SetSharedStyles(ElementId styleDataId, ref StyleSetData styleData, StyleId* newStyleBuffer, int newStyleCount) {

            if (styleData.styleChangeIndex == ushort.MaxValue) {
                styleData.styleChangeIndex = (ushort) entries.size;
                entries.Add(new SharedStyleChangeEntry(styleDataId, (StyleState2Byte)styleData.state));
            }

            // ref SharedStyleChangeEntry changeSetData = ref entries.GetReference(styleData.changeSetId);
            // changeSetData.oldStyleCount = (byte) styleData.styleIds.size;
            // changeSetData.newStyleCount = (byte) newStyleCount;
            //
            // StyleId* ptr = styleIds.Reserve(newStyleCount + styleData.styleIds.size);
            // changeSetData.pStyles = ptr;
            //
            // ptr += TypedUnsafe.MemCpyAdvance(ptr, styleData.styleIds.data, styleData.styleIds.size);

            for (int i = 0; i < newStyleCount; i++) {
                // ptr[i] = newStyleBuffer[i];
            }

        }

        public void Clear() {
            entries.size = 0;
            styleIds.Clear();
        }

        public void Dispose() {
            styleIds.Dispose();
            entries.Dispose();
            this = default;
        }

    }

}