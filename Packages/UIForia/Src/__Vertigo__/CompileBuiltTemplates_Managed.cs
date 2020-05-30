using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using FastExpressionCompiler;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public struct CompileBuiltTemplates_Managed : IJob {

        public int templateIndex;
        public GCHandleList<TemplateExpressionSet> compiledTemplateHandle;
        public GCHandleList<TemplateData> outputList;
        public PerThreadObject<Diagnostics> perThread_diagnostics;

        [NativeSetThreadIndex] public int threadIndex;

        public void Execute() {

            LightList<TemplateData> output = outputList.Get();
            TemplateExpressionSet set = compiledTemplateHandle.Get()[templateIndex];
            TemplateData templateData = new TemplateData(set.processedType.tagName);
            Diagnostics diagnostics = perThread_diagnostics.GetForThread(threadIndex);

            try {
                // could skip entry point to save time if not wanted (usually only 1 entry fn is used)
                templateData.entry = set.entryPoint?.TryCompileWithoutClosure<Func<TemplateSystem, UIElement>>();

                BlockExpression block = (BlockExpression) set.hydratePoint.Body;
                if (block.Expressions.Count == 0) {
                    templateData.hydrate = (system) => { };
                }
                else {
                    templateData.hydrate = set.hydratePoint.TryCompileWithoutClosure<Action<TemplateSystem>>();
                }

                templateData.elements = new Action<TemplateSystem>[set.elementTemplates.Length];
                templateData.bindings = new Action<LinqBindingNode>[set.bindings.Length];
                templateData.inputEventHandlers = new Action<LinqBindingNode, InputEventHolder>[set.inputEventHandlers.Length];

                for (int i = 0; i < set.elementTemplates.Length; i++) {
                    templateData.elements[i] = set.elementTemplates[i].expression.TryCompileWithoutClosure<Action<TemplateSystem>>();
                }

                for (int i = 0; i < set.bindings.Length; i++) {
                    // could try to fast compile first then do slow if failed. currently slow because dont know if user did something crazy and slow is safer
                    templateData.bindings[i] = (Action<LinqBindingNode>) set.bindings[i].expression.CompileFast();
                }

                for (int i = 0; i < set.inputEventHandlers.Length; i++) {
                    // could try to fast compile first then do slow if failed. currently slow because dont know if user did something crazy and slow is safer
                    try {
                        templateData.inputEventHandlers[i] = (Action<LinqBindingNode, InputEventHolder>) set.inputEventHandlers[i].expression.CompileFast();
                    }
                    catch (Exception e) {
                        Debugger.Break();
                    }
                }

                templateData.type = set.processedType.rawType;
                output.array[templateIndex] = templateData;

                // I think this needs to also be called for every closure in a binding
                GCHandle.Alloc(templateData); // unity issue where unreferenced type gets gc'd causing crash when they re-scan types

            }
            catch (Exception e) {
                Debug.Log(e);
                diagnostics.LogException(e);
                output.array[templateIndex] = null;
            }
        }

    }

}