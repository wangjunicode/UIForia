using System;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia {

    public class SelectorQueryBuilder {

        internal StructList<Filter> filters;
        internal Func<UIElement, bool> whereFn;
        internal SelectionSource source;

        internal SelectorQueryBuilder() {
            this.filters = null;
            this.whereFn = null;
            this.source = SelectionSource.Descendents;
        }

        public SelectorQueryBuilder WithState(StyleState2 state) {
            filters = filters ?? new StructList<Filter>();
            filters.Add(new Filter() {
                filterType = SelectorFilterType.ElementsWithState,
                state = state
            });
            return this;

        }

        public SelectorQueryBuilder WithoutState(StyleState2 state) {
            filters = filters ?? new StructList<Filter>();
            filters.Add(new Filter() {
                filterType = SelectorFilterType.ElementsWithState | SelectorFilterType.Inverted,
                state = state
            });
            return this;

        }

        public SelectorQueryBuilder WithTag(string tagName) {
            BasicStringFilter(tagName, SelectorFilterType.ElementsWithTag, false);
            return this;
        }
        
        public SelectorQueryBuilder WithoutTag(string tagName) {
            BasicStringFilter(tagName, SelectorFilterType.ElementsWithTag, true);
            return this;
        }

        public SelectorQueryBuilder WithStyle(string styleName) {
            BasicStringFilter(styleName, SelectorFilterType.ElementsWithStyle, false);
            return this;
        }

        public SelectorQueryBuilder WithoutStyle(string styleName) {
            BasicStringFilter(styleName, SelectorFilterType.ElementsWithStyle, true);
            return this;
        }

        public SelectorQueryBuilder WithAttr(string attrName) {
            AttrFilter(attrName, null, SelectorFilterType.ElementsWithAttribute, false);
            return this;
        }

        public SelectorQueryBuilder WithoutAttr(string attrName) {
            AttrFilter(attrName, null, SelectorFilterType.ElementsWithAttribute, true);
            return this;
        }

        public SelectorQueryBuilder WithAttrEqual(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueEquals, false);
            return this;
        }

        public SelectorQueryBuilder WithoutAttrEqual(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueEquals, true);
            return this;
        }

        public SelectorQueryBuilder WithAttrContains(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueContains, false);
            return this;
        }

        public SelectorQueryBuilder WithoutAttrContains(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueContains, true);
            return this;
        }

        public SelectorQueryBuilder WithAttrStartsWith(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueStartsWith, false);
            return this;
        }

        public SelectorQueryBuilder WithoutAttrStartsWith(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueStartsWith, true);
            return this;
        }

        public SelectorQueryBuilder WithAttrEndsWith(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueEndsWith, false);
            return this;
        }

        public SelectorQueryBuilder WithoutAttrEndsWith(string attrName, string expected) {
            AttrFilter(attrName, expected, SelectorFilterType.ElementsWithAttribute_ValueEndsWith, true);
            return this;
        }

        public SelectorQueryBuilder Where(Func<UIElement, bool> fn) {
            this.whereFn = fn;
            return this;
        }

        private void BasicStringFilter(string key, SelectorFilterType type, bool inverted) {
            if (inverted) {
                type |= SelectorFilterType.Inverted;
            }

            filters = filters ?? new StructList<Filter>();
            filters.Add(new Filter() {
                filterType = type,
                key = key,
            });
        }

        private void AttrFilter(string attrName, string value, SelectorFilterType filterType, bool inverted) {
            if (inverted) {
                filterType |= SelectorFilterType.Inverted;
            }

            filters = filters ?? new StructList<Filter>(4);

            filters.Add(new Filter() {
                filterType = filterType,
                key = attrName,
                value = value
            });

        }

        internal struct Filter {

            public SelectorFilterType filterType;
            public string key;
            public string value;
            public StyleState2 state;

        }

    }

}