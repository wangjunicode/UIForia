using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Burst;
using UnityEngine;

namespace UIForia.Layout {

    internal static unsafe class SolveSizeUtil {

        public struct LayoutTypeInfoList {

            public LayoutInfo2* array;
            public int size;

            [DebuggerStepThrough]
            public CheckedArray<LayoutInfo2> ToCheckedArray() {
                return new CheckedArray<LayoutInfo2>(array, size);
            }

        }

        public static void GatherBoxesForSpacingAndCollapse([NoAlias] LayoutTree* layoutTree, ref TempList<LayoutTypeInfoList> layoutTypeLists, [NoAlias] LayoutInfo2* layoutBuffer, RangeInt nodeRange) {
            layoutTypeLists.MemClear();

            for (int i = nodeRange.start; i < nodeRange.end; i++) {
                int idx = (int) layoutTree->nodeList[i].layoutBoxType;
                ref LayoutTypeInfoList list = ref layoutTypeLists.Get(idx);
                list.size++;
            }

            LayoutInfo2* allocPtr = layoutBuffer;

            for (int i = 0; i < layoutTypeLists.size; i++) {
                ref LayoutTypeInfoList list = ref layoutTypeLists.Get(i);
                list.array = allocPtr;
                allocPtr += list.size;
                list.size = 0; // will get reset in next loop
            }

            for (int i = nodeRange.start; i < nodeRange.end; i++) {

                int boxIdx = (int) layoutTree->nodeList[i].layoutBoxType;

                RangeInt range = layoutTree->nodeList[i].childRange;
                if (range.length == 0) {
                    continue;
                }

                ref LayoutTypeInfoList list = ref layoutTypeLists.Get(boxIdx);

                list.array[list.size++] = new LayoutInfo2() {
                    boxIndex = i,
                    childRange = range,
                };

            }
        }

    }

}