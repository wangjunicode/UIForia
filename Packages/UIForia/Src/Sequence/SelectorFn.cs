using System;

namespace UIForia {

    public struct SelectorFn {

        public Func<QueryContext, bool> queryFunction;
        public SelectorQueryScope scope;

        public SelectorFn(Func<QueryContext, bool> queryFunction, SelectorQueryScope scope = SelectorQueryScope.Descendents) {
            this.queryFunction = queryFunction;
            this.scope = scope;
        }

    }

}