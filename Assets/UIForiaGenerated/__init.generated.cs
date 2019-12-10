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
            templates[0] = Template_26ce8b7cec8f0b74fb351221db057e73; // Demo/Simple/Simple.xml
            templates[1] = Template_2588392126050a74bb87adad4b2708df; // BuiltInElements/Repeat.xml
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
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[12];
            bindings[0] = Binding_OnCreate_fa31f8094b181cf4c982850475d27b4e;
            bindings[1] = Binding_OnUpdate_7825b51faad674042954fb98fb9c08b9;
            bindings[2] = Binding_OnCreate_d49966d602ffedd4a9f9bd90e4b15ffe;
            bindings[3] = Binding_OnUpdate_b81ef071d886d0842871a8c509949b54;
            bindings[4] = Binding_OnUpdate_edd5fe3c9d71c6743b264619de0290b1;
            bindings[5] = Binding_OnUpdate_2ff67f44b1581d04f83ef7c40d401b02;
            bindings[6] = Binding_OnCreate_705878f8b6060af45b83cd90cbafc5fc;
            bindings[7] = Binding_OnUpdate_71fc4338b0c5e9f498b09d2c319fc430;
            bindings[8] = Binding_OnCreate_4ae708f41b575c54a818c38125574905;
            bindings[9] = Binding_OnUpdate_3a67287d08f6a5f4aa77303837cb4124;
            bindings[10] = Binding_OnCreate_401ff9034ef3d324f94176b8b8ec515a;
            bindings[11] = Binding_OnUpdate_2b232864b8913644ba023e668988f4e5;
            return bindings;

        }

        public Func<UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, TemplateScope, UIElement>[1];
            slots[0] = Slot_Children_Children_f534fec48178ddb49bee35b7b7dbc42e;
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 41:
                    return new Demo.Simple();
                case 52:
                    return new UIForia.Elements.UIDivElement();
                case 64:
                    return new UIForia.Elements.SlotTemplateElement();
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