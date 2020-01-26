using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        
        public Func<UIElement, TemplateScope, UIElement> Template_c88ffa0b92c11c94291f47a413392767 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {
                // new UnityEngine.RepeatALot
                root = scope.application.CreateElementFromPoolWithType(8, default(UIForia.Elements.UIElement), 2, 0, 0);
            }
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // container (from template Documentation/Features/RepeatALot.xml)
            styleList.array[0] = scope.application.GetTemplateMetaData(0).GetStyleById(1);
            root.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, root, root, -1, -1, 0, -1);
            // new UIForia.Elements.UIGroupElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(50, root, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(85, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "I am transformed!";
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[0] = targetElement_1;
            // new UIForia.Elements.UIRepeatCountElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(246, root, 0, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 1, -1);
            ((UIForia.Elements.UIRepeatElement)targetElement_1).templateSpawnId = 0;
            ((UIForia.Elements.UIRepeatElement)targetElement_1).templateContextRoot = root;
            ((UIForia.Elements.UIRepeatElement)targetElement_1).scope = scope;
            ((UIForia.Elements.UIRepeatElement)targetElement_1).indexVarId = 1;
            ((UIForia.Elements.UIRepeatElement)targetElement_1).itemVarId = 2;
            root.children.array[1] = targetElement_1;
            return root;
        }; 
        // binding id = 0
        public Action<UIElement, UIElement> Binding_OnUpdate_8150361b925f0f043a784ea2fcfed4de = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UnityEngine.RepeatALot __castElement;

            __castElement = ((UnityEngine.RepeatALot)__element);
            __castElement.OnUpdate();
        };
// binding id = 1
        public Action<UIElement, UIElement> Binding_OnUpdate_04a52cb352591d04ba8362da83eceff0 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UIRepeatElement<Documentation.DocumentationElements.PlayerData> __castElement;
            UnityEngine.RepeatALot __castRoot;
            System.Collections.Generic.IList<Documentation.DocumentationElements.PlayerData> __right;

            __castElement = ((UIForia.Elements.UIRepeatElement<Documentation.DocumentationElements.PlayerData>)__element);

            // list="players"
            __castRoot = ((UnityEngine.RepeatALot)__root);
            __right = ((System.Collections.Generic.IList<Documentation.DocumentationElements.PlayerData>)__castRoot.players);
            __castElement.list = __right;
        section_0_0:
            __castElement.OnUpdate();
        retn:
            return;
        };
// binding id = 5
        public Action<UIElement, UIElement> Binding_OnUpdate_f542d5ba3544f68428774ff4a0dfd21c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.DocumentationElements.PlayerDetail __castElement;
            UnityEngine.RepeatALot __castRoot;
            Documentation.DocumentationElements.PlayerData repeat_item_item;
            Documentation.DocumentationElements.PlayerData __right;

            __castElement = ((Documentation.DocumentationElements.PlayerDetail)__element);

            // player="$item"
            __castRoot = ((UnityEngine.RepeatALot)__root);
            // item
            repeat_item_item = __castElement.bindingNode.GetRepeatItem<Documentation.DocumentationElements.PlayerData>(2).value;
            __right = repeat_item_item;
            __castElement.player = __right;
        section_0_3:
        retn:
            return;
        };

        
        // Slot name="__template__" id = 0
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Template_0_158057d9b18ddf049ab047062ea04fca = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            // new Documentation.DocumentationElements.PlayerDetail
            targetElement_1 = scope.application.CreateElementFromPoolWithType(33, parent, 3, 0, 0);
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 5, -1);
            return targetElement_1;
        };

    }

}
                