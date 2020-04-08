using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateDataBuilder {

        internal LambdaExpression entryPoint;
        internal LambdaExpression hydrate;
        internal LightList<TemplateOutput> elementFns;
        internal LightList<LambdaExpression> bindingFns;

        internal int bindingIndex;
        internal int templateIndex;
        
        public TemplateDataBuilder() {
            elementFns = new LightList<TemplateOutput>();
            bindingFns = new LightList<LambdaExpression>();
        }

        public int GetNextTemplateIndex() {
            return templateIndex++;
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

        public void Clear() {
            entryPoint = null;
            hydrate = null;
            elementFns.Clear();
            templateIndex = 0;
            bindingIndex = 0;
        }

        public BindingIndices AddBindings(BindingResult bindingResult) {
            BindingIndices retn = default;

            retn.updateIndex = -1;
            retn.lateUpdateIndex = -1;
            
            if (bindingResult.updateLambda != null) {
                retn.updateIndex = bindingFns.size;
                bindingFns.Add(bindingResult.updateLambda);
            }

            if (bindingResult.lateLambda != null) {
                retn.lateUpdateIndex = bindingFns.size;
                bindingFns.Add(bindingResult.lateLambda);
            }

            return retn;
        }

    }

    public struct BindingIndices {

        public int updateIndex;
        public int lateUpdateIndex;

    }

}