using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        
        public Func<UIElement, TemplateScope, UIElement> Template_8b7861935f17af54498cbc103e1945b1 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;
            UIForia.Elements.UIElement targetElement_1;

            if (root == null)
            {
                // new UIForia.Elements.InputElement<int>
                root = scope.application.CreateElementFromPoolWithType(236, default(UIForia.Elements.UIElement), 2, 0, 2);
            }
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // input
            styleList.array[0] = root.templateMetaData.GetStyleById(0);
            root.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, root, root, -1, -1, 51, -1);
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(85, root, 0, 1, 2);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("id", "input-element-text");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // input-element-text
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(1);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            root.children.array[0] = targetElement_1;
            targetElement_1 = scope.application.CreateSlot2("placeholder", scope, 1, root, root);
            root.children.array[1] = targetElement_1;
            return root;
        }; 
        // binding id = 51
        public Action<UIElement, UIElement> Binding_OnUpdate_13b9cac6703e52d48a39152c815da900 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castElement;

            __castElement = ((UIForia.Elements.InputElement<int>)__element);
            __castElement.OnUpdate();
        };
// binding id = 52
        public Action<UIElement, UIElement> Binding_OnUpdate_7403f7cddc1dadb4d98603b5551dc689 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castRoot;
            UIForia.Elements.UITextElement __castElement;

            __castRoot = ((UIForia.Elements.InputElement<int>)__root);
            __castElement = ((UIForia.Elements.UITextElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(__castRoot.placeholder);
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        };

        
        // Slot name="placeholder" (Default) id = 1
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Default_1_5bcb89aac0b3d834fa4b502fff73dfde = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotOverride slotRoot;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            slotRoot = ((UIForia.Elements.UISlotOverride)scope.application.CreateElementFromPoolWithType(83, parent, 1, 0, 2));
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(85, slotRoot, 0, 1, 2);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("id", "placeholder-text");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // placeholder
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(2);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, slotRoot, targetElement_1, root, -1, -1, 52, -1);
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}
                