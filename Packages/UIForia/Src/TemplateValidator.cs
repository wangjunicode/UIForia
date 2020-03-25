using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Src {

    public class UIForiaLogger {

        public void Log() { }

    }

    public class DiagnosticProvider {

        private readonly object locker = new object();

        public void SetContext(string fileName, int lineNumber, int columnNumber) { }

    }

    internal static class TemplateValidator {

        public static void Validate(TemplateShell templateShell) {

            LightStack<TemplateNode> stack = LightStack<TemplateNode>.Get();

            Module module = templateShell.module;
            
            for (int i = 0; i < templateShell.templateRootNodes.size; i++) {
                string templateId = templateShell.templateRootNodes.array[i].templateName;
                for (int j = i + 1; j < templateShell.templateRootNodes.size; j++) {
                    if (templateId == templateShell.templateRootNodes.array[j].templateName) {
                        Debug.LogError($"Duplicate template name {templateId ?? "default"} in {templateShell.filePath}");
                        break;
                    }
                }
            }

            for (int i = 0; i < templateShell.templateRootNodes.size; i++) {
        
                TemplateRootNode rootNode = templateShell.templateRootNodes.array[i];
                stack.Push(rootNode);

                // todo -- revisit this and make it just verify that element nodes still resolve to same processed type and no types are missing
                
                // validate that container types do not have slot overrides
                // validate that slot parents are ElementNodes and not containers

                while (stack.size != 0) {
                    TemplateNode node = stack.Pop();
                    node.root = rootNode;

                    if (node is ElementNode elementNode) {

                        // module not found
                        // module not included as dependency
                        // tag not found, looked in following modules
                        
                        if (elementNode.processedType == null) {
                            // SetErrorContext(elementNode.lineInfo);
                            Debug.LogError($"Unable to resolve tag name <{elementNode.GetTagName()}> in file {rootNode.templateShell.filePath}");
                            continue;
                        }

                        if (elementNode.processedType.IsContainerElement) {
                            if (elementNode.slotOverrideNodes != null && elementNode.slotOverrideNodes.size != 0) {
                                Debug.LogError($"Container element <{elementNode.GetTagName()}> in file {rootNode.templateShell.filePath} cannot contain slot overrides");
                                continue;
                            }
                        }

                    }
                    else if (node is SlotNode slotNode) {

                        switch (slotNode.slotType) {

                            case SlotType.Forward: {

                                if (!(slotNode.parent is ElementNode expanded && !expanded.processedType.IsContainerElement)) {
                                    Debug.LogError(InvalidSlotOverride("forward", slotNode.parent.TemplateNodeDebugData, slotNode.TemplateNodeDebugData));
                                    continue;
                                }

                                break;
                            }
                            case SlotType.Override: {

                                if (!(slotNode.parent is ElementNode expanded && !expanded.processedType.IsContainerElement)) {
                                    Debug.LogError(InvalidSlotOverride("override", slotNode.parent.TemplateNodeDebugData, slotNode.TemplateNodeDebugData));
                                    continue;
                                }

                                break;
                            }
                        }

                    }

                    for (int c = 0; c < node.ChildCount; c++) {
                        stack.Push(node[c]);
                    }

                }

            }

            stack.Release();

        }

        public static string InvalidSlotOverride(string verb, TemplateNodeDebugData parentData, TemplateNodeDebugData childData) {
            return $"Error while parsing {parentData.fileName}. Slot overrides can only be defined as a direct child of an expanded template. <{parentData.tagName}> is not an expanded template and cannot support slot {verb} <{childData.tagName}>";
        }

    }

}