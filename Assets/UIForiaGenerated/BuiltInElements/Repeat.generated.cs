using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_2588392126050a74bb87adad4b2708df = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;

            if (root == null)
            {

                // new BuiltInElements.Repeat`1[System.String]
                root = scope.application.CreateElementFromPoolWithType(180, default(UIForia.Elements.UIElement), 1, 0, 1);
            }

            // <Children ctx:length="list.Count" count="list != null ? list.Count : 0" onSlotCreated="SlotCreated()" onSlotDestroyed="SlotDestroyed()" ctx:item="list[$element.siblingIndex]" ctx:list="list"/>
            // new SlotTemplateElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(64, root, 0, 0, 1);
            ((UIForia.Elements.SlotTemplateElement)targetElement_1).templateId = UIForia.Application.ResolveSlotId("Children", scope.slotInputs, -1);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 2, -1, 3);
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnCreate_d49966d602ffedd4a9f9bd90e4b15ffe = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.SlotTemplateElement __castElement;
            BuiltInElements.Repeat<string> __castRoot;
            System.Action evtFn;
            System.Action evtFn_0;

            __castElement = ((UIForia.Elements.SlotTemplateElement)__element);
            __castRoot = ((BuiltInElements.Repeat<string>)__root);
            __castElement.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<int>(1, "length"));
            evtFn = () =>
            {
                __castRoot.SlotCreated();
            };
            __castElement.onSlotCreated += evtFn;
            evtFn_0 = () =>
            {
                __castRoot.SlotDestroyed();
            };
            __castElement.onSlotDestroyed += evtFn_0;
            __castElement.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<string>(2, "item"));
            __castElement.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<System.Collections.Generic.List<string>>(3, "list"));
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_b81ef071d886d0842871a8c509949b54 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.SlotTemplateElement __castElement;
            BuiltInElements.Repeat<string> __castRoot;
            UIForia.Systems.ContextVariable<int> ctxVar_length;
            System.Collections.Generic.List<string> nullCheck;
            int ternaryOutput;
            int __right;
            UIForia.Systems.ContextVariable<string> ctxVar_item;
            System.Collections.Generic.List<string> nullCheck_1;
            int indexer;
            UIForia.Systems.ContextVariable<System.Collections.Generic.List<string>> ctxVar_list;

            __castElement = ((UIForia.Elements.SlotTemplateElement)__element);
            __castRoot = ((BuiltInElements.Repeat<string>)__root);
            ctxVar_length = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetLocalContextVariable("length"));
            nullCheck = __castRoot.list;
            if (nullCheck == null)
            {
                goto retn;
            }
            ctxVar_length.value = nullCheck.Count;

            // count="list != null ? list.Count : 0"
            if (__castRoot.list != null)
            {
                System.Collections.Generic.List<string> nullCheck_0;

                nullCheck_0 = __castRoot.list;
                if (nullCheck_0 == null)
                {
                    goto section_0_0;
                }
                ternaryOutput = nullCheck_0.Count;
            }
            else
            {
                ternaryOutput = 0;
            }
            __right = ternaryOutput;
            if (__castElement.count != __right)
            {
                __castElement.count = __right;
                __castElement.OnCountChanged();
            }
        section_0_0:
            ctxVar_item = ((UIForia.Systems.ContextVariable<string>)__castElement.bindingNode.GetLocalContextVariable("item"));
            nullCheck_1 = __castRoot.list;
            if (nullCheck_1 == null)
            {
                goto retn;
            }
            indexer = __castElement.siblingIndex;
            if ((indexer < 0) || (indexer >= nullCheck_1.Count))
            {
                goto retn;
            }
            ctxVar_item.value = nullCheck_1[indexer];
            ctxVar_list = ((UIForia.Systems.ContextVariable<System.Collections.Generic.List<string>>)__castElement.bindingNode.GetLocalContextVariable("list"));
            ctxVar_list.value = __castRoot.list;
        retn:
            return;
        };

        
    }

}