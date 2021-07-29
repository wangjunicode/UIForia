using System;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public unsafe class SelectorQuery : IScriptNode {

        public IScriptNode action;
        public LightList<QueryId> queries;

        private StructList<ElementId> results;
        private Func<QueryContext, bool> queryFunction;
        private SelectorQueryScope scope;
        private SequenceStepState state;

        public SelectorQuery(Func<QueryContext, bool> func, SelectorQueryScope scope = SelectorQueryScope.Descendents) {
            this.queryFunction = func;
            this.scope = scope;
        }

        public SelectorQuery(Func<QueryContext, bool> func, IScriptNode action) {
            this.queryFunction = func;
            this.scope = SelectorQueryScope.Descendents;
            this.action = action;
        }

        public SelectorQuery(Func<QueryContext, bool> func, SelectorQueryScope scope, IScriptNode action) {
            this.queryFunction = func;
            this.scope = scope;
            this.action = action;
        }

        public void Await(SequenceContext context, StructList<ElementId> targets) {

            Update(context, targets);

            if (!IsComplete && results == null || results.size == 0) {
                // run again 
                state = SequenceStepState.Initial;
            }

        }

        public bool IsComplete { get; set; }

        public void Reset() {
            results.size = 0;
            state = SequenceStepState.Initial;
            action?.Reset();
        }

        public void Update(SequenceContext context, StructList<ElementId> targets) {

            if (queryFunction != null) {

                switch (state) {

                    case SequenceStepState.Initial: {
                        RuntimeTraversalInfo runtimeTraversalInfo = context.queryContext.runtimeInfoTable[context.rootElementId.index];

                        int start = runtimeTraversalInfo.index + 1;
                        int end = runtimeTraversalInfo.lastChildIndex;

                        // todo -- filter out according to scope
                        results = StructList<ElementId>.Get();

                        for (int i = start; i < end; i++) {
                            ElementId elementId = context.queryContext.elementIdByActiveIndex[i];
                            // if not in same template -> ignore 
                            // if not child && select children -> ignore (mabye just use hierarchy table for this)
                            context.queryContext.SetElement(context.queryContext.elementTable[elementId.index]);
                            if (queryFunction.Invoke(context.queryContext)) {
                                results.Add(elementId);
                            }

                        }

                        if (action == null) {
                            state = SequenceStepState.Complete;
                        }

                        state = SequenceStepState.Running;

                        if (action != null) {
                            action.Update(context, results);
                            if (action.IsComplete) {
                                results.Release();
                                state = SequenceStepState.Complete;
                            }
                        }
                        else {
                            state = SequenceStepState.Complete;
                            results.Release();
                        }

                        break;
                    }

                    case SequenceStepState.Running: {

                        action.Update(context, results);

                        if (action.IsComplete) {
                            results.Release();
                            state = SequenceStepState.Complete;
                        }

                        break;
                    }

                    default:
                    case SequenceStepState.Complete:
                        break;
                }

                return;
            }

            switch (state) {
                case SequenceStepState.Initial: {
                    state = SequenceStepState.Running;
                    CheckedArray<TraversalInfo> traversalTable = context.traversalTable;

                    TraversalInfo rootInfo = traversalTable[context.rootElementId.index];

                    LongBoolMap results = context.GetSharedResultBuffer();

                    // depending on descendent count (known) this can go the other way and just check descendents directly

                    for (int i = 0; i < queries.size; i++) {

                        LongBoolMap queryResults = context.GetQueryResults(queries[i]);

                        for (int j = 0; j < queryResults.size; j++) {
                            results.map[j] &= queryResults.map[j];
                        }

                    }

                    // mabye stack alloc here if count isnt super high?
                    StructList<ElementId> elementIds = new StructList<ElementId>(results.PopCount());
                    elementIds.size = results.PopCount();

                    fixed (ElementId* ptr = elementIds.array) {
                        results.FillBuffer((int*) ptr, elementIds.size);
                    }

                    for (int i = 0; i < elementIds.size; i++) {
                        int elementIndex = elementIds.array[i].index;

                        if (!traversalTable[elementIndex].IsDescendentOf(rootInfo)) {
                            elementIds.array[i--] = elementIds.array[--elementIds.size];
                        }

                    }

                    Update(context, targets);
                    break;
                }

                case SequenceStepState.Running:
                    if (results.size != 0) {
                        action.Update(context, results);
                    }

                    break;

                case SequenceStepState.Complete:
                    break;
            }

            IsComplete = true;

        }

    }

}