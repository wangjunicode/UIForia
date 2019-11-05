using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_LoadTemplateFromFile {
        
        public Func<UIElement, TemplateScope2, UIElement> Template_bed6432ec30cfd648af747d30c2aab28 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.StructList<UIForia.Compilers.SlotUsage> slotUsage;

            if (root == null)
            {
                root = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.LoadTemplate0), default(UIForia.Elements.UIElement), 2, 0);
            }

            // <Text />    'I am text'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), root, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "I am text";
            root.children.array[0] = targetElement_1;

            // <LoadTemplateHydrate if=="refTypeThing.intVal > 145" ctxvar:ctxVar0="14244" ctxvar:ctxVar1="144" style=="style-one style-two {'style-three' + floatVal}" attr:nonconst="{computed}" attr:first="true" attr:some-attr="this-is-attr" floatVal=="42f" onDidSomething=="HandleStuffDone(valueName)" intVal=="$ctxVar0 + $ctxVar1" intVal2=="refTypeThing.intVal"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.LoadTemplateHydrate), root, 1, 3);
            targetElement_1.attributes.array[0] = new UIForia.Elements.ElementAttribute("nonconst", "");
            targetElement_1.attributes.array[1] = new UIForia.Elements.ElementAttribute("first", "true");
            targetElement_1.attributes.array[2] = new UIForia.Elements.ElementAttribute("some-attr", "this-is-attr");
            UIForia.Systems.LinqBindingNode.Get(scope.application, root, targetElement_1, 0, -1, 1);
            slotUsage = UIForia.Util.StructList<UIForia.Compilers.SlotUsage>.PreSize(1);
            // Slot_Children_Children_1c171275dc84de7478451663caff8b07
            slotUsage.array[0] = new UIForia.Compilers.SlotUsage("Children", 0);

            // Data/TemplateLoading/LoadTemplateHydrate.xml
            scope.application.HydrateTemplate(1, targetElement_1, new UIForia.Compilers.TemplateScope2(scope.application, slotUsage));
            slotUsage.Release();
            root.children.array[1] = targetElement_1;
            return root;
        }; 
        
        public Action<UIElement, UIElement> Binding_OnCreate_8c9f8ef492f26f042be3c9df180d987a = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Test.TestData.LoadTemplateHydrate __castElement;
            UIForia.Test.TestData.LoadTemplate0 __castRoot;
            UIForia.Systems.ContextVariable<int> ctxVar_ctxVar0;
            UIForia.Systems.ContextVariable<int> ctxVar_ctxVar1;
            System.Action<string> evtFn;

            __castElement = ((UIForia.Test.TestData.LoadTemplateHydrate)__element);
            __castRoot = ((UIForia.Test.TestData.LoadTemplate0)__root);
            __castElement.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<int>());
            ctxVar_ctxVar0 = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetLocalContextVariable("varName"));
            ctxVar_ctxVar0.value = 14244;
            __castElement.bindingNode.CreateLocalContextVariable(new UIForia.Systems.ContextVariable<int>());
            ctxVar_ctxVar1 = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetLocalContextVariable("varName"));
            ctxVar_ctxVar1.value = 144;

            // floatVal="42f"
            __castElement.floatVal = 42f;
        section_0_0:
            evtFn = (string valueName) =>
            {
                __castRoot.HandleStuffDone(valueName);
            };
            __castElement.onDidSomething += evtFn;
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_2f2316c3581062d458bfb55f268524ad = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Test.TestData.LoadTemplateHydrate __castElement;
            UIForia.Test.TestData.LoadTemplate0 __castRoot;
            UIForia.Test.TestData.RefTypeThing nullCheck;
            int ctxvar_ctxVar0;
            int ctxvar_ctxVar1;
            int __right;
            UIForia.Test.TestData.RefTypeThing nullCheck_0;
            int __right_0;
            UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer> styleList;

            __castElement = ((UIForia.Test.TestData.LoadTemplateHydrate)__element);
            __castRoot = ((UIForia.Test.TestData.LoadTemplate0)__root);
            nullCheck = __castRoot.refTypeThing;
            if (nullCheck == null)
            {
                goto section_0_0;
            }
            __castElement.SetEnabled(nullCheck.intVal > 145);

            // if="refTypeThing.intVal > 145"
            if (__castElement.isEnabled == false)
            {
            early_out:
                return ;
            }
        section_0_0:

            // nonconst="{computed}"
            __castElement.SetAttribute("nonconst", __castRoot.computed);

            // intVal="$ctxVar0 + $ctxVar1"
            ctxvar_ctxVar0 = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetContextVariable(0)).value;
            ctxvar_ctxVar1 = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetContextVariable(1)).value;
            __right = ctxvar_ctxVar0 + ctxvar_ctxVar1;
            if (__castElement.intVal != __right)
            {
                __castElement.intVal = __right;
                __castElement.HandleIntValChanged();
            }
        section_0_1:

            // intVal2="refTypeThing.intVal"
            nullCheck_0 = __castRoot.refTypeThing;
            if (nullCheck_0 == null)
            {
                goto section_0_2;
            }
            __right_0 = nullCheck_0.intVal;
            if (__castElement.intVal2 != __right_0)
            {
                __castElement.intVal2 = __right_0;
            }
        section_0_2:
            __castElement.OnUpdate();
            styleList = UIForia.Util.LightList<UIForia.Compilers.Style.UIStyleGroupContainer>.PreSize(3);
            // style-one
            styleList.array[0] = __element.templateMetaData.GetStyleById(0);
            // style-two
            styleList.array[1] = __element.templateMetaData.GetStyleById(1);
            // 'style-three' + floatVal
            styleList.array[2] = __element.templateMetaData.ResolveStyleByName(string.Concat("style-three", __castRoot.floatVal.ToString()));
            __element.style.SetBaseStyles(styleList);
        retn:
            return;
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_3c506917041fb3343ac6e2963b40e251 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Test.TestData.ThingWithParameters __castElement;
            UIForia.Test.TestData.LoadTemplate0 __castRoot;
            int ctxvar_ctxVar0;
            int __right;

            __castElement = ((UIForia.Test.TestData.ThingWithParameters)__element);
            __castRoot = ((UIForia.Test.TestData.LoadTemplate0)__root);

            // intParam="$ctxVar0"
            ctxvar_ctxVar0 = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetContextVariable(0)).value;
            __right = ctxvar_ctxVar0;
            __castElement.intParam = __right;
        section_0_3:
            return;
        };

        public Action<UIElement, UIElement> Binding_OnUpdate_b8f1abd1f166f20459e3dd45b954fb75 = (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
        {
            UIForia.Elements.UITextElement __castElement;
            UIForia.Test.TestData.LoadTemplate0 __castRoot;
            System.Text.StringBuilder __stringBuilder;
            int ctxvar_ctxVar0;

            __castElement = ((UIForia.Elements.UITextElement)__element);
            __castRoot = ((UIForia.Test.TestData.LoadTemplate0)__root);
            __stringBuilder = UIForia.Text.TextUtil.StringBuilder;
            __stringBuilder.Append("Hello Override ");
            ctxvar_ctxVar0 = ((UIForia.Systems.ContextVariable<int>)__castElement.bindingNode.GetContextVariable(0)).value;
            __stringBuilder.Append(ctxvar_ctxVar0);
            __castElement.SetText(__stringBuilder.ToString());
            __stringBuilder.Clear();
        };

        
        public Func<UIElement, TemplateScope2, UIElement> Slot_Children_Children_1c171275dc84de7478451663caff8b07 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;
            UIForia.Elements.UIElement targetElement_1;
            UIForia.Util.StructList<UIForia.Compilers.SlotUsage> slotUsage;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UISlotContent), default(UIForia.Elements.UIElement), 2, 0));

            // <ThingWithParameters intParam=="$ctxVar0"/>
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.ThingWithParameters), slotRoot, 1, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, slotRoot, targetElement_1, -1, -1, 2);
            slotUsage = UIForia.Util.StructList<UIForia.Compilers.SlotUsage>.PreSize(1);
            // Slot_Override_ThingSlot_c3d9154fd9d1e8045ad9a386855b6a31
            slotUsage.array[0] = new UIForia.Compilers.SlotUsage("ThingSlot", 2);

            // Data/TemplateLoading/ThingWithParameters.xml
            scope.application.HydrateTemplate(2, targetElement_1, new UIForia.Compilers.TemplateScope2(scope.application, slotUsage));
            slotUsage.Release();
            slotRoot.children.array[0] = targetElement_1;

            // <{DefineSlot}SomeSlot />
            targetElement_1 = scope.application.CreateSlot(UIForia.Application.ResolveSlotId("SomeSlot", scope.slotInputs, 3), slotRoot, new UIForia.Compilers.TemplateScope2(scope.application, scope.slotInputs));
            slotRoot.children.array[1] = targetElement_1;
            return slotRoot;
        };

        public Func<UIElement, TemplateScope2, UIElement> Slot_Override_ThingSlot_c3d9154fd9d1e8045ad9a386855b6a31 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UISlotContent), default(UIForia.Elements.UIElement), 1, 0));

            // 
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), slotRoot, 0, 0);
            UIForia.Systems.LinqBindingNode.Get(scope.application, slotRoot, targetElement_1, -1, -1, 3);
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

        public Func<UIElement, TemplateScope2, UIElement> Slot_Default_SomeSlot_391673697e5964d4989169656aa1da41 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UISlotContent slotRoot;
            UIForia.Elements.UIElement targetElement_1;

            slotRoot = ((UIForia.Elements.UISlotContent)scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UISlotContent), default(UIForia.Elements.UIElement), 1, 0));

            // 
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), slotRoot, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "I am groot";
            slotRoot.children.array[0] = targetElement_1;
            return slotRoot;
        };

    }

}