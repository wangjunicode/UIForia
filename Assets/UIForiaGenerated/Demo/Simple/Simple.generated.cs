using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_c557efc21297629428d443404b6bd07b = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.StructList<UIForia.Compilers.SlotUsage> slotUsage;

            if (root == null)
            {

                // new Demo.Simple
                root = scope.application.CreateElementFromPoolWithType(41, default(UIForia.Elements.UIElement), 1, 0, 0);
            }

            // <Repeat list="words"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(180, root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 0);
            slotUsage = UIForia.Util.StructList<UIForia.Compilers.SlotUsage>.PreSize(1);
            // Slot_Children_Children_421d5557a5c700842a20beaa6ee1995c
            slotUsage.array[0] = new UIForia.Compilers.SlotUsage("Children", 1);

            // BuiltInElements/Repeat.xml
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, slotUsage, root));
            slotUsage.Release();
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnUpdate_f0e9f42ba2deb6949b53b910c9586353 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            BuiltInElements.Repeat<string> __castElement;
            Demo.Simple __castRoot;
            System.Collections.Generic.List<string> __right;

            __castElement = ((BuiltInElements.Repeat<string>)__element);
            __castRoot = ((Demo.Simple)__root);

            // list="words"
            __right = __castRoot.words;
            __castElement.list = __right;
        section_0_0:
            __castElement.OnUpdate();
        retn:
            return;
        };

        
        public Func<UIElement, TemplateScope, UIElement> Slot_Children_Children_421d5557a5c700842a20beaa6ee1995c = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(77, default(UIForia.Elements.UIElement), 1, 0, 0));

            // 
            targetElement_1 = scope.application.CreateElementFromPoolWithType(78, slotRoot, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Text";
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}