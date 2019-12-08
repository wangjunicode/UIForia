using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {
            @"C:/Users/Matt/RiderProjects/UIForia/Assets\StreamingAssets\UIForia\GameApp\Demo/Simple/Simple.style",

        };

        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[1];
            templates[0] = Template_4983d08d89d9bfa4d862d9ac7366d6a5; // Demo/Simple/Simple.xml
            return templates;

        }

        public TemplateMetaData[] LoadTemplateMetaData(UIStyleGroupContainer[] styleMap) {
            TemplateMetaData[] templateData = new TemplateMetaData[1];
            TemplateMetaData template;
            template = new TemplateMetaData(0, "Demo/Simple/Simple.xml", styleMap, null);
            templateData[0] = template;
            return templateData;

        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[8];
            bindings[0] = Binding_OnCreate_41969f3d202973f488a0e2688ccafdcf;
            bindings[1] = Binding_OnUpdate_4219fd0cfa46f644f8d540b6b8919cf8;
            bindings[2] = Binding_OnCreate_533cc72a8d901c1409b103736c99f0d6;
            bindings[3] = Binding_OnUpdate_54bfde3312de0c242ab3e2f88c268c5e;
            bindings[4] = Binding_OnCreate_1d706c3a952e47a4c9e914ba83aa762c;
            bindings[5] = Binding_OnUpdate_24de277288e959c4d858f5895c77f22d;
            bindings[6] = Binding_OnCreate_327fe70a6ac59b14d908e1c8c873b436;
            bindings[7] = Binding_OnUpdate_0b6948b56aacb2a449931ca73c9cd300;
            return bindings;

        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<UIElement, TemplateScope2, UIElement>[0];
            return slots;

        }

    }

}