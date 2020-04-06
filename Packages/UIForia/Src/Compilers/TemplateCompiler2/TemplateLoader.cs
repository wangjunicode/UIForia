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

        private LightList<TemplateExpressionSet> expressionSets;
        private TemplateData[] templateData;
        
        public void Load() {
                    
        }

        public static TemplateLoader FromPrecompiled(Type module) {
            return new TemplateLoader();
        }

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

        private static readonly string TemplateDataTypeName = typeof(TemplateData).GetTypeName();

        public static void PreCompile(string outputPath, string appName, Type rootType) {

            TemplateLoader loader = CreateLoader(rootType);

            IndentedStringBuilder builder = new IndentedStringBuilder(1024);
            
            for (int i = 0; i < loader.expressionSets.size; i++) {
                builder.Clear();
                
                ProcessedType processedType = loader.expressionSets[i].processedType;
                
                builder.Append("namespace UIForia.GeneratedApplication {");
                builder.NewLine();
                builder.NewLine();
                builder.Indent();
                builder.Append("public partial class ");
                builder.AppendInline("Generated_");
                builder.AppendInline(appName);
                builder.AppendInline(" {");
                
                builder.Indent();
                builder.NewLine();
                builder.NewLine();
                // stringBuilder.Append("// " + processedType.templatePath);
                builder.Append("public static readonly ");
                builder.AppendInline(TemplateDataTypeName);
                builder.AppendInline(" template_");
                builder.AppendInline(Guid.NewGuid().ToString().Replace("-", "_"));
                builder.AppendInline(" = ");
                builder.NewLine();
                builder.Indent();

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
                
                string file = Path.Combine(outputPath, "Modules", moduleTypeName, fileName + ".generated.cs");
                
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                builder.NewLine();
                builder.Outdent();
                builder.Outdent();

                builder.Append("}");
                builder.NewLine();
                builder.NewLine();
                builder.Outdent();
                builder.Append("}");
                File.WriteAllText(file, builder.ToString());
            }

        }

    }

}