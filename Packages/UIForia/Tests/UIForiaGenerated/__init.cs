using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[2];
            templates[0] = Template_bb4779fbb31c8b740b4b897efd7a7d69; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_e2b341cce736bd3469011f20e8329d88; // Data/TemplateLoading/LoadTemplateHydrate.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[1];
            bindings[0] = Binding_32338abd483c2104a9467c29c79c154b;
            return bindings;

        }

    }

}