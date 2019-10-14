using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Elements {

    public struct DepthKeyedElement {

        public int depth;
        public UIElement element;

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PoolableElementAttribute : Attribute { }

    [PoolableElement]
    public class ElementPool {

        public Dictionary<Type, LightList<UIElement>> poolMap = new Dictionary<Type, LightList<UIElement>>();
        public StructList<DepthKeyedElement> activeElements;

        // traversal index could be super helpful here but would dereference of elements to use it. profile this
        
        public void FindDescendents(UIElement element, LightList<UIElement> output) {
            int depth = element.depth;
            int count = activeElements.size;
            DepthKeyedElement[] active = activeElements.array;

            for (int i = 0; i < count; i++) {
                if (active[i].depth <= depth) {
                    continue;
                }

                UIElement ptr = active[i].element;
                while (ptr != null && ptr.depth < depth) {
                    if (ptr == element) {
                        output.Add(active[i].element);
                        break;
                    }

                    ptr = ptr.parent;
                }
            }
        }
        
        public UIElement Get(ProcessedType type) {

            return type.CreateInstance();
//            if (type.isPoolable) {
//                UIElement x = (UIElement) FormatterServices.GetUninitializedObject(type.rawType);
                
//            }
            
            // todo -- support pooling for all types with [PoolableElement] attribute on it
            // todo -- for non poolable type generate a function that is equivalent to new ElementType() instead of using activator
            // fn = () => new StronglyTypedThing();
            // return ctorMap.Get(type)();
            
            return (UIElement) Activator.CreateInstance(type.rawType);
//            if (poolMap.TryGetValue(type, out LightList<UIElement> elements)) {
//                if (elements.Count > 0) {
//                    return elements.RemoveLast();
//                }
//
//                return (UIElement) FormatterServices.GetUninitializedObject(type);
//            }
//            else {
//                return (UIElement) FormatterServices.GetUninitializedObject(type);
//            }
        }

        public void Release(UIElement element) {
//            if (element.pool != null) {
//                element.pool.Release(element);
//            }
            return; // we don't support pooling yet
            Type type = element.GetType();

            if (poolMap.TryGetValue(type, out LightList<UIElement> elements)) {
                elements.Add(element);
            }
            else {
                LightList<UIElement> list = new LightList<UIElement>();
                list.Add(element);
                poolMap[type] = list;
            }
        }

    }

}