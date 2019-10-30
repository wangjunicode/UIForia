using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[3];
            templates[0] = Template_7c97eaf000fc49d42bf30d4893e75002; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_3109d0493fde86c429cfc36e5bef63cf; // Data/TemplateLoading/LoadTemplateHydrate.xml
            templates[2] = Template_c70b37a912bf5b74a81de1c65dc67470; // Data/TemplateLoading/ThingWithParameters.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[4];
            bindings[0] = Binding_OnCreate_3e13d42ef20baa64fad48deafa99e640;
            bindings[1] = Binding_OnUpdate_4874c4eed91e31f4aaaf4dbd81cae5c9;
            bindings[2] = Binding_OnUpdate_10584e347bba4814faec7bc37597c5d3;
            bindings[3] = Binding_OnUpdate_5530bd855f965fa4b9d0ee2ef334f0a2;
            return bindings;

        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<UIElement, TemplateScope2, UIElement>[4];
            slots[0] = Slot_Children_Children_e25aef31cc869454b8efe1e5db5b7cc2;
            slots[1] = Slot_Default_ThingSlot_52c23999ccfd3204aa2c447a360efbcc;
            slots[2] = Slot_Override_ThingSlot_d4449d5421b98ad42a861a3b48b5d7d3;
            slots[3] = Slot_Default_SomeSlot_17def6760df59e842abc7dce3bf68c53;
            return slots;

        }

    }

}