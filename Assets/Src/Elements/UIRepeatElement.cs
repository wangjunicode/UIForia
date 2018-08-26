using System.Collections.Generic;

namespace Src {

    public class UIRepeatElement<T> : UIElement {

        public readonly UIRepeatChildTemplate template;
        public readonly TemplateScope scope;
        public readonly List<UIElement> children;

        public T previousListRef;
        public T filteredList;

        // todo --  have an inline version of that has no layout / render presense
        public UIRepeatElement(UIRepeatChildTemplate template, TemplateScope scope) {
            this.template = template;
            this.scope = scope;
            this.children = new List<UIElement>();
        }

    }


}