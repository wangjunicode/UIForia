using UIForia.Compilers;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_Works : TemplateLoader {
    
        public static readonly TemplateData template_3413db6c_8e0e_47b2_9017_0008681cffab = new TemplateData ("SimpleTemplate") {
            entry = (UIForia.ElementSystem system) => {
                TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate element;

                element = new TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate();
                system.InitializeEntryPoint(element, 3, 2);
                system.InitializeStaticAttribute(@"x", @"x");
                system.InitializeStaticAttribute(@"y", @"y");
                system.InitializeDynamicAttribute(@"z");
                system.SetBindings(0, -1, 1, -1, 0);
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = (UIForia.ElementSystem system) => {
                system.AddChild(new UIForia.Elements.UIDivElement(), 0);
                system.AddChild(new UIForia.Elements.UIGroupElement(), 1);
            },
            elements = new System.Action<UIForia.ElementSystem>[] {
                //0 <Div> line 4:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new UIForia.Elements.UIPanelElement(), 2);
                    system.AddDragCreateHandler(UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, 0);
                    system.AddMouseEventHandler(UIForia.UIInput.InputEventType.MouseDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, 1);
                    system.AddKeyboardEventHandler(UIForia.UIInput.InputEventType.KeyDown, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, ((UnityEngine.KeyCode)2147483647), '\0', 2);
                    system.RegisterForKeyboardEvents();
                },
                //1 <Group> line 13:10
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                },
                //2 <Panel> line 5:14
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate(), 3);
                    system.SetBindings(2, -1, -1, -1, 1);
                    system.AddDragEventHandler(UIForia.UIInput.InputEventType.DragDrop, UIForia.UIInput.KeyboardModifiers.None, false, UIForia.UIInput.EventPhase.Bubble, 3);
                    system.CreateBindingVariable<string>(0, @"someContextValue");
                },
                //3 <ExpandedTemplate> line 6:18
                (UIForia.ElementSystem system) => {
                    system.InitializeHydratedElement(0, 0);
                    system.OverrideSlot(@"title", 4);
                    system.HydrateElement(typeof(TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate));
                    system.SetBindings(3, -1, -1, -1, 0);
                },
                //4 <override:title> line 7:22
                (UIForia.ElementSystem system) => {
                    system.InitializeSlotElement(1, 1, 2);
                    system.InitializeDynamicAttribute(@"x");
                    system.AddChild(new UIForia.Elements.UITextElement(), 5);
                    system.SetBindings(4, -1, -1, -1, 0);
                },
                //5 <Text> line 7:22
                (UIForia.ElementSystem system) => {
                    system.InitializeElement(0, 0);
                    system.SetText(@"Hello");
                }
            },
            bindings = new System.Action<UIForia.Systems.LinqBindingNode>[] {
                // 0 Update Binding <Contents> line 3:6
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    bindingNode.InvokeUpdate();
                },
                // 1 Const Binding <Contents> line 3:6
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate element;

                    element = bindingNode.element as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    element.SetAttribute(@"z", (5 * 5).ToString());
                },
                // 2 Update Binding <Panel> line 5:14
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate context_0;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    bindingNode.SetBindingVariable<string>(0, context_0.GetAttribute(@"y"));
                },
                // 3 Update Binding <ExpandedTemplate> line 6:18
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate context_0;
                    TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate element;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    element = bindingNode.element as TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate;
                    element.SetAttribute(@"x", context_0.GetAttribute(@"z"));
                },
                // 4 Update Binding <override:title> line 7:22
                (UIForia.Systems.LinqBindingNode bindingNode) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate refContext_1;
                    UIForia.Elements.UISlotOverride element;

                    refContext_1 = bindingNode.referencedContexts[1] as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    element = bindingNode.element as UIForia.Elements.UISlotOverride;
                    element.SetAttribute(@"x", refContext_1.GetAttribute(@"y"));
                }
            },
            inputEventHandlers = new System.Action<UIForia.Systems.LinqBindingNode, UIForia.Systems.InputEventHolder>[] {
                // 0
                (UIForia.Systems.LinqBindingNode bindingNode, UIForia.Systems.InputEventHolder __eventHolder) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate context_0;
                    UIForia.UIInput.MouseInputEvent __evt;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    __evt = __eventHolder.mouseEvent;
                    __eventHolder.dragCreateResult = new TemplateCompiler2Test.TestDragEvent();
                },
                // 1
                (UIForia.Systems.LinqBindingNode bindingNode, UIForia.Systems.InputEventHolder __eventHolder) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate context_0;
                    UIForia.UIInput.MouseInputEvent __evt;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    __evt = __eventHolder.mouseEvent;
                    context_0.HandleMouseDown(__evt);
                },
                // 2
                (UIForia.Systems.LinqBindingNode bindingNode, UIForia.Systems.InputEventHolder __eventHolder) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate context_0;
                    UIForia.UIInput.KeyboardInputEvent __evt;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    __evt = __eventHolder.keyEvent;
                    context_0.parentVal = @"set";
                },
                // 3
                (UIForia.Systems.LinqBindingNode bindingNode, UIForia.Systems.InputEventHolder __eventHolder) => {
                    TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate context_0;
                    UIForia.UIInput.DragEvent __evt;

                    context_0 = bindingNode.root as TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate;
                    __evt = __eventHolder.dragEvent;
                    context_0.parentVal = @"dropped";
                }
            }
        };

    }

}