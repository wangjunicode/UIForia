using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp : ITemplateLoader {
        
        public Func<UIElement, TemplateScope2, UIElement>[] LoadTemplates() {
            Func<UIElement, TemplateScope2, UIElement>[] templates = new Func<UIElement, TemplateScope2, UIElement>[3];
            templates[0] = Template_77fcce9dffb4f7b46b167aa882b5e8a6; // Data/TemplateLoading/LoadTemplate0.xml
            templates[1] = Template_47581214eec44d246ba608b024e109ff; // Data/TemplateLoading/LoadTemplateHydrate.xml
            templates[2] = Template_ce93fb92900361747b2cdcc31d6ec50c; // Data/TemplateLoading/ThingWithParameters.xml
            return templates;

        }
    
        public Action<UIElement, UIElement>[] LoadBindings() {
            Action<UIElement, UIElement>[] bindings = new Action<UIElement, UIElement>[3];
            bindings[0] = Binding_OnCreate_5cd7aa1ce1f93434ca9fd9d4ed00dd10;
            bindings[1] = Binding_OnUpdate_225a39346c85d634287095999c831fe9;
            bindings[2] = Binding_OnUpdate_dcaef7f434cce6f48983f402243d51ac;
            return bindings;

        }

        public Func<UIElement, TemplateScope2, UIElement>[] LoadSlots() {
            Func<UIElement, TemplateScope2, UIElement>[] slots = new Func<UIElement, TemplateScope2, UIElement>[4];
            slots[0] = Slot_Children_Children_bada918c25d9cf647b08dcee49e711f9;
            slots[1] = Slot_Default_ThingSlot_0cbf61a22299a344793d8ff0708d5e2b;
            slots[2] = Slot_Override_ThingSlot_dc37025bd6ea9fb46a0f56d7539bb0e8;
            slots[3] = Slot_Default_SomeSlot_e3c55711eaf392949812d73327c72284;
            return slots;

        }

    }

}