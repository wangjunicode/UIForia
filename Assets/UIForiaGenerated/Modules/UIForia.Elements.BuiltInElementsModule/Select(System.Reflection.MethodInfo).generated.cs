using System;
using UIForia.Compilers;
using UIForia.Elements;
#pragma warning disable 0164

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        // C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\Select.xml
        public Func<UIElement, TemplateScope, UIElement> Template_3907e139_1c6a_4c8c_8212_de4943957227 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;
            UIForia.Compilers.TemplateScope hydrateScope;

            targetElement_1 = scope.application.CreateSlot(@"select-display", scope, 0, root, root);
            targetElement_1 = scope.application.CreateSlot(@"placeholder", scope, 1, root, root);

            // new UIForia.Elements.UIGroupElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(200, root, 2, 0, 1);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, -1, -1);

            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(203, targetElement_1, 0, 0, 1);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, -1, -1);

            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(203, targetElement_1, 0, 0, 1);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, -1, -1);

            // new UIForia.Elements.ScrollView 162:10
            targetElement_1 = scope.application.CreateElementFromPoolWithType(214, root, 5, 1, 1);
            hydrateScope = new UIForia.Compilers.TemplateScope(scope.application);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute(@"id", @"option-list");
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, 11, -1, 12, -1);
            scope.application.HydrateTemplate(2, targetElement_1, hydrateScope);
            return root;
        }; 

        // binding id = 0
        public Action<UIElement, UIElement> Binding_OnUpdate_3d3016af_3804_491d_96e6_1015dab1274a = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.Select<System.Reflection.MethodInfo> __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__element.bindingNode.referencedContexts[0]);

            // if="validSelection"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(__slotCtx0_cast.validSelection);
        };

        // binding id = 1
        public Action<UIElement, UIElement> Binding_OnUpdate_fe63be93_242d_4e32_8045_48a003a6858c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            UIForia.Elements.Select<System.Reflection.MethodInfo> __castRoot;
            int indexer;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
            __castRoot = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__root);
            indexer = __castRoot.selectedIndex;
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(__castRoot.options[indexer].Label);
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        };

        // binding id = 2
        public Action<UIElement, UIElement> Binding_OnUpdate_f16a139b_0c62_41d7_9940_c785ab9eda59 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.Select<System.Reflection.MethodInfo> __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__element.bindingNode.referencedContexts[0]);

            // if="!validSelection"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(!__slotCtx0_cast.validSelection);
        };

        // binding id = 11
        public Action<UIElement, UIElement> Binding_OnCreate_47585015_94b7_4bc1_b0a2_2767dea0814d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __castElement;

            __castElement = ((UIForia.Elements.ScrollView)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseScroll, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                __castElement.OnMouseWheel(__evt);
            });
            __element.inputHandlers.AddDragCreator(UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Capture, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                return __castElement.OnMiddleMouseDrag(__evt);
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 12
        public Action<UIElement, UIElement> Binding_OnUpdate_3463114c_cd4c_4629_8d62_9aac576e323d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.Select<System.Reflection.MethodInfo> __castRoot;
            UIForia.Elements.ScrollView __castElement;

            __castRoot = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__root);

            // if="selecting"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(__castRoot.selecting);
            __castElement = ((UIForia.Elements.ScrollView)__element);

            // disableOverflowX="true"
            __castElement.disableOverflowX = true;
        section_0_0:
        retn:
            return;
        };


        // Slot name="select-display" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\Select.xml        id = 0
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_0_84937086_c7b8_4c95_9c03_599b22fe1099 = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 1, 0, 1));
            slotRoot.slotId = @"select-display";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, -1, -1, 0, -1, @"select-display", scope, 0);

            // new UIForia.Elements.UITextElement 149:10
            targetElement_1 = scope.application.CreateElementFromPoolWithType(226, slotRoot, 0, 0, 1);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 1, -1);
            return slotRoot;
        };

        // Slot name="placeholder" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\Select.xml        id = 1
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_1_b0576a68_7634_4821_9bb3_5cf36be2e6e4 = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 1, 0, 1));
            slotRoot.slotId = @"placeholder";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, -1, -1, 2, -1, @"placeholder", scope, 0);

            // new UIForia.Elements.UITextElement 153:10
            targetElement_1 = scope.application.CreateElementFromPoolWithType(226, slotRoot, 0, 0, 1);
            ((UIForia.Elements.UITextElement)targetElement_1).text = @"Select a value";
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, -1, -1);
            return slotRoot;
        };

    }

}
#pragma warning restore 0164

                