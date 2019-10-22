using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEditor;

namespace UIForia.Compilers {

    public class CompiledBinding {

        public int line;
        public int column;
        public string filePath;
        public string elementTag;
        public int bindingId;
        public string guid;
        public LambdaExpression bindingFn;

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

        internal UIElement Create(UIElement root, TemplateScope2 scope) {
            // todo -- get rid of the build call here
            scope.application.templateData.Build();
            return scope.application.templateData.templateFns[templateId](root, scope);
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