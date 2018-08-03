using System;
using System.Collections.Generic;
using System.Reflection;
using Src;

public class TemplateContext {
    public struct Binding {
        public readonly string key;
        public readonly string propName;
        public readonly UIElement boundElement;

        public Binding(string key, string propName, UIElement boundElement) {
            this.key = key;
            this.propName = propName;
            this.boundElement = boundElement;
        }
    }

    public readonly List<string> dirtyBindings;
    private readonly Dictionary<string, List<Binding>> bindingMap;

    public TemplateContext() {
        bindingMap = new Dictionary<string, List<Binding>>();
        dirtyBindings = new List<string>();
    }

    public void CreateBinding(PropertyBindPair binding, UIElement element) {
        List<Binding> list;
        if (!bindingMap.TryGetValue(binding.value, out list)) {
            list = new List<Binding>();
            bindingMap[binding.value] = list;
        }

        // probably want to type check that too
        list.Add(new Binding(binding.key, binding.value, element));
    }

    public List<Binding> GetBoundElements(string bindingName) {
        List<Binding> list;
        return bindingMap.TryGetValue(bindingName, out list) ? list : null;
    }

    public ObservedProperty GetBinding(string bindingName) {
        return null;
    }
}