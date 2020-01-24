using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Util;
using UnityEditor;

namespace UIForia {

    public class TemplateCodeGenerator {

        private static readonly string s_Indent8 = new string(' ', 8);
        private static readonly string s_Indent12 = new string(' ', 12);
        private static readonly string s_Indent16 = new string(' ', 16);
        private static readonly string s_Indent20 = new string(' ', 20);

        private StyleSheetImporter styleSheetImporter;

        public static bool Generate(Type type, TemplateSettings templateSettings) {
            CompiledTemplateData compiledTemplateData = TemplateCompiler.CompileTemplates(type, templateSettings);

            string path = templateSettings.outputPath;
            string extension = "." + templateSettings.codeFileExtension;

            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            string styleFilePaths = GenerateStyleCode(compiledTemplateData);

            GenerateTemplateCode(path, extension, compiledTemplateData);

            GenerateInitCode(path, extension, compiledTemplateData, styleFilePaths);

            return true;
        }

        private static void GenerateInitCode(string path, string extension, CompiledTemplateData compiledTemplateData, string styleFilePaths) {
            string template = TemplateConstants.InitSource;
            template = template.Replace("::APPNAME::", compiledTemplateData.templateSettings.StrippedApplicationName);
            template = template.Replace("::TEMPLATE_CODE::", GenerateTemplateLoadCode(compiledTemplateData));
            template = template.Replace("::STYLE_FILE_PATHS::", styleFilePaths);
            template = template.Replace("::TEMPLATE_META_CODE::", GenerateTemplateMetaDataCode(compiledTemplateData));
            template = template.Replace("::SLOT_CODE::", GenerateSlotCode(compiledTemplateData));
            template = template.Replace("::BINDING_CODE::", GenerateBindingCode(compiledTemplateData));
            template = template.Replace("::ELEMENT_CONSTRUCTORS::", GenerateElementConstructors());

            string initPath = Path.Combine(path, "__init" + extension);
            Directory.CreateDirectory(Path.GetDirectoryName(initPath));

            if (File.Exists(initPath)) {
                File.Delete(initPath);
            }

            File.WriteAllText(initPath, template);
            AssetDatabase.Refresh();
        }

        private static string GenerateTemplateLoadCode(CompiledTemplateData compiledTemplateData) {
            LightList<CompiledTemplate> compiledTemplates = compiledTemplateData.compiledTemplates;
            StringBuilder builder = new StringBuilder(2048);

            builder.AppendLine($"{s_Indent12}Func<UIElement, TemplateScope, UIElement>[] templates = new Func<{nameof(UIElement)}, {nameof(TemplateScope)}, {nameof(UIElement)}>[{compiledTemplates.size}];");

            for (int i = 0; i < compiledTemplates.size; i++) {
                builder.AppendLine($"{s_Indent12}templates[{i}] = Template_{compiledTemplates.array[i].guid.ToString()}; // {compiledTemplates.array[i].filePath}");
            }

            builder.AppendLine($"{s_Indent12}return templates;");
            return builder.ToString();
        }

        private static string GenerateTemplateMetaDataCode(CompiledTemplateData compiledTemplateData) {
            StringBuilder builder = new StringBuilder(2048);
            LightList<CompiledTemplate> compiledTemplates = compiledTemplateData.compiledTemplates;

            builder.AppendLine($"{s_Indent12}{nameof(TemplateMetaData)}[] templateData = new {nameof(TemplateMetaData)}[{compiledTemplates.size}];");
            builder.AppendLine($"{s_Indent12}{nameof(TemplateMetaData)} template;");
            builder.AppendLine($"{s_Indent12}{nameof(StyleSheetReference)}[] styleSheetRefs;");

            for (int i = 0; i < compiledTemplates.size; i++) {
                TemplateMetaData meta = compiledTemplates[i].templateMetaData;

                if (meta.styleReferences != null && meta.styleReferences.Length > 0) {
                    builder.Append(s_Indent12);
                    builder.Append("styleSheetRefs = new StyleSheetReference[");
                    builder.Append(meta.styleReferences.Length);
                    builder.AppendLine("];");

                    for (int j = 0; j < meta.styleReferences.Length; j++) {
                        StyleSheetReference sheetReference = meta.styleReferences[j];
                        builder.Append(s_Indent12);
                        builder.Append("styleSheetRefs[");
                        builder.Append(j);
                        builder.Append("] = new StyleSheetReference(");
                        builder.Append(sheetReference.alias ?? "null");
                        builder.Append(", sheetMap[@\"");
                        builder.Append(sheetReference.styleSheet.path);
                        builder.AppendLine("\"]);");
                    }

                    builder.AppendLine($"{s_Indent12}template = new {nameof(TemplateMetaData)}({compiledTemplates[i].templateId}, @\"{compiledTemplates[i].filePath}\", styleMap, styleSheetRefs);");
                }
                else {
                    builder.AppendLine($"{s_Indent12}template = new {nameof(TemplateMetaData)}({compiledTemplates[i].templateId}, @\"{compiledTemplates[i].filePath}\", styleMap, null);");
                }

                builder.AppendLine($"{s_Indent12}template.BuildSearchMap();");
                builder.AppendLine($"{s_Indent12}templateData[{i}] = template;");
            }

            builder.AppendLine($"{s_Indent12}return templateData;");

            return builder.ToString();
        }

        private static string GenerateSlotCode(CompiledTemplateData compiledTemplateData) {
            StringBuilder builder = new StringBuilder(2048);

            LightList<CompiledSlot> compiledSlots = compiledTemplateData.compiledSlots;

            builder.AppendLine($"{s_Indent12}Func<UIElement, UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, UIElement, TemplateScope, UIElement>[{compiledSlots.size}];");

            for (int i = 0; i < compiledSlots.size; i++) {
                builder.AppendLine($"{s_Indent12}slots[{i}] = {compiledSlots.array[i].GetVariableName()};");
            }

            builder.AppendLine($"{s_Indent12}return slots;");
            return builder.ToString();
        }

        private static string GenerateBindingCode(CompiledTemplateData compiledTemplateData) {
            StringBuilder builder = new StringBuilder(2048);
            LightList<CompiledBinding> compiledBindings = compiledTemplateData.compiledBindings;
            builder.AppendLine($"{s_Indent12}Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[{compiledBindings.size}];");

            for (int i = 0; i < compiledBindings.size; i++) {
                builder.AppendLine($"{s_Indent12}bindings[{i}] = Binding_{compiledBindings.array[i].bindingType}_{compiledBindings.array[i].guid};");
            }

            builder.AppendLine($"{s_Indent12}return bindings;");
            return builder.ToString();
        }

        private static string GenerateStyleCode(CompiledTemplateData compiledTemplateData) {
            StyleSheet[] sheets = compiledTemplateData.styleImporter.GetImportedStyleSheets();

            string styleFilePathArray = "";

            if (sheets.Length > 0) {
                string streamingAssetPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia", compiledTemplateData.templateSettings.StrippedApplicationName);

                if (Directory.Exists(streamingAssetPath)) {
                    Directory.Delete(streamingAssetPath, true);
                }

                Directory.CreateDirectory(streamingAssetPath);

                for (int i = 0; i < sheets.Length; i++) {
                    string filepath = Path.Combine(streamingAssetPath, sheets[i].path);
                    string directory = Path.GetDirectoryName(filepath);
                    if (!Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }

                    styleFilePathArray += s_Indent12 + "@\"" + filepath + "\",\n";

                    File.WriteAllText(filepath, sheets[i].source);
                }
            }

            return styleFilePathArray;
        }

        private static string GenerateElementConstructors() {
            StringBuilder builder = new StringBuilder(2048);

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {
                if (kvp.Key.IsAbstract || kvp.Value.references == 0 || kvp.Value.id < 0) {
                    continue;
                }

                builder.Append(s_Indent16);
                builder.Append("case ");
                builder.Append(kvp.Value.id);
                builder.AppendLine(":");
                builder.Append(s_Indent20);
                builder.Append("return new ");
                TypeNameGenerator.GetTypeName(kvp.Key, builder);
                builder.Append("();");
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static int genericCounter = 0;

        private static void GenerateTemplateCode(string path, string extension, CompiledTemplateData compiledTemplateData) {
            TemplateSettings templateSettings = compiledTemplateData.templateSettings;

            for (int i = 0; i < compiledTemplateData.compiledTemplates.size; i++) {
                CompiledTemplate compiled = compiledTemplateData.compiledTemplates.array[i];

                string file = compiled.filePath;

                bool isGeneric = false;
                if (compiled.elementType.rawType.IsGenericType) {
                    isGeneric = true;
                    file = Path.ChangeExtension(file, "");
                    file = file.Substring(0, file.Length - 1);
                    file += "__" + (genericCounter++);
                }

                if (!string.IsNullOrEmpty(compiled.templateName)) {
                    file = Path.ChangeExtension(file, "");
                    file = file.Substring(0, file.Length - 1);
                    file += "__" + compiled.templateName;
                    file = Path.Combine(path, Path.ChangeExtension(file, extension));
                }
                else {
                    file = Path.Combine(path, Path.ChangeExtension(file, extension));
                }

                Directory.CreateDirectory(Path.GetDirectoryName(file));

                string bindingCode = string.Empty;
                string slotCode = string.Empty;

                CompiledTemplate compiledTemplate = compiledTemplateData.compiledTemplates[i];

                LightList<CompiledBinding> compiledBindings = compiledTemplateData.compiledBindings;
                LightList<CompiledSlot> compiledSlots = compiledTemplateData.compiledSlots;

                // todo -- optimize search or sort by file name at least
                for (int b = 0; b < compiledBindings.size; b++) {
                    CompiledBinding binding = compiledBindings[b];
                    // todo -- needs to be unique by generic type. probably need an actual back reference & compare
                    if (binding.filePath == compiledTemplate.filePath && binding.templateName == compiled.templateName) {
                        bindingCode += $"// binding id = {binding.bindingId}";
                        bindingCode += $"\n{s_Indent8}public Action<UIElement, UIElement> Binding_{compiledBindings.array[b].bindingType}_{binding.guid} = ";
                        bindingCode += binding.bindingFn.ToTemplateBodyFunction();
                        bindingCode += "\n";
                    }
                }

                // todo -- optimize search or sort by file name at least
                for (int s = 0; s < compiledSlots.size; s++) {
                    CompiledSlot compiledSlot = compiledSlots[s];

                    if (compiledSlot.filePath == compiledTemplate.filePath && compiledSlot.templateName == compiled.templateName) {
                        slotCode += $"\n{s_Indent8}// {compiledSlot.GetComment()}";
                        slotCode += $"\n{s_Indent8}public Func<UIElement, UIElement, TemplateScope, UIElement> {compiledSlot.GetVariableName()} = ";
                        slotCode += compiledSlot.templateFn.ToTemplateBodyFunction();
                        slotCode += "\n";
                    }
                }

                string templateBody = compiledTemplate.templateFn.ToTemplateBodyFunction();
                string template = TemplateConstants.TemplateSource;
                template = template.Replace("::GUID::", compiledTemplate.guid.ToString());
                template = template.Replace("::CODE::", templateBody);
                template = template.Replace("::BINDINGS::", bindingCode);
                template = template.Replace("::SLOTS::", slotCode);
                template = template.Replace("::APPNAME::", templateSettings.StrippedApplicationName);
                File.WriteAllText(file, template);
            }
        }

    }

}