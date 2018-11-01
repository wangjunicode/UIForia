using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Input;
using Src.InputBindings;
using Src.Rendering;
using Src.StyleBindings;
using Src.Util;

namespace Src {

    // this class contains all the data a system needs to create
    // and instance of an element. Except for the element instance
    // itself and the context, all other fields are references
    // to lists setup via template.Compile()

    // todo -- reduce this to be a pair of <UIElement, UIElementTemplate>
    [DebuggerDisplay("{" + nameof(element) + "}")]
    public class MetaData {

        private static readonly ObjectPool<MetaData> s_ObjectPool = new ObjectPool<MetaData>();

        public UIElement element;
        public UITemplateContext context;
        public List<MetaData> children;

        // these are SHARED between all instances of a template
        // therefore we can't combine them into one single list
        // to be processed by various systems. We save a ton of
        // allocation because of this sharing, the small annoyance
        // of having so many fields on these objects is worth it
        public Binding[] bindings;
        public Binding[] constantBindings;
        public List<UIStyleGroup> baseStyles;
        public List<StyleBinding> constantStyleBindings;
        
        // cannot combine handlers because the arrays are shared and used directly by the input system
        public KeyboardEventHandler[] keyboardEventHandlers;
        public MouseEventHandler[] mouseEventHandlers;
        public DragEventHandler[] dragEventHandlers;
        public DragEventCreator[] dragEventCreators;

        public MetaData() {
            this.children = ListPool<MetaData>.Get();
        }

        public MetaData(UIElement element, UITemplateContext context) {
            this.element = element;
            this.context = context;
            this.children = ListPool<MetaData>.Get();
        }

        public int elementId => element.id;
        public string name => element.name;

        public void AddChild(MetaData child) {
            children.Add(child);
            child.element.parent = element;
        }

        public static MetaData GetFromPool() {
            return s_ObjectPool.Get();
        }

        public static void Release(ref MetaData metaData) {
            // since the arrays are shared we don't release or clear them
            metaData.element = null;
            metaData.context = null;
            metaData.bindings = null;
            metaData.baseStyles = null;
            metaData.constantBindings = null;
            metaData.constantStyleBindings = null;
            metaData.keyboardEventHandlers = null;
            metaData.mouseEventHandlers = null;
            metaData.dragEventHandlers = null;
            metaData.dragEventCreators = null;

            for (int i = 0; i < metaData.children.Count; i++) {
                MetaData child = metaData.children[i];
                Release(ref child);
            }
            ListPool<MetaData>.Release(ref metaData.children);
            
            s_ObjectPool.Release(metaData);
            metaData = null;
        }

    }

}