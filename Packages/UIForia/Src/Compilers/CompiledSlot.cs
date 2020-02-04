using System;
using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEditor;

namespace UIForia.Compilers {

    public class CompiledSlot {

        public string filePath;
        public int slotId;
        public int overrideDepth;
        public GUID guid;
        public string slotName;
        public SlotType slotType;
        public LambdaExpression templateFn;
        public string templateName;
        public Type rootElementType;
        public ContextVariableDefinition[] scopedVariables;
        public AttributeDefinition[] exposedAttributes;
        public StructList<AttributeDefinition> originalAttributes;
        public LightList<ExposedVariableData> exposedVariableDataList;

        public CompiledSlot() {
            this.slotId = -1;
        }

        public string GetVariableName() {
            return $"Slot_{slotType}_{slotId}_{guid}";
        }

        public string GetComment() {
            string retn = "Slot name=\"" + slotName + "\"";

            switch (slotType) {
                case SlotType.Define:
                    retn += " (Define) " + filePath;
                    break;

                case SlotType.Forward:
                    retn += " (Forward) " + filePath;
                    break;

                case SlotType.Override:
                    retn += " (Override) " + filePath;
                    break;
                case SlotType.Template:
                    retn += " (Template) " + filePath;
                    break;
            }

            retn += "        id = " + slotId;
            return retn;
        }

    }

}