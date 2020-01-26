using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        
        public Func<UIElement, TemplateScope, UIElement> Template_8a8ee51d452844e4499fc5da9b82c737 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {
                // new Documentation.DocumentationElements.PlayerDetail
                root = scope.application.CreateElementFromPoolWithType(33, default(UIForia.Elements.UIElement), 3, 0, 1);
            }
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            root.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(53, root, 1, 0, 1);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(85, targetElement_1, 0, 0, 1);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 2, -1);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[0] = targetElement_1;
            // new UIForia.Elements.UIHeading1Element
            targetElement_1 = scope.application.CreateElementFromPoolWithType(58, root, 0, 0, 1);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Friends:";
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            root.children.array[1] = targetElement_1;
            // new UIForia.Elements.UIGroupElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(50, root, 1, 0, 1);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // friend-container (from template Documentation/DocumentationElements/PlayerDetail.xml)
            styleList.array[0] = scope.application.GetTemplateMetaData(1).GetStyleById(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIRepeatCountElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(246, targetElement_1, 0, 0, 1);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 3, -1);
            ((UIForia.Elements.UIRepeatElement)targetElement_2).templateSpawnId = 1;
            ((UIForia.Elements.UIRepeatElement)targetElement_2).templateContextRoot = root;
            ((UIForia.Elements.UIRepeatElement)targetElement_2).scope = scope;
            ((UIForia.Elements.UIRepeatElement)targetElement_2).indexVarId = 3;
            ((UIForia.Elements.UIRepeatElement)targetElement_2).itemVarId = 4;
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[2] = targetElement_1;
            return root;
        }; 
        // binding id = 2
        public Action<UIElement, UIElement> Binding_OnUpdate_3274fd0009ce5704d983b315cccb7d28 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.DocumentationElements.PlayerDetail __castRoot;
            UIForia.Elements.UITextElement __castElement;
            Documentation.DocumentationElements.PlayerData nullCheck;
            Documentation.DocumentationElements.PlayerData nullCheck_0;

            __castRoot = ((Documentation.DocumentationElements.PlayerDetail)__root);
            __castElement = ((UIForia.Elements.UITextElement)__element);
            nullCheck = __castRoot.player;
            if (nullCheck == null)
            {
                goto retn;
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(nullCheck.id);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append("  ");
            nullCheck_0 = __castRoot.player;
            if (nullCheck_0 == null)
            {
                goto retn;
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(nullCheck_0.name);
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        retn:
            return;
        };
// binding id = 3
        public Action<UIElement, UIElement> Binding_OnUpdate_05024e36d3c875c4ab53d4d2a7204bc8 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UIRepeatElement<Documentation.DocumentationElements.PlayerData> __castElement;
            Documentation.DocumentationElements.PlayerDetail __castRoot;
            Documentation.DocumentationElements.PlayerData nullCheck;
            System.Collections.Generic.IList<Documentation.DocumentationElements.PlayerData> __right;

            __castElement = ((UIForia.Elements.UIRepeatElement<Documentation.DocumentationElements.PlayerData>)__element);

            // list="player.friends"
            __castRoot = ((Documentation.DocumentationElements.PlayerDetail)__root);
            nullCheck = __castRoot.player;
            if (nullCheck == null)
            {
                goto section_0_1;
            }
            __right = ((System.Collections.Generic.IList<Documentation.DocumentationElements.PlayerData>)nullCheck.friends);
            __castElement.list = __right;
        section_0_1:
            __castElement.OnUpdate();
        retn:
            return;
        };
// binding id = 4
        public Action<UIElement, UIElement> Binding_OnUpdate_1e0ea6674cc3063499349365bb284fbe = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.DocumentationElements.PlayerDetail __castElement;
            Documentation.DocumentationElements.PlayerDetail __castRoot;
            Documentation.DocumentationElements.PlayerData repeat_item_item;
            Documentation.DocumentationElements.PlayerData __right;

            __castElement = ((Documentation.DocumentationElements.PlayerDetail)__element);

            // player="$item"
            __castRoot = ((Documentation.DocumentationElements.PlayerDetail)__root);
            // item
            repeat_item_item = __castElement.bindingNode.GetRepeatItem<Documentation.DocumentationElements.PlayerData>(4).value;
            __right = repeat_item_item;
            __castElement.player = __right;
        section_0_2:
        retn:
            return;
        };

        
        // Slot name="__template__" id = 1
        public Func<UIElement, UIElement, TemplateScope, UIElement> Slot_Template_1_6139291d0b413b344a737cb9b4b1ff00 = (UIForia.Elements.UIElement root, UIForia.Elements.UIElement parent, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            // new Documentation.DocumentationElements.PlayerDetail
            targetElement_1 = scope.application.CreateElementFromPoolWithType(33, parent, 3, 0, 1);
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 4, -1);
            return targetElement_1;
        };

    }

}
                