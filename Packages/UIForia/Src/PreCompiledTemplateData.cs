using System;
using System.Reflection;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia {

    public class RuntimeTemplateData : ICompiledTemplateData {

        public void LoadTemplates() {
            
        }

        public void GenerateCode() {
            throw new NotImplementedException();
        }

    }

    public partial class PreCompiledTemplateData : ICompiledTemplateData {

        private readonly MethodInfo methodInfo;
        private Action<UIElement, TemplateScope2>[] templates;
        
        public PreCompiledTemplateData(string applicationName) {
            methodInfo = GetType().GetMethod("LoadApplication__" + applicationName, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void LoadTemplates() {
            methodInfo?.Invoke(this, null);
        }

        public void GenerateCode() {
            
        }

    }

}