using System;
using System.Reflection;

namespace Src {

    public class ExpressionBinding {

        [Flags]
        public enum ExpressionFlag {

            Inverted = 1 << 0,
            Simple = 1 << 1,
            Dotted = 1 << 2,
            FnCall = 1 << 3,
            HasParameters =  1 << 4

        }
        
        public ExpressionFlag flags;
        public string expressionString;
        public string contextName;
        public string[] parameters;
        public FieldInfo fieldInfo;
        public int contextId;
        public bool isConstant;
        public bool isMultipart;
        public ExpressionBindingPart[] parts;
        
        public void SetValue(UIElement target) {
            TemplateContext context = target.referenceContext.GetContext(contextId);
            if ((flags & ExpressionFlag.Inverted) != 0) {
                bool value = (bool) context.GetBindingValue(expressionString);
                fieldInfo.SetValue(target, value);
            }
            else {
                fieldInfo.SetValue(target, context.GetBindingValue(expressionString));
            }
        }

        public object Evaluate() {
            ExpressionBinding binding;
            
            Func<ExpressionBinding, object> lookupProperty = (b) => {
                //contextRoot.GetValue(b.id);
                return null;
            };
            
            //Func<object, object> AddIntegers = () => { return (int) a.value + (int) b.value; };
            
//            for (int i = 0; i < binding.actions.Length; i++) {
//                
//            }
//            
            return null;
        }
        
    }

    public class ExpressionBindingPart {
        public bool isConstant;
        public string contextName;
        public string expression;
    }
    
    
}