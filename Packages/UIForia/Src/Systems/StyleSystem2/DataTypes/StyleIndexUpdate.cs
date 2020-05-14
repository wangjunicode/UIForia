using System;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct StyleIndexUpdateSet : IPerThreadCompatible {

        // ideally this is 1 list that grows inwards, added starts adding from 0, removed starts adding from size -1
        private Allocator allocator;
        public List_StyleIndexUpdate addedStyles;
        public List_StyleIndexUpdate removedStyles;

        public void InitializeForThread(Allocator allocator) {
            this.allocator = allocator;
            ListInterfaceUtil<StyleIndexUpdate>.Create(ref addedStyles, 128, allocator);
            ListInterfaceUtil<StyleIndexUpdate>.Create(ref removedStyles, 128, allocator);
        }

        public bool IsInitialized {
            get => addedStyles.array != null && removedStyles.array != null;
        }

        public void Add(in StyleIndexUpdate styleIndexUpdate) {
            ListInterfaceUtil<StyleIndexUpdate>.Add(ref addedStyles, styleIndexUpdate, allocator);
        }

        public void Remove(in StyleIndexUpdate styleIndexUpdate) {
            ListInterfaceUtil<StyleIndexUpdate>.Add(ref removedStyles, styleIndexUpdate, allocator);
        }

        public void Dispose() {
            ListInterfaceUtil<StyleIndexUpdate>.Dispose(ref addedStyles, allocator);
            ListInterfaceUtil<StyleIndexUpdate>.Dispose(ref removedStyles, allocator);
        }

    }

    public struct StyleIndexUpdate : IComparable<StyleIndexUpdate> {

        public readonly StyleId styleId;
        public readonly ElementId elementId;

        public StyleIndexUpdate(ElementId elementId, StyleId styleId) {
            this.elementId = elementId;
            this.styleId = styleId;
        }

        public int CompareTo(StyleIndexUpdate other) {
            return styleId - other.styleId;
        }

    }

}