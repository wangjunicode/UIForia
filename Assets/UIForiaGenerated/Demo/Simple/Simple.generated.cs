using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope2, UIElement> Template_4983d08d89d9bfa4d862d9ac7366d6a5 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;

            if (root == null)
            {
                root = scope.application.CreateElementFromPoolWithType(typeof(Demo.Simple), default(UIForia.Elements.UIElement), 4, 0, 0);
            }

            // <Text evt:onMouseClick="ToggleLanguage()" style=="header"/>    '{HeaderText}'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), root, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 0, -1, 1);
            root.children.array[0] = targetElement_1;

            // <Div style=="box box-black"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 2, -1, -1);

            // 
            targetElement_2 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, -1, -1, 3);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[1] = targetElement_1;

            // <Div style=="box box-red"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 4, -1, -1);

            // 
            targetElement_2 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, -1, -1, 5);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[2] = targetElement_1;

            // <Div style=="box box-gold"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), root, 1, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 6, -1, -1);

            // 
            targetElement_2 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, -1, -1, 7);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[3] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnCreate_41969f3d202973f488a0e2688ccafdcf = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_4219fd0cfa46f644f8d540b6b8919cf8 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            Demo.Simple __castRoot;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((Demo.Simple)__root);
            UIForia.Text.TextUtil.StringBuilder.Append(__castRoot.HeaderText.ToString());
            ((UIForia.Elements.UITextElement)__castElement).SetText(UIForia.Text.TextUtil.StringBuilder.ToString());
            UIForia.Text.TextUtil.StringBuilder.Clear();
        };

        public Action<UIElement, UIElement> Binding_OnCreate_533cc72a8d901c1409b103736c99f0d6 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        public Action<UIElement, UIElement> Binding_OnUpdate_54bfde3312de0c242ab3e2f88c268c5e = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        public Action<UIElement, UIElement> Binding_OnCreate_1d706c3a952e47a4c9e914ba83aa762c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        public Action<UIElement, UIElement> Binding_OnUpdate_24de277288e959c4d858f5895c77f22d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        public Action<UIElement, UIElement> Binding_OnCreate_327fe70a6ac59b14d908e1c8c873b436 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        public Action<UIElement, UIElement> Binding_OnUpdate_0b6948b56aacb2a449931ca73c9cd300 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
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

        
    }

}