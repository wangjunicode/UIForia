using System;
using System.Collections;
using System.Collections.Generic;

namespace Src {

    public class UITemplateContext : ExpressionContext {

        private List<ValueTuple<int, int>> switchValues;

        public readonly UIView view;
        public IList activeList;

        public UITemplateContext(UIView view) : base(null) {
            this.view = view;
        }

        public UIElement rootElement {
            get { return (UIElement) rootContext; }
            set { rootContext = value; }
        }

        public void SetSwitchValue(int id, int index) {
            if (switchValues == null) {
                switchValues = new List<ValueTuple<int, int>>(1);
                switchValues.Add(ValueTuple.Create(id, index));
                return;
            }

            for (int i = 0; i < switchValues.Count; i++) {
                if (switchValues[i].Item1 == id) {
                    switchValues[i] = ValueTuple.Create(id, index);
                    return;
                }
            }

            switchValues.Add(ValueTuple.Create(id, index));
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