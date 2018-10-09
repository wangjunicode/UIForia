using System;
using System.Collections;
using System.Collections.Generic;
using Src.Util;

namespace Src {

    public class UITemplateContext : ExpressionContext {

        private List<ValueTuple<int, int>> switchValues;
        private Dictionary<string, IList> aliasMap;

        public readonly UIView view;
        public UIElement currentElement;

        public UITemplateContext(UIView view) : base(null) {
            this.view = view;
        }

        public UIElement rootElement {
            get { return (UIElement) rootContext; }
            set { rootContext = value; }
        }

        public int GetSwitchValue(int id) {
            if (switchValues == null) return -1;
            for (int i = 0; i < switchValues.Count; i++) {
                if (switchValues[i].Item1 == id) {
                    return switchValues[i].Item2;
                }
            }

            return -1;
        }

    }

}