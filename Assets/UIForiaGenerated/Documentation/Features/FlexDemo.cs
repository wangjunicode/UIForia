using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp {
        
        public Func<UIElement, TemplateScope2, UIElement> Template_c9ff60277e0f52c48aac2597c191eed4 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;
            UIForia.Elements.UIElement targetElement_3;
            UIForia.Elements.UIElement targetElement_4;

            if (root == null)
            {
                root = scope.application.CreateElementFromPoolWithType(typeof(Documentation.Features.FlexDemo), default(UIForia.Elements.UIElement), 1, 0);
            }

            // <Div style=="flex-demo"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), root, 2, 0);

            // targetElement_1.style.Add();
            
            // <Heading3 />    'Flex-Start'
            targetElement_2 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIHeading3Element), targetElement_1, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Flex-Start";
            targetElement_1.children.array[0] = targetElement_2;

            // <Group style=="flex-demo-content flex-start col"/>
            targetElement_2 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIGroupElement), targetElement_1, 5, 0);

            // <Div style=="flex-item"/>
            targetElement_3 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), targetElement_2, 1, 0);

            // 
            targetElement_4 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_3, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Item 1";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[0] = targetElement_3;

            // <Div style=="flex-item"/>
            targetElement_3 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), targetElement_2, 1, 0);

            // 
            targetElement_4 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_3, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Item 2";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[1] = targetElement_3;

            // <Div style=="flex-item"/>
            targetElement_3 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), targetElement_2, 1, 0);

            // 
            targetElement_4 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_3, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Item 3";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[2] = targetElement_3;

            // <Div style=="flex-item"/>
            targetElement_3 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), targetElement_2, 1, 0);

            // 
            targetElement_4 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_3, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Item 4";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[3] = targetElement_3;

            // <Div style=="flex-item"/>
            targetElement_3 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UIDivElement), targetElement_2, 1, 0);

            // 
            targetElement_4 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), targetElement_3, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Item 5";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[4] = targetElement_3;
            targetElement_1.children.array[1] = targetElement_2;
            root.children.array[0] = targetElement_1;
            return root;
        }; 
        
        
    }

}