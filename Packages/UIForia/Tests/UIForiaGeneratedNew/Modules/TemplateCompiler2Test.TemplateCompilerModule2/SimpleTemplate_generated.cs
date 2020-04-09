using System;
using TemplateCompiler2Test;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Systems;

// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_TestApp : TemplateLoader {
    
        public static readonly TemplateData template_28f7eb09_98c3_46bf_82b6_1bce7cdb8b64 = new TemplateData ("SimpleTemplate") {
            entry = system => {
                TestTemplateCompiler2.SimpleTemplate element;

                element = new TestTemplateCompiler2.SimpleTemplate();
                system.InitializeEntryPoint(element, 2, 2);
                system.InitializeStaticAttribute(@"x", @"x");
                system.InitializeStaticAttribute(@"y", @"y");
                system.SetBindings(0, -1, 0);
                system.HydrateEntryPoint();
                return element;
            },
            hydrate = system => {
                system.AddChild(new UIDivElement(), 0);
                system.AddChild(new UIGroupElement(), 1);
            },
            elements = new Action<ElementSystem>[] {
                //0 <Div> line 3:10
                system => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new UIPanelElement(), 2);
                },
                //1 <Group> line 12:10
                system => {
                    system.InitializeElement(0, 0);
                },
                //2 <Panel> line 4:14
                system => {
                    system.InitializeElement(0, 1);
                    system.AddChild(new TestTemplateCompiler2.ExpandedTemplate(), 3);
                    system.SetBindings(1, -1, 1);
                    system.CreateBindingVariable<string>(0, @"someContextValue");
                },
                //3 <ExpandedTemplate> line 5:18
                system => {
                    system.InitializeElement(0, 2);
                    system.OverrideSlot(@"title", 0);
                    system.SetBindings(2, 3, 1);
                    system.CreateBindingVariable<string>(0, @"sync:value"); // system.contextVariables.Push();
                    system.ReferenceBindingVariable(1, @"someContextValue"); // findContextVariable
                    // system.ReferenceExposedBindingVariablestring>(2, @"xx");
                    system.HydrateElement(typeof(TestTemplateCompiler2.ExpandedTemplate));
                }
            },
            bindings = new Action<LinqBindingNode>[] {
                // 0
                bindingNode => {
                    bindingNode.InvokeUpdate();
                },
                // 1
                bindingNode => {
                    TestTemplateCompiler2.SimpleTemplate context_0;

                    context_0 = bindingNode.root as TestTemplateCompiler2.SimpleTemplate;
                    bindingNode.SetBindingVariable(0, context_0.GetAttribute(@"y"));
                },
                // 2
                bindingNode => {
                    TestTemplateCompiler2.SimpleTemplate context_0;
                    TestTemplateCompiler2.ExpandedTemplate element;
                    string __value;

                    context_0 = bindingNode.root as TestTemplateCompiler2.SimpleTemplate;
                    element = bindingNode.element as TestTemplateCompiler2.ExpandedTemplate;
                    element.internal__dontcallmeplease_SetEnabledIfBinding(context_0.GetAttribute(@"x") == @"x");
                    __value = context_0.parentVal;
                    element.value = __value;
                    bindingNode.SetBindingVariable(0, __value);
                },
                // 3
                bindingNode => {
                    TestTemplateCompiler2.ExpandedTemplate element;
                    TestTemplateCompiler2.SimpleTemplate context_0;
                    string __right;

                    element = bindingNode.element as TestTemplateCompiler2.ExpandedTemplate;
                    context_0 = bindingNode.root as TestTemplateCompiler2.SimpleTemplate;
                    __right = context_0.parentVal;
                    if (bindingNode.GetBindingVariable<string>(0) == __right)
                    {
                        context_0.parentVal = element.value;
                        context_0.ValueChanged(__right, PropertyChangeSource.Synchronized);
                    }
                }
            }
        };

    }

}