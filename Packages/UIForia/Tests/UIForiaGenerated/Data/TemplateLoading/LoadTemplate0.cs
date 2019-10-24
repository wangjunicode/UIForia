using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Test.TestData;
using UIForia.Util;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp {

        public Func<UIElement, TemplateScope2, UIElement> Template_fa6a5fb489954c54280fa62ba2a00431 = (UIElement root, TemplateScope2 scope) => {
            UIElement targetElement_1;

            if (root == null) {
                root = scope.application.CreateElementFromPoolWithType(typeof(LoadTemplate0), default(UIElement), 2, 0);
            }

            // <Text />    'I am text'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UITextElement), root, 0, 0);
            ((UITextElement) targetElement_1).text = "I am text";
            root.children.array[0] = targetElement_1;

            // <LoadTemplateHydrate if=="refTypeThing.intVal > 145" attr:nonconst="{computed}" attr:first="true" attr:some-attr="this-is-attr" floatVal=="42f" onDidSomething=="HandleStuffDone(valueName)" onMouseDown=="HandleDown($event, 'stuff')" intVal=="refTypeThing.intVal" intVal2=="refTypeThing.intVal"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(LoadTemplateHydrate), root, 1, 3);
            targetElement_1.attributes.array[0] = new ElementAttribute("nonconst", "");
            targetElement_1.attributes.array[1] = new ElementAttribute("first", "true");
            targetElement_1.attributes.array[2] = new ElementAttribute("some-attr", "this-is-attr");
            scope.application.HydrateTemplate(1, targetElement_1, new TemplateScope2(scope.application, scope.bindingNode, default(StructList<SlotUsage>)));
            LinqBindingNode.Get(scope.application, root, targetElement_1);
            root.children.array[1] = targetElement_1;
            return root;
        };

        public Action<UIElement, UIElement> Binding_0749e1201f576314fb3ca1690b81e43d = (UIElement __root, UIElement __element) => {
            LoadTemplateHydrate __castElement;
            LoadTemplate0 __castRoot;
            Action<string> evtFn;

            __castElement = ((LoadTemplateHydrate) __element);
            __castRoot = ((LoadTemplate0) __root);

            // floatVal="42f"
            __castElement.floatVal = 42f;
            evtFn = (string valueName) => { __castRoot.HandleStuffDone(valueName); };
            TemplateCompiler2.EventUtil.Subscribe(__castElement, "onDidSomething", evtFn);
        };

        public Action<UIElement, UIElement> Binding_5ae07bd74fafa6e43a7a559f862a22d3 = (UIElement __root, UIElement __element) => {
            LoadTemplateHydrate __castElement;
            LoadTemplate0 __castRoot;

            __castElement = ((LoadTemplateHydrate) __element);
            __castRoot = ((LoadTemplate0) __root);
            __castElement.OnUpdate();
        };

    }

}