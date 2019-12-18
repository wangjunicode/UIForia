using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_0066a1c68ecfd7c40a2e071e3abafebd = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.StructList<UIForia.Compilers.SlotUsage> slotUsage;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {
                // new Demo.Simple
                root = scope.application.CreateElementFromPoolWithType(42, default(UIForia.Elements.UIElement), 2, 0, 0);
            }
            // new BuiltInElements.Repeat<string>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(190, root, 2, 0, 0);
            slotUsage = UIForia.Util.StructList<UIForia.Compilers.SlotUsage>.PreSize(1);
            // Children
            slotUsage.array[0] = new UIForia.Compilers.SlotUsage("Children", 4, root);
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, slotUsage));
            slotUsage.Release();
            root.children.array[0] = targetElement_1;
            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(53, root, 1, 0, 0);
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(80, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Text";
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[1] = targetElement_1;
            return root;
        }; 
        
        
        // Slot name="Children" (OverrideDemo/Simple/Simple.xml)
        public Func<UIElement, TemplateScope, UIElement> Slot_Override_Children_d0646c98a2f21e6468fb6ab023a2bc41 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotOverride slotRoot;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;

            slotRoot = ((UIForia.Elements.UISlotOverride)scope.application.CreateElementFromPoolWithType(78, default(UIForia.Elements.UIElement), 1, 0, 0));
            // new UIForia.Elements.UIPanelElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(51, slotRoot, 1, 0, 0);
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(80, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Children!";
            targetElement_1.children.array[0] = targetElement_2;
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}
                