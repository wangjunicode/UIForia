using UIForia.Compilers;

namespace UIForia.Generated {

    public partial class Generated_TestApp : TemplateLoader {
    
        public static readonly TemplateData template_d4b877d9_9092_4451_b909_208a4e34ccd4 = new TemplateData ("SimpleTemplate") {
            entry = (UIForia.ElementSystem system) => {
                TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate element;

                element = new TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate();
                system.InitializeEntryPoint(element, 2, 2);
                system.InitializeStaticAttribute(@"x", @"x");
                system.InitializeStaticAttribute(@"y", @"y");
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = (UIForia.ElementSystem system) => {
                system.AddChild(new UIForia.Elements.UIDivElement(), 0);
                system.AddChild(new UIForia.Elements.UIPanelElement(), 1);
            },
            elements = new System.Action<UIForia.ElementSystem>[] {
                //0 <Div> line 3:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new UIForia.Elements.UIPanelElement(), 2);
                },
                //1 <Panel> line 12:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                },
                //2 <Panel> line 4:14
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate(), 3);
                },
                //3 <ExpandedTemplate> line 5:18
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 2);
                    system.OverrideSlot(@"title", 0);
                    system.HydrateElement(typeof(TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate));
                    // system.SetBindings(0);
                },
                //4 <override:title> line 6:22
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(1, 1);
                    system.InitializeStaticAttribute(@"x", @"expandedTempate.GetX()");
                    system.AddChild(new UIForia.Elements.UITextElement(), 5);
                },
                //5 <Text> line 6:22
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetText(@"Hello");
                }
            }
        };

    }

}