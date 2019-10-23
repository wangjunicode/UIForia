using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp {

        public Func<UIElement, TemplateScope2, UIElement> Template_1a5ba50a1dc0e844d9e7315d4514d940 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) => {
            UIForia.Elements.UIElement targetElement_1;

            if (root == null) {
                root = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.LoadTemplate0), default(UIForia.Elements.UIElement), 2, 0);
            }

            // <Text />    'I am text'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), root, 0, 0);
            ((UIForia.Elements.UITextElement) targetElement_1).text = "I am text";
            root.children.array[0] = targetElement_1;

            // <LoadTemplateHydrate if=="refTypeThing.intVal > 145" attr:nonconst="{computed}" attr:first="true" attr:some-attr="this-is-attr" floatVal=="42f" evt:onMouseDown="HandleDown()" intVal=="refTypeThing.intVal" intVal2=="refTypeThing.intVal"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.LoadTemplateHydrate), root, 1, 3);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("nonconst", "");
            targetElement_1.attributes.array[1] = new UIForia.Elements.ElementAttribute("first", "true");
            targetElement_1.attributes.array[2] = new UIForia.Elements.ElementAttribute("some-attr", "this-is-attr");
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope2(scope.application, scope.bindingNode, default(UIForia.Util.StructList<UIForia.Compilers.SlotUsage>)));
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1);
            root.children.array[1] = targetElement_1;
            return root;
        };

        public Action<UIElement, UIElement> Binding_40b61b53e5bd0954ba7862cc741e170f = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) => {
            UIForia.Test.TestData.LoadTemplateHydrate __castElement;
            UIForia.Test.TestData.LoadTemplate0 __castRoot;

            __castElement = ((UIForia.Test.TestData.LoadTemplateHydrate) __element);
            __castRoot = ((UIForia.Test.TestData.LoadTemplate0) __root);

            // floatVal="42f"
            __castElement.floatVal = 42f;
            section_0_0:
            return;
        };

        public Action<UIElement, UIElement> Binding_15087741023767c4bae4b9f015839745 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) => {
            UIForia.Test.TestData.LoadTemplateHydrate __castElement;
            UIForia.Test.TestData.LoadTemplate0 __castRoot;
            UIForia.Test.TestData.RefTypeThing nullCheck;
            UIForia.Test.TestData.RefTypeThing nullCheck_0;
            int __right;
            UIForia.Test.TestData.RefTypeThing nullCheck_1;
            int __right_0;

            __castElement = ((UIForia.Test.TestData.LoadTemplateHydrate) __element);
            __castRoot = ((UIForia.Test.TestData.LoadTemplate0) __root);
            nullCheck = __castRoot.refTypeThing;
            if (nullCheck == null) {
                goto section_0_1;
            }

            __castElement.SetEnabled(nullCheck.intVal > 145);

            // if="refTypeThing.intVal > 145"
            if (__castElement.isEnabled == false) {
                goto retn;
            }

            section_0_1:

            // nonconst="{computed}"
            __castElement.SetAttribute("nonconst", __castRoot.computed);

            // intVal="refTypeThing.intVal"
            nullCheck_0 = __castRoot.refTypeThing;
            if (nullCheck_0 == null) {
                goto section_0_2;
            }

            __right = nullCheck_0.intVal;
            if (__castElement.intVal != __right) {
                __castElement.intVal = __right;
                __castElement.HandleIntValChanged();
            }

            section_0_2:

            // intVal2="refTypeThing.intVal"
            nullCheck_1 = __castRoot.refTypeThing;
            if (nullCheck_1 == null) {
                goto section_0_3;
            }

            __right_0 = nullCheck_1.intVal;
            if (__castElement.intVal2 != __right_0) {
                __castElement.intVal2 = __right_0;
            }

            section_0_3:
            __castElement.OnUpdate();
            retn:
            return;
        };

    }

}