using System;

namespace UIForia {

    public class SelectorBuilder : EffectBuilder {

        internal string name;
        internal SelectorQueryBuilder queryBuilder;
        
        internal SelectorBuilder(string name) {
            this.name = name;
        }

        public SelectorQueryBuilder QueryFrom(SelectionSource source) {
            queryBuilder = new SelectorQueryBuilder();
            queryBuilder.source = source;
            return queryBuilder;
        }
        
        public void Enter(Action<StyleEventBuilder> builder) {
            throw new NotImplementedException();
        }

        public void Exit(Action<StyleEventBuilder> builder) {
            throw new NotImplementedException();
        }

    }

}