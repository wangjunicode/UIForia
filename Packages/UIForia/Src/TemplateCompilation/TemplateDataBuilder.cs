using System;
using System.Linq;
using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    internal struct AttrInfo {

        public string key;
        public string value;
        public int depth;
        public int line;
        public int column;
        public AttributeType type;
        public AttributeFlags flags;
        public bool isInjected;

        public AttrInfo(int depth, in AttributeDefinition attr) {
            this.depth = depth;
            this.isInjected = false;

            // normal attribute data
            this.key = attr.key;
            this.value = attr.value;
            this.line = attr.line;
            this.column = attr.column;
            this.type = attr.type;
            this.flags = attr.flags;
        }

        public AttrInfo(int depth, in AttrInfo attr, bool isInjected = false) {
            this.depth = depth;
            this.isInjected = isInjected;

            this.key = attr.key;
            this.value = attr.value;
            this.line = attr.line;
            this.column = attr.column;
            this.type = attr.type;
            this.flags = attr.flags;
        }

        public AttrInfo(int depth, in AttributeDefinition2 attr, bool isInjected = false) {
            this.depth = depth;
            this.isInjected = isInjected;

            this.key = attr.key;
            this.value = attr.value;
            this.line = attr.line;
            this.column = attr.column;
            this.type = attr.type;
            this.flags = attr.flags;
        }

        public string StrippedValue {
            get {
                if (value[0] == '{' && value[value.Length - 1] == '}') {
                    return value.Substring(1, value.Length - 2);
                }

                return value;
            }
        }

    }

    public enum BindingType {

        Update,
        LateUpdate,
        Const,
        Enable

    }

    public struct BindingOutput {

        public BindingType bindingType;
        public LambdaExpression expression;
        public string tagName;
        public LineInfo lineInfo;

    }

    public struct TemplateOutput {

        public LambdaExpression expression;
        public string tagName;
        public LineInfo lineInfo;

    }

    public struct InputEventHandlerOutput {

        public LambdaExpression expression;

    }

    internal struct SlotOverrideInfo {

        public ProcessedType rootType;
        public AttrInfo[] attributes;
        public TemplateNodeReference slotNodeRef;

    }

    internal class SlotOverrideChain {

        public string slotName;
        public SlotOverrideInfo[] chain;

        public SlotOverrideChain(string slotName, SlotOverrideInfo[] chain) {
            this.slotName = slotName;
            this.chain = chain;
        }

    }

    internal class TemplateDataBuilder {

        internal LambdaExpression entryPoint;
        internal LambdaExpression hydrate;
        internal LightList<TemplateOutput> elementFns;
        internal LightList<BindingOutput> bindingFns;
        internal LightList<InputEventHandlerOutput> inputHandlerFns;
        internal LightList<SlotOverrideChain> slotOverrideChains;
        private static readonly SlotOverrideInfo[] s_EmptySlotOverrideInfo = { };

        private int dragCreatorIndex;
        private int elementFnIndex;
        private int eventHandlerIndex;
        private int templateId;
        private ITemplateCompilationHandler compilation;
        private TemplateExpressionSet expressionSet;
        
        public TemplateDataBuilder() {
            inputHandlerFns = new LightList<InputEventHandlerOutput>();
            elementFns = new LightList<TemplateOutput>();
            bindingFns = new LightList<BindingOutput>();
            slotOverrideChains = new LightList<SlotOverrideChain>();
        }

        public void Initialize(ITemplateCompilationHandler compilation, TemplateExpressionSet expressionSet) {
            Clear();
            this.templateId = expressionSet.index;
            this.expressionSet = expressionSet;
            this.compilation = compilation;
        }

        public int GetNextDragCreateIndex() {
            return dragCreatorIndex++;
        }

        public int GetNextElementFunctionIndex() {
            return elementFnIndex++;
        }

        public int GetNextInputHandlerIndex() {
            return eventHandlerIndex++;
        }

        public void SetElementTemplate(string tagName, LineInfo lineInfo, int elementSlotId, LambdaExpression expression) {
            elementFns.EnsureCapacity(elementSlotId + 1);

            if (elementFns.size <= elementSlotId) {
                elementFns.size = elementSlotId + 1;
            }

            elementFns.array[elementSlotId] = new TemplateOutput() {
                expression = expression,
                tagName = tagName,
                lineInfo = lineInfo
            };

            compilation.OnExpressionReady(new CompiledExpression() {
                expression = expression,
                type = CompileTarget.Element,
                targetIndex = elementSlotId,
                templateId = templateId
            });

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

        public TemplateExpressionSet Build() {
            expressionSet.bindings = bindingFns.ToArray();
            expressionSet.entryPoint = entryPoint;
            expressionSet.hydratePoint = hydrate;
            expressionSet.elementTemplates = elementFns.ToArray();
            expressionSet.slotOverrideChains = slotOverrideChains.ToArray();
            expressionSet.inputEventHandlers = inputHandlerFns.ToArray();
            return expressionSet;
        }

        public void SetEntryPoint(LambdaExpression entryPoint) {
            this.entryPoint = entryPoint;
            compilation.OnExpressionReady(new CompiledExpression() {
                expression = entryPoint,
                type = CompileTarget.EntryPoint,
                targetIndex = 0, // unused
                templateId = templateId
            });
        }

        public void SetHydratePoint(LambdaExpression hydrate) {
            this.hydrate = hydrate;
            compilation.OnExpressionReady(new CompiledExpression() {
                expression = hydrate,
                type = CompileTarget.HydratePoint,
                targetIndex = 0, // unused
                templateId = templateId
            });
        }

        private void Clear() {
            entryPoint = null;
            hydrate = null;
            elementFns.Clear();
            bindingFns.Clear();
            slotOverrideChains.Clear();
            templateId = -1;
            elementFnIndex = 0;
            dragCreatorIndex = 0;
            eventHandlerIndex = 0;
        }

        public BindingIndices AddBindings(BindingResult bindingResult) {
            BindingIndices retn = default;

            retn.updateIndex = -1;
            retn.lateUpdateIndex = -1;
            retn.constIndex = -1;
            retn.enableIndex = -1;

            if (bindingResult.updateLambda != null) {
                retn.updateIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    expression = bindingResult.updateLambda,
                    bindingType = BindingType.Update
                });
            }

            if (bindingResult.lateLambda != null) {
                retn.lateUpdateIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    expression = bindingResult.lateLambda,
                    bindingType = BindingType.LateUpdate
                });
            }

            if (bindingResult.constLambda != null) {
                retn.constIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    expression = bindingResult.constLambda,
                    bindingType = BindingType.Const
                });
            }

            if (bindingResult.enableLambda != null) {
                retn.enableIndex = bindingFns.size;
                bindingFns.Add(new BindingOutput() {
                    expression = bindingResult.enableLambda,
                    bindingType = BindingType.Enable
                });
            }

            return retn;
        }

        public SlotOverrideChain CreateSlotOverrideChain(ProcessedType rootType, TemplateNodeReference slotNodeRef, SizedArray<AttrInfo> attributes, SlotOverrideInfo[] extendChain) {
            if (extendChain == null) extendChain = s_EmptySlotOverrideInfo;

            SlotOverrideInfo[] list = new SlotOverrideInfo[extendChain.Length + 1];

            for (int i = 0; i < extendChain.Length; i++) {
                list[i] = extendChain[i];
            }

            list[list.Length - 1] = new SlotOverrideInfo() {
                rootType = rootType,
                slotNodeRef = slotNodeRef,
                attributes = attributes.ToArray()
            };

            throw new NotImplementedException("Fix slots");
//            SlotOverrideChain chain = new SlotOverrideChain(slotNodeRef.fileShell.GetSlotName(slotNodeRef.templateNodeId), list);
//            slotOverrideChains.Add(chain);
//            return chain;
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

    [Flags]
    public enum InputEventClass {

        Mouse = 1,
        Keyboard = 1 << 1,
        Drag = 1 << 2,
        DragCreate = 1 << 3

    }

    public struct InputHandlerResult {

        public InputHandlerDescriptor descriptor;
        public LambdaExpression lambdaExpression;
        public InputEventClass eventClass;

    }

    public class BindingResult {

        public LambdaExpression lateLambda;
        public LambdaExpression updateLambda;
        public LambdaExpression enableLambda;
        public LambdaExpression constLambda;
        public StructList<BindingVariableDesc> localVariables;
        public StructList<InputHandlerResult> inputHandlers;

        public BindingResult() {
            this.localVariables = new StructList<BindingVariableDesc>();
            this.inputHandlers = new StructList<InputHandlerResult>();
        }

        public int localVariableCount {
            get => localVariables.size;
        }

        public bool HasValue {
            get => lateLambda != null || updateLambda != null || constLambda != null || enableLambda != null;
        }

        public void Clear() {
            this.lateLambda = null;
            this.updateLambda = null;
            this.enableLambda = null;
            this.constLambda = null;
            this.localVariables.size = 0;
            inputHandlers.size = 0;
        }

    }

}