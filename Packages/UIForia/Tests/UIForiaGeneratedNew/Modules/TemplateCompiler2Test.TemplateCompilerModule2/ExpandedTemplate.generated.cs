namespace UIForia.GeneratedApplication {

    public partial class Generated_TestApp {

        public static readonly UIForia.Compilers.TemplateData template_bb09d82e_b1a7_48d2_9df9_7ef4cf075c6c = 

            new UIForia.Compilers.TemplateData ("ExpandedTemplate") {
                entry = (UIForia.ElementSystem system) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate element;

                    element = new TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate();
                    system.InitializeEntryPoint(element, 0, 2);
                    system.HydrateEntryPoint();
                    return element;
                },
                hydrate = (UIForia.ElementSystem system) => {
                    system.AddChild(new UIForia.Elements.UITextElement(), 0);
                    system.AddSlotChild(new UIForia.Elements.UISlotDefinition(), @"title", 0);
                },
                elements = new System.Action<UIForia.ElementSystem>[] {
                    //0 <Text> line 5:10
                    (UIForia.ElementSystem system) => {
                        system.InitializeElement(0, 0);
                    }
                }
            };

    }

}