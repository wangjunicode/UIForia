using System;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Compilers {

    public class InputEventHolder {

        public MouseInputEvent mouseEvent;
        public DragEvent dragEvent;
        public KeyboardInputEvent keyEvent;
        public DragEvent dragCreateResult;

    }

    public class TemplateData {

        public string tagName;
        public string templateId;
        public Type type;

        public TemplateData(string tagName = null) {
            this.tagName = tagName;
        }

        public Func<TemplateSystem, UIElement> entry;

        public Action<TemplateSystem> hydrate;
        public Action<LinqBindingNode>[] bindings;
        public Action<TemplateSystem>[] elements;
        public Action<LinqBindingNode, InputEventHolder>[] inputEventHandlers;

    }

    public class TemplateExpressionSet {

        public ProcessedType processedType;
        public LambdaExpression entryPoint;
        public LambdaExpression hydratePoint;
        public BindingOutput[] bindings;
        public TemplateOutput[] elementTemplates;
        public InputEventHandlerOutput[] inputEventHandlers;
        public LightList<SlotOverrideChain> slotOverrideChains;

        private static readonly string s_ElementFnTypeName = typeof(Action<TemplateSystem>[]).GetTypeName();
        private static readonly string s_BindingFnTypeName = typeof(Action<LinqBindingNode>[]).GetTypeName();
        private static readonly string s_InputEventHandlerTypeName = typeof(Action<LinqBindingNode, InputEventHolder>[]).GetTypeName();

        private string guid;

        public string GetGUID() {
            if (guid == null) {
                guid = Guid.NewGuid().ToString().Replace("-", "_");
            }

            return guid;
        }

        public IndentedStringBuilder ToCSharpCode(IndentedStringBuilder stringBuilder) {
            stringBuilder.AppendInline("new ");
            stringBuilder.AppendInline(nameof(TemplateData));
            stringBuilder.AppendInline(" (");
            stringBuilder.AppendInline("\"");
            stringBuilder.AppendInline(processedType.tagName);
            stringBuilder.AppendInline("\"");
            stringBuilder.AppendInline(") {");
            stringBuilder.NewLine();
            stringBuilder.Indent();
            stringBuilder.Indent();

            BuildEntryPoint(stringBuilder);
            stringBuilder.NewLine();

            BuildHydratePoint(stringBuilder);

            stringBuilder.NewLine();
            stringBuilder.Indent();

            stringBuilder.Append(nameof(TemplateData.elements));
            stringBuilder.AppendInline(" = new ");
            stringBuilder.AppendInline(s_ElementFnTypeName);
            stringBuilder.AppendInline(" {\n");

            stringBuilder.Indent();
            for (int i = 0; i < elementTemplates.Length; i++) {
                stringBuilder.Append("//");
                stringBuilder.AppendInline(i.ToString());
                stringBuilder.AppendInline(" <");
                stringBuilder.AppendInline(elementTemplates[i].tagName);
                stringBuilder.AppendInline("> line ");
                stringBuilder.AppendInline(elementTemplates[i].lineInfo.ToString());
                stringBuilder.NewLine();
                stringBuilder.Append(elementTemplates[i].expression.ToTemplateBody(3));
                if (i != elementTemplates.Length - 1) {
                    stringBuilder.AppendInline(",\n");
                }
            }

            stringBuilder.Outdent();

            stringBuilder.NewLine();
            stringBuilder.Append("},");

            stringBuilder.NewLine();

            stringBuilder.Append(nameof(TemplateData.bindings));
            stringBuilder.AppendInline(" = new ");
            stringBuilder.AppendInline(s_BindingFnTypeName);
            stringBuilder.AppendInline(" {\n");
            stringBuilder.Indent();

            for (int i = 0; i < bindings.Length; i++) {
                stringBuilder.Append("// ");
                stringBuilder.AppendInline(i.ToString());
                stringBuilder.AppendInline(" ");
                stringBuilder.AppendInline(GetBindingType(bindings[i].bindingType));
                stringBuilder.AppendInline(" <");
                stringBuilder.AppendInline(bindings[i].templateNode.tagName);
                stringBuilder.AppendInline("> line ");
                stringBuilder.AppendInline(bindings[i].templateNode.lineInfo.ToString());
                stringBuilder.NewLine();
                stringBuilder.Append(bindings[i].expression.ToTemplateBody(3));
                if (i != bindings.Length - 1) {
                    stringBuilder.AppendInline(",\n");
                }
            }

            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("},");
            stringBuilder.NewLine();
            stringBuilder.Append(nameof(TemplateData.inputEventHandlers));
            stringBuilder.AppendInline(" = new ");
            stringBuilder.AppendInline(s_InputEventHandlerTypeName);
            stringBuilder.AppendInline(" {\n");
            stringBuilder.Indent();

            for (int i = 0; i < inputEventHandlers.Length; i++) {
                stringBuilder.Append("// ");
                stringBuilder.AppendInline(i.ToString());
                // stringBuilder.AppendInline(" ");
                // stringBuilder.AppendInline(GetBindingType(inputEventHandlers[i].bindingType));
                // stringBuilder.AppendInline(" <");
                // stringBuilder.AppendInline(inputEventHandlers[i].templateNode.GetTagName());
                // stringBuilder.AppendInline("> line ");
                // stringBuilder.AppendInline(inputEventHandlers[i].templateNode.lineInfo.ToString());
                stringBuilder.NewLine();
                stringBuilder.Append(inputEventHandlers[i].expression.ToTemplateBody(3));
                if (i != inputEventHandlers.Length - 1) {
                    stringBuilder.AppendInline(",\n");
                }
            }

            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("}");

            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("};");
            return stringBuilder;
        }

        private string GetBindingType(BindingType bindingType) {
            switch (bindingType) {
                case BindingType.Update:
                    return "Update Binding";

                case BindingType.LateUpdate:
                    return "Late Update Binding";

                case BindingType.Const:
                    return "Const Binding";

                case BindingType.Enable:
                    return "Enable Binding";

                default:
                    throw new ArgumentOutOfRangeException(nameof(bindingType), bindingType, null);
            }
        }

        private void BuildEntryPoint(IndentedStringBuilder stringBuilder) {
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.entry));
            stringBuilder.AppendInline(" = ");
            stringBuilder.AppendInline(entryPoint.ToTemplateBody(2));
            stringBuilder.AppendInline(",");
            stringBuilder.Outdent();
        }

        private void BuildHydratePoint(IndentedStringBuilder stringBuilder) {
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.hydrate));
            stringBuilder.AppendInline(" = ");
            stringBuilder.AppendInline(hydratePoint.ToTemplateBody(2));
            stringBuilder.AppendInline(",");
            stringBuilder.Outdent();
        }

        public SlotOverrideInfo[] GetSlotOverrideChain(string slotName) {
            if (slotOverrideChains == null) return null;

            for (int i = 0; i < slotOverrideChains.size; i++) {
                if (slotOverrideChains.array[i].slotName == slotName) {
                    return slotOverrideChains[i].chain;
                }
            }

            return null;
        }

    }

    public readonly struct AttrInfo {

        public readonly string key;
        public readonly string value;
        public readonly int depth;
        public readonly int line;
        public readonly int column;
        public readonly AttributeType type;
        public readonly AttributeFlags flags;
        public readonly bool isInjected;

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
        public TemplateNode templateNode;

    }

    public struct TemplateOutput {

        public TemplateNode templateNode;
        public LambdaExpression expression;
        public string tagName;
        public LineInfo lineInfo;

    }

    public struct InputEventHandlerOutput {

        public LambdaExpression expression;

    }

    public struct SlotOverrideInfo {

        public ProcessedType rootType;
        public AttrInfo[] attributes;
        public TemplateNodeReference slotNodeRef;

    }

    public class SlotOverrideChain {

        public string slotName;
        public SlotOverrideInfo[] chain;

        public SlotOverrideChain(string slotName, SlotOverrideInfo[] chain) {
            this.slotName = slotName;
            this.chain = chain;
        }

    }

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