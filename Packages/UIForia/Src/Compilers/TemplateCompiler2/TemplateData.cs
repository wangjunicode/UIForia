using System;
using UIForia.Elements;
using UIForia.Systems;

namespace UIForia.Compilers {

    public class TemplateData {

        public string tagName;
        public string templateId;

        public TemplateData(string tagName = null) {
            this.tagName = tagName;
        }
        
        public Action<LinqBindingNode>[] bindings;

        // retn, system, root, parent, this, data
        public Action<ElementSystem>[] elements;
        public Func<ElementSystem, UIElement, UIElement>[] slots;

        // retn, system, parent, data
        public Func<ElementSystem, UIElement> entry;
        public Action<ElementSystem> hydrate;

    }

}