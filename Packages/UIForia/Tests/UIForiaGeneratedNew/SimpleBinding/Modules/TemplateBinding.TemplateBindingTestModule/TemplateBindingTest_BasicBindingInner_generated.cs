using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_SimpleBinding : TemplateLoader {
    
        public static readonly TemplateData template_8cbf920d_335a_4e41_b0cd_1be9b3d22214 = new TemplateData ("TemplateBindingTest_BasicBindingInner") {
            entry = (UIForia.ElementSystem system) => {
                TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingInner element;

                element = new TemplateBinding.TemplateBindingTests.TemplateBindingTest_BasicBindingInner();
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