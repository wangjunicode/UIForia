// using System.Runtime.InteropServices;
// using UIForia.Compilers;
// using UIForia.Elements;
// using UIForia.Parsing;
// using Unity.Jobs;
//
// namespace UIForia {
//
//     /// <summary>
//     /// Resolves all tag names in a module and ensures the template hierarchies are valid.
//     /// </summary>
//     internal struct ResolveTagNamesAndValidateTemplates_ManagedJob : IJob {
//
//         private GCHandle moduleHandle;
//
//         public ResolveTagNamesAndValidateTemplates_ManagedJob(Module module) {
//             this.moduleHandle = GCHandle.Alloc(module);
//         }
//
//         public void Execute() {
//             Module module = (Module) moduleHandle.Target;
//
//             Diagnostics diagnostics = module.GetDiagnostics();
//
//             for (int i = 0; i < module.templateShells.size; i++) {
//
//                 ref TemplateFileShell templateFile = ref module.templateShells.array[i];
//
//                 if (templateFile.processedTypes == null || templateFile.processedTypes.Length < templateFile.templateNodes.Length) {
//                     templateFile.processedTypes = new ProcessedType[templateFile.templateNodes.Length];
//                 }
//
//                 for (int j = 0; j < templateFile.templateNodes.Length; j++) {
//
//                     ProcessTemplate(templateFile, ref templateFile.templateNodes[j], module, diagnostics);
//                     
//                 }
//
//             }
//
//             moduleHandle.Free();
//         }
//
//         private static void ProcessTemplate(TemplateFileShell templateFile, ref TemplateASTNode templateNode, Module module, Diagnostics diagnostics) {
//
//             switch (templateNode.templateNodeType) {
//
//                 case TemplateNodeType.TextSpan: {
//                     templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UITextSpanElement));
//                     return;
//                 }
//                 case TemplateNodeType.Text: {
//                     templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UITextElement));
//                     return;
//                 }
//                 case TemplateNodeType.Unresolved:
//                 case TemplateNodeType.Expanded:
//                 case TemplateNodeType.Container: {
//
//                     string moduleName = templateFile.GetModuleName(templateNode.index);
//                     string tagName = templateFile.GetRawTagName(templateNode.index);
//
//                     // todo -- file line & column
//                     if (!module.TryResolveTagName(moduleName, tagName, diagnostics, out ProcessedType processedType)) {
//                         return;
//                     }
//
//                     templateFile.processedTypes[templateNode.index] = processedType;
//
//                     if (templateNode.templateNodeType == TemplateNodeType.Unresolved) {
//                         if (typeof(UITextElement).IsAssignableFrom(processedType.rawType)) {
//                             templateNode.templateNodeType = TemplateNodeType.Text;
//                         }
//                         else if (processedType.IsContainerElement) {
//                             templateNode.templateNodeType = TemplateNodeType.Container;
//                         }
//                         else {
//                             templateNode.templateNodeType = TemplateNodeType.Expanded;
//                         }
//                     }
//                     
//                     if (!processedType.IsUnresolvedGeneric) {
//                         return;
//                     }
//
//                     break;
//                 }
//
//                 case TemplateNodeType.SlotDefine:
//                 case TemplateNodeType.SlotForward:
//                 case TemplateNodeType.SlotOverride: {
//
//                     templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));
//
//                     if (templateNode.templateNodeType == TemplateNodeType.SlotForward || templateNode.templateNodeType == TemplateNodeType.SlotOverride) {
//                         ref TemplateASTNode parent = ref templateFile.templateNodes[templateNode.parentId];
//                         string name = templateNode.templateNodeType == TemplateNodeType.SlotForward ? "forward" : "override";
//                         if (parent.templateNodeType != TemplateNodeType.Expanded) {
//                             diagnostics.LogError(templateFile.GetFormattedTagName(null, templateNode.index) + " does not support " + name + " slot nodes");
//                             return;
//                         }
//                     }
//
//                     break;
//                 }
//
//                 case TemplateNodeType.Root:
//                     return;
//
//                 case TemplateNodeType.Repeat:
//                     templateFile.processedTypes[templateNode.index] = TypeProcessor.GetProcessedType(typeof(UIRepeatElement<>));
//                     break;
//
//             }
//
//             ProcessedType elementType = templateFile.processedTypes[templateNode.index];
//        
//             string genericTypeResolver = templateFile.GetGenericTypeResolver(templateNode.index);
//
//             if (!string.IsNullOrEmpty(genericTypeResolver)) {
//
//                 elementType = TypeProcessor.ResolveGenericElementType(elementType, genericTypeResolver, templateFile.referencedNamespaces, diagnostics);
//
//                 if (elementType == null) {
//                     return;
//                 }
//
//                 templateFile.processedTypes[templateNode.index] = elementType;
//
//             }
//         }
//
//     }
//
// }