using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[2];
            templates[0] = Template_fa6a5fb489954c54280fa62ba2a00431; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_5c83330fa98374c4097c5961795ab060; // Data/TemplateLoading/LoadTemplateHydrate.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[2];
            bindings[0] = Binding_0749e1201f576314fb3ca1690b81e43d;
            bindings[1] = Binding_5ae07bd74fafa6e43a7a559f862a22d3;
            return bindings;

        }

    }

}