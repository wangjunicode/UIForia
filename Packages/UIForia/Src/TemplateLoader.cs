using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public static class TemplateLoader {

        public static CompiledTemplateData LoadRuntimeTemplates(Type type, TemplateSettings templateSettings) {
            CompiledTemplateData compiledTemplateData = TemplateCompiler.CompileTemplates(type, templateSettings);

            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[compiledTemplateData.compiledTemplates.size];
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[compiledTemplateData.compiledBindings.size];
            Func<UIElement, UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, UIElement, TemplateScope, UIElement>[compiledTemplateData.compiledSlots.size];
            TemplateMetaData[] templateMetaData = new TemplateMetaData[compiledTemplateData.compiledTemplates.size];

            for (int i = 0; i < templates.Length; i++) {
                templates[i] = (Func<UIElement, TemplateScope, UIElement>) compiledTemplateData.compiledTemplates[i].templateFn.Compile();
            }

            for (int i = 0; i < slots.Length; i++) {
                slots[i] = (Func<UIElement, UIElement, TemplateScope, UIElement>) compiledTemplateData.compiledSlots[i].templateFn.Compile();
            }

            for (int i = 0; i < bindings.Length; i++) {
                try {
                    bindings[i] = (Action<UIElement, UIElement>) compiledTemplateData.compiledBindings[i].bindingFn.Compile();
                }
                catch (Exception e) {
                    Debug.Log("binding " + compiledTemplateData.compiledBindings[i].bindingFn.ToCSharpCode());
                    Debug.Log(e);
                }
            }

            LightList<UIStyleGroupContainer> styleList = new LightList<UIStyleGroupContainer>(128);

            StyleSheet[] sheets = compiledTemplateData.styleImporter.GetImportedStyleSheets();

            for (int i = 0; i < sheets.Length; i++) {
                StyleSheet sheet = sheets[i];
                styleList.EnsureAdditionalCapacity(sheet.styleGroupContainers.Length);
                for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                    styleList.array[styleList.size++] = sheet.styleGroupContainers[j];
                }
            }

            for (int i = 0; i < templateMetaData.Length; i++) {
                templateMetaData[i] = compiledTemplateData.compiledTemplates[i].templateMetaData;
                templateMetaData[i].styleMap = styleList.array;
                templateMetaData[i].BuildSearchMap();
            }

            Dictionary<int, Func<UIElement>> constructorFnMap = new Dictionary<int, Func<UIElement>>(37);

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

            compiledTemplateData.bindings = bindings;
            compiledTemplateData.slots = slots;
            compiledTemplateData.templates = templates;
            compiledTemplateData.templateMetaData = templateMetaData;
            compiledTemplateData.constructElement = (int typeId) => constructorFnMap[typeId].Invoke();

            return compiledTemplateData;
        }

        public static CompiledTemplateData LoadPrecompiledTemplates(TemplateSettings templateSettings) {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName(templateSettings.assemblyName);
            Type type = assembly.GetType("UIForia.Generated.UIForiaGeneratedTemplates_" + templateSettings.StrippedApplicationName);

            CompiledTemplateData compiledTemplateData = new CompiledTemplateData(templateSettings);

            try {
                ITemplateLoader loader = (ITemplateLoader) Activator.CreateInstance(type);
                string[] files = loader.StyleFilePaths;

                compiledTemplateData.styleImporter.Reset(); // reset because in testing we will already have parsed files, nuke these

                LightList<UIStyleGroupContainer> styleList = new LightList<UIStyleGroupContainer>(128);
                Dictionary<string, StyleSheet> styleSheetMap = new Dictionary<string, StyleSheet>(128);

                string streamingAssetPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia", compiledTemplateData.templateSettings.StrippedApplicationName);

                for (int i = 0; i < files.Length; i++) {
                    StyleSheet sheet = compiledTemplateData.styleImporter.ImportStyleSheetFromFile(files[i]);
                    styleList.EnsureAdditionalCapacity(sheet.styleGroupContainers.Length);

                    for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                        styleList.array[styleList.size++] = sheet.styleGroupContainers[j];
                    }

                    styleSheetMap.Add(sheet.path.Substring(streamingAssetPath.Length + 1), sheet);
                }

                compiledTemplateData.templates = loader.LoadTemplates();
                compiledTemplateData.slots = loader.LoadSlots();
                compiledTemplateData.bindings = loader.LoadBindings();
                compiledTemplateData.templateMetaData = loader.LoadTemplateMetaData(styleSheetMap, styleList.array);

                for (int i = 0; i < compiledTemplateData.templateMetaData.Length; i++) {
                    compiledTemplateData.templateMetaData[i].compiledTemplateData = compiledTemplateData;
                }

                compiledTemplateData.constructElement = loader.ConstructElement;
            }
            catch (Exception e) {
                throw e;
            }

            return compiledTemplateData;
        }

    }

}