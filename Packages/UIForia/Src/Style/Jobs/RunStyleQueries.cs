using System;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    
    internal struct AttributeMemberTable {

        public DataList<ElementId> elementIds; // chunked list? dunno how to handle this really but allocation per table feels dirty 

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct RunStyleQueries : IJob, IJobParallelForBatch, IUIForiaParallel {

        public ElementMap activeMap;
        public CheckedArray<int> childCounts;    // by flattened index 
        public CheckedArray<int> descendentCounts;    // by flattened index 
        public CheckedArray<ElementId> activeElementIds;

        public CheckedArray<QueryTable> queryTables;
        public CheckedArray<HierarchyInfo> hierarchyTable;

        public DataList<LongBoolMap>.Shared results;

        public CheckedArray<StyleState> states;
        public CheckedArray<int> activeIndexByElementId;
        public CheckedArray<int> siblingIndexByActiveIndex;
        
        public CheckedArray<AttributeMemberTable> attributeTables;

        public CheckedArray<int> parentIndexByActiveElementIndex;
        public CheckedArray<CheckedArray<bool>> styleConditionsByViewIndex;

        public ParallelParams parallel { get; set; }

        public void Execute(int startIndex, int count) {
            RunExecute(startIndex, startIndex + count);
        }

        public void Execute() {
            RunExecute(0, queryTables.size);
        }
        
        public void RunExecute(int start, int end) {

            // todo -- take a look at what indexing the result tables by elementId and not flattened index, some queries want the conversion, others do not
            for (int tableIdx = start; tableIdx < end; tableIdx++) {
            
                // active element result table
                LongBoolMap resultTable = results[tableIdx];

                // Note -- results are index based, not element based 

                switch (queryTables[tableIdx].queryType) {

                    case QueryType.ChildCount: {
                        QueryTable.RunChildCountQuery(childCounts, queryTables[tableIdx].queryData.childCountInfo, results[tableIdx]);
                        break;
                    }

                    case QueryType.State: {
                        QueryTable.RunStateQuery(states, queryTables[tableIdx].queryData.stateInfo, results[tableIdx]);
                        break;
                    }

                    case QueryType.OnlyChild: {
                        for (int e = 0; e < activeElementIds.size; e++) {

                            ElementId elementId = activeElementIds[e];
                            ref HierarchyInfo hierarchy = ref hierarchyTable.Get(elementId.index);
                            if (hierarchy.childCount == 1) {
                                resultTable.Set(activeIndexByElementId[hierarchy.firstChildId.index]);
                            } 
                            
                        }
                        break;
                    }
                    case QueryType.HasAttribute: 
                    case QueryType.HasAttributeWithValue: {
                        
                        int keyTagId = queryTables[tableIdx].queryData.attrInfo.tagId;

                        AttributeMemberTable attrKeyTable = attributeTables[keyTagId];
                        DataList<ElementId> elementIds = attrKeyTable.elementIds;
                        
                        for (int i = 0; i < elementIds.size; i++) {
                            resultTable.Set(activeIndexByElementId[elementIds[i].index]);    
                        }
                        
                        break;
                    }

                    case QueryType.HasStyle:
                        break;

                    case QueryType.HasTag:
                        // treat this as an implicit attribute maybe? 
                        break;

                    case QueryType.HasSiblings: {

                        for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                            int parentIndex = parentIndexByActiveElementIndex[activeElementIndex];
                            int parentChildCount = childCounts[parentIndex];
                            if (parentChildCount > 1) {
                                resultTable.Set(activeElementIndex);
                            }
                        }   
                        break;
                    }
                    
                    case QueryType.NthChild: {
                        QueryDataNthChildInfo nth = queryTables[tableIdx].queryData.nthChild;
                        bool negativeStep = nth.stepSize < 0;
                        if (nth.stepSize == 0) {
                            for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                                if (siblingIndexByActiveIndex[activeElementIndex] + 1 == nth.offset) {
                                    resultTable.Set(activeElementIndex);
                                }
                            }
                        }
                        else {
                            for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                                int siblingIndex = siblingIndexByActiveIndex[activeElementIndex];
                                bool matchesPattern = (siblingIndex + 1 + nth.offset) % nth.stepSize == 0;
                                bool isInBounds = (negativeStep && siblingIndex + 1 <= nth.offset && matchesPattern) ||
                                                  (!negativeStep && siblingIndex + 1 >= nth.offset && matchesPattern);

                                if (matchesPattern && isInBounds) {
                                    resultTable.Set(activeElementIndex);
                                }
                            }
                        }

                        break;
                    }

                    case QueryType.FirstChild: {
                        for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                            if (siblingIndexByActiveIndex[activeElementIndex] == 0) {
                                resultTable.Set(activeElementIndex);
                            }
                        }

                        break;
                    }

                    case QueryType.LastChild: {
                        for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                            int parentIndex = parentIndexByActiveElementIndex[activeElementIndex];
                            int parentChildCount = childCounts[parentIndex];
                            if (siblingIndexByActiveIndex[activeElementIndex] == parentChildCount - 1) {
                                resultTable.Set(activeElementIndex);
                            }
                        }
                        break;
                    }

                    case QueryType.Root:
                        break;

                    case QueryType.Condition:
                        int conditionId = queryTables[tableIdx].queryData.conditionInfo.tagId;
                        for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                            // todo -- matt wants to make this right
                            CheckedArray<bool> styleConditions = styleConditionsByViewIndex[0];
                            if (styleConditions[conditionId]) {
                                resultTable.Set(activeElementIndex);
                            }
                        }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (queryTables[tableIdx].invert) {
                    LongBoolMap table = resultTable;
                    for (int i = 0; i < table.size; i++) {
                        table.map[i] = ~table.map[i] & activeMap.map[i];
                    }
                }

            }
        }

    }

}