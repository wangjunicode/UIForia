using System;
using System.Collections;
using System.Reflection;

namespace Src {

    public struct Binding {

        public string propName; // shared
        public object previousValue; // needed?
        public ExpressionBinding expression; // shared
        public BindingChangeHandler changeHandler; //shared
        public UIElement element; // instanced -> maybe not even needed because of traversal order

    }

    public class ManualExpressionBinding : ExpressionBinding {

        private readonly Func<TemplateContext, bool> fn;
        
        public ManualExpressionBinding(Func<TemplateContext, bool> fn) {
            this.fn = fn;
        }

        public override object Evaluate(TemplateContext context) {
            return fn.Invoke(context);
        }

    }

    public class StyleValueSetter : ExpressionBinding {

        public override object Evaluate(TemplateContext context) {
            context.element.view.MarkForRendering(context.element);
            return base.Evaluate(context);
        }

    }

    public class TextValueSetter { }

    public class PropertySetter { }

    public class FieldSetter { }

    public class ListFilterSetter { }

    public class ListSetter { }

    public class VisibilitySetter { }

    public class RepeatChildIndexSetter : ExpressionBinding {

        public override object Evaluate(TemplateContext context) {
            context.currentIndex = ((UIRepeatChild) context.element).index;
        }

    }
    
    public abstract class BindingChangeHandler {

        public abstract void HandleChange(TemplateContext context, object newValue, object oldValue);

    }

    
    
    public class PropSetter : BindingChangeHandler {

        public override void HandleChange(TemplateContext context, object newValue, object oldValue) {
            context.element.GetType().GetField("").SetValue(context, newValue);
            context.element.OnPropsChanged(null);
        }

    }

    public class ListAssignmentWatcher : BindingChangeHandler {

        public override void HandleChange(TemplateContext context, object newValue, object oldValue) {
            context.SetListSource((IList) newValue);
        }

    }
    
    public class ListSizeWatcher : BindingChangeHandler {

        public override void HandleChange(TemplateContext context, object newValue, object oldValue) {
            int oldSize = (int) oldValue;
            int newSize = (int) newValue;
            if (oldSize > newSize) {
                
            }
//            context.currentList.Resize()
        }

    }
    
    public class ExpressionBinding {

        public void Run(UIElement element) {
            // if list isze is different
            // element.view.resize list
            // element.listproperty[index] = evaluate(element); 
        }
        
        public virtual object Evaluate(TemplateContext context) {
            return null;
        }

        public virtual void HandleChange(TemplateContext context) {
            // context.currentElement.setValue
            // context.currentElement.style.whatever = yes
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