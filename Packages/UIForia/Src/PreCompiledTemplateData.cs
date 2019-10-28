using System;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Generated;
using UnityEditor;

namespace UIForia {

    public class PreCompiledTemplateData : CompiledTemplateData {

        private static readonly string s_Indent8 = new string(' ', 8);
        private static readonly string s_Indent12 = new string(' ', 12);
        
        public PreCompiledTemplateData(TemplateSettings templateSettings) : base(templateSettings) { }

        public void LoadTemplates() {
            // todo read data from template settings input object
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName(templateSettings.assemblyName);
            Type type = assembly.GetType("UIForia.Generated.UIForiaGeneratedTemplates_" + templateSettings.applicationName);
            ITemplateLoader loader = (ITemplateLoader) Activator.CreateInstance(type);
            templates = loader.LoadTemplates();
        }

        public void GenerateCode() {
            string path = templateSettings.outputPath;
            string extension = "." + templateSettings.codeFileExtension;

            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            for (int i = 0; i < compiledTemplates.size; i++) {
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
                template = template.Replace("::APPNAME::", templateSettings.applicationName);
                File.WriteAllText(file, template);
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
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            ::BINDING_CODE::
        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            ::SLOT_CODE::
        }

    }

}".Trim();

            string arrayName = "templates";

            string initCode = $"Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<{nameof(UIElement)}, {nameof(TemplateScope2)}, {nameof(UIElement)}>[{compiledTemplates.size}];";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(initCode);

            for (int i = 0; i < compiledTemplates.size; i++) {
                builder.AppendLine($"{s_Indent12}templates[{i}] = Template_{compiledTemplates.array[i].guid.ToString()}; // {compiledTemplates.array[i].filePath}");
            }

            builder.AppendLine($"{s_Indent12}return templates;");

            loadFn = loadFn.Replace("::APPNAME::", templateSettings.applicationName);
            loadFn = loadFn.Replace("::TEMPLATE_CODE::", builder.ToString());
            builder.Clear();


            builder.AppendLine($"Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<{nameof(UIElement)}, {nameof(TemplateScope2)}, {nameof(UIElement)}>[{compiledSlots.size}];");
            
            for (int i = 0; i < compiledSlots.size; i++) {
                builder.AppendLine($"{s_Indent12}slots[{i}] = {compiledSlots.array[i].GetVariableName()};");
            }
            
            builder.AppendLine($"{s_Indent12}return slots;");
            loadFn = loadFn.Replace("::SLOT_CODE::", builder.ToString());

            builder.Clear();
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

    }

}