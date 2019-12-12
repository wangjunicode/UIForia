using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope, UIElement> Template_59e50219eab7cd247bc92ebfdfe5984c = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;

            if (root == null)
            {

                // new Demo.Simple
                root = scope.application.CreateElementFromPoolWithType(41, default(UIForia.Elements.UIElement), 1, 0, 0);
            }

            // <Repeat list="words"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(180, root, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, default(UIForia.Elements.UIElement), -1, -1, 1);

            // BuiltInElements/Repeat.xml
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>), default(UIForia.Elements.UIElement)));
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnUpdate_e0f1cf8d9bc5e6945bc5a81377c16319 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        
    }

}