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
        
        public void SetValue(UIElement target) {
            TemplateContext context = target.referenceContext;
            if ((flags & ExpressionFlag.Inverted) != 0) {
                bool value = (bool) context.GetBindingValue(expressionString);
                fieldInfo.SetValue(target, value);
            }
            else {
                fieldInfo.SetValue(target, context.GetBindingValue(expressionString));
            }
        }

        public object Evaluate() {
           
            return null;
        }
        
    }

    
    
}