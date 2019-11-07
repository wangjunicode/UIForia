using System;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEditor;

namespace UIForia {

    public class PreCompiledTemplateData : CompiledTemplateData {

        private static readonly string s_Indent8 = new string(' ', 8);
        private static readonly string s_Indent12 = new string(' ', 12);

        public PreCompiledTemplateData(TemplateSettings templateSettings) : base(templateSettings) { }

        public override void LoadTemplates() {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName(templateSettings.assemblyName);
            Type type = assembly.GetType("UIForia.Generated.UIForiaGeneratedTemplates_" + templateSettings.StrippedApplicationName);
            try {
                ITemplateLoader loader = (ITemplateLoader) Activator.CreateInstance(type);
                templates = loader.LoadTemplates();
                slots = loader.LoadSlots();
                bindings = loader.LoadBindings();
                templateMetaData = loader.LoadTemplateMetaData();
                for (int i = 0; i < templateMetaData.Length; i++) {
                    templateMetaData[i].compiledTemplateData = this;
                }
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

            string loadFn = @"
using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_::APPNAME:: : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            ::TEMPLATE_CODE::
        }

        public TemplateMetaData[] LoadTemplateMetaData() {
            ::TEMPLATE_META_CODE::
        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            ::BINDING_CODE::
        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            ::SLOT_CODE::
        }

    }

}".Trim();

            loadFn = loadFn.Replace("::APPNAME::", templateSettings.StrippedApplicationName);

            // Print Template Code
            string initCode = $"Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<{nameof(UIElement)}, {nameof(TemplateScope2)}, {nameof(UIElement)}>[{compiledTemplates.size}];";
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
                builder.AppendLine($"{s_Indent12}template = new {nameof(TemplateMetaData)}({compiledTemplates[i].templateId}, \"{compiledTemplates[i].filePath}\", null);");
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
            builder.AppendLine($"Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<{nameof(UIElement)}, {nameof(TemplateScope2)}, {nameof(UIElement)}>[{compiledSlots.size}];");

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

            string initPath = Path.Combine(path, "__init" + extension);
            Directory.CreateDirectory(Path.GetDirectoryName(initPath));

            if (File.Exists(initPath)) {
                File.Delete(initPath);
            }

            File.WriteAllText(initPath, loadFn);
            AssetDatabase.Refresh();
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
        
        public Func<UIElement, TemplateScope2, UIElement> Template_::GUID:: = ::CODE:: 
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
                    slotCode += $"\n{s_Indent8}public Func<UIElement, TemplateScope2, UIElement> {compiledSlot.GetVariableName()} = ";
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