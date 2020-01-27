using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        // Elements/InputElement.xml
        public Func<UIElement, TemplateScope, UIElement> Template_d5a95c44b9599244c9c60e0cc53caef6 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            // new UIForia.Elements.UITextElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(85, root, 0, 1, 1);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("id", "input-element-text");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // input-element-text (from template Elements/InputElement.xml)
            styleList.array[0] = scope.application.GetTemplateMetaData(1).GetStyleById(1);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            root.children.array[0] = targetElement_1;
            return root;
        }; 


    }

}
                