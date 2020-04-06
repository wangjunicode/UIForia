using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateDataBuilder {

        internal Dictionary<ProcessedType, int> idMap;
        internal Dictionary<string, int> slotIdMap;
        internal LambdaExpression entryPoint;
        internal LambdaExpression hydrate;
        internal LightList<TemplateOutput> elementFns;
        internal LightList<TemplateOutput> slotFns;

        public TemplateDataBuilder() {
            elementFns = new LightList<TemplateOutput>();
            slotFns = new LightList<TemplateOutput>();
        }

        public int GetTemplateIndex(ProcessedType processedType) {
            idMap = idMap ?? new Dictionary<ProcessedType, int>();
            if (idMap.TryGetValue(processedType, out int id)) {
                return id;
            }

            id = idMap.Count;
            idMap[processedType] = id;
            return id;
        }

        public int GetSlotIndex(string slotName) {
            slotIdMap = slotIdMap ?? new Dictionary<string, int>();
            if (slotIdMap.TryGetValue(slotName, out int id)) {
                return id;
            }

            id = slotIdMap.Count;
            slotIdMap[slotName] = id;
            return id;

        }

        public void SetElementTemplate(TemplateNode templateNode, int elementSlotId, LambdaExpression expression) {
            elementFns.EnsureCapacity(elementSlotId);
            
            if (elementFns.size <= elementSlotId) {
                elementFns.size = elementSlotId + 1;
            }

            elementFns.array[elementSlotId] = new TemplateOutput() {
                expression = expression,
                templateNode = templateNode
            };
        }

        public TemplateExpressionSet Build(ProcessedType processedType) {
            return new TemplateExpressionSet() {
                bindings = null,
                processedType = processedType,
                entryPoint = entryPoint,
                hydratePoint = hydrate,
                elementTemplates = elementFns.ToArray(),
            };
        }

        public void SetEntryPoint(LambdaExpression entryPoint) {
            this.entryPoint = entryPoint;
        }

        public void SetHydratePoint(LambdaExpression hydrate) {
            this.hydrate = hydrate;
        }

        public void SetSlotTemplate(SlotNode slotNode, int slotId, LambdaExpression expression) {
            slotFns.EnsureCapacity(slotId);
            
            if (slotFns.size <= slotId) {
                slotFns.size = slotId + 1;
            }

            slotFns.array[slotId] = new TemplateOutput() {
                expression = expression,
                templateNode = slotNode
            };
        }
        
        public void Clear() {
            entryPoint = null;
            hydrate = null;
            elementFns.Clear();
            idMap?.Clear();
            slotIdMap?.Clear();
        }

    }

}