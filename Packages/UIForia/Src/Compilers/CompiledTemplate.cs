using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEditor;

namespace UIForia.Compilers {

    public class CompiledTemplate {

        internal ProcessedType elementType;
        internal AttributeDefinition2[] attributes;
        public string filePath;
        public int templateId;
        public StructList<SlotDefinition> slotDefinitions;
        public LambdaExpression templateFn;
        public GUID guid;
        public TemplateMetaData templateMetaData;
        public StructStack<ContextVariableDefinition> contextStack;
        public string templateName;

        public bool TryGetSlotData(string slotName, out SlotDefinition slotDefinition) {
            if (slotDefinitions == null) {
                slotDefinition = default;
                return false;
            }

            for (int i = 0; i < slotDefinitions.Count; i++) {
                if (slotDefinitions[i].slotName == slotName) {
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
        
    }

}