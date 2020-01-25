using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        
        public Func<UIElement, TemplateScope, UIElement> Template_a96bf8a5a021ff544a3f52e43c4343f5 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;
            UIForia.Elements.UIElement targetElement_1;

            if (root == null)
            {
                // new UIForia.Elements.InputElement<float>
                root = scope.application.CreateElementFromPoolWithType(235, default(UIForia.Elements.UIElement), 2, 0, 1);
            }
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // input
            styleList.array[0] = root.templateMetaData.GetStyleById(0);
            root.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, root, root, -1, -1, 43, -1);
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(85, root, 0, 1, 1);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("id", "input-element-text");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // input-element-text
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(1);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            root.children.array[0] = targetElement_1;
            targetElement_1 = scope.application.CreateSlot2("placeholder", scope, 0, root, root);
            root.children.array[1] = targetElement_1;
            return root;
        }; 
        // binding id = 43
        public Action<UIElement, UIElement> Binding_OnUpdate_6bd8f714b1b927743a3a4835debff7d7 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<float> __castElement;

            __castElement = ((UIForia.Elements.InputElement<float>)__element);
            __castElement.OnUpdate();
        };
// binding id = 44
        public Action<UIElement, UIElement> Binding_OnUpdate_60c411e3d38c0f643a7c969603a00ee2 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<float> __castRoot;
            UIForia.Elements.UITextElement __castElement;

            __castRoot = ((UIForia.Elements.InputElement<float>)__root);
            __castElement = ((UIForia.Elements.UITextElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(__castRoot.placeholder);
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        };

        
        // Slot name="placeholder" (Default) id = 0
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Default_0_e788ac9467bc2224cacdcbd68e444c35 = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotOverride slotRoot;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            slotRoot = ((UIForia.Elements.UISlotOverride)scope.application.CreateElementFromPoolWithType(83, parent, 1, 0, 1));
            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(85, slotRoot, 0, 1, 1);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("id", "placeholder-text");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // placeholder
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(2);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, slotRoot, targetElement_1, root, -1, -1, 44, -1);
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}
                