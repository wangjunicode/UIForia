using System;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class RuntimeTemplateData : CompiledTemplateData {

        public RuntimeTemplateData(TemplateSettings settings) : base(settings) { }

        public override void LoadTemplates() {
            templates = new Func<UIElement, TemplateScope2, UIElement>[compiledTemplates.size];
            bindings = new Action<UIElement, UIElement>[compiledBindings.size];
            slots = new Func<UIElement, TemplateScope2, UIElement>[compiledSlots.size];
            templateMetaData = new TemplateMetaData[compiledTemplates.size];

            for (int i = 0; i < templates.Length; i++) {
                templates[i] = (Func<UIElement, TemplateScope2, UIElement>) compiledTemplates[i].templateFn.Compile();
            }

            for (int i = 0; i < slots.Length; i++) {
                slots[i] = (Func<UIElement, TemplateScope2, UIElement>) compiledSlots[i].templateFn.Compile();
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

        }

    }

}