namespace UIForia.GeneratedApplication {

    public partial class Generated_TestApp {

        public static readonly UIForia.Compilers.TemplateData template_e5dcd86a_ed23_4c39_aa1d_920958cc8cd4 = 

            new UIForia.Compilers.TemplateData ("SimpleTemplate") {
                entry = (UIForia.ElementSystem system) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate element;

                    element = new TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate();
                    system.InitializeEntryPoint(element, 2, 4);
                    system.InitializeStaticAttribute(@"x", @"x");
                    system.InitializeStaticAttribute(@"y", @"y");
                    system.HydrateEntryPoint();
                    return element;
                },
                hydrate = (UIForia.ElementSystem system) => {
                    system.AddChild(new UIForia.Elements.UIDivElement(), 0);
                    system.AddChild(new UIForia.Elements.UIPanelElement(), 1);
                    system.AddChild(new UIForia.Elements.UIGroupElement(), 2);
                    system.AddSlotChild(new UIForia.Elements.UISlotDefinition(), @"someslot", 0);
                },
                elements = new System.Action<UIForia.ElementSystem>[] {
                    //0 <Div> line 3:10
                    (UIForia.ElementSystem system) => {
                        system.InitializeElement(0, 1);
                        system.AddChild(new UIForia.Elements.UIPanelElement(), 1);
                    },
                    //1 <Panel> line 4:14
                    (UIForia.ElementSystem system) => {
                        system.InitializeElement(0, 1);
                        system.AddHydratedChild(new TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate(), 3);
                    },
                    //2 <Group> line 13:10
                    (UIForia.ElementSystem system) => {
                        system.InitializeElement(0, 0);
                    },
                    //3 <ExpandedTemplate> line 5:18
                    (UIForia.ElementSystem system) => {
                        system.InitializeElement(0, 0);
                        system.ForwardSlot(@"title", 0);
                        system.HydrateElement(typeof(TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate));
                    }
                }
            };

    }

}