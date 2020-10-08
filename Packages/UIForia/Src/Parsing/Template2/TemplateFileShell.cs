using System;
using System.Collections.Generic;
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
        public ProcessedType[] typeMappings;
        public bool successfullyParsed;
        public bool checkedTimestamp;

        private Dictionary<RangeInt, string> stringMap;
        // ----------------------

        public TemplateFileShell(string templateLocation = null) {
            this.filePath = templateLocation;
        }

        // public bool TryGetTextContent(int nodeIndex, out RangeInt textContent) {
        //     if (textContents == null) {
        //         textContent = default;
        //         return false;
        //     }
        //
        //     ref TemplateASTNode node = ref templateNodes[nodeIndex];
        //     RangeInt range = node.textContentRange;
        //     for (int i = range.start; i < textContents.Length; i++) {
        //         if (textContents[i].index == nodeIndex) {
        //             textContent = textContents[i].textExpressions;
        //             return true;
        //         }
        //     }
        //
        //     textContent = default;
        //     return false;
        // }

        public TemplateASTRoot GetRootTemplateForType(ProcessedType processedType) {
            
            if (processedType.genericBase != null) {
                processedType = processedType.genericBase;
            }
            
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

        public bool TryGetTextContent(int templateNodeId, StructList<TextContent> textContents) {
            RangeInt range = templateNodes[templateNodeId].textContentRange;
            if (range.length == 0) {
                return false;
            }
            for (int i = range.start; i < range.end; i++) {
               textContents.Add(this.textContents[i]);
            }

            return true;
        }
        
        public bool IsTextConstant(int templateNodeId) {
            RangeInt range = templateNodes[templateNodeId].textContentRange;

            if (range.length == 0) {
                return false;
            }
            
            for (int i = range.start; i < range.end; i++) {
                if (textContents[i].isExpression) {
                    return false;
                }
            }

            return true;
        }
        
        public void Serialize(ref ManagedByteBuffer buffer) {
            buffer.Write(filePath);
            buffer.Write(lastWriteTime);
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
            buffer.Read(out lastWriteTime);
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

            stringMap = stringMap ?? new Dictionary<RangeInt, string>();

            if (stringMap.TryGetValue(stringRange, out string retn)) {
                return retn;
            }
            retn = new string(charBuffer, stringRange.start, stringRange.length);
            stringMap[stringRange] = retn;
            return retn;
        }

        public unsafe CharSpan GetCharSpan(RangeInt stringRange) {
            if (stringRange.length == 0) {
                return default;
            }

            if (stringRange.start < 0 || stringRange.start >= charBuffer.Length) {
                return default;
            }

            if (stringRange.end < 0 || stringRange.end > charBuffer.Length || stringRange.end < stringRange.start) {
                return default;
            }

            fixed (char* buffer = charBuffer) {
                return new CharSpan(buffer, stringRange.start, stringRange.end);
            }
        }

    }

}