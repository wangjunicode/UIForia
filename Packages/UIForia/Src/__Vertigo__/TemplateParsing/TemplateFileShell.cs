using System;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public class TemplateFileShell {

        [SerializeField] internal string filePath;
        [SerializeField] internal DateTime lastWriteTime; // make ulong if needed for serializer

        [SerializeField] internal IndexedString[] tagNames;
        [SerializeField] internal IndexedString[] moduleNames;
        [SerializeField] internal IndexedString[] requiredTypes;
        [SerializeField] internal IndexedString[] genericTypeResolvers;
        [SerializeField] internal TextContent[] textContents;
        [SerializeField] internal TextExpression[] textExpressions;

        [SerializeField] internal TemplateASTRoot[] rootNodes;
        [SerializeField] internal TemplateASTNode[] templateNodes;
        [SerializeField] internal AttributeDefinition2[] attributeList;
        [SerializeField] internal UsingDeclaration[] usings;
        [SerializeField] internal string[] referencedNamespaces;
        [SerializeField] internal SlotAST[] slots;

        internal ProcessedType[] processedTypes;
        [NonSerialized] public bool successfullyParsed;

        public TemplateFileShell(string templateLocation) {
            this.filePath = templateLocation;
        }

        public string GetSlotName(int templateNodeId) {
            if((templateNodes[templateNodeId].templateNodeType & TemplateNodeType.Slot) == 0) {
                return null;
            }
            return FindValueForIndex(tagNames, templateNodeId);
        }
        
        public string GetRequiredType(int nodeIndex) {
            return FindValueForIndex(requiredTypes, nodeIndex);
        }

        public string GetRawTagName(int nodeIndex) {
            return FindValueForIndex(tagNames, nodeIndex);
        }

        public string GetModuleName(int nodeIndex) {
            return FindValueForIndex(moduleNames, nodeIndex);
        }

        public string GetGenericTypeResolver(int nodeIndex) {
            return FindValueForIndex(genericTypeResolvers, nodeIndex);
        }

        public bool TryGetTextContent(int nodeIndex, out RangeInt textContent) {
            if (textContents == null) {
                textContent = default;
                return false;
            }

            for (int i = 0; i < textContents.Length; i++) {
                if (textContents[i].index == nodeIndex) {
                    textContent = textContents[i].textExpressions;
                    return true;
                }
            }

            textContent = default;
            return false;
        }

        private static string FindValueForIndex(IndexedString[] list, int target) {
            // todo -- binary search if large
            if (list == null) return null;
            for (int i = 0; i < list.Length; i++) {
                if (list[i].index == target) {
                    return list[i].value;
                }
            }

            return null;
        }

        public TemplateASTRoot GetRootTemplateForType(ProcessedType processedType) {

            for (int i = 0; i < rootNodes.Length; i++) {

                if (rootNodes[i].templateName == processedType.templateId) {
                    return rootNodes[i];
                }

            }

            return default;

        }

        public bool IsTextConstant(int templateNodeId) {

            if (TryGetTextContent(templateNodeId, out RangeInt textExpressionRange)) {
                if (textExpressionRange.length == 0) return false;

                for (int i = textExpressionRange.start; i < textExpressionRange.end; i++) {
                    if (textExpressions[i].isExpression) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        
        public string GetFormattedTagName(ProcessedType processedType, int templateNodeIndex) {
            ref TemplateASTNode node = ref templateNodes[templateNodeIndex];
            switch (node.templateNodeType) {

                case TemplateNodeType.Unresolved:
                    return "unresolved";

                case TemplateNodeType.SlotDefine:
                    return "define:" + GetRawTagName(templateNodeIndex);

                case TemplateNodeType.SlotForward:
                    return "<forward:" + GetRawTagName(templateNodeIndex);

                case TemplateNodeType.SlotOverride:
                    return "override:" + GetRawTagName(templateNodeIndex);



                case TemplateNodeType.Root:
                    return processedType.tagName;

                case TemplateNodeType.Expanded:
                case TemplateNodeType.Container:
                case TemplateNodeType.Text:
                case TemplateNodeType.TextSpan: {
                    // todo -- might want to print out a nice generic string if type is generic
                    string moduleName = GetModuleName(templateNodeIndex);
                    string tagName = processedType.tagName; //GetRawTagName(templateNodeIndex);
                    if (moduleName != null) {
                        return moduleName + ":" + tagName;
                    }

                    return tagName;
                }

                case TemplateNodeType.Repeat:
                    return "Repeat";
            }

            return null;
        }

        public string GetTypeName(int nodeIndex) {
            return processedTypes[nodeIndex].rawType.GetTypeName();
        }


    }

}