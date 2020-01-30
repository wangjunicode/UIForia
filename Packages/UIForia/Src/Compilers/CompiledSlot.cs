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
        public GUID guid;
        public string slotName;
        public SlotType slotType;
        public LambdaExpression templateFn;
        public string templateName;
        public Type rootElementType;
        public ScopedContextVariable[] scopedVariables;
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
            if (slotType == SlotType.Define) {
                retn += " (Default)";
            }
            else if (slotType == SlotType.Override) {
                retn += " (Override" + filePath + ")";
            }
            else if (slotType == SlotType.Children) {
                retn += " (Children" + filePath + ")";
            }

            retn += " id = " + slotId;
            return retn;
        }

    }

}