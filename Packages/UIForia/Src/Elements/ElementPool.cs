using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UIForia.Util;

namespace UIForia.Elements {

    public class ElementPool {

        public Dictionary<Type, LightList<UIElement>> poolMap = new Dictionary<Type, LightList<UIElement>>();

        public UIElement Get(Type type) {
            if (poolMap.TryGetValue(type, out LightList<UIElement> elements)) {
                if (elements.Count > 0) {
                    return elements.RemoveLast();
                }

                return (UIElement) FormatterServices.GetUninitializedObject(type);
            }
            else {
                return (UIElement) FormatterServices.GetUninitializedObject(type);
            }
        }

        public void Release(UIElement element) {
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