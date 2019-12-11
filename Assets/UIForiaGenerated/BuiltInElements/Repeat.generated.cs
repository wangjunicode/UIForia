using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_96f16a46e62b7254fa04bfc7ebd77d51 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {

                // new BuiltInElements.Repeat`1[System.String]
                root = scope.application.CreateElementFromPoolWithType(180, default(UIForia.Elements.UIElement), 1, 0, 1);
            }

            // <Panel />

            // new UIForia.Elements.UIPanelElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(50, root, 1, 0, 1);

            // <Children />
            targetElement_2 = scope.application.CreateSlot(UIForia.Application.ResolveSlotId("Children", scope.slotInputs, 0), root, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, scope.slotInputs, root));
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        
        public Func<UIElement, TemplateScope, UIElement> Slot_Children_Children_4e903e138f8a6264ab32bb56f508e4cb = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(77, default(UIForia.Elements.UIElement), 0, 0, 1));
            return slotRoot;
        };

    }

}