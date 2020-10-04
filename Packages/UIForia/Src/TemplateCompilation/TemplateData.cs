using System;
using UIForia.Elements;
using UIForia.Systems;

namespace UIForia.Compilers {

    public class TemplateData {

        public string tagName;
        public string templateId;
        public Type type;

        public TemplateData(string tagName = null) {
            this.tagName = tagName;
        }

        public Func<TemplateSystem, UIElement> entry;

        public Action<TemplateSystem> hydrate;
        public Action<LinqBindingNode>[] bindings;
        public Action<TemplateSystem>[] elements;
        public Action<LinqBindingNode, InputEventHolder>[] inputEventHandlers;

    }

}