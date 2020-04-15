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
        
        public Func<ElementSystem, UIElement> entry;
        
        public Action<ElementSystem> hydrate;
        public Action<LinqBindingNode>[] bindings;
        public Action<ElementSystem>[] elements;
        public Action<LinqBindingNode, InputEventHolder>[] inputEventHandlers;

    }

}