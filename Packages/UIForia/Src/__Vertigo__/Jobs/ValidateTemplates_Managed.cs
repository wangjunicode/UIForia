using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public struct GatherDiagnostics_Managed : IJob {

        public PerThreadObject<Diagnostics> perThread_diagnostics;
        public GCHandle<Diagnostics> gathered;
        
        public void Execute() {

            Diagnostics[] list = perThread_diagnostics.handle.Get();
            Diagnostics output = gathered.Get();
            
            for(int i = 0; i < list.Length; i++) {
                
                if (list[i] != null) {
                    output.diagnosticList.AddRange(list[i].diagnosticList);            
                }   
                
            }

        }

    }

    public struct ValidateTemplates_Managed : IVertigoParallel {

        public ParallelParams parallel { get; set; }
        public GCHandleArray<TemplateFileShell> templateFileArray;
        public PerThreadObject<Diagnostics> perThread_diagnostics;

        [NativeSetThreadIndex] public int threadIndex;

        public void Execute() {
            Run(0, templateFileArray.Get().Length);
        }

        public void Execute(int start, int end) {
            Run(start, end);
        }

        private void Run(int start, int end) {
            TemplateFileShell[] templateFileShells = templateFileArray.Get();
            Diagnostics diagnostics = perThread_diagnostics.GetForThread(threadIndex);

            for (int i = start; i < end; i++) {

                TemplateFileShell templateFile = templateFileShells[i];

                if (!templateFile.successfullyParsed) {
                    continue;
                }

                if (templateFile.processedTypes == null || templateFile.processedTypes.Length < templateFile.templateNodes.Length) {
                    templateFile.processedTypes = new ProcessedType[templateFile.templateNodes.Length];
                }

                int diagnosticCount = diagnostics.diagnosticList.Count;
                
                for (int j = 0; j < templateFile.templateNodes.Length; j++) {
                    try {
                        ProcessTemplate(templateFile, ref templateFile.templateNodes[j], diagnostics);
                    }
                    catch (Exception e) {
                        UnityEngine.Debug.Log(e);
                        templateFile.successfullyValidated = false;
                    }
                }

                templateFile.successfullyValidated = diagnosticCount == diagnostics.diagnosticList.Count;

            }

        }

        private static void ProcessTemplate(TemplateFileShell templateFile, ref TemplateASTNode templateNode, Diagnostics diagnostics) {

            switch (templateNode.templateNodeType) {

                case TemplateNodeType.TextSpan: {
                    templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UITextSpanElement));
                    return;
                }

                case TemplateNodeType.Text: {
                    templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UITextElement));
                    return;
                }

                case TemplateNodeType.Unresolved:
                case TemplateNodeType.Expanded:
                case TemplateNodeType.Container: {

                    string moduleName = templateFile.GetModuleName(templateNode.index);
                    string tagName = templateFile.GetRawTagName(templateNode.index);

                    if (!templateFile.module.TryResolveTagName(moduleName, tagName, diagnostics, templateFile.filePath, templateNode.GetLineInfo(), out ProcessedType processedType)) {
                        return;
                    }

                    templateFile.processedTypes[templateNode.index] = processedType;

                    if (templateNode.templateNodeType == TemplateNodeType.Unresolved) {
                        if (typeof(UITextElement).IsAssignableFrom(processedType.rawType)) {
                            templateNode.templateNodeType = TemplateNodeType.Text;
                        }
                        else if (processedType.IsContainerElement) {
                            templateNode.templateNodeType = TemplateNodeType.Container;
                        }
                        else {
                            templateNode.templateNodeType = TemplateNodeType.Expanded;
                        }
                    }

                    if (!processedType.IsUnresolvedGeneric) {
                        return;
                    }

                    break;
                }

                case TemplateNodeType.SlotDefine:
                case TemplateNodeType.SlotForward:
                case TemplateNodeType.SlotOverride: {

                    templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));

                    if (templateNode.templateNodeType == TemplateNodeType.SlotForward || templateNode.templateNodeType == TemplateNodeType.SlotOverride) {
                        ref TemplateASTNode parent = ref templateFile.templateNodes[templateNode.parentId];
                        string name = templateNode.templateNodeType == TemplateNodeType.SlotForward ? "forward" : "override";
                        if (parent.templateNodeType != TemplateNodeType.Expanded) {
                            diagnostics.LogError(templateFile.GetFormattedTagName(null, templateNode.index) + " does not support " + name + " slot nodes");
                            return;
                        }
                    }

                    break;
                }

                case TemplateNodeType.Root:
                    return;

                case TemplateNodeType.Repeat:
                    templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UIRepeatElement<>));
                    break;

            }

            ProcessedType elementType = templateFile.processedTypes[templateNode.index];

            string genericTypeResolver = templateFile.GetGenericTypeResolver(templateNode.index);

            if (!string.IsNullOrEmpty(genericTypeResolver)) {

                elementType = TypeProcessor.ResolveGenericElementType(elementType, genericTypeResolver, templateFile.referencedNamespaces, diagnostics);

                if (elementType == null) {
                    return;
                }

                templateFile.processedTypes[templateNode.index] = elementType;

            }
        }

    }

}