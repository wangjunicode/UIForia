using System;
using UIForia.Prototype;
using UIForia.Style;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct ConstructLayoutHierarchy : IJob {

        public CheckedArray<HierarchyInfo> hierarchyTable;
        public CheckedArray<ElementId> elementIdByIndex;
        public CheckedArray<int> activeIndexByElementId;
        public CheckedArray<int> parentIndexByActiveElementIndex;
        public CheckedArray<TemplateInfo> templateInfoTable;
        public CheckedArray<ushort> depthTable;

        public SolverParameters solverParameters;

        [NativeDisableUnsafePtrRestriction] public TextDataTable* textDataTable;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;
        [NativeDisableUnsafePtrRestriction] public LayoutTree* output_layoutTree;
        [NativeDisableUnsafePtrRestriction] public LockedBumpAllocator* perFrameBumpAllocator;
        [NativeDisableUnsafePtrRestriction] public PropertyTables* propertyTables;
        [NativeDisableUnsafePtrRestriction] public PropertySolverGroup_LayoutBehaviorTypeFontSize* solverGroup;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutData* perFrameLayoutData;
        
        public void Execute() {
            // job is small enough that we can just use the temp allocator 
            // todo -- find a better metric than consuming all of temp 
            BumpAllocator bumpAllocator = new BumpAllocator(TypedUnsafe.Kilobytes(15), Allocator.Temp);
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, &bumpAllocator);
            bumpAllocator.Dispose();
            Run();
        }

        private void Run() {

            LayoutBehavior* behaviorTable = styleTables->LayoutBehavior;
            UIFontSize* fontSizeTable = styleTables->TextFontSize;
            
            int maxDepth = 0;
            for (int i = 0; i < depthTable.size; i++) {
                if (depthTable[i] > maxDepth) maxDepth = depthTable[i];
            }

            maxDepth++; // include the last level 

            int* _counts = stackalloc int[maxDepth];
            int* _ignoredCounts = stackalloc int[maxDepth];
            
            CheckedArray<int> counts = new CheckedArray<int>(_counts, maxDepth);
            CheckedArray<int> ignoredCounts = new CheckedArray<int>(_ignoredCounts, maxDepth);

            for (int i = 0; i < elementIdByIndex.size; i++) {
                ElementId elementId = elementIdByIndex[i];
                int depth = depthTable[elementId.index];
                counts[depth]++;
            }

            for (int i = 0; i < elementIdByIndex.size; i++) {
                ElementId elementId = elementIdByIndex[i];
                if (behaviorTable[elementId.index] == LayoutBehavior.Ignored) {
                    int depth = depthTable[elementId.index];
                    counts[depth]--;
                    ignoredCounts[depth]++;
                }
            }

            output_layoutTree->depthLevels = perFrameBumpAllocator->AllocateCheckedArrayCleared<LayoutDepthLevel>(maxDepth);
            output_layoutTree->elementIdList = perFrameBumpAllocator->AllocateCheckedArray<ElementId>(elementIdByIndex.size);
            output_layoutTree->nodeList = perFrameBumpAllocator->AllocateCheckedArray<LayoutNode>(elementIdByIndex.size);
            output_layoutTree->elementIdToLayoutIndex = perFrameBumpAllocator->AllocateCheckedArray<int>(activeIndexByElementId.size);

            CheckedArray<LayoutNode> buffer = new CheckedArray<LayoutNode>(output_layoutTree->nodeList.array, output_layoutTree->nodeList.size);

            int nodeOffset = 0;
            for (int i = 0; i < maxDepth; i++) {

                output_layoutTree->depthLevels.array[i].nodeRange = new RangeInt(nodeOffset, 0);
                output_layoutTree->depthLevels.array[i].ignoredRange = new RangeInt(nodeOffset + counts[i], 0);

                nodeOffset += counts[i] + ignoredCounts[i];

            }

            int* layoutIndexByActiveIndex = TypedUnsafe.Malloc<int>(elementIdByIndex.size, Allocator.Temp);

            for (int i = 0; i < elementIdByIndex.size; i++) {

                ElementId elementId = elementIdByIndex[i];

                int depth = depthTable[elementId.index];

                int parentIndex = depth == 0 ? -1 : layoutIndexByActiveIndex[parentIndexByActiveElementIndex[i]];

                LayoutBehavior behavior = behaviorTable[elementId.index];

                int layoutIndex;

                if (behavior == LayoutBehavior.Ignored) {
                    buffer.array[parentIndex].childRange.length--;
                    layoutIndex = output_layoutTree->depthLevels[depth].ignoredRange.end;
                    output_layoutTree->depthLevels.array[depth].ignoredRange.length++;
                }
                else {
                    layoutIndex = output_layoutTree->depthLevels[depth].nodeRange.end;
                    output_layoutTree->depthLevels.array[depth].nodeRange.length++;
                }

                layoutIndexByActiveIndex[i] = layoutIndex;

                buffer.array[layoutIndex] = new LayoutNode() {
                    parentIndex = parentIndex,
                    elementId = elementId,
                    childRange = new RangeInt(0, hierarchyTable[elementId.index].childCount),
                    layoutBoxType = default,
                };
            }

            TypedUnsafe.Dispose(layoutIndexByActiveIndex, Allocator.Temp);

            for (int d = 0; d < output_layoutTree->depthLevels.size; d++) {

                RangeInt nodeRange = output_layoutTree->depthLevels[d].nodeRange;
                nodeRange.length += output_layoutTree->depthLevels[d].ignoredRange.length;
                int childOffset = 0;
                for (int i = nodeRange.start; i < nodeRange.end; i++) {
                    buffer.array[i].childRange.start = nodeRange.end + childOffset;
                    childOffset += buffer.array[i].childRange.length;
                }
            }

#if DEBUG
            for (int i = 0; i < output_layoutTree->depthLevels.size; i++) {
                output_layoutTree->depthLevels.array[i].tree = output_layoutTree;
            }
#endif

            TypedUnsafe.MemSet(output_layoutTree->elementIdToLayoutIndex.array, output_layoutTree->elementIdToLayoutIndex.size, byte.MaxValue);

            // separate list for just element ids since this is a common lookup 
            for (int i = 0; i < output_layoutTree->elementCount; i++) {
                ElementId elementId = output_layoutTree->nodeList[i].elementId;
                output_layoutTree->elementIdList[i] = elementId;
                output_layoutTree->elementIdToLayoutIndex[elementId.index] = i;
            }

            ComputeEmSizes(perFrameLayoutData->emTable, perFrameLayoutData->lineHeightTable, output_layoutTree, fontSizeTable, styleTables->TextFontAsset);

            AssignTemplateTypes();

        }

        private void AssignTemplateTypes() {
            for (int i = 0; i < output_layoutTree->elementCount; i++) {
                ref LayoutNode node = ref output_layoutTree->nodeList.Get(i);
                ElementId elementId = node.elementId;
                int elementIndex = elementId.index;

                switch (templateInfoTable[elementIndex].typeClass) {

                    default:
                    case ElementTypeClass.Container:
                    case ElementTypeClass.Template:
                        node.layoutBoxType = (LayoutBoxType) (byte) styleTables->LayoutType[elementIndex];
                        break;

                    case ElementTypeClass.ScrollView:
                        node.layoutBoxType = LayoutBoxType.Scroll;
                        break;

                    case ElementTypeClass.Text:
                        // todo -- detect text direction vertical and assign layout type accordingly 
                        node.layoutBoxType = LayoutBoxType.TextHorizontal;
                        textDataTable->elementIdToTextId.TryGetValue(elementId, out TextId textId);
                        textDataTable->boxIndexToTextId.Add(i, textId);
                        break;

                    case ElementTypeClass.Image:
                        node.layoutBoxType = LayoutBoxType.Image;
                        break;
                }

            }
        }

        // todo -- see if we can extract this so we can start text jobs sooner without a scheduling bottleneck
        private void ComputeEmSizes([NoAlias] float* outputFontSizes,  [NoAlias] float * lineHeights, [NoAlias] LayoutTree* layoutTree, [NoAlias] UIFontSize* fontSizeTable, FontAssetId * fontAssetIdTable) {

            TempList<EmEntry> emEntries = TypedUnsafe.MallocSizedTempList<EmEntry>(elementIdByIndex.size, Allocator.Temp);

            for (int i = 0; i < elementIdByIndex.size; i++) {
                emEntries.array[i] = new EmEntry() {
                    parentIndex = parentIndexByActiveElementIndex[i],
                    styleValue = fontSizeTable[elementIdByIndex[i].index],
                    resolvedValue = 0
                };
            }

            for (int i = 0; i < elementIdByIndex.size; i++) {

                ref EmEntry emValue = ref emEntries.array[i];
                float parentValue = emEntries.array[emValue.parentIndex].resolvedValue;

                switch (emValue.styleValue.unit) {

                    default:
                    case UIFontSizeUnit.Default:
                        emValue.resolvedValue = parentValue;
                        break;

                    case UIFontSizeUnit.Pixel:
                        emValue.resolvedValue = emValue.styleValue.value;
                        break;

                    case UIFontSizeUnit.Em:
                        emValue.resolvedValue = parentValue * emValue.styleValue.value;
                        break;

                }

            }

            // the em table was solved based on flattened hierarchy indices
            // the order I have for layout is different, I need to resolve the 
            // em value at layout index, not at flattened index.
            for (int i = 0; i < layoutTree->elementCount; i++) {
                ElementId elementId = layoutTree->elementIdList[i];
                int flatIndex = activeIndexByElementId[elementId.index];
                outputFontSizes[i] = emEntries.array[flatIndex].resolvedValue;
            }

            DataList<SDFFontUnmanaged> fontTable = textDataTable->fontTable;
            
            for (int i = 0; i < layoutTree->elementCount; i++) {
                ElementId elementId = layoutTree->elementIdList[i];
                FontAssetId id = fontAssetIdTable[elementId.index];
                float fontSize = outputFontSizes[i];
                lineHeights[i] = fontSize - (fontTable[id.id].descent * fontSize);
            }

            emEntries.Dispose();

        }

        private struct EmEntry {

            public int parentIndex;
            public UIFontSize styleValue;
            public float resolvedValue;

        }

        

    }

}