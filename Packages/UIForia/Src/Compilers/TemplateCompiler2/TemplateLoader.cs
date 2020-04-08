using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FastExpressionCompiler;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateLoader {

        public TemplateData[] templateData;
        public Dictionary<Type, TemplateData> templateDataMap;
        
        private LightList<TemplateExpressionSet> expressionSets;
        
        public static TemplateLoader RuntimeCompileModule(Module module, IEnumerable<Type> dynamicRoots = null) {
            return new TemplateLoader();
        }

        public static TemplateLoader RuntimeCompile(Type rootType) {
            
            ProcessedType[] typesToCompile = ModuleSystem.GetTemplateTypes(rootType);
            
            TemplateCompiler2 compiler = new TemplateCompiler2();

            TemplateLoader loader = new TemplateLoader() {
                expressionSets = new LightList<TemplateExpressionSet>(typesToCompile.Length + 16)
            };

            string output = "";
            
            for (int i = 0; i < typesToCompile.Length; i++) {
                
                // todo -- handle discovered generics
                TemplateExpressionSet expressionSet = compiler.CompileTemplate(typesToCompile[i]);

                loader.expressionSets.Add(expressionSet);

                output += expressionSet.ToCSharpCode(new IndentedStringBuilder(512));

            }
            
            File.WriteAllText(UnityEngine.Application.dataPath + "/tmp.txt", output);
            
            loader.templateData = new TemplateData[loader.expressionSets.size];
            
            for (int i = 0; i < loader.expressionSets.size; i++) {
                
                // if changed and not in cache -> compile
                
                TemplateExpressionSet set = loader.expressionSets.array[i];
                
                // todo -- generic w/ tag name might need to be unique
                TemplateData templateData = new TemplateData(set.processedType.tagName);

                try {
                    templateData.entry = set.entryPoint.TryCompileWithoutClosure<Func<ElementSystem, UIElement>>();

                    templateData.hydrate = set.hydratePoint.TryCompileWithoutClosure<Action<ElementSystem>>();

                    templateData.elements = new Action<ElementSystem>[set.elementTemplates.Length];
                    
                    for (int j = 0; j < set.elementTemplates.Length; j++) {
                        templateData.elements[j] = set.elementTemplates[j].expression.TryCompileWithoutClosure<Action<ElementSystem>>();
                    }

                    GCHandle.Alloc(templateData); // unity issue where unreferenced type gets gc'd causing crash when they re-scan types

                }
                catch (Exception e) {
                    Debug.Log(e);
                }

                loader.templateData[i] = templateData;
                
            }
            
            return loader;
            
        }

        public void LoadRoot(Application application, UIView rootView) {
            application.elementSystem.CreateEntryPoint(rootView, templateData[0]);
        }

        public TemplateData GetRootTemplateData() {
            return templateData[0];
        }

        private static TemplateLoader CreateLoader(Type rootType) {
            ProcessedType[] typesToCompile = ModuleSystem.GetTemplateTypes(rootType);
            
            TemplateCompiler2 compiler = new TemplateCompiler2();

            TemplateLoader loader = new TemplateLoader() {
                expressionSets = new LightList<TemplateExpressionSet>(typesToCompile.Length + 16)
            };
            
            for (int i = 0; i < typesToCompile.Length; i++) {
                
                // todo -- handle discovered generics
                TemplateExpressionSet expressionSet = compiler.CompileTemplate(typesToCompile[i]);

                loader.expressionSets.Add(expressionSet);

            }

            return loader;
        }
        
        public static string InitTemplate(string appName, string templates, string templateMap) {
            return 
$@"using UIForia.Compilers;
using System;
using System.Collections.Generic;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {{

    public partial class Generated_{appName} : TemplateLoader {{

        public Generated_{appName}() {{
            templateData = new[] {{
{templates}
            }};

            templateDataMap = new Dictionary<Type, TemplateData>() {{
{templateMap}
            }};
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

            TemplateLoader loader = CreateLoader(rootType);

            IndentedStringBuilder builder = new IndentedStringBuilder(1024);
           
            TemplatePair[] templateNames = new TemplatePair[loader.expressionSets.size];
            for (int i = 0; i < loader.expressionSets.size; i++) {
                builder.Clear();

                ProcessedType processedType = loader.expressionSets[i].processedType;
                string templateName = "template_" + Guid.NewGuid().ToString().Replace("-", "_");
                templateNames[i] = new TemplatePair() {
                    type = processedType.rawType,
                    templateName = templateName
                };
                
                loader.expressionSets[i].ToCSharpCode(builder);

                string data = TemplateFile(appName, templateName, builder.ToString());
                
                string fileName;
                
                loader.expressionSets[i].ToCSharpCode(builder);
                string moduleTypeName = processedType.module.GetType().GetTypeName();
                
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
            
            
            for (int i = 0; i < templateNames.Length; i++) {
                builder.Append(templateNames[i].templateName);
                if (i != templateNames.Length - 1) {
                    builder.AppendInline(",\n");
                }
            }

            string templateArray = builder.ToString();
            builder.Clear();
            builder.Indent();
            builder.Indent();
            builder.Indent();
            builder.Indent();
            
            for (int i = 0; i < templateNames.Length; i++) {
                builder.Append("{ typeof(");
                builder.AppendInline(templateNames[i].type.GetTypeName());
                builder.AppendInline("), ");
                builder.AppendInline(templateNames[i].templateName);
                builder.AppendInline("}");
                if (i != templateNames.Length - 1) {
                    builder.AppendInline(",\n");
                }
            }

            File.WriteAllText(Path.Combine(outputPath, "init_generated.cs"), InitTemplate(appName, templateArray, builder.ToString()));

        }


    }
    
    
    public interface ITemplateLoader2 {

        void Load();

    }

}