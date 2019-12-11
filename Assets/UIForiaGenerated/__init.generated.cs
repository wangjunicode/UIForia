using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp\Demo/Simple/Simple.style",

        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[2];
            templates[0] = Template_c557efc21297629428d443404b6bd07b; // Demo/Simple/Simple.xml
            templates[1] = Template_96f16a46e62b7254fa04bfc7ebd77d51; // BuiltInElements/Repeat.xml
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
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[1];
            bindings[0] = Binding_OnUpdate_f0e9f42ba2deb6949b53b910c9586353;
            return bindings;

        }

        public Func<UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, TemplateScope, UIElement>[2];
            slots[0] = Slot_Children_Children_4e903e138f8a6264ab32bb56f508e4cb;
            slots[1] = Slot_Children_Children_421d5557a5c700842a20beaa6ee1995c;
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 41:
                    return new Demo.Simple();
                case 50:
                    return new UIForia.Elements.UIPanelElement();
                case 66:
                    return new UIForia.Elements.UIChildrenElement();
                case 76:
                    return new UIForia.Elements.UISlotDefinition();
                case 77:
                    return new UIForia.Elements.UISlotContent();
                case 78:
                    return new UIForia.Elements.UITextElement();
                case 180:
                    return new BuiltInElements.Repeat<string>();

            }
            return null;
        }

    }

}