using System;
using UIForia.Compilers;
using UIForia.Elements;
#pragma warning disable 0164

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        // C:\Users\Matt\RiderProjects\UIForia\Assets\CompilerDemo\Tmp.xml
        public Func<UIElement, TemplateScope, UIElement> Template_085de499_af7d_4253_b66f_7252e20cfb3c = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Compilers.TemplateScope hydrateScope;

            // new Tmp
            root = scope.application.CreateElementFromPoolWithType(233, default(UIForia.Elements.UIElement), 3, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, root, root, -1, -1, -1, -1);

            // new UIForia.Elements.Select<System.Reflection.MethodInfo> 32:10
            targetElement_1 = scope.application.CreateElementFromPoolWithType(283, root, 4, 0, 0);
            hydrateScope = new UIForia.Compilers.TemplateScope(scope.application);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, 13, -1, 14, 15);
            scope.application.HydrateTemplate(1, targetElement_1, hydrateScope);

            // new UIForia.Elements.UITextElement 34:10
            targetElement_1 = scope.application.CreateElementFromPoolWithType(226, root, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 16, -1);

            // new UIForia.Elements.ScrollView 36:10
            targetElement_1 = scope.application.CreateElementFromPoolWithType(214, root, 5, 0, 0);
            hydrateScope = new UIForia.Compilers.TemplateScope(scope.application);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, 17, -1, -1, -1);
            scope.application.HydrateTemplate(2, targetElement_1, hydrateScope);
            return root;
        }; 

        // binding id = 13
        public Action<UIElement, UIElement> Binding_OnCreate_cc6b9418_fbb0_47ce_8671_551a5fe9c6a2 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Tmp __castRoot;
            UIForia.Elements.Select<System.Reflection.MethodInfo> __castElement;

            __castRoot = ((Tmp)__root);
            __element.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<System.Reflection.MethodInfo>(1, @"selectedValue", default(System.Reflection.MethodInfo)));
            __element.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<System.Reflection.MethodInfo>(2, @"sync_selectedValue", default(System.Reflection.MethodInfo)));
            __castElement = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, true, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Joystick8Button19, '\0', (UIForia.UIInput.KeyboardInputEvent __evt) =>
            {
                __castElement.OnKeyDownNavigate(__evt);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyHeldDown, UIForia.UIInput.KeyboardModifiers.None, true, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Joystick8Button19, '\0', (UIForia.UIInput.KeyboardInputEvent __evt) =>
            {
                __castElement.OnKeyboardNavigate(__evt);
            });
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                __castElement.BeginSelecting(__evt);
            });
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseMove, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                __castElement.OnMouseMove();
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 14
        public Action<UIElement, UIElement> Binding_OnUpdate_f1a9fd4c_7a93_49b7_8fe9_9b32af213f99 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.Select<System.Reflection.MethodInfo> __castElement;
            Tmp __castRoot;
            System.Reflection.MethodInfo __right;
            UIForia.Systems.ContextVariable<System.Reflection.MethodInfo> changeHandler_selectedValue;
            System.Reflection.MethodInfo __oldVal;
            UIForia.Systems.ContextVariable<System.Reflection.MethodInfo> sync_selectedValue;
            System.Collections.Generic.IList<UIForia.Elements.ISelectOption<System.Reflection.MethodInfo>> __right_0;
            System.Collections.Generic.IList<UIForia.Elements.ISelectOption<System.Reflection.MethodInfo>> __oldVal_0;

            __castElement = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__element);

            // selectedValue="selected"
            __castRoot = ((Tmp)__root);
            __right = __castRoot.selected;
            // selectedValue
            changeHandler_selectedValue = ((UIForia.Systems.ContextVariable<System.Reflection.MethodInfo>)__castElement.bindingNode.GetContextVariable(1));
            changeHandler_selectedValue.value = __right;
            __oldVal = __castElement.selectedValue;
            if (__castElement.selectedValue != __right)
            {
                __castElement.selectedValue = __right;
                __castElement.OnSelectedValueChanged();
            }
            // selectedValue
            sync_selectedValue = ((UIForia.Systems.ContextVariable<System.Reflection.MethodInfo>)__element.bindingNode.GetContextVariable(2));
            sync_selectedValue.value = __right;
        section_0_1:

            // options="options"
            __right_0 = ((System.Collections.Generic.IList<UIForia.Elements.ISelectOption<System.Reflection.MethodInfo>>)__castRoot.options);
            __oldVal_0 = __castElement.options;
            if (__castElement.options != __right_0)
            {
                __castElement.options = __right_0;
                __castElement.OnSelectionChanged();
            }
        section_0_2:
        retn:
            return;
        };

        // binding id = 15
        public Action<UIElement, UIElement> Binding_OnLateUpdate_4ccf6fd8_9f67_4ebc_9a35_20094eecdcbf = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.Select<System.Reflection.MethodInfo> __castElement;
            Tmp __castRoot;
            System.Reflection.MethodInfo __right;
            System.Reflection.MethodInfo sync_selectedValue;
            UIForia.Systems.ContextVariable<System.Reflection.MethodInfo> changeHandler_selectedValue;

            __castElement = ((UIForia.Elements.Select<System.Reflection.MethodInfo>)__element);
            __castRoot = ((Tmp)__root);
            __right = __castRoot.selected;
            sync_selectedValue = ((UIForia.Systems.ContextVariable<System.Reflection.MethodInfo>)__element.bindingNode.GetContextVariable(2)).value;
            if (sync_selectedValue == __right)
            {
                __castRoot.selected = __castElement.selectedValue;
            }
        section_0_0:
            // selectedValue
            changeHandler_selectedValue = ((UIForia.Systems.ContextVariable<System.Reflection.MethodInfo>)__castElement.bindingNode.GetContextVariable(1));
            if (changeHandler_selectedValue.value != __castElement.selectedValue)
            {
                __castRoot.OnValueChanged();
            }
        retn:
            return;
        };

        // binding id = 16
        public Action<UIElement, UIElement> Binding_OnUpdate_e80aa9ca_a901_454f_afa3_26d895d1054c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Tmp __castRoot;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
            __castRoot = ((Tmp)__root);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(__castRoot.expression);
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        };

        // binding id = 17
        public Action<UIElement, UIElement> Binding_OnCreate_b10ed04b_3e6e_48e0_9957_d53c4d4568cc = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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


    }

}
#pragma warning restore 0164

                