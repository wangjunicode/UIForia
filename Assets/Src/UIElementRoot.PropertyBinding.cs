using System.Collections.Generic;

    public class PropertyBinding {

        public readonly string key;
        public readonly BindingType bindingType;
        public List<UIElement> dependents;
        
        public object Value { get; set; }

        public PropertyBinding(string key, BindingType bindingType) {
            this.key = key;
            this.bindingType = bindingType;
        }
        
        public void AddDependentChild(UIElement element) {}
        
    }
