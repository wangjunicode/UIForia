using System.Collections.Generic;

namespace Src {

    public class TemplateScope {

        public UIView view;
        public UITemplateContext context;
        public List<UIElementCreationData> inputChildren;

        public readonly List<UIElementCreationData> outputList;

        public TemplateScope(List<UIElementCreationData> outputList) {
            this.outputList = outputList;
        }

        public void SetParent(UIElementCreationData child, UIElementCreationData parent) {
            if (parent == null) {
                outputList.Add(child);
                return;
            }
            child.element.parent = parent.element;
            parent.children.Add(child);
        }

        public void Clear() {
            inputChildren.Clear();
            outputList.Clear();
        }

        private void Register(List<UIElementCreationData> list) {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++) {
                view.Register(list[i]);
                Register(list[i].children);
            }
            list.Clear();
        }
        
        public void RegisterAll() {
            Register(outputList);
            Clear();
        }

    }

}