using System;
using System.Collections.Generic;
using Rendering;

namespace Src.Elements {
    
    /*
     * Binding philosophy: Always work.
     * Optimizations can be made when checked properties are observable or props
     * if not just use reflection and dirty checking
     */
    
    public class UIElementRepeat<T> : UIElement {
        
        [Prop] public List<T> list;
        [Prop] public string alias = "item";
        [Prop] public Func<T, bool> filter;
        
        private List<T> previousList;
        private int lastListSize;

        public UIElementTemplate[] childTemplates;

        public void OnPropsUpdated() {
            if (list != previousList) {
                //RecycleChildren()
                //AppendifNeeded();
                // for each new item in list
                    // AddChild(item);
                // for each removed item
                    // RemoveChild
            }         
        }

        private UIElement AddChild(T dataSource) {
            for (int i = 0; i < childTemplates.Length; i++) {
                UIElementTemplate template = childTemplates[i];
                // todo -- make scoped children work, may be separate method ie CreateScopedElement()
                //scopedChildren = template.CreateScopedChildren(context);
                UIElement child = template.CreateElement();
                // binding.contextIndex = index;
                // referenceContext.SetAlias(alias, dataSource);
                // for each binding in child.bindings
                    // child.binding.create(referenceContext)
                // referenceContext.RemoveAlias(alias);
                /*
                 * PropertyLookupBinding() {
                 *    object context = referenceContext.GetContext(contextName);
                 *    
                 *    property.GetValue("").GetType().GetField("x").GetValue();
                 * }
                 */
            }

            return null;
        }

        public UIElementRepeat(UIView view) : base(view) { }

    }
}