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

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[2];
            templates[1] = Template_c94e139042b0bee4789648cc70495ee9; // BuiltInElements/Repeat.xml
            return templates;

        }

        public TemplateMetaData[] LoadTemplateMetaData(UIStyleGroupContainer[] styleMap) {
            TemplateMetaData[] templateData = new TemplateMetaData[2];
            TemplateMetaData template;
            template = new TemplateMetaData(0, "Demo/Simple/Simple.xml", styleMap, null);
            templateData[0] = template;
            template = new TemplateMetaData(1, "BuiltInElements/Repeat.xml", styleMap, null);
            templateData[1] = template;
            return templateData;

        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[2];
            bindings[0] = Binding_OnUpdate_302a4b8c9c00fdb4d987841733b359a3;
            return bindings;

        }

        public Func<UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, TemplateScope, UIElement>[1];
            slots[0] = Slot_Default_slot0_b58a08bc1fa09954aab106ced9ace6dc;
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 41:
                    return new Demo.Simple();
                case 50:
                    return new UIForia.Elements.UIPanelElement();
                case 76:
                    return new UIForia.Elements.UISlotDefinition();
                case 77:
                    return new UIForia.Elements.UISlotOverride();
                case 78:
                    return new UIForia.Elements.UITextElement();
                case 180:
                    return new BuiltInElements.Repeat<string>();

            }
            return null;
        }

    }

}