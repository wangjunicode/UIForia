using System;
using System.Reflection;

namespace Src {

    public struct Binding {

        public string propName; // shared
        public object previousValue; // needed?
        public ExpressionBinding expression; // shared
        public UIElement element; // instanced -> maybe not even needed because of traversal order
        public event Action onChange; // could take an element id instead of each getting its own handler.
                                      // potential big savings for lists

    }
    
    public class ExpressionBinding {
        
        public virtual object Evaluate(TemplateContext context) {
            return null;
        }
        
    }

    
    
}

/*



class CodeGenedExpressionBinding_Safe {
    // {item.x[$i].y}
    // todo can avoid lots of checks via ? operator, only check for null when needed. dont check value types for null
    public object Evaluate(TemplateContext context) {
        Thing item = context.GetContext(contextId) as item;
        if(item == null) return null;
        var list = item.x;
        if(list == null) return null;
        var indexed = list[context.currentIndex];
        if(indexed == null) return null;
        return index.y;
    }
    
    public bool DirtyCheck(UIElement element, TemplateContext context) {
        var last = element.propValue;
        var newVal = Evaluate(context);
        if(last != newVal) {    
            onChange(element.id, propName, newValue, lastValue);       
            return true;
        }
        return false;
    }
    
}

*/