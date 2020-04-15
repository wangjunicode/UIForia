using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using FastExpressionCompiler;
using Mono.Linq.Expressions;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateLoader {

        public Dictionary<Type, TemplateData> templateDataMap;
        
        private LightList<TemplateExpressionSet> expressionSets;
        public TemplateData mainEntryPoint;

        public static TemplateLoader RuntimeCompileModule(Module module, IEnumerable<Type> dynamicRoots = null) {
            return new TemplateLoader();
        }

        public static TemplateLoader RuntimeCompile(Type rootType) {
            
            ModuleSystem.ParseTemplates(rootType);
            
            TemplateCompiler2 compiler = new TemplateCompiler2();

            TemplateLoader loader = new TemplateLoader();

            compiler.onTemplateCompiled -= CompileResult;
            
            loader.templateDataMap = new Dictionary<Type, TemplateData>();

            compiler.onTemplateCompiled += CompileResult;
            
            compiler.CompileTemplate(TypeProcessor.GetProcessedType(rootType));
            
            compiler.onTemplateCompiled -= CompileResult;
            
            loader.mainEntryPoint = loader.templateDataMap[rootType];
            
            return loader;

            // ideally this is just enqueued on worker thread and done via producer / consumer
            void CompileResult(TemplateExpressionSet set) {
                // todo -- generic w/ tag name might need to be unique
                TemplateData templateData = new TemplateData(set.processedType.tagName);

                try {
                    // could skip entry point to save time if not wanted (usually only 1 entry fn is used)
                    templateData.entry = set.entryPoint.TryCompileWithoutClosure<Func<ElementSystem, UIElement>>();

                    BlockExpression block = (BlockExpression) set.hydratePoint.Body;
                    if (block.Expressions.Count == 0) {
                        templateData.hydrate = (system) => { };
                    }
                    else {
                        templateData.hydrate = set.hydratePoint.TryCompileWithoutClosure<Action<ElementSystem>>();
                    }

                    templateData.elements = new Action<ElementSystem>[set.elementTemplates.Length];
                    templateData.bindings = new Action<LinqBindingNode>[set.bindings.Length];
                    templateData.inputEventHandlers = new Action<LinqBindingNode, InputEventHolder>[set.inputEventHandlers.Length];

                    for (int j = 0; j < set.elementTemplates.Length; j++) {
                        templateData.elements[j] = set.elementTemplates[j].expression.TryCompileWithoutClosure<Action<ElementSystem>>();
                    }

                    for (int j = 0; j < set.bindings.Length; j++) {
                        // could try to fast compile first then do slow if failed. currently slow because dont know if user did something crazy and slow is safer
                        templateData.bindings[j] = (Action<LinqBindingNode>) set.bindings[j].expression.Compile();
                    }
                    
                    for (int j = 0; j < set.inputEventHandlers.Length; j++) {
                        // could try to fast compile first then do slow if failed. currently slow because dont know if user did something crazy and slow is safer
                        templateData.inputEventHandlers[j] = (Action<LinqBindingNode, InputEventHolder>) set.inputEventHandlers[j].expression.Compile();
                    }

                    GCHandle.Alloc(templateData); // unity issue where unreferenced type gets gc'd causing crash when they re-scan types

                }
                catch (Exception e) {
                    Debug.Log(e);
                }

                loader.templateDataMap.Add(set.processedType.rawType, templateData);
            }

            
        }

        public UIElement LoadRoot(Application application, UIView rootView, TemplateData templateData) {
            return application.elementSystem.CreateEntryPoint(rootView, templateData);
        }

        public static string InitTemplate(string appName, string templateMap, string mainEntryPoint) {
            return 
$@"using UIForia.Compilers;
using System;
using System.Collections.Generic;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {{

    public partial class Generated_{appName} : TemplateLoader {{

        public Generated_{appName}() {{

            templateDataMap = new Dictionary<Type, TemplateData>() {{
{templateMap}
            }};

            mainEntryPoint = {mainEntryPoint};
        }}

    }}

}}";
        }

        public static string TemplateFile(string appName, string templateName, string templateInfo) {
            return
$@"using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {{

    public partial class Generated_{appName} : TemplateLoader {{
    
        public static readonly {nameof(TemplateData)} {templateName} = {templateInfo}

    }}

}}";
        }

        private struct TemplatePair {

            public Type type;
            public string templateName;

        }

        public static void PreCompile(string outputPath, string appName, Type rootType) {

            IndentedStringBuilder builder = new IndentedStringBuilder(1024);

            ModuleSystem.ParseTemplates(rootType);
            
            TemplateCompiler2 compiler = new TemplateCompiler2();

            List<TemplatePair> templateNames = new List<TemplatePair>();
            
            compiler.onTemplateCompiled += CompileResult;
            
            compiler.CompileTemplate(TypeProcessor.GetProcessedType(rootType));
            
            compiler.onTemplateCompiled -= CompileResult;
            
            void CompileResult(TemplateExpressionSet set) {
                
                builder.Clear();

                ProcessedType processedType = set.processedType;
                string templateName = "template_" + set.GetGUID();
                templateNames.Add(new TemplatePair() {
                    type = processedType.rawType,
                    templateName = templateName
                });
                
                set.ToCSharpCode(builder);

                string data = TemplateFile(appName, templateName, builder.ToString());
                
                string moduleTypeName = processedType.module.GetType().GetTypeName();
                
                string fileName;
                if (processedType.rawType.IsGenericType) {
                    string typeName = processedType.rawType.GetTypeName();
                    int idx = typeName.IndexOf('<');
                    
                    fileName = processedType.tagName + typeName.Substring(idx).InlineReplace('<', '(').InlineReplace('>', ')');
                }
                else {
                    fileName = processedType.tagName;
                }
                
                string file = Path.Combine(outputPath, "Modules", moduleTypeName, fileName + "_generated.cs");
                
                Directory.CreateDirectory(Path.GetDirectoryName(file));

                File.WriteAllText(file, data);
            }
            
            builder.Clear();
            builder.Indent();
            builder.Indent();
            builder.Indent();
            builder.Indent();
            
            for (int i = 0; i < templateNames.Count; i++) {
                builder.Append("{ typeof(");
                builder.AppendInline(templateNames[i].type.GetTypeName());
                builder.AppendInline("), ");
                builder.AppendInline(templateNames[i].templateName);
                builder.AppendInline("}");
                if (i != templateNames.Count - 1) {
                    builder.AppendInline(",\n");
                }
            }

            File.WriteAllText(Path.Combine(outputPath, "init_generated.cs"), InitTemplate(appName, builder.ToString(), "template_" + compiler.CompileTemplate(TypeProcessor.GetProcessedType(rootType)).GetGUID()));

        }


    }
    

}