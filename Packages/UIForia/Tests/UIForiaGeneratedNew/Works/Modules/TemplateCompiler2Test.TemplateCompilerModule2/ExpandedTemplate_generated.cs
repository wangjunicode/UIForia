using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_Works : TemplateLoader {
    
        public static readonly TemplateData template_dc19de8a_2f09_40d7_b72b_df6ae7547964 = new TemplateData ("ExpandedTemplate") {
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
                    system.InitializeElement(1, 1);
                    system.InitializeDynamicAttribute(@"x");
                    system.AddChild(new UIForia.Elements.UITextElement(), 2);
                    system.SetBindings(0, -1, -1, -1, 0);
                },
                //2 <Text> line 6:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetText(@"Goodbye");
                }
            },
            bindings = new System.Action<UIForia.Systems.LinqBindingNode>[] {
                // 0 Update Binding <define:title> line 6:10
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate context_0;
                    UIForia.Elements.UISlotDefinition element;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate;
                    element = bindingNode.element as UIForia.Elements.UISlotDefinition;
                    element.SetAttribute(@"x", context_0.GetAttribute(@"y"));
                }
            },
            inputEventHandlers = new System.Action<UIForia.Systems.LinqBindingNode, UIForia.Systems.InputEventHolder>[] {

            }
        };

    }

}