using System;
using UIForia.Extensions;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    // this is intended to be immutable after being constructed
    public class TemplateFileShell {

        public string filePath;
        public DateTime lastWriteTime; // make ulong if needed for serializer

        public TextContent[] textContents;
        public TemplateASTRoot[] rootNodes;
        public TemplateASTNode[] templateNodes;
        public AttributeDefinition3[] attributeList;
        public UsingDeclaration[] usings;
        public SlotAST[] slots;
        public char[] charBuffer;
        public StyleDeclaration[] styles;

        // --- not serialized ---
        public ProcessedType[] processedTypes; // todo -- remove 
        public bool successfullyParsed;
        public bool successfullyValidated;

        // ----------------------

        public TemplateFileShell(string templateLocation = null) {
            this.filePath = templateLocation;
        }
        
        public bool TryGetTextContent(int nodeIndex, out RangeInt textContent) {
            if (textContents == null) {
                textContent = default;
                return false;
            }

            // for (int i = 0; i < textContents.Length; i++) {
            //     if (textContents[i].index == nodeIndex) {
            //         textContent = textContents[i].textExpressions;
            //         return true;
            //     }
            // }

            textContent = default;
            return false;
        }

        public TemplateASTRoot GetRootTemplateForType(ProcessedType processedType) {
            unsafe {

                fixed (char* buffer = charBuffer) {

                    for (int i = 0; i < rootNodes.Length; i++) {
                        RangeInt range = rootNodes[i].templateNameRange;
                        if (StringUtil.EqualsRangeUnsafe(processedType.templateId, buffer, range.start, range.length))
                            return rootNodes[i];
                    }
                }
            }

            return default;
        }

        public bool IsTextConstant(int templateNodeId) {
            if (TryGetTextContent(templateNodeId, out RangeInt textExpressionRange)) {
                if (textExpressionRange.length == 0) return false;

                for (int i = textExpressionRange.start; i < textExpressionRange.end; i++) {
                    // if (textExpressions[i].isExpression) {
                    //     return false;
                    // }
                }

                return true;
            }

            return false;
        }

        public string GetFormattedTagName(ProcessedType processedType, int templateNodeIndex) {
            ref TemplateASTNode node = ref templateNodes[templateNodeIndex];
            // switch (node.templateNodeType) {
            //     case TemplateNodeType.Unresolved:
            //         return "unresolved";
            //
            //     case TemplateNodeType.SlotDefine:
            //         return "define:" + GetRawTagName(templateNodeIndex);
            //
            //     case TemplateNodeType.SlotForward:
            //         return "<forward:" + GetRawTagName(templateNodeIndex);
            //
            //     case TemplateNodeType.SlotOverride:
            //         return "override:" + GetRawTagName(templateNodeIndex);
            //
            //     case TemplateNodeType.Root:
            //         return processedType.tagName;
            //
            //     case TemplateNodeType.Expanded:
            //     case TemplateNodeType.Container:
            //     case TemplateNodeType.Text: {
            //         // todo -- might want to print out a nice generic string if type is generic
            //         string moduleName = GetModuleName(templateNodeIndex);
            //         string tagName = processedType.tagName; //GetRawTagName(templateNodeIndex);
            //         if (moduleName != null) {
            //             return moduleName + ":" + tagName;
            //         }
            //
            //         return tagName;
            //     }
            //
            //     case TemplateNodeType.Repeat:
            //         return "Repeat";
            // }

            return null;
        }

        public string GetTypeName(int nodeIndex) {
            return processedTypes[nodeIndex].rawType.GetTypeName();
        }

        public void Serialize(ref ManagedByteBuffer buffer) {

            buffer.Write(filePath);
            buffer.Write(rootNodes);
            buffer.Write(templateNodes);
            buffer.Write(slots);
            buffer.Write(attributeList);
            buffer.Write(charBuffer);
            buffer.Write(textContents);
            buffer.Write(styles);

        }

        public void Deserialize(ref ManagedByteBuffer buffer) {
            buffer.Read(out filePath);
            buffer.Read(out rootNodes);
            buffer.Read(out templateNodes);
            buffer.Read(out slots);
            buffer.Read(out attributeList);
            buffer.Read(out charBuffer);
            buffer.Read(out textContents);
            buffer.Read(out styles);
        }

        public string GetString(RangeInt stringRange) {
            if (stringRange.length == 0) {
                return null;
            }

            if (stringRange.start < 0 || stringRange.start >= charBuffer.Length) {
                return null;
            }
            
            if (stringRange.end < 0 || stringRange.end > charBuffer.Length || stringRange.end < stringRange.start) {
                return null;
            }

            return new string(charBuffer, stringRange.start, stringRange.length);

        }

    }

}