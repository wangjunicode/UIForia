using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateDataBuilder {

        internal LambdaExpression entryPoint;
        internal LambdaExpression hydrate;
        internal LightList<TemplateOutput> elementFns;
        internal LightList<BindingOutput> bindingFns;
        internal LightList<InputEventHandlerOutput> inputHandlerFns;
        internal LightList<SlotOverrideChain> slotOverrideChains;
        private static readonly SlotOverrideInfo[] s_EmptySlotOverrideInfo = { };

        private int dragCreatorIndex;
        private int templateIndex;
        private int eventHandlerIndex;

        public TemplateDataBuilder() {
            inputHandlerFns = new LightList<InputEventHandlerOutput>();
            elementFns = new LightList<TemplateOutput>();
            bindingFns = new LightList<BindingOutput>();
            slotOverrideChains = new LightList<SlotOverrideChain>();
        }

        public int GetNextDragCreateIndex() {
            return dragCreatorIndex++;
        }
        
        public int GetNextTemplateIndex() {
            return templateIndex++;
        }
        
        public int GetNextInputHandlerIndex() {
            return eventHandlerIndex++;
        }

        public void SetElementTemplate(TemplateNode templateNode, int elementSlotId, LambdaExpression expression) {
            elementFns.EnsureCapacity(elementSlotId + 1);

            if (elementFns.size <= elementSlotId) {
                elementFns.size = elementSlotId + 1;
            }

            elementFns.array[elementSlotId] = new TemplateOutput() {
                expression = expression,
                templateNode = templateNode
            };
        }

        public void SetInputHandlerFn(int index, InputHandlerResult handler) {
            inputHandlerFns.EnsureCapacity(index + 1);

            if (inputHandlerFns.size <= index) {
                inputHandlerFns.size = index + 1;
            }

            inputHandlerFns.array[index] = new InputEventHandlerOutput() {
                expression = handler.lambdaExpression,
            };
        }

        public TemplateExpressionSet Build(ProcessedType processedType) {
            return new TemplateExpressionSet() {
                bindings = bindingFns.ToArray(),
                processedType = processedType,
                entryPoint = entryPoint,
                hydratePoint = hydrate,
                elementTemplates = elementFns.ToArray(),
                slotOverrideChains = slotOverrideChains?.ToArray(),
                inputEventHandlers = inputHandlerFns.ToArray()
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
            slotOverrideChains.Clear();
            templateIndex = 0;
            dragCreatorIndex = 0;
            eventHandlerIndex = 0;
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

        public SlotOverrideChain CreateSlotOverrideChain(ProcessedType rootType, SlotNode slotNode, SizedArray<AttrInfo> attributes, SlotOverrideInfo[] extendChain) {

            if (extendChain == null) extendChain = s_EmptySlotOverrideInfo;

            SlotOverrideInfo[] list = new SlotOverrideInfo[extendChain.Length + 1];

            for (int i = 0; i < extendChain.Length; i++) {
                list[i] = extendChain[i];
            }

            list[list.Length - 1] = new SlotOverrideInfo() {
                rootType = rootType,
                slotNode = slotNode,
                attributes = attributes.ToArray()
            };

            SlotOverrideChain chain = new SlotOverrideChain(slotNode.slotName, list);
            slotOverrideChains.Add(chain);
            return chain;
        }

        public SlotOverrideChain GetSlotOverrideChain(string slotName) {
            for (int i = 0; i < slotOverrideChains.size; i++) {
                if (slotOverrideChains.array[i].slotName == slotName) {
                    return slotOverrideChains[i];
                }
            }

            return null;
        }


    }

    public struct BindingIndices {

        public int updateIndex;
        public int lateUpdateIndex;
        public int constIndex;
        public int enableIndex;

    }

}