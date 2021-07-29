using System;
using UIForia.Util;

namespace UIForia {

    public class SelectorCondition : IScriptNode, IAwaitableNode {

        private bool conditionTrue;
        private SelectorFn selectorFnQuery;
        private Func<StructList<ElementId>, bool> test;
        private IScriptNode next;
        private StructList<ElementId> results;
        private bool ran;

        public SelectorCondition(SelectorFn selectorFnQuery, Func<StructList<ElementId>, bool> func, IScriptNode next) {
            this.selectorFnQuery = selectorFnQuery;
            this.test = func;
            this.next = next;
            this.results = new StructList<ElementId>();
        }
        public SelectorCondition(SelectorFn selectorFnQuery, IScriptNode next) {
            this.selectorFnQuery = selectorFnQuery;
            this.test = default;
            this.next = next;
            this.results = new StructList<ElementId>();
        }
        
        public bool IsComplete { get; private set; }

        public void Reset() {
            conditionTrue = false;
            results.size = 0;
            ran = false;
            next?.Reset();
        }

        public void Await(SequenceContext context, StructList<ElementId> targets) {
            
            if (!conditionTrue) {
                RuntimeTraversalInfo runtimeTraversalInfo = context.queryContext.runtimeInfoTable[context.rootElementId.index];

                int start = runtimeTraversalInfo.index + 1;
                int end = runtimeTraversalInfo.lastChildIndex;

                // todo -- filter out according to scope
                results.size = 0;

                for (int i = start; i < end; i++) {
                    ElementId elementId = context.queryContext.elementIdByActiveIndex[i];
                    // if not in same template -> ignore 
                    // if not child && select children -> ignore (mabye just use hierarchy table for this)
                    context.queryContext.SetElement(context.queryContext.elementTable[elementId.index]);
                    if (selectorFnQuery.queryFunction.Invoke(context.queryContext)) {
                        results.Add(elementId);
                    }

                }

                conditionTrue = test?.Invoke(results) ?? results.size > 0;

            }

            if (conditionTrue) {

                if (next != null) {
                    next.Update(context, results);
                }
            }

            IsComplete = conditionTrue && (next?.IsComplete ?? true);
            
        }


        public void Update(SequenceContext context, StructList<ElementId> targets) {

            if (IsComplete) return;
            
            if (ran && conditionTrue) {
                next?.Update(context, results);
                IsComplete = next?.IsComplete ?? true;
                return;
            }
            
            if (!conditionTrue) {
                RuntimeTraversalInfo runtimeTraversalInfo = context.queryContext.runtimeInfoTable[context.rootElementId.index];

                int start = runtimeTraversalInfo.index + 1;
                int end = runtimeTraversalInfo.lastChildIndex;

                // todo -- filter out according to scope
                results.size = 0;

                for (int i = start; i < end; i++) {
                    ElementId elementId = context.queryContext.elementIdByActiveIndex[i];
                    // if not in same template -> ignore 
                    // if not child && select children -> ignore (mabye just use hierarchy table for this)
                    context.queryContext.SetElement(context.queryContext.elementTable[elementId.index]);
                    if (selectorFnQuery.queryFunction.Invoke(context.queryContext)) {
                        results.Add(elementId);
                    }

                }

                conditionTrue = test?.Invoke(results) ?? results.size > 0;
                
            }

            if (conditionTrue) {

                if (next != null) {
                    next.Update(context, results);
                }
            }

            IsComplete = conditionTrue && (next?.IsComplete ?? true);
            
        }

    }

}