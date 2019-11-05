using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_LoadTemplateFromFile {
        
        public Func<UIElement, TemplateScope2, UIElement> Template_30edf6ceb73738c4897ae7a5539fc310 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
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
        
        
        public Func<UIElement, TemplateScope2, UIElement> Slot_Default_ThingSlot_0ad5313594992bd4d97b855ccdb6fd86 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
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