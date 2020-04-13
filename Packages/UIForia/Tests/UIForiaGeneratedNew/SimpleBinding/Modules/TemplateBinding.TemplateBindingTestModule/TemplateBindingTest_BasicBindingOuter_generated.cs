using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_SimpleBinding : TemplateLoader {
    
        public static readonly TemplateData template_e4be5b4b_fe4a_4242_8e16_84fa63d84cc7 = new TemplateData ("TemplateBindingTest_BasicBindingOuter") {
            entry = (UIForia.ElementSystem system) => {
                TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingOuter element;

                element = new TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingOuter();
                system.InitializeEntryPoint(element, 0, 1);
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = (UIForia.ElementSystem system) => {
                system.AddChild(new TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingInner(), 0);
            },
            elements = new System.Action<UIForia.ElementSystem>[] {
                //0 <TemplateBindingTest_BasicBindingInner> line 5:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetBindings(-1, -1, 0, -1, 0);
                    system.HydrateElement(typeof(TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingInner));
                }
            },
            bindings = new System.Action<UIForia.Systems.LinqBindingNode>[] {
                // 0 Const Binding <TemplateBindingTest_BasicBindingInner> line 5:10
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingInner element;
                    TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingOuter context_0;
                    int __value;

                    element = bindingNode.element as TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingInner;
                    context_0 = bindingNode.root as TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingOuter;
                    __value = 5 * 5;
                    element.intVal = __value;
                }
            }
        };

    }

}