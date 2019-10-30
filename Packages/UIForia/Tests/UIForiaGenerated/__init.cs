using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[3];
            templates[0] = Template_ad0f13290ef810b4baaab7d79c7f050c; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_2b9906e0dd45f1a478aba7cb30f40283; // Data/TemplateLoading/LoadTemplateHydrate.xml
            templates[2] = Template_bc24e635c5a64ff47934d1c752f1ee0f; // Data/TemplateLoading/ThingWithParameters.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[4];
            bindings[0] = Binding_OnCreate_34b8dae9d24300b42987a759a71ff767;
            bindings[1] = Binding_OnUpdate_e836368690ffea449abb7084964a06b8;
            bindings[2] = Binding_OnUpdate_4870dc68a8757ec419258564e5210559;
            bindings[3] = Binding_OnUpdate_1a9eeb14d0d5e4949a107c9393e6dc0b;
            return bindings;

        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<UIElement, TemplateScope2, UIElement>[4];
            slots[0] = Slot_Children_Children_8eb41d19fe5704d428ab4fd01220c291;
            slots[1] = Slot_Default_ThingSlot_a9285a831ee18904f9543084d78255c4;
            slots[2] = Slot_Override_ThingSlot_40c4115120eee3d408ee3b88e0d782af;
            slots[3] = Slot_Default_SomeSlot_c51ac020e855e58418a44d747983db34;
            return slots;

        }

    }

}