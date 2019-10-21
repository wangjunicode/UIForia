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

        public PreCompiledTemplateData(TemplateSettings templateSettings) : base(templateSettings) { }

        public void LoadTemplates() {
            // todo read data from template settings input object
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblyByName(templateSettings.assemblyName);
            Type type = assembly.GetType("UIForia.Generated.UIForiaGeneratedTemplates_" + templateSettings.applicationName);
            ITemplateLoader loader = (ITemplateLoader)Activator.CreateInstance(type);
            templates = loader.LoadTemplates();
        }

        public void GenerateCode() {
            string path = templateSettings.outputPath;
            string extension = "." + templateSettings.codeFileExtension;
            
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            extension = "." + extension;
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

    }

}
                ".Trim();
                string templateBody = compiledTemplates[i].templateFn.ToTemplateBodyFunction();
                template = template.Replace("::GUID::", compiledTemplates[i].guid.ToString());
                template = template.Replace("::CODE::", templateBody);
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
            ::CODE::
        }

    }

}".Trim();

            string arrayName = nameof(UIForiaGeneratedTemplates.templates);

            string initCode = $"Func<UIElement, TemplateScope2, UIElement>[] {arrayName} = new Func<{nameof(UIElement)}, {nameof(TemplateScope2)}, {nameof(UIElement)}>[{compiledTemplates.size}];";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(initCode);

            for (int i = 0; i < compiledTemplates.size; i++) {
                builder.AppendLine($"{new string(' ', 12)}{arrayName}[{i}] = Template_{compiledTemplates.array[i].guid.ToString()}; // {compiledTemplates.array[i].filePath}");
            }

            builder.AppendLine($"{new string(' ', 12)}return {arrayName};");
            
            loadFn = loadFn.Replace("::APPNAME::", templateSettings.applicationName);
            loadFn = loadFn.Replace("::CODE::", builder.ToString());

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