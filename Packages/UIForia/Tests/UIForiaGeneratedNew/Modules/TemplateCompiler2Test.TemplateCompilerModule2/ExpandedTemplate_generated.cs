using UIForia.Compilers;

namespace UIForia.Generated {

    public partial class Generated_TestApp : TemplateLoader {
    
        public static readonly TemplateData template_15d71eda_bd8c_494d_b0a2_9905fbcc3584 = new TemplateData ("ExpandedTemplate") {
            entry = (UIForia.ElementSystem system) => {
                TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate element;

                element = new TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate();
                system.InitializeEntryPoint(element, 0, 2);
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = (UIForia.ElementSystem system) => {
                system.AddChild(new UIForia.Elements.UITextElement(), 0);
                system.AddSlotChild(new UIForia.Elements.UISlotDefinition(), @"title", 1);
            },
            elements = new System.Action<UIForia.ElementSystem>[] {
                //0 <Text> line 5:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetText(@"this is inside the template");
                },
                //1 <define:title> line 6:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new UIForia.Elements.UITextElement(), 2);
                },
                //2 <Text> line 6:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetText(@"Goodbye");
                }
            }
        };

    }

}