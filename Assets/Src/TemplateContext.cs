using System;
using System.Collections;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class TemplateContext {

        public int currentIndex;
        public IList currentList;
        public object target;
        public Type targetType;

        public TemplateContext(object target) {
            this.target = target;
            this.targetType = target?.GetType();
        }

    }

    public class UITemplateContext : TemplateContext {

        public readonly UIView view;

        public UITemplateContext(UIView view) : base(null) {
            this.view = view;
        }

        public UIElement rootElement {
            get { return (UIElement) target; }
            set {
                target = value;
                targetType = target?.GetType();
            }
        }

    }

}