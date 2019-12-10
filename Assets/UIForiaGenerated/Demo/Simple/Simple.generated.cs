using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_26ce8b7cec8f0b74fb351221db057e73 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.StructList<UIForia.Compilers.SlotUsage> slotUsage;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {

                // new Demo.Simple
                root = scope.application.CreateElementFromPoolWithType(41, default(UIForia.Elements.UIElement), 5, 0, 0);
            }

            // <Text mouse:click="ToggleLanguage()" style="header"/>    '{HeaderText}'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(78, root, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 0, -1, 1);
            root.children.array[0] = targetElement_1;

            // <Repeat list="words"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(180, root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, -1, -1, 4);
            slotUsage = UIForia.Util.StructList<UIForia.Compilers.SlotUsage>.PreSize(1);
            // Slot_Children_Children_f534fec48178ddb49bee35b7b7dbc42e
            slotUsage.array[0] = new UIForia.Compilers.SlotUsage("Children", 0);

            // BuiltInElements/Repeat.xml
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, slotUsage));
            slotUsage.Release();
            root.children.array[1] = targetElement_1;

            // <Div style="box box-black"/>

            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(52, root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 6, -1, -1);

            // 
            targetElement_2 = scope.application.CreateElementFromPoolWithType(78, targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, -1, -1, 7);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[2] = targetElement_1;

            // <Div style="box box-red"/>

            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(52, root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 8, -1, -1);

            // 
            targetElement_2 = scope.application.CreateElementFromPoolWithType(78, targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, -1, -1, 9);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[3] = targetElement_1;

            // <Div style="box box-gold"/>

            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(52, root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 10, -1, -1);

            // 
            targetElement_2 = scope.application.CreateElementFromPoolWithType(78, targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, -1, -1, 11);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[4] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnCreate_fa31f8094b181cf4c982850475d27b4e = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // header
            styleList.array[0] = __element.templateMetaData.GetStyleById(1);
            __element.style.internal_Initialize(styleList);
            __castElement.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __castElement.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ToggleLanguage();
            });
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_7825b51faad674042954fb98fb9c08b9 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            UIForia.Text.TextUtil.StringBuilder.Append(__castRoot.HeaderText.ToString());
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_edd5fe3c9d71c6743b264619de0290b1 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            BuiltInElements.Repeat<string> __castElement;
            Demo.Simple __castRoot;
            System.Collections.Generic.List<string> __right;

            __castElement = ((BuiltInElements.Repeat<string>)__element);
            __castRoot = ((Demo.Simple)__root);

            // list="words"
            __right = __castRoot.words;
            __castElement.list = __right;
        section_0_1:
            __castElement.OnUpdate();
        retn:
            return;
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_2ff67f44b1581d04f83ef7c40d401b02 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;
            string ctxvar_item;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            ctxvar_item = ((UIForia.Systems.ContextVariable<string>)__castElement.bindingNode.GetContextVariable(2)).value;
            UIForia.Text.TextUtil.StringBuilder.Append(ctxvar_item.ToString());
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        };

        public Action<UIElement, UIElement> Binding_OnCreate_705878f8b6060af45b83cd90cbafc5fc = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UIDivElement __castElement;
            Demo.Simple __castRoot;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            __castElement = ((UIForia.Elements.UIDivElement)__element);
            __castRoot = ((Demo.Simple)__root);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // box
            styleList.array[0] = __element.templateMetaData.GetStyleById(2);
            // box-black
            styleList.array[1] = __element.templateMetaData.GetStyleById(3);
            __element.style.internal_Initialize(styleList);
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_71fc4338b0c5e9f498b09d2c319fc430 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            UIForia.Text.TextUtil.StringBuilder.Append("Clicked ");
            UIForia.Text.TextUtil.StringBuilder.Append(__castRoot.boxClicks1.ToString());
            UIForia.Text.TextUtil.StringBuilder.Append(" times");
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        };

        public Action<UIElement, UIElement> Binding_OnCreate_4ae708f41b575c54a818c38125574905 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UIDivElement __castElement;
            Demo.Simple __castRoot;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            __castElement = ((UIForia.Elements.UIDivElement)__element);
            __castRoot = ((Demo.Simple)__root);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // box
            styleList.array[0] = __element.templateMetaData.GetStyleById(2);
            // box-red
            styleList.array[1] = __element.templateMetaData.GetStyleById(4);
            __element.style.internal_Initialize(styleList);
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_3a67287d08f6a5f4aa77303837cb4124 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            UIForia.Text.TextUtil.StringBuilder.Append("Clicked ");
            UIForia.Text.TextUtil.StringBuilder.Append(__castRoot.boxClicks2.ToString());
            UIForia.Text.TextUtil.StringBuilder.Append(" times");
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        };

        public Action<UIElement, UIElement> Binding_OnCreate_401ff9034ef3d324f94176b8b8ec515a = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UIDivElement __castElement;
            Demo.Simple __castRoot;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            __castElement = ((UIForia.Elements.UIDivElement)__element);
            __castRoot = ((Demo.Simple)__root);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // box
            styleList.array[0] = __element.templateMetaData.GetStyleById(2);
            // box-gold
            styleList.array[1] = __element.templateMetaData.GetStyleById(5);
            __element.style.internal_Initialize(styleList);
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_2b232864b8913644ba023e668988f4e5 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            UIForia.Text.TextUtil.StringBuilder.Append("Clicked ");
            UIForia.Text.TextUtil.StringBuilder.Append(__castRoot.boxClicks3.ToString());
            UIForia.Text.TextUtil.StringBuilder.Append(" times");
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        };

        
        public Func<UIElement, TemplateScope, UIElement> Slot_Children_Children_f534fec48178ddb49bee35b7b7dbc42e = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(77, default(UIForia.Elements.UIElement), 1, 0, 0));

            // 
            targetElement_1 = scope.application.CreateElementFromPoolWithType(78, slotRoot, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, slotRoot, targetElement_1, -1, -1, 5);
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}