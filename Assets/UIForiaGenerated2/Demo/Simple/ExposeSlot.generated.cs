using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_e4e1820cb4f16c948a2b4249a96ca1c7 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {
                // new Demo.ExposeSlot
                root = scope.application.CreateElementFromPoolWithType(41, default(UIForia.Elements.UIElement), 1, 0, 2);
            }
            // new UIForia.Elements.UIPanelElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(51, root, 2, 0, 2);
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(80, targetElement_1, 0, 0, 2);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Next child is a slot";
            targetElement_1.children.array[0] = targetElement_2;
            targetElement_2 = scope.application.CreateSlot2("slot0", scope, 0, root, targetElement_1);
            targetElement_1.children.array[1] = targetElement_2;
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        
        // Slot name="slot0" (Default)
        public Func<UIElement, TemplateScope, UIElement> Slot_Default_slot0_18aa2b24a99ddfe4689751619875dbe6 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(76, default(UIForia.Elements.UIElement), 1, 0, 2));
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(80, slotRoot, 0, 0, 2);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "I am default exposed content";
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}
                