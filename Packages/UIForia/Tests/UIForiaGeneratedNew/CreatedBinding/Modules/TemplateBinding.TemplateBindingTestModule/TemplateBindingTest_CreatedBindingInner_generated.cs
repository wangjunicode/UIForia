using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_CreatedBinding : TemplateLoader {
    
        public static readonly TemplateData template_32fb95b4_34ba_4654_a4dd_4377c156131d = new TemplateData ("TemplateBindingTest_CreatedBindingInner") {
            entry = (UIForia.ElementSystem system) => {
                TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingInner element;

                element = new TemplateBinding.TemplateBindingTests.TemplateBindingTest_CreatedBindingInner();
                system.InitializeEntryPoint(element, 0, 0);
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = (UIForia.ElementSystem system) => {
            },
            elements = new System.Action<UIForia.ElementSystem>[] {

            },
            bindings = new System.Action<UIForia.Systems.LinqBindingNode>[] {

            }
        };

    }

}