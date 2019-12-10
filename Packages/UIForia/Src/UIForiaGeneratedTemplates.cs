using System;
using System.Reflection;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates {

        private readonly MethodInfo methodInfo;
        internal Func<UIElement, TemplateScope, UIElement>[] templates;
        
        public UIForiaGeneratedTemplates(string appName) {
            methodInfo = GetType().GetMethod("LoadApp_" + appName, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void Load() {
            methodInfo?.Invoke(this, null);
        }

    }

}