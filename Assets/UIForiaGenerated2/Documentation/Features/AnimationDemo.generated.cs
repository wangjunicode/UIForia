using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_GameApp2 {
        
        public Func<UIElement, TemplateScope, UIElement> Template_76c28ddd16392524a91a6698ec1d038e = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope scope) =>
        {
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Elements.UIElement targetElement_2;
            UIForia.Elements.UIElement targetElement_3;
            UIForia.Elements.UIElement targetElement_4;

            if (root == null)
            {
                // new Documentation.Features.AnimationDemo
                root = scope.application.CreateElementFromPoolWithType(12, default(UIForia.Elements.UIElement), 9, 0, 0);
            }
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // container
            styleList.array[0] = root.templateMetaData.GetStyleById(9);
            root.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(53, root, 1, 0, 0);
            // new UIForia.Elements.UITextElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(85, targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 0, -1);
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[0] = targetElement_1;
            // new UIForia.Elements.UIHeading2Element
            targetElement_1 = scope.application.CreateElementFromPoolWithType(59, root, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Click on an animation type to see the result";
            root.children.array[1] = targetElement_1;
            // new UIForia.Elements.UIGroupElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(50, root, 38, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button-container
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(4);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 1, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Bounce";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[0] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 2, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Flash";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[1] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 3, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Pulse";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[2] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 4, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Rubber Band";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[3] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 5, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Shake";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[4] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 6, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Head Shake";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[5] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 7, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Swing";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[6] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 8, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Tada";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[7] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 9, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "Wobble";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[8] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 10, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "heartBeat";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[9] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 11, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceIn";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[10] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 12, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceOut";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[11] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 13, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceInDown";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[12] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 14, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceInUp";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[13] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 15, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceInLeft";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[14] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 16, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceInRight";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[15] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 17, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceOutDown";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[16] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 18, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceOutLeft";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[17] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 19, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceOutRight";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[18] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 20, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "bounceOutUp";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[19] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 21, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeIn";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[20] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 22, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInDown";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[21] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 23, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInDownBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[22] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 24, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInLeft";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[23] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 25, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInLeftBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[24] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 26, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInRight";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[25] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 27, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInRightBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[26] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 28, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInUp";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[27] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 29, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeInUpBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[28] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 30, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOut";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[29] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 31, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutDown";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[30] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 32, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutDownBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[31] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 33, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutLeft";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[32] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 34, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutLeftBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[33] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 35, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutRightBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[34] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 36, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutUp";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[35] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 37, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "fadeOutUpBig";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[36] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(3);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 38, -1, -1, -1);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "rotateIn";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[37] = targetElement_2;
            root.children.array[2] = targetElement_1;
            // new UIForia.Elements.UIHeading2Element
            targetElement_1 = scope.application.CreateElementFromPoolWithType(59, root, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Result";
            root.children.array[3] = targetElement_1;
            // new UIForia.Elements.UIGroupElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(50, root, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // anim-container
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 1, 0);
            targetElement_2.attributes.array[0] = new UIForia.Elements.ElementAttribute("id", "animation-target");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // anims
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(1);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_3).text = "A simple text that you can animate";
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[0] = targetElement_2;
            root.children.array[4] = targetElement_1;
            // new UIForia.Elements.UIGroupElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(50, root, 11, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(0);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, root, -1, -1, 39, -1);
            // new UIForia.Elements.UILabelElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(56, targetElement_1, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 40, -1, 41, -1);
            targetElement_1.children.array[0] = targetElement_2;
            // new UIForia.Elements.UILabelElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(56, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Duration (in milliseconds)";
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 42, -1, -1, -1);
            targetElement_1.children.array[1] = targetElement_2;
            // new UIForia.Elements.InputElement<float>
            targetElement_2 = scope.application.CreateElementFromPoolWithType(235, targetElement_1, 2, 0, 0);
            scope.application.HydrateTemplate(1, targetElement_2, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // input
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(11);
            // input
            styleList.array[1] = targetElement_2.templateMetaData.GetStyleById(11);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 45, 46);
            targetElement_1.children.array[2] = targetElement_2;
            // new UIForia.Elements.UILabelElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(56, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Delay (in milliseconds)";
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 47, -1, -1, -1);
            targetElement_1.children.array[3] = targetElement_2;
            // new UIForia.Elements.InputElement<float>
            targetElement_2 = scope.application.CreateElementFromPoolWithType(235, targetElement_1, 2, 0, 0);
            scope.application.HydrateTemplate(1, targetElement_2, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // input
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(11);
            // input
            styleList.array[1] = targetElement_2.templateMetaData.GetStyleById(11);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 48, 49);
            targetElement_1.children.array[4] = targetElement_2;
            // new UIForia.Elements.UILabelElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(56, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Iterations (-1 is infinite)";
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 50, -1, -1, -1);
            targetElement_1.children.array[5] = targetElement_2;
            // new UIForia.Elements.InputElement<int>
            targetElement_2 = scope.application.CreateElementFromPoolWithType(236, targetElement_1, 2, 0, 0);
            scope.application.HydrateTemplate(2, targetElement_2, new UIForia.Compilers.TemplateScope(scope.application, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(2);
            // input
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(11);
            // input
            styleList.array[1] = targetElement_2.templateMetaData.GetStyleById(11);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 53, 54);
            targetElement_1.children.array[6] = targetElement_2;
            // new UIForia.Elements.UILabelElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(56, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Easing Function";
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 55, -1, -1, -1);
            targetElement_1.children.array[7] = targetElement_2;
            // new UIForia.Elements.UILabelElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(56, targetElement_1, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_2).text = "Animation Direction";
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, 56, -1, -1, -1);
            targetElement_1.children.array[8] = targetElement_2;
            // new UIForia.Elements.UISectionElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(52, targetElement_1, 4, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(4);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_2, root, -1, -1, 57, -1);
            // new UIForia.Elements.UIDivElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(53, targetElement_2, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(3);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_3, root, 58, -1, 59, -1);
            // new UIForia.Elements.UITextElement
            targetElement_4 = scope.application.CreateElementFromPoolWithType(85, targetElement_3, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Run";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[0] = targetElement_3;
            // new UIForia.Elements.UIDivElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(53, targetElement_2, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(3);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_3, root, 60, -1, 61, -1);
            // new UIForia.Elements.UITextElement
            targetElement_4 = scope.application.CreateElementFromPoolWithType(85, targetElement_3, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Pause";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[1] = targetElement_3;
            // new UIForia.Elements.UIDivElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(53, targetElement_2, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(3);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_3, root, 62, -1, 63, -1);
            // new UIForia.Elements.UITextElement
            targetElement_4 = scope.application.CreateElementFromPoolWithType(85, targetElement_3, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Resume";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[2] = targetElement_3;
            // new UIForia.Elements.UIDivElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(53, targetElement_2, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // button
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(3);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_3, root, 64, -1, 65, -1);
            // new UIForia.Elements.UITextElement
            targetElement_4 = scope.application.CreateElementFromPoolWithType(85, targetElement_3, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_4).text = "Stop!";
            targetElement_3.children.array[0] = targetElement_4;
            targetElement_2.children.array[3] = targetElement_3;
            targetElement_1.children.array[9] = targetElement_2;
            // new UIForia.Elements.UIDivElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(53, targetElement_1, 1, 0, 0);
            // new UIForia.Elements.UITextElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(85, targetElement_2, 0, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_3, root, -1, -1, 66, -1);
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[10] = targetElement_2;
            root.children.array[5] = targetElement_1;
            // new UIForia.Elements.UIHeading2Element
            targetElement_1 = scope.application.CreateElementFromPoolWithType(59, root, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "SpriteSheet Animations";
            root.children.array[6] = targetElement_1;
            // new UIForia.Elements.UIParagraphElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(57, root, 0, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Let's see if the animation plays when you hover the center of the squares.";
            root.children.array[7] = targetElement_1;
            // new UIForia.Elements.UIDivElement
            targetElement_1 = scope.application.CreateElementFromPoolWithType(53, root, 6, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // flex-horizontal
            styleList.array[0] = targetElement_1.templateMetaData.GetStyleById(10);
            targetElement_1.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(50, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(21);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(50, targetElement_2, 0, 1, 0);
            targetElement_3.attributes.array[0] = new UIForia.Elements.ElementAttribute("spriteanimation", "idle");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-panel
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(22);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[0] = targetElement_2;
            // new UIForia.Elements.UIGroupElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(50, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(21);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(50, targetElement_2, 0, 1, 0);
            targetElement_3.attributes.array[0] = new UIForia.Elements.ElementAttribute("spriteanimation", "walk");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-panel
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(22);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[1] = targetElement_2;
            // new UIForia.Elements.UIGroupElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(50, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(21);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(50, targetElement_2, 0, 1, 0);
            targetElement_3.attributes.array[0] = new UIForia.Elements.ElementAttribute("spriteanimation", "kick");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-panel
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(22);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[2] = targetElement_2;
            // new UIForia.Elements.UIGroupElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(50, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(21);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(50, targetElement_2, 0, 1, 0);
            targetElement_3.attributes.array[0] = new UIForia.Elements.ElementAttribute("spriteanimation", "hurt");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-panel
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(22);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[3] = targetElement_2;
            // new UIForia.Elements.UIGroupElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(50, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(21);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(50, targetElement_2, 0, 1, 0);
            targetElement_3.attributes.array[0] = new UIForia.Elements.ElementAttribute("spriteanimation", "sneak");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-panel
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(22);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[4] = targetElement_2;
            // new UIForia.Elements.UIGroupElement
            targetElement_2 = scope.application.CreateElementFromPoolWithType(50, targetElement_1, 1, 0, 0);
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-container
            styleList.array[0] = targetElement_2.templateMetaData.GetStyleById(21);
            targetElement_2.style.internal_Initialize(styleList);
            styleList.Release();
            // new UIForia.Elements.UIGroupElement
            targetElement_3 = scope.application.CreateElementFromPoolWithType(50, targetElement_2, 0, 1, 0);
            targetElement_3.attributes.array[0] = new UIForia.Elements.ElementAttribute("spriteanimation", "flipbook");
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(1);
            // spritesheet-panel
            styleList.array[0] = targetElement_3.templateMetaData.GetStyleById(22);
            targetElement_3.style.internal_Initialize(styleList);
            styleList.Release();
            targetElement_2.children.array[0] = targetElement_3;
            targetElement_1.children.array[5] = targetElement_2;
            root.children.array[8] = targetElement_1;
            return root;
        }; 
        // binding id = 0
        public Action<UIElement, UIElement> Binding_OnUpdate_eee71d8bffc06d244b146449e397df4c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;
            UIForia.Elements.UITextElement __castElement;
            UIForia.Animation.AnimationTask nullCheck;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __castElement = ((UIForia.Elements.UITextElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append("Animation Task Status: ");
            nullCheck = __castRoot.animationTask;
            if (nullCheck == null)
            {
                goto retn;
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(nullCheck.state.ToString());
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        retn:
            return;
        };
// binding id = 1
        public Action<UIElement, UIElement> Binding_OnCreate_8d93bae2bc6021a43bfb5072e94ab71b = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounce");
            });
        };
// binding id = 2
        public Action<UIElement, UIElement> Binding_OnCreate_593d59cde129d154db475d26c4d97aa8 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("flash");
            });
        };
// binding id = 3
        public Action<UIElement, UIElement> Binding_OnCreate_38d0613b7c70a2c488ba8bb6782fa2da = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("pulse");
            });
        };
// binding id = 4
        public Action<UIElement, UIElement> Binding_OnCreate_70042549899bca041bc06e7fee664909 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("rubberBand");
            });
        };
// binding id = 5
        public Action<UIElement, UIElement> Binding_OnCreate_4e2b5b56f9edf24408c626f63f8d9909 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("shake");
            });
        };
// binding id = 6
        public Action<UIElement, UIElement> Binding_OnCreate_f2ec6bc3a7309284f97fb1952bfb3f85 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("headShake");
            });
        };
// binding id = 7
        public Action<UIElement, UIElement> Binding_OnCreate_70586a026ba3c8f4198f421e30950a5a = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("swing");
            });
        };
// binding id = 8
        public Action<UIElement, UIElement> Binding_OnCreate_dfbfc16ff1e80324396dba99e5ecff93 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("tada");
            });
        };
// binding id = 9
        public Action<UIElement, UIElement> Binding_OnCreate_f6dbbd1a8ba59914b90359037924180e = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("wobble");
            });
        };
// binding id = 10
        public Action<UIElement, UIElement> Binding_OnCreate_9e61b7088876b894a863ca0ad4ab6b87 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("heartBeat");
            });
        };
// binding id = 11
        public Action<UIElement, UIElement> Binding_OnCreate_9b53a14527daf7c419b39cfaa5d1f3c8 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceIn");
            });
        };
// binding id = 12
        public Action<UIElement, UIElement> Binding_OnCreate_6c9074d60683abb4685b7105537b60b4 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceOut");
            });
        };
// binding id = 13
        public Action<UIElement, UIElement> Binding_OnCreate_a32d2f7605168514084014c577e95b17 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceInDown");
            });
        };
// binding id = 14
        public Action<UIElement, UIElement> Binding_OnCreate_f7e8c9e82d474c545a87c46a76cb558a = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceInUp");
            });
        };
// binding id = 15
        public Action<UIElement, UIElement> Binding_OnCreate_1d0222c6a82ba3a4797ef16bd56a9e88 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceInLeft");
            });
        };
// binding id = 16
        public Action<UIElement, UIElement> Binding_OnCreate_e0adaa0abe7977449bb7e535e04c3eb6 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceInRight");
            });
        };
// binding id = 17
        public Action<UIElement, UIElement> Binding_OnCreate_46bbaa32e544fec44be79ae43ef82af5 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceOutDown");
            });
        };
// binding id = 18
        public Action<UIElement, UIElement> Binding_OnCreate_3b676e09a951fa6479c1ae03b2a03b58 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceOutLeft");
            });
        };
// binding id = 19
        public Action<UIElement, UIElement> Binding_OnCreate_443b1b8d7598c394790db388c410c9c4 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceOutRight");
            });
        };
// binding id = 20
        public Action<UIElement, UIElement> Binding_OnCreate_03e0df23b15e9814cbfc4389c115bcde = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("bounceOutUp");
            });
        };
// binding id = 21
        public Action<UIElement, UIElement> Binding_OnCreate_c465acc15369aff43a2cd2a32a6a556a = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeIn");
            });
        };
// binding id = 22
        public Action<UIElement, UIElement> Binding_OnCreate_059513629b460ea41a928fbff6142c4b = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInDown");
            });
        };
// binding id = 23
        public Action<UIElement, UIElement> Binding_OnCreate_8ea117ce17d3fb84fb4fbe85d34c552e = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInDownBig");
            });
        };
// binding id = 24
        public Action<UIElement, UIElement> Binding_OnCreate_e3c611adad2945e489952bd31aba6c73 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInLeft");
            });
        };
// binding id = 25
        public Action<UIElement, UIElement> Binding_OnCreate_e961987bc9835124b9902f7b17ff8616 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInLeftBig");
            });
        };
// binding id = 26
        public Action<UIElement, UIElement> Binding_OnCreate_c2a835d045416704b86f1d7f342ac76d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInRight");
            });
        };
// binding id = 27
        public Action<UIElement, UIElement> Binding_OnCreate_25b2f28bba2b9e54facab983f0f39abf = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInRightBig");
            });
        };
// binding id = 28
        public Action<UIElement, UIElement> Binding_OnCreate_0183e9c25fb6a68478e84dcaa62312bb = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInUp");
            });
        };
// binding id = 29
        public Action<UIElement, UIElement> Binding_OnCreate_c823c299188799a45b7136b2589c9d83 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeInUpBig");
            });
        };
// binding id = 30
        public Action<UIElement, UIElement> Binding_OnCreate_f5ab2921368b7734dafbb859f92e755d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOut");
            });
        };
// binding id = 31
        public Action<UIElement, UIElement> Binding_OnCreate_26bed92de49bd604ea8620bfdc4902ff = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutDown");
            });
        };
// binding id = 32
        public Action<UIElement, UIElement> Binding_OnCreate_7ff9a0e9b9d94b54e9ce85eb8270e1e8 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutDownBig");
            });
        };
// binding id = 33
        public Action<UIElement, UIElement> Binding_OnCreate_40cf342e2fcfb8040be2bbde069f129f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutLeft");
            });
        };
// binding id = 34
        public Action<UIElement, UIElement> Binding_OnCreate_f6d79bad9c786a14a8d5709815a4813b = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutLeftBig");
            });
        };
// binding id = 35
        public Action<UIElement, UIElement> Binding_OnCreate_0720f7835c8de1b4d91157a44d3fd177 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutRightBig");
            });
        };
// binding id = 36
        public Action<UIElement, UIElement> Binding_OnCreate_a9e158a3e629df74fb56b11380025e4d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutUp");
            });
        };
// binding id = 37
        public Action<UIElement, UIElement> Binding_OnCreate_eee4543966b84dd41b98e5e321d0dced = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("fadeOutUpBig");
            });
        };
// binding id = 38
        public Action<UIElement, UIElement> Binding_OnCreate_a51596fc34c57ff4fb18204c2ea5723c = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ChangeAnimation("rotateIn");
            });
        };
// binding id = 39
        public Action<UIElement, UIElement> Binding_OnUpdate_aa94628d935a3224eb9180379fa89255 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.SetEnabled(__castRoot.animationData.name != null);

            // if="animationData.name != null"
            if (__element.isEnabled == false)
            {
                goto section_0_0;
            }
        section_0_0:
        retn:
            return;
        };
// binding id = 40
        public Action<UIElement, UIElement> Binding_OnCreate_0e21de367cc2b8c4e92a5eb8a952120f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UILabelElement __castElement;

            __castElement = ((UIForia.Elements.UILabelElement)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnClick();
            });
        };
// binding id = 41
        public Action<UIElement, UIElement> Binding_OnUpdate_c13e4263a85aa4842a8ca6b5d7acacd4 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;
            UIForia.Elements.UILabelElement __castElement;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __castElement = ((UIForia.Elements.UILabelElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append("Name: ");
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(__castRoot.animationData.name);
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        };
// binding id = 42
        public Action<UIElement, UIElement> Binding_OnCreate_26b5f1d3945844145b4da1077a0077fe = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UILabelElement __castElement;

            __castElement = ((UIForia.Elements.UILabelElement)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnClick();
            });
        };
// binding id = 45
        public Action<UIElement, UIElement> Binding_OnUpdate_c69ef37abc897244382f18a2b748f926 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<float> __castElement;
            Documentation.Features.AnimationDemo __castRoot;
            float __right;

            __castElement = ((UIForia.Elements.InputElement<float>)__element);

            // value="duration"
            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __right = __castRoot.duration;
            __castElement.value = __right;
        section_0_1:
            __castElement.OnUpdate();
        retn:
            return;
        };
// binding id = 46
        public Action<UIElement, UIElement> Binding_OnLateUpdate_afd9e3d721f798e42ab4660eb41fb500 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<float> __castElement;
            Documentation.Features.AnimationDemo __castRoot;

            __castElement = ((UIForia.Elements.InputElement<float>)__element);
            __castRoot = ((Documentation.Features.AnimationDemo)__root);

            // value="duration"
            __castRoot.duration = __castElement.value;
        section_0_0:
        retn:
            return;
        };
// binding id = 47
        public Action<UIElement, UIElement> Binding_OnCreate_59537bc48d47e5c4e8dd2fd647327ca9 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UILabelElement __castElement;

            __castElement = ((UIForia.Elements.UILabelElement)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnClick();
            });
        };
// binding id = 48
        public Action<UIElement, UIElement> Binding_OnUpdate_82131eda5e6737347bd1a6c3a9915152 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<float> __castElement;
            Documentation.Features.AnimationDemo __castRoot;
            float __right;

            __castElement = ((UIForia.Elements.InputElement<float>)__element);

            // value="delay"
            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __right = __castRoot.delay;
            __castElement.value = __right;
        section_0_2:
            __castElement.OnUpdate();
        retn:
            return;
        };
// binding id = 49
        public Action<UIElement, UIElement> Binding_OnLateUpdate_7ede0fca07fbe504d916a0104d7e47b9 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<float> __castElement;
            Documentation.Features.AnimationDemo __castRoot;

            __castElement = ((UIForia.Elements.InputElement<float>)__element);
            __castRoot = ((Documentation.Features.AnimationDemo)__root);

            // value="delay"
            __castRoot.delay = __castElement.value;
        section_0_1:
        retn:
            return;
        };
// binding id = 50
        public Action<UIElement, UIElement> Binding_OnCreate_c7fd48d105c380b43a5221a4ff1f9991 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UILabelElement __castElement;

            __castElement = ((UIForia.Elements.UILabelElement)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnClick();
            });
        };
// binding id = 53
        public Action<UIElement, UIElement> Binding_OnUpdate_ca0216731e067ba4f8ac5ba7aaab5579 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castElement;
            Documentation.Features.AnimationDemo __castRoot;
            int __right;

            __castElement = ((UIForia.Elements.InputElement<int>)__element);

            // value="iterations"
            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __right = __castRoot.iterations;
            __castElement.value = __right;
        section_0_3:
            __castElement.OnUpdate();
        retn:
            return;
        };
// binding id = 54
        public Action<UIElement, UIElement> Binding_OnLateUpdate_d9d387c647aa3714bb43d6481bc28d51 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.InputElement<int> __castElement;
            Documentation.Features.AnimationDemo __castRoot;

            __castElement = ((UIForia.Elements.InputElement<int>)__element);
            __castRoot = ((Documentation.Features.AnimationDemo)__root);

            // value="iterations"
            __castRoot.iterations = __castElement.value;
        section_0_2:
        retn:
            return;
        };
// binding id = 55
        public Action<UIElement, UIElement> Binding_OnCreate_acfc25e1d1bb1364c8041785c69d996d = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UILabelElement __castElement;

            __castElement = ((UIForia.Elements.UILabelElement)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnClick();
            });
        };
// binding id = 56
        public Action<UIElement, UIElement> Binding_OnCreate_4ba1090ff92644e49b62a8238eb878a5 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UILabelElement __castElement;

            __castElement = ((UIForia.Elements.UILabelElement)__element);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castElement.OnClick();
            });
        };
// binding id = 57
        public Action<UIElement, UIElement> Binding_OnUpdate_34f430a69cf60294b935ad097853e92f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.SetEnabled(__castRoot.animationTask != null);

            // if="animationTask != null"
            if (__element.isEnabled == false)
            {
                goto section_0_4;
            }
        section_0_4:
        retn:
            return;
        };
// binding id = 58
        public Action<UIElement, UIElement> Binding_OnCreate_3a3b6876f8325fc41aaec2cddda10167 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.RunAnimationAgain();
            });
        };
// binding id = 59
        public Action<UIElement, UIElement> Binding_OnUpdate_71b9cffeb472bf549b96b35ce33e672f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.SetEnabled(__castRoot.ShowRunButton);

            // if="ShowRunButton"
            if (__element.isEnabled == false)
            {
                goto section_0_5;
            }
        section_0_5:
        retn:
            return;
        };
// binding id = 60
        public Action<UIElement, UIElement> Binding_OnCreate_64a384c059f2ef144a83671f93bfd6e3 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.PauseAnimation();
            });
        };
// binding id = 61
        public Action<UIElement, UIElement> Binding_OnUpdate_16137b3ed10efca4abc1c83dde6cc3f8 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.SetEnabled(__castRoot.ShowPauseButton);

            // if="ShowPauseButton"
            if (__element.isEnabled == false)
            {
                goto section_0_6;
            }
        section_0_6:
        retn:
            return;
        };
// binding id = 62
        public Action<UIElement, UIElement> Binding_OnCreate_edfad68de7637e44c8c9ea067ae66fd0 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.ResumeAnimation();
            });
        };
// binding id = 63
        public Action<UIElement, UIElement> Binding_OnUpdate_87627d7b68fbc96439ac3d4d2a3c3ae3 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.SetEnabled(__castRoot.ShowResumeButton);

            // if="ShowResumeButton"
            if (__element.isEnabled == false)
            {
                goto section_0_7;
            }
        section_0_7:
        retn:
            return;
        };
// binding id = 64
        public Action<UIElement, UIElement> Binding_OnCreate_7b8f584215096c649b422f69937c1e87 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.inputHandlers = new UIForia.Elements.InputHandlerGroup();
            __element.inputHandlers.AddMouseEvent(UIForia.UIInput.InputEventType.MouseClick, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, (UIForia.UIInput.GenericInputEvent __evt) =>
            {
                __castRoot.StopAnimation();
            });
        };
// binding id = 65
        public Action<UIElement, UIElement> Binding_OnUpdate_38065ee9967aca047914fe7fd884a1b1 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __element.SetEnabled(__castRoot.ShowStopButton);

            // if="ShowStopButton"
            if (__element.isEnabled == false)
            {
                goto section_0_8;
            }
        section_0_8:
        retn:
            return;
        };
// binding id = 66
        public Action<UIElement, UIElement> Binding_OnUpdate_2bded98b79e10a8498643ed8c91be99f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            Documentation.Features.AnimationDemo __castRoot;
            UIForia.Elements.UITextElement __castElement;
            UIForia.Animation.AnimationTask nullCheck;

            __castRoot = ((Documentation.Features.AnimationDemo)__root);
            __castElement = ((UIForia.Elements.UITextElement)__element);
            UIForia.Util.StringUtil.s_CharStringBuilder.Append("Animation Task Status: ");
            nullCheck = __castRoot.animationTask;
            if (nullCheck == null)
            {
                goto retn;
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Append(nullCheck.state.ToString());
            if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
            {
                __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
            }
            UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
        retn:
            return;
        };

        
    }

}
                