using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Input;
using Src.InputBindings;
using Src.Rendering;
using Src.StyleBindings;

namespace Src {

    // this class contains all the data a system needs to create
    // and instance of an element. Except for the element instance
    // itself and the context, all other fields are references
    // to lists setup via template.Compile()
    
    // todo -- pool these
    [DebuggerDisplay("{" + nameof(element) + "}")]
    public class MetaData {

        public readonly UIElement element;
        public UITemplateContext context;
        public readonly List<MetaData> children;

        // these are SHARED between all instances of a template
        // therefore we can't combine them into one single list
        // to be processed by various systems. We save a ton of
        // allocation because of this sharing, the small annoyance
        // of having so many fields on these objects is worth it
        public Binding[] bindings;
        public Binding[] constantBindings;
        public Binding[] conditionalBindings;
        public InputBinding[] inputBindings;
        public List<UIBaseStyleGroup> baseStyles;
        public List<StyleBinding> constantStyleBindings;
        public KeyboardEventHandler[] keyboardEventHandlers;
        public MouseEventHandler[] mouseEventHandlers;
        public DragEventHandler[] dragEventHandlers;
        public DragEventCreator[] dragEventCreators;
        
        public MetaData(UIElement element, UITemplateContext context) {
            this.element = element;
            this.context = context;
            this.children = new List<MetaData>();
        }
        
        public int elementId => element.id;
        public string name => element.name;
        
        public void AddChild(MetaData child) {
            children.Add(child);
            child.element.parent = element;
        }

    }

}