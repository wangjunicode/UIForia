using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_CreatedBinding : TemplateLoader {
    
        public static readonly TemplateData template_5f72bc0b_f980_4d2e_bcca_69a862b5dc0a = new TemplateData ("TemplateBindingTest_CreatedBindingOuter") {
            entry = (UIForia.ElementSystem system) => {
                TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingOuter element;

                element = new TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingOuter();
                system.InitializeEntryPoint(element, 0, 1);
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = (UIForia.ElementSystem system) => {
                system.AddChild(new TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingInner(), 0);
            },
            elements = new System.Action<UIForia.ElementSystem>[] {
                //0 <TemplateBindingTest_CreatedBindingInner> line 5:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetBindings(-1, -1, 0, -1, 0);
                    system.HydrateElement(typeof(TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingInner));
                }
            },
            bindings = new System.Action<UIForia.Systems.LinqBindingNode>[] {
                // 0 Const Binding <TemplateBindingTest_CreatedBindingInner> line 5:10
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingInner element;
                    TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingOuter context_0;
                    int __value;

                    element = bindingNode.element as TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingInner;
                    context_0 = bindingNode.root as TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingOuter;
                    __value = context_0.GetValue();
                    element.intVal = __value;
                }
            }
        };

    }

}