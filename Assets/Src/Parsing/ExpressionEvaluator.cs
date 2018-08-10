using System;
using System.Collections;
using System.Reflection;
using Rendering;

namespace Src {

   

    public class FieldSetterStruct_Dynamic<T> where T : struct {

        

    }

    public class FieldSetterClass_Dynamic<T> where T : class {

        public static readonly FieldInfo fieldInfo; 
        public static readonly ExpressionEvaluator getter;
        
        public void Execute(UIElement element, TemplateContext context) {
            T newValue = (T)getter.Evaluate(context);
            T oldValue = (T)fieldInfo.GetValue(element);
            
            if (newValue == oldValue) return;
            
            fieldInfo.SetValue(element, newValue);
            element.OnPropsChanged();
        }

    }
    
    public class FieldSetterBinding<T> : Binding {

        public FieldInfo fieldInfo; // can probably be static
        public ExpressionEvaluator getter; // can probably be static
        public T previousValue; // can probably be read with a getter(static) instead or straight from field info
        
        public override void Execute(UIElement element, TemplateContext context) {
            T newValue = (T)getter.Evaluate(context);
            if (newValue.Equals(previousValue)) {
                previousValue = newValue;
                fieldInfo.SetValue(element, newValue);
                element.OnPropsChanged();
            }
        }

    }

    public class RepeatEnterBinding : Binding {

        public Func<UIElement, IList> GetList;
        
        public override void Execute(UIElement element, TemplateContext context) {
            // check if list changed
            // apply filter
            // setup indices
            // set current list
            context.currentList = GetList(element);
            // filter list
            // handle pooling / destroying things as needed
            
            for (int i = 0; i < element.children.Count; i++) {
                ((UIRepeatChild) element.children[i]).index = i;
            }        
        }

    }

    public class FieldSetter {


    }
    
    public class ExpressionEvaluator {

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