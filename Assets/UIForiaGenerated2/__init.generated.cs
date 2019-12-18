using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Compilers.Style;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp : ITemplateLoader {
        
        public string[] StyleFilePaths => styleFilePaths;

        private string[] styleFilePaths = {

        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[3];
            templates[0] = Template_0066a1c68ecfd7c40a2e071e3abafebd; // Demo/Simple/Simple.xml
            templates[1] = Template_d0f356faf13096f4ab6400e8e2bb231c; // BuiltInElements/Repeat.xml
            templates[2] = Template_e4e1820cb4f16c948a2b4249a96ca1c7; // Demo/Simple/ExposeSlot.xml
            return templates;

        }

        public TemplateMetaData[] LoadTemplateMetaData(UIStyleGroupContainer[] styleMap) {
            TemplateMetaData[] templateData = new TemplateMetaData[3];
            TemplateMetaData template;
            template = new TemplateMetaData(0, @"Demo/Simple/Simple.xml", styleMap, null);
            templateData[0] = template;
            template = new TemplateMetaData(1, @"BuiltInElements/Repeat.xml", styleMap, null);
            templateData[1] = template;
            template = new TemplateMetaData(2, @"Demo/Simple/ExposeSlot.xml", styleMap, null);
            templateData[2] = template;
            return templateData;

        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[0];
            return bindings;

        }

        public Func<UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, TemplateScope, UIElement>[5];
            slots[0] = Slot_Default_slot0_18aa2b24a99ddfe4689751619875dbe6;
            slots[1] = Slot_Override_Children_3b5318c0173d89348820e9f3a1d5c218;
            slots[2] = Slot_Extern_slot0_8718c671d3a2748428d382efd67a719f;
            slots[3] = Slot_Children_Children_661a5fd68f9b313458137bd9a4e75ae2;
            slots[4] = Slot_Override_Children_d0646c98a2f21e6468fb6ab023a2bc41;
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 41:
                    return new Demo.ExposeSlot();
                case 42:
                    return new Demo.Simple();
                case 51:
                    return new UIForia.Elements.UIPanelElement();
                case 53:
                    return new UIForia.Elements.UIDivElement();
                case 66:
                    return new UIForia.Elements.UIChildrenElement();
                case 76:
                    return new UIForia.Elements.UISlotDefinition();
                case 78:
                    return new UIForia.Elements.UISlotOverride();
                case 80:
                    return new UIForia.Elements.UITextElement();
                case 190:
                    return new BuiltInElements.Repeat<string>();

            }
            return null;
        }

    }

}