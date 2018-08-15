using System.Diagnostics;

namespace Src {

    [DebuggerDisplay("{key}={value}")]
    public class AttributeDefinition {

        private string[] modifiers;

        public readonly string key;
        public readonly string value;

        private static readonly string[] EmptyModifierList = new string[0];
        public ExpressionNode bindingExpression;

        public AttributeDefinition(string key, string value) {
            this.key = key.Trim();
            this.value = value.Trim();
        }

        public string[] KeyPath {
            get {
                if (modifiers != null) {
                    return modifiers;
                }
                
                if (key.IndexOf('.') == -1) {
                    modifiers = EmptyModifierList;
                    return modifiers;
                }
            
                modifiers = key.Split('.');
            
                for (int i = 0; i < modifiers.Length; i++) {
                    modifiers[i] = modifiers[i].Trim();
                }
            
                return modifiers;
            }
        }
    }

}