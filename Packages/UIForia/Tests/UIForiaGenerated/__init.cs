using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[2];
            templates[0] = Template_2c72910b8fc257e4fbc9d2bc804c2a54; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_dbc412ec15ef3684fb97d17debe86768; // Data/TemplateLoading/LoadTemplateHydrate.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[2];
            bindings[0] = Binding_OnCreate_4feee72f3b1592c45966fed00869c2d1;
            bindings[1] = Binding_OnUpdate_2ee8b0c6ab675ed4db36ca6068e10db5;
            return bindings;

        }

    }

}