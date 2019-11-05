using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_LoadTemplateFromFile : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[3];
            templates[0] = Template_bed6432ec30cfd648af747d30c2aab28; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_cd728537a59857d428446fe01307faf2; // Data/TemplateLoading/LoadTemplateHydrate.xml
            templates[2] = Template_30edf6ceb73738c4897ae7a5539fc310; // Data/TemplateLoading/ThingWithParameters.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[4];
            bindings[0] = Binding_OnCreate_8c9f8ef492f26f042be3c9df180d987a;
            bindings[1] = Binding_OnUpdate_2f2316c3581062d458bfb55f268524ad;
            bindings[2] = Binding_OnUpdate_3c506917041fb3343ac6e2963b40e251;
            bindings[3] = Binding_OnUpdate_b8f1abd1f166f20459e3dd45b954fb75;
            return bindings;

        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<UIElement, TemplateScope2, UIElement>[4];
            slots[0] = Slot_Children_Children_1c171275dc84de7478451663caff8b07;
            slots[1] = Slot_Default_ThingSlot_0ad5313594992bd4d97b855ccdb6fd86;
            slots[2] = Slot_Override_ThingSlot_c3d9154fd9d1e8045ad9a386855b6a31;
            slots[3] = Slot_Default_SomeSlot_391673697e5964d4989169656aa1da41;
            return slots;

        }

    }

}