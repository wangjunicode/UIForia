using System;
using System.Collections;
using System.Collections.Generic;
using Src.Util;

namespace Src {

    public class UITemplateContext : ExpressionContext {

        private Dictionary<string, IList> aliasMap;

        public readonly UIView view;
        public UIElement currentElement;

        public UITemplateContext(UIElement rootContext, UIView view = null) : base(rootContext) {
            this.view = view;
        }

        public UIElement rootElement {
            get { return (UIElement) rootContext; }
            set { rootContext = value; }
        }

    }

}