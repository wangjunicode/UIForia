using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateDataBuilder {

        internal LambdaExpression entryPoint;
        internal LambdaExpression hydrate;
        internal LightList<TemplateOutput> elementFns;
        internal LightList<BindingOutput> bindingFns;

        internal int bindingIndex;
        internal int templateIndex;

        public TemplateDataBuilder() {
            elementFns = new LightList<TemplateOutput>();
            bindingFns = new LightList<BindingOutput>();
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
                bindings = bindingFns.ToArray(),
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
            bindingFns.Clear();
            templateIndex = 0;
            bindingIndex = 0;
        }

        public BindingIndices AddBindings(TemplateNode templateNode, BindingResult bindingResult) {
            BindingIndices retn = default;

            retn.updateIndex = -1;
            retn.lateUpdateIndex = -1;
            retn.constIndex = -1;
            retn.enableIndex = -1;

            if (bindingResult.updateLambda != null) {
                retn.updateIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    templateNode = templateNode,
                    expression = bindingResult.updateLambda,
                    bindingType = BindingType.Update
                });

            }

            if (bindingResult.lateLambda != null) {
                retn.lateUpdateIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    templateNode = templateNode,
                    expression = bindingResult.lateLambda,
                    bindingType = BindingType.LateUpdate
                });
            }

            if (bindingResult.constLambda != null) {
                retn.constIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    templateNode = templateNode,
                    expression = bindingResult.constLambda,
                    bindingType = BindingType.Const
                });
            }
            
            if (bindingResult.enableLambda != null) {
                retn.enableIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    templateNode = templateNode,
                    expression = bindingResult.enableLambda,
                    bindingType = BindingType.Enable
                });
            }

            return retn;
        }

    }

    public struct BindingIndices {

        public int updateIndex;
        public int lateUpdateIndex;
        public int constIndex;
        public int enableIndex;

    }

}