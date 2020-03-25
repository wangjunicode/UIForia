using System;
using UIForia.Compilers;
using UIForia.Elements;
#pragma warning disable 0164

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        // C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\ScrollView.xml
        public Func<UIElement, TemplateScope, UIElement> Template_0d573934_05a5_45f3_ae53_db84632f1f39 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;

            targetElement_1 = scope.application.CreateSlot(@"Children", scope, 2, root, root);
            targetElement_1 = scope.application.CreateSlot(@"vertical-scrollbar", scope, 3, root, root);
            targetElement_1 = scope.application.CreateSlot(@"vertical-scrollbar-handle", scope, 4, root, root);
            targetElement_1 = scope.application.CreateSlot(@"horizontal-scrollbar", scope, 5, root, root);
            targetElement_1 = scope.application.CreateSlot(@"horizontal-handle", scope, 6, root, root);
            return root;
        }; 

        // binding id = 3
        public Action<UIElement, UIElement> Binding_OnCreate_7a7fca2b_4b6f_4e55_8d81_5ab1c8680d93 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                __slotCtx0_cast.OnClickVertical(__evt);
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 4
        public Action<UIElement, UIElement> Binding_OnUpdate_af6358e0_81f7_40a7_9ef4_0e352b37ecf5 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);

            // if="verticalScrollingEnabled"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(__slotCtx0_cast.verticalScrollingEnabled);
        };

        // binding id = 5
        public Action<UIElement, UIElement> Binding_OnCreate_177be235_d4b1_4c61_842f_59728c4d061f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddDragCreator(UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                return __slotCtx0_cast.OnCreateVerticalDrag(__evt);
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 6
        public Action<UIElement, UIElement> Binding_OnUpdate_5a3f1290_cc75_4e7d_bd43_595ae5847a8b = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);

            // if="verticalScrollingEnabled"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(__slotCtx0_cast.verticalScrollingEnabled);
        };

        // binding id = 7
        public Action<UIElement, UIElement> Binding_OnCreate_16eb10da_4aef_49b4_9582_b815aaee2d43 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                __slotCtx0_cast.OnClickHorizontal(__evt);
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 8
        public Action<UIElement, UIElement> Binding_OnUpdate_d1bd509f_07e0_4ffc_a7e6_d2006235348d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);

            // if="horizontalScrollingEnabled"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(__slotCtx0_cast.horizontalScrollingEnabled);
        };

        // binding id = 9
        public Action<UIElement, UIElement> Binding_OnCreate_558d71dd_b6e5_42ed_b32a_967eee441061 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddDragCreator(UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                return __slotCtx0_cast.OnCreateHorizontalDrag(__evt);
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 10
        public Action<UIElement, UIElement> Binding_OnUpdate_ea3bb75a_e825_4fe7_8126_f1b2145e214c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.ScrollView __slotCtx0_cast;

            __slotCtx0_cast = ((UIForia.Elements.ScrollView)__element.bindingNode.referencedContexts[0]);

            // if="horizontalScrollingEnabled"
            __element.internal__dontcallmeplease_SetEnabledIfBinding(__slotCtx0_cast.horizontalScrollingEnabled);
        };


        // Slot name="Children" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\ScrollView.xml        id = 2
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_2_b4e34b45_623f_4834_abd7_ced2a2f3b9ba = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 0, 0, 2));
            slotRoot.slotId = @"Children";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, -1, -1, -1, -1, @"Children", scope, 0);
            return slotRoot;
        };

        // Slot name="vertical-scrollbar" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\ScrollView.xml        id = 3
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_3_4a6eb501_f843_4bed_822a_e76516bf86dd = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 0, 0, 2));
            slotRoot.slotId = @"vertical-scrollbar";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, 3, -1, 4, -1, @"vertical-scrollbar", scope, 0);
            return slotRoot;
        };

        // Slot name="vertical-scrollbar-handle" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\ScrollView.xml        id = 4
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_4_6b405a98_8af0_4640_a04e_41b4dc27bcb3 = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 0, 0, 2));
            slotRoot.slotId = @"vertical-scrollbar-handle";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, 5, -1, 6, -1, @"vertical-scrollbar-handle", scope, 0);
            return slotRoot;
        };

        // Slot name="horizontal-scrollbar" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\ScrollView.xml        id = 5
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_5_63a50f47_ea8b_4a4a_a1af_336e01f8ce6c = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 0, 0, 2));
            slotRoot.slotId = @"horizontal-scrollbar";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, 7, -1, 8, -1, @"horizontal-scrollbar", scope, 0);
            return slotRoot;
        };

        // Slot name="horizontal-handle" (Define) C:\Users\Matt\RiderProjects\UIForia\Packages\UIForia\Src\Elements\ScrollView.xml        id = 6
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Define_6_5428b8be_d2b7_4f7a_877f_6626b715106c = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotDefinition slotRoot;

            slotRoot = ((UIForia.Elements.UISlotDefinition)scope.application.CreateElementFromPoolWithType(223, parent, 0, 0, 2));
            slotRoot.slotId = @"horizontal-handle";
            UIForia.Systems.LinqBindingNode.GetSlotNode(scope.application, root, slotRoot, root, 9, -1, 10, -1, @"horizontal-handle", scope, 0);
            return slotRoot;
        };

    }

}
#pragma warning restore 0164

                