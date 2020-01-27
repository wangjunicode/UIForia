using System;
using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/Documentation.style",
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/Features/AnimationDemo.style",
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Elements/InputElement.xml.style",

        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[2];
            templates[0] = Template_2fcb9b0a8b6e2454b88a9f96d1520de9; // Documentation/Features/AnimationDemo.xml
            templates[1] = Template_d5a95c44b9599244c9c60e0cc53caef6; // Elements/InputElement.xml
            return templates;

        }

        public TemplateMetaData[] LoadTemplateMetaData(Dictionary<string, StyleSheet> sheetMap, UIStyleGroupContainer[] styleMap) {
            TemplateMetaData[] templateData = new TemplateMetaData[2];
            TemplateMetaData template;
            StyleSheetReference[] styleSheetRefs;
            styleSheetRefs = new StyleSheetReference[1];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Documentation/Features/AnimationDemo.style"]);
            template = new TemplateMetaData(0, @"Documentation/Features/AnimationDemo.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[0] = template;
            styleSheetRefs = new StyleSheetReference[1];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Elements/InputElement.xml.style"]);
            template = new TemplateMetaData(1, @"Elements/InputElement.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[1] = template;
            return templateData;

        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[3];
            bindings[0] = Binding_OnCreate_09a97b00ac6698142baa6cc738a9f0c5;
            bindings[1] = Binding_OnUpdate_b71c413220f58ef48b78d272da06c362;
            bindings[2] = Binding_OnLateUpdate_bc49db6fadc63434eb73cf8c61780a57;
            return bindings;

        }

        public Func<UIElement, UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, UIElement, TemplateScope, UIElement>[0];
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 12:
                    return new Documentation.Features.AnimationDemo();
                case 85:
                    return new UIForia.Elements.UITextElement();
                case 246:
                    return new UIForia.Elements.InputElement<int>();

            }
            return null;
        }

    }

}