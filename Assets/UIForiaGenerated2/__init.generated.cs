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
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/Features/RepeatALot.style",
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/Features/RepeatALot.xml.style",
            @"C:/Seed/UIForia/UIElements/Assets\StreamingAssets\UIForia\GameApp2\Documentation/DocumentationElements/PlayerDetail.xml.style",

        };

        public Func<UIElement, TemplateScope, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope, UIElement>[] templates = new Func<UIElement, TemplateScope, UIElement>[2];
            templates[0] = Template_c88ffa0b92c11c94291f47a413392767; // Documentation/Features/RepeatALot.xml
            templates[1] = Template_8a8ee51d452844e4499fc5da9b82c737; // Documentation/DocumentationElements/PlayerDetail.xml
            return templates;

        }

        public TemplateMetaData[] LoadTemplateMetaData(Dictionary<string, StyleSheet> sheetMap, UIStyleGroupContainer[] styleMap) {
            TemplateMetaData[] templateData = new TemplateMetaData[2];
            TemplateMetaData template;
            StyleSheetReference[] styleSheetRefs;
            styleSheetRefs = new StyleSheetReference[2];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Documentation/Features/RepeatALot.style"]);
            styleSheetRefs[1] = new StyleSheetReference("temp", sheetMap[@"Documentation/Features/RepeatALot.xml.style"]);
            template = new TemplateMetaData(0, @"Documentation/Features/RepeatALot.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[0] = template;
            styleSheetRefs = new StyleSheetReference[1];
            styleSheetRefs[0] = new StyleSheetReference(null, sheetMap[@"Documentation/DocumentationElements/PlayerDetail.xml.style"]);
            template = new TemplateMetaData(1, @"Documentation/DocumentationElements/PlayerDetail.xml", styleMap, styleSheetRefs);
            template.BuildSearchMap();
            templateData[1] = template;
            return templateData;

        }

        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[6];
            bindings[0] = Binding_OnUpdate_8150361b925f0f043a784ea2fcfed4de;
            bindings[1] = Binding_OnUpdate_04a52cb352591d04ba8362da83eceff0;
            bindings[2] = Binding_OnUpdate_3274fd0009ce5704d983b315cccb7d28;
            bindings[3] = Binding_OnUpdate_05024e36d3c875c4ab53d4d2a7204bc8;
            bindings[4] = Binding_OnUpdate_1e0ea6674cc3063499349365bb284fbe;
            bindings[5] = Binding_OnUpdate_f542d5ba3544f68428774ff4a0dfd21c;
            return bindings;

        }

        public Func<UIElement, UIElement, TemplateScope, UIElement>[] LoadSlots() {
            Func<UIElement, UIElement, TemplateScope, UIElement>[] slots = new Func<UIElement, UIElement, TemplateScope, UIElement>[2];
            slots[0] = Slot_Template_0_158057d9b18ddf049ab047062ea04fca;
            slots[1] = Slot_Template_1_6139291d0b413b344a737cb9b4b1ff00;
            return slots;

        }

        public UIElement ConstructElement(int typeId) {
            switch(typeId) {
                case 8:
                    return new UnityEngine.RepeatALot();
                case 33:
                    return new Documentation.DocumentationElements.PlayerDetail();
                case 50:
                    return new UIForia.Elements.UIGroupElement();
                case 53:
                    return new UIForia.Elements.UIDivElement();
                case 58:
                    return new UIForia.Elements.UIHeading1Element();
                case 85:
                    return new UIForia.Elements.UITextElement();
                case 246:
                    return new UIForia.Elements.UIRepeatElement<Documentation.DocumentationElements.PlayerData>();

            }
            return null;
        }

    }

}