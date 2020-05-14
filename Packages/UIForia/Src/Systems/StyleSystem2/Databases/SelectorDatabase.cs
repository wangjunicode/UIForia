using System;
using UIForia.Elements;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia {
    
    public unsafe struct SelectorDatabase {

        public IntListMap<SelectorTypeIndex> table_SelectorIndices;
        public DataList<SelectorQuery>.Shared table_SelectorQueries;
        public DataList<SelectorEvent>.Shared table_SelectorEnterEvents;
        public DataList<SelectorEvent>.Shared table_SelectorExitEvents;
        public DataList<SelectorStyleEffect>.Shared table_SelectorStyles;

        public PagedList<SelectorTypeIndex> buffer_SelectorIndices;
        public PagedList<SelectorFilter> pagetable_Filters;

        public LightList<Func<UIElement, bool>> table_WhereFilters;

        public static SelectorDatabase Create() {
            
            return new SelectorDatabase() {
              
                table_SelectorIndices = new IntListMap<SelectorTypeIndex>(64, Allocator.Persistent), 
                table_SelectorQueries = new DataList<SelectorQuery>.Shared(64, Allocator.Persistent),
                table_SelectorEnterEvents = new DataList<SelectorEvent>.Shared(16, Allocator.Persistent),
                table_SelectorExitEvents = new DataList<SelectorEvent>.Shared(16, Allocator.Persistent),
                table_SelectorStyles = new DataList<SelectorStyleEffect>.Shared(128, Allocator.Persistent),
                buffer_SelectorIndices = new PagedList<SelectorTypeIndex>(512, Allocator.Persistent),
                pagetable_Filters = new PagedList<SelectorFilter>(256, Allocator.Persistent),
                table_WhereFilters = new LightList<Func<UIElement, bool>>()
            };

        }

        public void Dispose() {
            table_SelectorIndices.Dispose();
            table_SelectorQueries.Dispose();
            table_SelectorEnterEvents.Dispose();
            table_SelectorExitEvents.Dispose();
            table_SelectorStyles.Dispose();
            buffer_SelectorIndices.Dispose();
            pagetable_Filters.Dispose();
        }

        public void SetFiltersForQuery(int queryIndex, ref DataList<SelectorFilter> buffer) {

            RangeInt range = pagetable_Filters.AddRange(buffer.GetArrayPointer(), buffer.size);

            table_SelectorQueries[queryIndex].filters = pagetable_Filters.GetPointer(range.start);
            table_SelectorQueries[queryIndex].filterCount = range.length;
        }

        public void BuildSelectorIndices(int styleIndex, StyleState2 state, ref DataList<SelectorTypeIndex> buffer) {

            RangeInt range = buffer_SelectorIndices.AddRange(buffer.GetArrayPointer(), buffer.size);

            SelectorTypeIndex* typeIndexPtr = buffer_SelectorIndices.GetPointer(range.start);
            ListHandle handle = new ListHandle(range.length, typeIndexPtr, range.length);
            table_SelectorIndices.Add(BitUtil.SetHighLowBits(styleIndex, (int) (state)), handle);
        }

    }

    public struct SelectorEvent { }

    public struct SelectorStyleEffect {

        public int baseOffsetInBytes;
        public int offset;
        public int totalPropertyCount;
        public int enterEventCount;
        public int exitEventCount;

    }

}