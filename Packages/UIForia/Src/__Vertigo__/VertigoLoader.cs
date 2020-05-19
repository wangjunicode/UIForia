using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using FastExpressionCompiler;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Systems;
using UIForia.Util;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public struct CompilationId {

        public readonly int id;

        internal CompilationId(int id) {
            this.id = id;
        }

    }

    public class Compilation : IDisposable {

        public readonly CompilationId compilationId;
        public readonly Type entryType;
        public readonly Diagnostics diagnostics;

        public Compilation(CompilationId compilationId, Type entryType, Diagnostics diagnostics) {
            this.compilationId = compilationId;
            this.entryType = entryType;
            this.diagnostics = diagnostics;
        }

        public void Dispose() { }

    }

    public class VertigoLoader {

        private static int idGenerator;

        private static readonly List<Compilation> compilations = new List<Compilation>();
        private static readonly Dictionary<string, TemplateFileShell> s_ParseCacheMap = new Dictionary<string, TemplateFileShell>(64);

        public static CompilationId Compile(Type type, Diagnostics diagnostics) {

            ModuleSystem.Initialize();

            if (!type.IsSubclassOf(typeof(UIElement))) {
                diagnostics.LogError($"Cannot compile entry type {type.GetTypeName()} because it is not a subclass of {typeof(UIElement).GetTypeName()}");
                return new CompilationId(-1);
            }

            CompilationId retn = new CompilationId(idGenerator++);

            Compilation result = new Compilation(retn, type, diagnostics);

            BeginCompile(result);

            return retn;

        }

        private static void BeginCompile(Compilation result) {

            ProcessedType processedEntryType = TypeProcessor.GetProcessedType(result.entryType);

            if (processedEntryType == null) {
                result.diagnostics.LogError($"Cannot find processed type for entry type {result.entryType.GetTypeName()}");
                return;
            }

            if (processedEntryType.IsUnresolvedGeneric) {
                result.diagnostics.LogError($"Cannot use an open generic type as an application entry point. {result.entryType.GetTypeName()} is invalid.");
                return;
            }

            if (processedEntryType.module == null) {
                result.diagnostics.LogError($"{result.entryType.GetTypeName()} had no module associated with it.");
                return;
            }

            Module[] moduleList = processedEntryType.module.GetFlattenedDependencyTree();

            StructList<StyleFile> styleFiles = new StructList<StyleFile>(64);

            JobHandle styleGather = new GatherStyleFiles_Managed(moduleList, styleFiles, UnityEngine.Application.dataPath).Schedule();
            // new GatherStyleFiles_Managed(moduleList, styleFiles, UnityEngine.Application.dataPath).Execute();

            string tempFileLocation = EditorPrefs.GetString("UIFORIA_TEMPLATE_PARSE_CACHE");

            // need UIFORIA parsecache or something
            if (tempFileLocation != null && File.Exists(tempFileLocation)) { }
            else {
                tempFileLocation = FileUtil.GetUniqueTempPathInProject();
                EditorPrefs.SetString("UIFORIA_TEMPLATE_PARSE_CACHE", tempFileLocation);
            }

            LightList<TemplateFileShell> templatesToParse = LightList<TemplateFileShell>.Get();

            // needed? maybe not
            s_ParseCacheMap.Clear();

            for (int i = 0; i < moduleList.Length; i++) {

                Module module = moduleList[i];

                RebuildParseCache(module, tempFileLocation);

                for (int j = 0; j < module.templateShells.size; j++) {

                    if (!s_ParseCacheMap.TryGetValue(module.templateShells[j].filePath, out TemplateFileShell cachedFile)) {
                        templatesToParse.Add(module.templateShells[j]);
                    }

                }

            }

            // todo this could be multithreaded pretty easily
            // would want to sort by parser type etc

            TemplateParser parser = TemplateParser.GetParserForFileType("xml");

            TemplateFileShellBuilder builder = new TemplateFileShellBuilder();

            for (int i = 0; i < templatesToParse.size; i++) {

                TemplateFileShell templateFile = templatesToParse.array[i];

                string filePath = templatesToParse.array[i].filePath;

                if (!File.Exists(filePath)) {
                    result.diagnostics.LogError("Unable to find template file at path: " + filePath);
                    continue;
                }

                string contents = File.ReadAllText(filePath);

                parser.Setup(filePath, result.diagnostics);

                templateFile.successfullyParsed = parser.TryParse(contents, builder);

                if (templateFile.successfullyParsed) {
                    builder.Build(templateFile);
                    s_ParseCacheMap[filePath] = templateFile;
                }

                parser.Reset();

            }

            for (int i = 0; i < moduleList.Length; i++) {
                ValidateModuleTemplates(moduleList[i]);
            }

            SerializeParsedTemplates(moduleList);

            // start parsing style sheets

            bool allModulesValid = true;
            for (int i = 0; i < moduleList.Length; i++) {
                if (moduleList[i].GetDiagnostics().HasErrors) {
                    allModulesValid = false;
                    break;
                }
            }

            if (allModulesValid) {
                CompileTemplates(processedEntryType);
            }
            styleGather.Complete();
        }

        private struct TemplatePair {

            public Type type;
            public string templateName;

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

        private static void CompileTemplates(ProcessedType processedEntryType) {

            string appName = "DefaultApp";
            string path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGeneratedNew", appName));

            TemplateCompiler2 compiler = new TemplateCompiler2();
            IndentedStringBuilder builder = new IndentedStringBuilder(1024);
            List<TemplatePair> templateNames = new List<TemplatePair>();

            Action<TemplateExpressionSet> callback = (set) => {
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

                Debug.Log(data);
                //  string file = Path.Combine(path, "Modules", moduleTypeName, fileName + "_generated.cs");

                // Directory.CreateDirectory(Path.GetDirectoryName(file));
                // File.WriteAllText(file, data);

                // builder.Clear();
                // builder.Indent();
                // builder.Indent();
                // builder.Indent();
                // builder.Indent();
                //
                // for (int i = 0; i < templateNames.Count; i++) {
                //     builder.Append("{ typeof(");
                //     builder.AppendInline(templateNames[i].type.GetTypeName());
                //     builder.AppendInline("), ");
                //     builder.AppendInline(templateNames[i].templateName);
                //     builder.AppendInline("}");
                //     if (i != templateNames.Count - 1) {
                //         builder.AppendInline(",\n");
                //     }
                // }
                //
                // File.WriteAllText(Path.Combine(outputPath, "init_generated.cs"), InitTemplate(appName, builder.ToString(), "template_" + compiler.CompileTemplate(TypeProcessor.GetProcessedType(rootType)).GetGUID()));

            };

            compiler.onTemplateCompiled += callback;

            compiler.CompileTemplate(processedEntryType);

            compiler.onTemplateCompiled -= callback;

        }

        private static void OnTemplateCompiled(TemplateExpressionSet obj) {
            throw new NotImplementedException();
        }

        private static void ValidateModuleTemplates(Module module) {
            // totally parallel, todo -- make this real
            new ResolveTagNamesAndValidateTemplates_ManagedJob(module).Execute();
        }

        private static void RebuildParseCache(Module module, string cacheLocation) {
            return; // todo -- verify this 
            string cachepath = cacheLocation + "/" + module.location + "/cache.json";
            if (File.Exists(cachepath)) {
                TemplateFileShell[] fileShells = null;
                try {
                    fileShells = JsonUtility.FromJson<TemplateFileShell[]>(cachepath);

                    for (int j = 0; j < fileShells.Length; j++) {

                        if (!File.Exists(module.templateShells[j].filePath)) {
                            continue;
                        }

                        DateTime writeTime = File.GetLastWriteTime(module.templateShells[j].filePath);

                        if (fileShells[j].lastWriteTime != writeTime) {
                            continue;
                        }

                        s_ParseCacheMap.Add(fileShells[j].filePath, fileShells[j]);
                    }

                }
                catch (Exception e) {
                    Debug.Log(e);
                }
            }

        }

        private static void SerializeParsedTemplates(Module[] modules) {
// kick off job to update the parsed templates
// can be parallel, 1 job per module
// 1 file per module or 1 file per template? will have to profile but 1 per module i think
            for (int i = 0; i < modules.Length;
                i++) {
                new SerializeTemplateParseResults().Schedule();
            }
        }

        private struct SerializeTemplateParseResults : IJob {

            public SerializeTemplateParseResults(Module module) { }

            public void Execute() { }

        }

        public static bool IsCompleted(CompilationId compilationId) {
            return false;
        }

        public static Compilation GetCompilationResult(CompilationId id) {
            if (id.id < 0 || !IsCompleted(id)) {
                return null;
            }

            for (int i = 0; i < compilations.Count;
                i++) {
                if (compilations[i].compilationId.id == id.id) {
                    Compilation compilation = compilations[i];
                    compilations[i] = compilations[compilations.Count - 1];
                    return compilation;
                }
            }

            return null;
        }

    }

    public struct StyleFile {

        public Module module;
        public string filePath;
        public DateTime lastWriteTime;
        public string contents;

    }

}