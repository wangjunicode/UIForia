using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_d0f356faf13096f4ab6400e8e2bb231c = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;
            UIForia.Util.StructList<UIForia.Compilers.SlotUsage> slotUsage;

            // new UIForia.Elements.UIPanelElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(51, root, 2, 0, 1);
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(80, targetElement_1, 0, 0, 1);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "I am repeat";
            targetElement_1.children.array[0] = targetElement_2;
            // new Demo.ExposeSlot
            targetElement_2 = scope.application.CreateElementFromPoolWithType(41, targetElement_1, 1, 0, 1);
            slotUsage = UIForia.Util.StructList<UIForia.Compilers.SlotUsage>.PreSize(1);
            // Children
            slotUsage.array[0] = new UIForia.Compilers.SlotUsage("Children", 1, root);
            scope.ForwardSlotUsageWithFallback("slot0", slotUsage, root, 2);
            scope.application.HydrateTemplate(2, targetElement_2, new UIForia.Compilers.TemplateScope(scope.application, slotUsage));
            slotUsage.Release();
            targetElement_1.children.array[1] = targetElement_2;
            root.children.array[0] = targetElement_1;
            targetElement_1 = scope.application.CreateSlot2("Children", scope, 3, root, root);
            root.children.array[1] = targetElement_1;
            return root;
        }; 
        
        
        // Slot name="Children" (OverrideBuiltInElements/Repeat.xml)
        public Func<UIElement, TemplateScope, UIElement> Slot_Override_Children_3b5318c0173d89348820e9f3a1d5c218 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotOverride slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotOverride)scope.application.CreateElementFromPoolWithType(78, default(UIForia.Elements.UIElement), 1, 0, 1));
            targetElement_1 = scope.application.CreateSlot2("slot0", scope, 2, slotRoot, slotRoot);
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

        // Slot name="slot0"
        public Func<UIElement, TemplateScope, UIElement> Slot_Extern_slot0_8718c671d3a2748428d382efd67a719f = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(76, default(UIForia.Elements.UIElement), 1, 0, 1));
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(80, slotRoot, 0, 0, 1);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Extern Default content";
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

        // Slot name="Children" (ChildrenBuiltInElements/Repeat.xml)
        public Func<UIElement, TemplateScope, UIElement> Slot_Children_Children_661a5fd68f9b313458137bd9a4e75ae2 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(76, default(UIForia.Elements.UIElement), 1, 0, 1));
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(80, slotRoot, 0, 0, 1);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Default children";
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}
                