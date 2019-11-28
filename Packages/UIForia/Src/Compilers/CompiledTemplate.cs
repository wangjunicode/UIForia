using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEditor;
using UnityEditor.Graphs;

namespace UIForia.Compilers {

    public class CompiledSlot {

        public string filePath;
        public int slotId;
        public GUID guid;
        public string slotName;
        public SlotType slotType;
        public LambdaExpression templateFn;

        public string GetVariableName() {
            return $"Slot_{slotType}_{slotName}_{guid}";
        }
        
    }

    public enum ContextVariableAccessType {

        Public, 
        Private

    }

    public struct ContextVariableDefinition {

        public int id;
        public Type type;
        public string name;
        public ContextVariableAccessType accessType;

    }

    public class CompiledTemplate {

        internal ProcessedType elementType;
        internal AttributeDefinition2[] attributes;
        public string filePath;
        public int templateId;
        public StructList<SlotDefinition> slotDefinitions;
        public int childCount;
        public LambdaExpression templateFn;
        public GUID guid;
        public string slotName;
        public TemplateMetaData templateMetaData;
        public StructStack<ContextVariableDefinition> contextStack;

        public bool TryGetSlotData(string slotName, out SlotDefinition slotDefinition) {
            if (slotDefinitions == null) {
                slotDefinition = default;
                return false;
            }

            for (int i = 0; i < slotDefinitions.Count; i++) {
                if (slotDefinitions[i].tagName == slotName) {
                    slotDefinition = slotDefinitions[i];
                    return true;
                }
            }

            slotDefinition = default;
            return false;
        }

        public int AddSlotData(SlotDefinition slotDefinition) {
            slotDefinitions = slotDefinitions ?? StructList<SlotDefinition>.Get();
            slotDefinition.slotId = (short) slotDefinitions.Count;
            slotDefinitions.Add(slotDefinition);
            return slotDefinition.slotId;
        }

        public int GetSlotId(string slotName) {
            if (slotDefinitions == null) {
                throw new ArgumentOutOfRangeException(slotName, $"Slot name {slotName} was not registered");
            }

            for (int i = 0; i < slotDefinitions.Count; i++) {
                if (slotDefinitions.array[i].tagName == slotName) {
                    return slotDefinitions[i].slotId;
                }
            }

            throw new ArgumentOutOfRangeException(slotName, $"Slot name {slotName} was not registered");
        }

        public void ValidateSlotHierarchy(LightList<string> slotList) {
            // ensure no duplicates
            for (int i = 0; i < slotList.size; i++) {
                string target = slotList[i];
                for (int j = i + 1; j < slotList.size; j++) {
                    if (slotList[j] == target) {
                        throw TemplateParseException.DuplicateSlotName(filePath, target);
                    }
                }
            }

            for (int i = 0; i < slotList.size; i++) {
                TryGetSlotData(slotList[i], out SlotDefinition slotDefinition);

                for (int j = 0; j < 4; j++) {
                    int slotId = slotDefinition[j];
                    if (slotId != SlotDefinition.k_UnassignedParent) {
                        SlotDefinition parentSlotDef = slotDefinitions[slotId];
                        if (slotList.Contains(parentSlotDef.tagName)) {
                            throw TemplateParseException.InvalidSlotHierarchy(filePath, elementType.rawType, slotDefinition.tagName, parentSlotDef.tagName);
                        }
                    }
                    else {
                        break;
                    }
                }
            }
        }

        public IList<string> GetValidSlotNames() {
            if (slotDefinitions == null || slotDefinitions.size == 0) {
                return null;
            }
            LightList<string> retn = new LightList<string>();
            for (int i = 0; i < slotDefinitions.Count; i++) {
                retn.Add(slotDefinitions[i].tagName);
            }

            return retn;
        }

       

    }

}