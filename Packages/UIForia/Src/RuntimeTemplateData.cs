using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class RuntimeTemplateData : CompiledTemplateData {

        public RuntimeTemplateData(TemplateSettings settings) : base(settings) { }
        private Dictionary<int, Func<UIElement>> constructorFnMap;

        public void LoadTemplates() {
            templates = new Func<UIElement, TemplateScope, UIElement>[compiledTemplates.size];
            bindings = new Action<UIElement, UIElement>[compiledBindings.size];
            slots = new Func<UIElement, TemplateScope, UIElement>[compiledSlots.size];
            templateMetaData = new TemplateMetaData[compiledTemplates.size];

            for (int i = 0; i < templates.Length; i++) {
                templates[i] = (Func<UIElement, TemplateScope, UIElement>) compiledTemplates[i].templateFn.Compile();
            }

            for (int i = 0; i < slots.Length; i++) {
                slots[i] = (Func<UIElement, TemplateScope, UIElement>) compiledSlots[i].templateFn.Compile();
            }

            for (int i = 0; i < bindings.Length; i++) {
                try {
                    bindings[i] = (Action<UIElement, UIElement>) compiledBindings[i].bindingFn.Compile();
                }
                catch (Exception e) {
                    Debug.Log("binding " + compiledBindings[i].bindingFn.ToCSharpCode());
                    Debug.Log(e);
                }
            }

            LightList<UIStyleGroupContainer> styleList = new LightList<UIStyleGroupContainer>(128);

            StyleSheet[] sheets = styleImporter.GetImportedStyleSheets();

            for (int i = 0; i < sheets.Length; i++) {
                StyleSheet sheet = sheets[i];
                styleList.EnsureAdditionalCapacity(sheet.styleGroupContainers.Length);
                for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                    styleList.array[styleList.size++] = sheet.styleGroupContainers[j];
                }
            }

            for (int i = 0; i < templateMetaData.Length; i++) {
                templateMetaData[i] = compiledTemplates[i].templateMetaData;
                templateMetaData[i].styleMap = styleList.array;
            }

            constructorFnMap = new Dictionary<int, Func<UIElement>>(37); 
            
            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {
                if (kvp.Key.IsAbstract || kvp.Value.references == 0) {
                    continue;
                }

                ConstructorInfo ctor = kvp.Key.GetConstructor(Type.EmptyTypes);
                
                if (ctor == null) {
                    throw new CompileException(kvp.Key + " must provide a default constructor in order to be used in templates");
                }
                
                constructorFnMap[kvp.Value.id] = Expression.Lambda<Func<UIElement>>(Expression.New(ctor)).Compile();
            }

            constructElement = DoConstructElement;

        }

        private UIElement DoConstructElement(int typeId) {
            return constructorFnMap[typeId].Invoke();
        }

    }

}