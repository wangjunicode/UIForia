using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp {
        
        public Func<UIElement, TemplateScope2, UIElement> Template_c70b37a912bf5b74a81de1c65dc67470 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;

            if (root == null)
            {
                root = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.ThingWithParameters), default(UIForia.Elements.UIElement), 1, 0);
            }

            // <{DefineSlot}ThingSlot />
            targetElement_1 = scope.application.CreateSlot(UIForia.Application.ResolveSlotId("ThingSlot", scope.slotInputs, 1), root, new UIForia.Compilers.TemplateScope2(scope.application, scope.slotInputs));
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        
        public Func<UIElement, TemplateScope2, UIElement> Slot_Default_ThingSlot_52c23999ccfd3204aa2c447a360efbcc = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UISlotContent), default(UIForia.Elements.UIElement), 1, 0));

            // 
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), slotRoot, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Hello Default";
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}