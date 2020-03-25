using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Mono.Linq.Expressions;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Compilers {

    public static class TemplateLoader {

        public static CompiledTemplateData LoadRuntimeTemplates(Type type, TemplateSettings templateSettings) {
            CompiledTemplateData compiledTemplateData = TemplateCompiler.CompileTemplates(type, templateSettings);

            // Stopwatch stopwatch = Stopwatch.StartNew();

            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[compiledTemplateData.compiledTemplates.size];
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[compiledTemplateData.compiledBindings.size];
            Func<UIElement, UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, UIElement, TemplateScope, UIElement>[compiledTemplateData.compiledSlots.size];
            TemplateMetaData[] templateMetaData = new TemplateMetaData[compiledTemplateData.compiledTemplates.size];
            OrderablePartitioner<Tuple<int, int>> partition;

            if (templateMetaData.Length < 10) {
                for (int i = 0; i < templateMetaData.Length; i++) {
                    templates[i] = (Func<UIElement, TemplateScope, UIElement>) compiledTemplateData.compiledTemplates[i].templateFn.Compile();
                }
            }
            else {
                partition = Partitioner.Create(0, templateMetaData.Length);

                Parallel.ForEach(partition, (range, loopState) => {
                    for (int i = range.Item1; i < range.Item2; i++) {
                        templates[i] = (Func<UIElement, TemplateScope, UIElement>) compiledTemplateData.compiledTemplates[i].templateFn.Compile();
                    }
                });
            }

            if (compiledTemplateData.compiledSlots.size < 10) {
                for (int i = 0; i < compiledTemplateData.compiledSlots.size; i++) {
                    slots[i] = (Func<UIElement, UIElement, TemplateScope, UIElement>) compiledTemplateData.compiledSlots[i].templateFn.Compile();
                }
            }
            else {
                partition = Partitioner.Create(0, compiledTemplateData.compiledSlots.size);
                Parallel.ForEach(partition, (range, loopState) => {
                    for (int i = range.Item1; i < range.Item2; i++) {
                        slots[i] = (Func<UIElement, UIElement, TemplateScope, UIElement>) compiledTemplateData.compiledSlots[i].templateFn.Compile();
                    }
                });
            }

            if (bindings.Length < 10) {
                for (int i = 0; i < bindings.Length; i++) {
                    try {
                        bindings[i] = (Action<UIElement, UIElement>) compiledTemplateData.compiledBindings[i].bindingFn.Compile();
                    }
                    catch (Exception e) {
                        Debug.Log("binding " + compiledTemplateData.compiledBindings[i].bindingFn.ToCSharpCode());
                        Debug.Log(e);
                    }
                }
            }
            else {
                partition = Partitioner.Create(0, bindings.Length);
                Parallel.ForEach(partition, (range, loopState) => {
                    for (int i = range.Item1; i < range.Item2; i++) {
                        try {
                            bindings[i] = (Action<UIElement, UIElement>) compiledTemplateData.compiledBindings[i].bindingFn.Compile();
                        }
                        catch (Exception e) {
                            Debug.Log("binding " + compiledTemplateData.compiledBindings[i].bindingFn.ToCSharpCode());
                            Debug.Log(e);
                        }
                    }
                });
            }

            LightList<UIStyleGroupContainer> styleList = new LightList<UIStyleGroupContainer>(128);

            // StyleSheet[] sheets = compiledTemplateData.styleImporter.GetImportedStyleSheets();
            //
            // for (int i = 0; i < sheets.Length; i++) {
            //     StyleSheet sheet = sheets[i];
            //     styleList.EnsureAdditionalCapacity(sheet.styleGroupContainers.Length);
            //     for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
            //         styleList.array[styleList.size++] = sheet.styleGroupContainers[j];
            //     }
            // }

            for (int i = 0; i < templateMetaData.Length; i++) {
                templateMetaData[i] = compiledTemplateData.compiledTemplates[i].templateMetaData;
                templateMetaData[i].styleMap = styleList.array;
                templateMetaData[i].BuildSearchMap();
            }

            Dictionary<int, Func<ConstructedElement>> constructorFnMap = new Dictionary<int, Func<ConstructedElement>>(37);

            ConstructorInfo constructedTypeCtor = typeof(ConstructedElement).GetConstructor(new Type[] {typeof(int), typeof(UIElement)});
            System.Diagnostics.Debug.Assert(constructedTypeCtor != null, nameof(constructedTypeCtor) + " != null");
            Expression[] parameters = new Expression[2];

            // todo -- this can be improved, cannot currently parallelize because the write target (constructorFnMap) is a dictionary which is not threadsafe
            // can convert the constructorFnMap to an array but would need a unique index for each type that is sequential

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {
                if (kvp.Key.IsAbstract || kvp.Value.references == 0 || kvp.Value.id < 0 || kvp.Key.IsGenericTypeDefinition) {
                    continue;
                }

                ConstructorInfo ctor = kvp.Key.GetConstructor(Type.EmptyTypes);

                if (ctor == null) {
                    throw new TemplateCompileException(kvp.Key + " must provide a default constructor in order to be used in templates");
                }

                parameters[0] = Expression.Constant(compiledTemplateData.GetTagNameId(kvp.Value.tagName));
                parameters[1] = Expression.New(ctor);
                constructorFnMap[kvp.Value.id] = Expression.Lambda<Func<ConstructedElement>>(Expression.New(constructedTypeCtor, parameters)).Compile();
            }

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.genericTypeMap) {
                if (kvp.Key.IsAbstract || kvp.Value.references == 0 || kvp.Value.id < 0 || kvp.Key.IsGenericTypeDefinition) {
                    continue;
                }

                ConstructorInfo ctor = kvp.Key.GetConstructor(Type.EmptyTypes);

                if (ctor == null) {
                    throw new TemplateCompileException(kvp.Key + " must provide a default constructor in order to be used in templates");
                }

                parameters[0] = Expression.Constant(compiledTemplateData.GetTagNameId(kvp.Value.tagName));
                parameters[1] = Expression.New(ctor);
                constructorFnMap[kvp.Value.id] = Expression.Lambda<Func<ConstructedElement>>(Expression.New(constructedTypeCtor, parameters)).Compile();
            }

            compiledTemplateData.bindings = bindings;
            compiledTemplateData.slots = slots;
            compiledTemplateData.templates = templates;
            compiledTemplateData.templateMetaData = templateMetaData;
            compiledTemplateData.constructorFnMap = constructorFnMap;
            compiledTemplateData.constructElement = (typeId) => compiledTemplateData.constructorFnMap[typeId].Invoke();

            // stopwatch.Stop();
            // Debug.Log("Loaded UIForia templates in " + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " seconds");

            return compiledTemplateData;
        }

        public static CompiledTemplateData LoadPrecompiledTemplates(TemplateSettings templateSettings) {
            throw new NotImplementedException();
            // Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName(templateSettings.assemblyName);
            // Type type = assembly.GetType("UIForia.Generated.UIForiaGeneratedTemplates_" + templateSettings.StrippedApplicationName);
            //
            // if (type == null) {
            //     throw new ArgumentException("Trying to use precompiled templates for " + templateSettings.StrippedApplicationName + " but couldn't find the type. Maybe you need to regenerate the code?");
            // }
            //
            // CompiledTemplateData compiledTemplateData = new CompiledTemplateData(templateSettings);
            //
            // compiledTemplateData.styleImporter.importResolutionPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, "UIForia", compiledTemplateData.templateSettings.StrippedApplicationName);
            //
            // ITemplateLoader loader = (ITemplateLoader) Activator.CreateInstance(type);
            // string[] files = loader.StyleFilePaths;
            //
            // compiledTemplateData.styleImporter.Reset(); // reset because in testing we will already have parsed files, nuke these
            //
            // LightList<UIStyleGroupContainer> styleList = new LightList<UIStyleGroupContainer>(128);
            // Dictionary<string, StyleSheet> styleSheetMap = new Dictionary<string, StyleSheet>(128);
            //
            // for (int i = 0; i < files.Length; i++) {
            //     StyleSheet sheet = compiledTemplateData.styleImporter.ImportStyleSheetFromFile(files[i]);
            //     styleList.EnsureAdditionalCapacity(sheet.styleGroupContainers.Length);
            //
            //     for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
            //         styleList.array[styleList.size++] = sheet.styleGroupContainers[j];
            //     }
            //     
            //     styleSheetMap.Add(sheet.path, sheet);
            //
            // }
            //
            // compiledTemplateData.templates = loader.LoadTemplates();
            // compiledTemplateData.slots = loader.LoadSlots();
            // compiledTemplateData.bindings = loader.LoadBindings();
            // compiledTemplateData.templateMetaData = loader.LoadTemplateMetaData(styleSheetMap, styleList.array);
            //
            // for (int i = 0; i < compiledTemplateData.templateMetaData.Length; i++) {
            //     compiledTemplateData.templateMetaData[i].compiledTemplateData = compiledTemplateData;
            // }
            //
            // compiledTemplateData.constructElement = loader.ConstructElement;
            // compiledTemplateData.dynamicTemplates = loader.DynamicTemplates;
            //
            // return compiledTemplateData;
        }

    }

}