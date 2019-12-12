using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Rendering;
using UIForia.Util;
using UnityEditor;
using UnityEngine;

namespace UIForia {

    public class PreCompiledTemplateData : CompiledTemplateData {

        private static readonly string s_Indent8 = new string(' ', 8);
        private static readonly string s_Indent12 = new string(' ', 12);
        private static readonly string s_Indent16 = new string(' ', 16);
        private static readonly string s_Indent20 = new string(' ', 20);

        public PreCompiledTemplateData(TemplateSettings templateSettings) : base(templateSettings) { }

        public override void LoadTemplates() {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName(templateSettings.assemblyName);
            Type type = assembly.GetType("UIForia.Generated.UIForiaGeneratedTemplates_" + templateSettings.StrippedApplicationName);

            try {
                ITemplateLoader loader = (ITemplateLoader) Activator.CreateInstance(type);
                string[] files = loader.StyleFilePaths;

                styleImporter.Reset(); // reset because in testing we will already have parsed files, nuke these

                LightList<UIStyleGroupContainer> styleList = new LightList<UIStyleGroupContainer>(128);

                for (int i = 0; i < files.Length; i++) {
                    StyleSheet sheet = styleImporter.ImportStyleSheetFromFile(files[i]);
                    styleList.EnsureAdditionalCapacity(sheet.styleGroupContainers.Length);
                    for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                        styleList.array[styleList.size++] = sheet.styleGroupContainers[j];
                    }
                }

                templates = loader.LoadTemplates();
                slots = loader.LoadSlots();
                bindings = loader.LoadBindings();
                templateMetaData = loader.LoadTemplateMetaData(styleList.array);

                for (int i = 0; i < templateMetaData.Length; i++) {
                    templateMetaData[i].compiledTemplateData = this;
                }

                constructElement = loader.ConstructElement;
                
            }
            catch (Exception e) {
                throw e;
            }
        }

        public void GenerateCode() {
            string path = templateSettings.outputPath;
            string extension = "." + templateSettings.codeFileExtension;

            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            for (int i = 0; i < compiledTemplates.size; i++) {
                GenerateTemplateCode(path, i, extension);
            }

            StyleSheet[] sheets = styleImporter.GetImportedStyleSheets();

            string styleFilePathArray = "";

            if (sheets.Length > 0) {
                string streamingAssetPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia", templateSettings.StrippedApplicationName);

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


            string loadFn = @"
using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_::APPNAME:: : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {
::STYLE_FILE_PATHS::
        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            ::TEMPLATE_CODE::
        }

        public TemplateMetaData[] LoadTemplateMetaData(UIStyleGroupContainer[] styleMap) {
            ::TEMPLATE_META_CODE::
        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            ::BINDING_CODE::
        }

        public Func<UIElement, TemplateScope, UIElement>[] LoadSlots() {
            ::SLOT_CODE::
        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
::ELEMENT_CONSTRUCTORS::
            }
            return null;
        }

    }

}".Trim();

            loadFn = loadFn.Replace("::STYLE_FILE_PATHS::", styleFilePathArray);

            loadFn = loadFn.Replace("::APPNAME::", templateSettings.StrippedApplicationName);

            // Print Template Code
            string initCode = $"Func<UIElement, TemplateScope, UIElement>[] templates = new Func<{nameof(UIElement)}, {nameof(TemplateScope)}, {nameof(UIElement)}>[{compiledTemplates.size}];";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(initCode);

            for (int i = 0; i < compiledTemplates.size; i++) {
                builder.AppendLine($"{s_Indent12}templates[{i}] = Template_{compiledTemplates.array[i].guid.ToString()}; // {compiledTemplates.array[i].filePath}");
            }

            builder.AppendLine($"{s_Indent12}return templates;");

            loadFn = loadFn.Replace("::TEMPLATE_CODE::", builder.ToString());
            builder.Clear();

            // Print Template Meta Data Code
            builder.AppendLine($"{nameof(TemplateMetaData)}[] templateData = new {nameof(TemplateMetaData)}[{compiledTemplates.size}];");
            builder.AppendLine($"{s_Indent12}{nameof(TemplateMetaData)} template;");

            for (int i = 0; i < compiledTemplates.size; i++) {
                builder.AppendLine($"{s_Indent12}template = new {nameof(TemplateMetaData)}({compiledTemplates[i].templateId}, \"{compiledTemplates[i].filePath}\", styleMap, null);");
                // todo -- reference style ids
                // todo -- maybe other references?
                // referencedStyles[0] = new StyleReference(styleSheets[4], "alias");
                // referencedStyles[1] = new StyleReference(styleSheets[5], "alias");
                builder.AppendLine($"{s_Indent12}templateData[{i}] = template;");
            }

            builder.AppendLine($"{s_Indent12}return templateData;");

            loadFn = loadFn.Replace("::TEMPLATE_META_CODE::", builder.ToString());

            builder.Clear();

            // Print Slot Code
            builder.AppendLine($"Func<UIElement, TemplateScope, UIElement>[] slots = new Func<{nameof(UIElement)}, {nameof(TemplateScope)}, {nameof(UIElement)}>[{compiledSlots.size}];");

            for (int i = 0; i < compiledSlots.size; i++) {
                builder.AppendLine($"{s_Indent12}slots[{i}] = {compiledSlots.array[i].GetVariableName()};");
            }

            builder.AppendLine($"{s_Indent12}return slots;");
            loadFn = loadFn.Replace("::SLOT_CODE::", builder.ToString());

            builder.Clear();

            // Print Binding Code
            builder.AppendLine($"Action<UIElement, UIElement>[] bindings = new Action<{nameof(UIElement)}, {nameof(UIElement)}>[{compiledBindings.size}];");

            for (int i = 0; i < compiledBindings.size; i++) {
                builder.AppendLine($"{s_Indent12}bindings[{i}] = Binding_{compiledBindings.array[i].bindingType}_{compiledBindings.array[i].guid};");
            }

            builder.AppendLine($"{s_Indent12}return bindings;");

            loadFn = loadFn.Replace("::BINDING_CODE::", builder.ToString());

            builder.Clear();

            foreach (KeyValuePair<Type, ProcessedType> kvp in TypeProcessor.typeMap) {
                if (kvp.Key.IsAbstract || kvp.Value.references == 0) {
                    continue;
                }

                builder.Append(s_Indent16);
                builder.Append("case ");
                builder.Append(kvp.Value.id);
                builder.AppendLine(":");
                builder.Append(s_Indent20);
                builder.Append("return new ");
                VisitType(kvp.Key, builder);
                builder.Append("();");
                builder.AppendLine();
            }

            loadFn = loadFn.Replace("::ELEMENT_CONSTRUCTORS::", builder.ToString());

            string initPath = Path.Combine(path, "__init" + extension);
            Directory.CreateDirectory(Path.GetDirectoryName(initPath));

            if (File.Exists(initPath)) {
                File.Delete(initPath);
            }

            File.WriteAllText(initPath, loadFn);
            AssetDatabase.Refresh();
        }

        public static string CleanGenericName(Type type) {
            string name = GetPrintableTypeName(type);
            int position = name.LastIndexOf("`");
            if (position == -1) {
                return name;
            }

            return name.Substring(0, position);
        }

        private static string GetSimpleTypeName(Type type) {
            if (type == typeof(void))
                return "void";
            if (type == typeof(object))
                return "object";

            if (type.IsEnum) {
                return GetPrintableTypeName(type);
            }

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Char:
                    return "char";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Single:
                    return "float";
                case TypeCode.String:
                    return "string";
                case TypeCode.UInt16:
                    return "ushort";
                case TypeCode.UInt32:
                    return "uint";
                case TypeCode.UInt64:
                    return "ulong";
                default:
                    return GetPrintableTypeName(type);
            }
        }

        private static string GetPrintableTypeName(Type type) {
            string typeName = type.FullName;

            if (typeName.Contains("+")) {
                return typeName.Replace("+", ".");
            }

            return typeName;
        }

        public static void VisitType(Type type, StringBuilder builder) {
            if (type.IsArray) {
                VisitArrayType(type, builder);
                return;
            }

            if (type.IsGenericParameter) {
                builder.Append(GetPrintableTypeName(type));
                return;
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition) {
                VisitGenericTypeDefinition(type, builder);
                return;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition) {
                VisitGenericTypeInstance(type, builder);
                return;
            }

            builder.Append(GetSimpleTypeName(type));
        }

        private static void VisitGenericTypeInstance(Type type, StringBuilder builder) {
            Type[] genericArguments = type.GetGenericArguments();
            int argIndx = 0;

            // namespace.basetype
            // for each type in chain that has generic arguments
            // replace `{arg count} with < ,? > until no more args
            // UIForia.Test.NamespaceTest.SomeNamespace.NamespaceTestClass+SubType1`1+NestedSubType1`1[System.Int32,System.Int32]

            if (type.IsNullableType()) {
                VisitType(type.GetGenericArguments()[0], builder);
                builder.Append("?");
                return;
            }

            string typeName = type.ToString();

            for (int i = 0; i < typeName.Length; i++) {
                if (typeName[i] == '`') {
                    i++;
                    int count = int.Parse(typeName[i].ToString());
                    builder.Append("<");
                    for (int c = 0; c < count; c++) {
                        VisitType(genericArguments[argIndx++], builder);

                        if (c != count - 1) {
                            builder.Append(", ");
                        }
                    }

                    builder.Append(">");
                }
                else {
                    if (typeName[i] == '[') {
                        return;
                    }

                    if (typeName[i] == '+') {
                        builder.Append(".");
                    }
                    else {
                        builder.Append(typeName[i].ToString());
                    }
                }
            }
        }
        
        private static void VisitArrayType(Type type, StringBuilder builder) {
            VisitType(type.GetElementType(), builder);
            builder.Append("[");
            for (int i = 1; i < type.GetArrayRank(); i++) {
                builder.Append(",");
            }

            builder.Append("]");
        }

        private static void VisitGenericTypeDefinition(Type type, StringBuilder builder) {
            builder.Append(CleanGenericName(type));
            builder.Append("<");
            var arity = type.GetGenericArguments().Length;
            for (int i = 1; i < arity; i++) {
                builder.Append(",");
            }

            builder.Append(">");
        }

        private void GenerateStyleCode(StyleSheet styleSheet) {
            string template = @"
using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_::APPNAME:: {

        ::STYLE_CODE::     

    }

}";


            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < styleSheet.styleGroupContainers.Length; i++) {
                SerializeStyleGroup(builder, styleSheet.styleGroupContainers[i]);
            }
        }

        private void SerializeStyleGroup(StringBuilder builder, UIStyleGroupContainer groupContainer) {
            string styleTemplate = @"
public UIStyleGroupContainer Style_::GUID::() {
    UIStyleGroupContainer retn;
        ::STYLE_CODE::
    return retn;
}".Replace("::GUID::", groupContainer.guid.ToString());
            StringBuilder fnBuilder = new StringBuilder(512);
            fnBuilder.AppendLine("retn = new UIStyleGroupContainer();");

            for (int i = 0; i < groupContainer.groups.Count; i++) {
                fnBuilder.AppendLine($"group = new {nameof(UIStyleGroup)}()");
                UIStyleGroup group = groupContainer.groups[i];
                fnBuilder.AppendLine($"runCommand = new {nameof(UIStyleRunCommand)}()");

                for (int j = 0; j < group.normal.style.PropertyCount; j++) {
                    // todo -- need to serialize all this somehow into code
                    // or need to parse styles in exactly the same order as originally done so the ids match
                    // or stringify the contents directly into code and parse that in order. 
                }

                fnBuilder.AppendLine($"style = new {nameof(UIStyle)}[{group.normal.style.PropertyCount}]");
            }

            builder.AppendLine(styleTemplate.Replace("::STYLE_CODE::", fnBuilder.ToString()));
        }

        private void GenerateTemplateCode(string path, int i, string extension) {
            string file = Path.Combine(path, Path.ChangeExtension(compiledTemplates.array[i].filePath, extension));
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            string template = @"
using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_::APPNAME:: {
        
        public Func<UIElement, TemplateScope, UIElement> Template_::GUID:: = ::CODE:: 
        ::BINDINGS::
        ::SLOTS::
    }

}
                ".Trim();

            string bindingCode = string.Empty;
            string slotCode = string.Empty;
            CompiledTemplate compiledTemplate = compiledTemplates[i];

            // todo -- optimize search or sort by file name at least
            for (int b = 0; b < compiledBindings.size; b++) {
                CompiledBinding binding = compiledBindings[b];
                if (binding.filePath == compiledTemplate.filePath) {
                    bindingCode += $"\n{s_Indent8}public Action<UIElement, UIElement> Binding_{compiledBindings.array[b].bindingType}_{binding.guid} = ";
                    bindingCode += binding.bindingFn.ToTemplateBodyFunction();
                    bindingCode += "\n";
                }
            }

            // todo -- optimize search or sort by file name at least
            for (int s = 0; s < compiledSlots.size; s++) {
                CompiledSlot compiledSlot = compiledSlots[s];
                if (compiledSlot.filePath == compiledTemplate.filePath) {
                    slotCode += $"\n{s_Indent8}// {compiledSlot.GetComment()}";
                    slotCode += $"\n{s_Indent8}public Func<UIElement, TemplateScope, UIElement> {compiledSlot.GetVariableName()} = ";
                    slotCode += compiledSlot.templateFn.ToTemplateBodyFunction();
                    slotCode += "\n";
                }
            }

            string templateBody = compiledTemplates[i].templateFn.ToTemplateBodyFunction();
            template = template.Replace("::GUID::", compiledTemplates[i].guid.ToString());
            template = template.Replace("::CODE::", templateBody);
            template = template.Replace("::BINDINGS::", bindingCode);
            template = template.Replace("::SLOTS::", slotCode);
            template = template.Replace("::APPNAME::", templateSettings.StrippedApplicationName);
            File.WriteAllText(file, template);
        }

    }

}