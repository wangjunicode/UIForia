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
            templates[0] = Template_4d47bed5145e6bc4ba9d9e7912676b75; // Demo/Simple/Simple.xml
            templates[1] = Template_32e24b13bc1791e4e8fd53e909ab7e51; // BuiltInElements/Repeat.xml
            templates[2] = Template_00844ce891a8b0847a6f78835e457940; // Demo/Simple/ExposeSlot.xml
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
            Func<UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, TemplateScope, UIElement>[6];
            slots[0] = Slot_Default_slot0_2ef2435c1cc29584fbf69395d9e3b213;
            slots[1] = Slot_Override_Children_4af0bb7b84f96f5488d9388f1d929bcb;
            slots[2] = Slot_Extern_slot0_b1b0b65b5fde6ab48947b6d44b5d8324;
            slots[3] = Slot_Children_Children_7bae235a8be17194dae6722ce3522351;
            slots[4] = Slot_Override_Children_9aa78f1371843cd4d9891de7d978dd28;
            slots[5] = Slot_Override_slot0_5edceda011da0c442bd591c45a15fb2e;
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
                case 203:
                    return new BuiltInElements.Repeat<string>();

            }
            return null;
        }

    }

}