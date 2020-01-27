using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        // Documentation/Features/AnimationDemo.xml
        public Func<UIElement, TemplateScope, UIElement> Template_2fcb9b0a8b6e2454b88a9f96d1520de9 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;
            UIForia.Elements.UIElement targetElement_1;

            // new Documentation.Features.AnimationDemo
            root = scope.application.CreateElementFromPoolWithType(12, default(UIForia.Elements.UIElement), 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // container (from template Documentation/Features/AnimationDemo.xml)
            styleList.array[0] = scope.application.GetTemplateMetaData(0).GetStyleById(9);
            root.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.InputElement<int>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(246, root, 1, 0, 0);
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // input (from template Elements/InputElement.xml)
            styleList.array[0] = scope.application.GetTemplateMetaData(1).GetStyleById(0);
            // input (from template Documentation/Features/AnimationDemo.xml)
            styleList.array[1] = scope.application.GetTemplateMetaData(0).GetStyleById(11);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, 0, -1, 1, 2);
            root.children.array[0] = targetElement_1;
            return root;
        }; 

        // binding id = 0
        public Action<UIElement, UIElement> Binding_OnCreate_09a97b00ac6698142baa6cc738a9f0c5 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castElement;

            __castElement = ((UIForia.Elements.InputElement<int>)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnMouseClick(__evt.AsMouseInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Joystick8Button19, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.EnterText(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Home, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleHome(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.End, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleEnd(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Backspace, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleBackspace(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyHeldDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Backspace, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleBackspaceHeld(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyHeldDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Delete, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleDeleteHeld(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.Delete, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleDelete(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyHeldDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.LeftArrow, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleLeftArrowHeld(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.LeftArrow, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleLeftArrowDown(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyHeldDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.RightArrow, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleRightArrowHeld(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.RightArrow, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleRightArrow(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.Control, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.C, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleCopy(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.Control, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.X, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleCut(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.Control, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.V, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandlePaste(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddKeyboardEvent(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.Control, false, UIForia.UIInput.EventPhase.Bubble, UnityEngine.KeyCode.A, '\0', (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.HandleSelectAll(__evt.AsKeyInputEvent);
            });
            __element.inputHandlers.AddDragCreator(UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.MouseInputEvent __evt) =>
            {
                return __castElement.CreateDragEvent(__evt);
            });
            __element.application.InputSystem.RegisterKeyboardHandler(__element);
        };

        // binding id = 1
        public Action<UIElement, UIElement> Binding_OnUpdate_b71c413220f58ef48b78d272da06c362 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castElement;
            Documentation.Features.AnimationDemo __castRoot;
            int __right;

            __castElement = ((UIForia.Elements.InputElement<int>)__element);

            // value="iterations"
            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __right = __castRoot.iterations;
            __castElement.value = __right;
        section_0_0:
            __castElement.OnUpdate();
        retn:
            return;
        };

        // binding id = 2
        public Action<UIElement, UIElement> Binding_OnLateUpdate_bc49db6fadc63434eb73cf8c61780a57 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castElement;
            Documentation.Features.AnimationDemo __castRoot;

            __castElement = ((UIForia.Elements.InputElement<int>)__element);
            __castRoot = ((Documentation.Features.AnimationDemo)__root);

            // value="iterations"
            __castRoot.iterations = __castElement.value;
        section_0_0:
        retn:
            return;
        };


    }

}
                