using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[2];
            templates[0] = Template_1a5ba50a1dc0e844d9e7315d4514d940; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_8c7d3e5f42e4b38438e56af4c75d8ab4; // Data/TemplateLoading/LoadTemplateHydrate.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[2];
            bindings[0] = Binding_40b61b53e5bd0954ba7862cc741e170f;
            bindings[1] = Binding_15087741023767c4bae4b9f015839745;
            return bindings;

        }

    }

}