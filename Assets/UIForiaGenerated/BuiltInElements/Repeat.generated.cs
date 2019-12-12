using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_c94e139042b0bee4789648cc70495ee9 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
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

            // <DefineSlot slot:name="slot0"/>
            targetElement_2 = scope.application.CreateSlot(UIForia.Application.ResolveSlotId("slot0", scope.slotInputs, 0), root, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, scope.slotInputs, default(UIForia.Elements.UIElement)));
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnUpdate_302a4b8c9c00fdb4d987841733b359a3 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            BuiltInElements.Repeat<string> __castRoot;
            System.Collections.Generic.List<string> nullCheck;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((BuiltInElements.Repeat<string>)__root);
            UIForia.Text.TextUtil.StringBuilder.Append("List size = ");
            nullCheck = __castRoot.list;
            if (nullCheck == null)
            {
                goto retn;
            }
            UIForia.Text.TextUtil.StringBuilder.Append(nullCheck.Count.ToString());
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        retn:
            return;
        };

        
        // Slot name="slot0"(Default)
        public Func<UIElement, TemplateScope, UIElement> Slot_Default_slot0_b58a08bc1fa09954aab106ced9ace6dc = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotOverride slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotOverride)scope.application.CreateElementFromPoolWithType(77, default(UIForia.Elements.UIElement), 1, 0, 1));

            // <Text />    'List size = {list.Count}'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(78, slotRoot, 0, 0, 1);
            UIForia.Systems.LinqBindingNode.Get(scope.application, slotRoot, targetElement_1, default(UIForia.Elements.UIElement), -1, -1, 0);
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}