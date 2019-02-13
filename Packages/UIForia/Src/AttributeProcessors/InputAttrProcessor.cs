using System;
using System.Collections.Generic;
using System.Reflection;

namespace UIForia.AttributeProcessors {

    public class InputAttrProcessor : IAttributeProcessor {

        public void Process(UIElement element, UITemplate template, IReadOnlyList<ElementAttribute> attributes) {
//            if (!(element is UIInputFieldElement inputFieldElement)) return;
//
//            // x-model="valueToMapTo" 
//            // todo -- handle dotted property access ie model="rootThing.dot.value" 
//            // todo -- replace reflection w/ linq
//            
//            if (currentAttr.name != "model") {
//                return;
//            }
//
//            Type templateRootType = element.templateContext.rootObject.GetType();
//
//            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfo(templateRootType, currentAttr.name);
//                
//            if (fieldInfo != null) {
//                inputFieldElement.onValueChanged += (value) => fieldInfo.SetValue(element.templateContext.rootObject, value);
//                return;
//            }
//
//            PropertyInfo propertyInfo = ReflectionUtil.GetPropertyInfo(templateRootType, currentAttr.name);
//
//            if (propertyInfo != null) {
//                inputFieldElement.onValueChanged += (value) => propertyInfo.SetValue(element.templateContext.rootObject, value);
//            }
        }

    }

}