using System;
using System.Collections.Generic;
using Src;

public class TemplateContext {

    public readonly UIElement element;
    public readonly List<string> dirtyBindings;

    private readonly List<object> contextProviders;
    private readonly Dictionary<string, List<UIElement>> bindingMap;

    public TemplateContext(UIElement element) {
        this.element = element;
        bindingMap = new Dictionary<string, List<UIElement>>();
        dirtyBindings = new List<string>();
    }

    public void CreateBinding(ExpressionBinding binding, UIElement element) {
        List<UIElement> list;
        // todo this will need to change when expressions get more interesting
        if (!bindingMap.TryGetValue(binding.expressionString, out list)) {
            list = new List<UIElement>();
            bindingMap[binding.expressionString] = list;
        }
        list.Add(element);
    }

    public object GetBindingValue(string bindingValue) {
        //return element.observedProperties[bindingValue].RawValue;
//        for (int i = 0; i < element.observedProperties.Length; i++) {
//            if (element.observedProperties[i].name == bindingValue) {
//                return element.observedProperties[i].RawValue;
//            }
//        }
        // might be a dotted property 
        // might be a function call
        // for now its just a look up
        return null;
    }

    public List<UIElement> GetBoundElements(string bindingName) {
        List<UIElement> list;
        return bindingMap.TryGetValue(bindingName, out list) ? list : null;
    }

    public void FlushChanges() {
        for (int i = 0; i < dirtyBindings.Count; i++) {
            string bindingName = dirtyBindings[i];
            List<UIElement> boundElements = GetBoundElements(bindingName);
            for (int j = 0; j < boundElements.Count; j++) {
                UIElement element = boundElements[j];
                // todo -- CHANGE THIS
                // element.originTemplate.expressionBindings[0].SetValue(element);
            }
        }
    }

    public object GetContext(int contextIndex) {
        return contextProviders[contextIndex];
    }

    public void AddContext(object dataSource) {
        contextProviders.Add(dataSource);
    }

}