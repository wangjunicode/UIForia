using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using SlotType = UIForia.Parsing.SlotType;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        public readonly CompilationContext2 context;
        public readonly TemplateDataBuilder templateDataBuilder;

        private ProcessedType rootProcessedType;
        private TemplateRootNode templateRootNode;
        private readonly AttributeCompiler attributeCompiler;
        private SizedArray<DeferredCompilationData> deferredData;
        private SizedArray<AttrInfo> scratchAttributes;
        private SizedArray<TemplateContextReference> scratchContextReferences;

        private static readonly Dictionary<Type, MethodInfo> s_SyncMethodCache = new Dictionary<Type, MethodInfo>();

        public TemplateCompiler2() {
            this.deferredData = new SizedArray<DeferredCompilationData>(8);
            this.context = new CompilationContext2();
            this.templateDataBuilder = new TemplateDataBuilder();
            this.attributeCompiler = new AttributeCompiler();
            this.scratchAttributes = new SizedArray<AttrInfo>(16);
            this.scratchContextReferences = new SizedArray<TemplateContextReference>(8);
        }

        public TemplateExpressionSet CompileTemplate(ProcessedType processedType) {
            rootProcessedType = processedType;

            templateDataBuilder.Clear();
            deferredData.Clear();

            CompileEntryPoint(processedType);

            CompileHydratePoint(processedType.templateRootNode);

            // // size will expand as we compile the elements
            for (int i = 0; i < deferredData.size; i++) {
                CompileNode(deferredData.array[i]);
            }

            return templateDataBuilder.Build(processedType);

        }

        private void CompileNode(in DeferredCompilationData data) {
            switch (data.target) {
                case ContainerNode containerNode:
                    CompileContainerElement(containerNode, data.index);
                    break;

                case TextNode textNode:
                    CompileTextElement(textNode, data.index);
                    break;

                case ExpandedNode expandedNode:
                    CompileExpandedNode(expandedNode, data.index);
                    break;

                case SlotNode slotNode:
                    CompileSlotNode(slotNode, data.index);
                    break;

                case RepeatNode repeatNode:
                default:
                    return;
            }
        }

        private void CompileSlotNode(SlotNode slotNode, int index) {
            switch (slotNode.slotType) {

                case SlotType.Define:
                    CompileSlotDefine(slotNode, index);
                    break;

                case SlotType.Forward:
                    CompileSlotForward(slotNode, index);
                    break;

                case SlotType.Override:
                    CompileSlotOverride(slotNode, index);
                    break;

                case SlotType.Template:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CompileSlotDefine(SlotNode slotNode, int slotId) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = slotNode.CountRealAttributes();
            int childCount = slotNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
            InitializeElementAttributes(systemParam, scratchAttributes);

            SetupChildren(systemParam, slotNode.children);

            templateDataBuilder.SetElementTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));

        }

        private void CompileSlotOverride(SlotNode slotNode, int slotId) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = slotNode.CountRealAttributes();
            int childCount = slotNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
            AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(systemParam, scratchAttributes);
            SetupChildren(systemParam, slotNode.children);

            templateDataBuilder.SetElementTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));
        }

        private void CompileSlotForward(SlotNode slotNode, int slotId) { }

        private void CompileEntryPoint(ProcessedType processedType) {

            context.Setup<UIElement>();

            templateRootNode = processedType.templateRootNode;

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");
            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");

            context.Assign(elementParam, ExpressionFactory.New(processedType.GetConstructor()));

            int attrCount = templateRootNode.CountRealAttributes();
            int childCount = templateRootNode.ChildCount;

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeEntryPoint, elementParam, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
            AttributeMerger.ConvertAttributeDefinitions(templateRootNode.attributes, ref scratchAttributes);
            InitializeElementAttributes(systemParam, scratchAttributes);

            scratchContextReferences.Clear();
            scratchContextReferences.Add(new TemplateContextReference(processedType, templateRootNode));

            CompileBindings(AttributeSetType.EntryPoint, systemParam, processedType, templateRootNode, scratchContextReferences);
            
            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_HydrateEntryPoint));

            context.AddStatement(elementParam);

            templateDataBuilder.SetEntryPoint(context.Build(processedType.GetType().GetTypeName()));

        }

        private void CompileHydratePoint(TemplateRootNode templateRootNode) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            SetupChildren(systemParam, templateRootNode.children);

            templateDataBuilder.SetHydratePoint(context.Build(templateRootNode.GetType().GetTypeName()));

        }

        private ProcessedType ResolveProcessedType(TemplateNode templateNode) {
            if (!templateNode.processedType.IsUnresolvedGeneric) {
                return templateNode.processedType;
            }

            throw new NotImplementedException("Resolve generic type");

        }

        private static int CountRealAttributes(ReadOnlySizedArray<AttrInfo> attributes) {
            int count = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        private void CompileExpandedNode(ExpandedNode expandedNode, int templateId) {
            context.Setup();

            ProcessedType processedType = ResolveProcessedType(expandedNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            TemplateRootNode toExpand = processedType.templateRootNode;

            AttributeMerger.MergeExpandedAttributes2(expandedNode.attributes, toExpand.attributes, ref scratchAttributes);

            int attrCount = CountRealAttributes(scratchAttributes);

            int childCount = toExpand.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            // setup slot overrides

            for (int i = 0; i < expandedNode.slotOverrideNodes.size; i++) {

                SlotNode overrider = expandedNode.slotOverrideNodes.array[i];

                if (!toExpand.TryGetSlotNode(overrider.slotName, out SlotNode _)) {
                    // todo -- diagnostic
                    Debug.LogError("Cannot find to slot to override with name : " + overrider.slotName);
                    continue;
                }

                deferredData.Add(new DeferredCompilationData(overrider, templateDataBuilder.GetNextTemplateIndex()));

                if (overrider.slotType == SlotType.Override) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_OverrideSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(i)));
                }
                else if (overrider.slotType == SlotType.Forward) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_ForwardSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(i)));
                }
                else {
                    // todo -- diagnostics
                    Debug.Log("Invalid slot");
                }

            }

            InitializeElementAttributes(systemParam, scratchAttributes);

            // for each attribute I need to know where it came from, both the type and possibly index in context array
            // for container and text elements depth is always 1 and points to the root context
            // for expanded elements depth is always 2 and points to root and 'self'
            // for slot elements depth is n (at least 2) and points to the last n contexts were n == forward levels + 2

            // now I need to know the context stack at compile time...which we dont have atm
            // need to traverse down the slot hierarchy until we the <define> for the slot case

            scratchContextReferences.Clear();
            scratchContextReferences.Add(new TemplateContextReference(rootProcessedType, templateRootNode));
            scratchContextReferences.Add(new TemplateContextReference(expandedNode.processedType, expandedNode));

            CompileBindings(AttributeSetType.Expanded, systemParam, processedType, expandedNode, scratchContextReferences);
         
            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_HydrateElement, Expression.Constant(processedType.rawType)));

            templateDataBuilder.SetElementTemplate(expandedNode, templateId, context.Build(expandedNode.GetTagName()));

        }

        private void CompileBindings(AttributeSetType attributeSetType, Expression systemParam, ProcessedType processedType, TemplateNode node, ReadOnlySizedArray<TemplateContextReference> contextReferences) {
            AttributeSet attributeSet = new AttributeSet(scratchAttributes, attributeSetType, contextReferences);

            BindingResult bindingResult = new BindingResult();

            bindingResult.syncData = new SizedArray<SyncPropertyData>(8); // todo -- cache + clear
           
            attributeCompiler.CompileAttributes(processedType, node, attributeSet, ref bindingResult);

            if (!bindingResult.HasValue) {
                return;
            }

            for (int i = 0; i < bindingResult.syncData.size; i++) {
                MethodInfo info = GetSyncVarMethod(bindingResult.syncData.array[i].type);
                context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, info, ExpressionUtil.GetIntConstant(i), ExpressionUtil.GetStringConstant(""))); // todo -- add ebug name
            }

            BindingIndices bindingIds = templateDataBuilder.AddBindings(bindingResult);

            // todo -- only if has bindings
            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_SetBindings,
                    ExpressionUtil.GetIntConstant(bindingIds.updateIndex),
                    ExpressionUtil.GetIntConstant(bindingIds.lateUpdateIndex)
                    // ExpressionUtil.GetIntConstant(0)
                )
            );

        }
        
        private static MethodInfo GetSyncVarMethod(Type type) {
            if (s_SyncMethodCache.TryGetValue(type, out MethodInfo generic)) {
                return generic;
            }

            generic = MemberData.ElementSystem_AddSyncVariable.MakeGenericMethod(type);

            s_SyncMethodCache.Add(type, generic);
            return generic;
        }

        private void CompileContainerElement(ContainerNode containerNode, int templateId) {
            context.Setup();

            ProcessedType processedType = ResolveProcessedType(containerNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = containerNode.CountRealAttributes();
            int childCount = containerNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(containerNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(systemParam, scratchAttributes);

            SetupChildren(systemParam, containerNode.children);

            templateDataBuilder.SetElementTemplate(containerNode, templateId, context.Build(containerNode.GetTagName()));

        }

        private void CompileTextElement(TextNode textNode, int templateId) {
            context.Setup();

            // ProcessedType processedType = ResolveProcessedType(textNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = textNode.CountRealAttributes();
            int childCount = textNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(textNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(systemParam, scratchAttributes);

            if (textNode.IsTextConstant()) {
                context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_SetText, Expression.Constant(textNode.GetStringContent())));
            }

            SetupChildren(systemParam, textNode.children);

            templateDataBuilder.SetElementTemplate(textNode, templateId, context.Build(textNode.GetTagName()));

        }

        private void SetupChildren(Expression systemParam, ReadOnlySizedArray<TemplateNode> children) {

            for (int i = 0; i < children.size; i++) {

                TemplateNode child = children.array[i];

                Expression childNew = ExpressionFactory.New(child.processedType.GetConstructor());

                switch (child) {
                    case SlotNode slotNode: {
                        Assert.IsTrue(slotNode.slotType == SlotType.Define);
                        DeferredCompilationData data = deferredData.Add(new DeferredCompilationData(child, templateDataBuilder.GetNextTemplateIndex()));
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(slotNode.slotName), ExpressionUtil.GetIntConstant(data.index)));
                        break;
                    }

                    default: {
                        DeferredCompilationData data = deferredData.Add(new DeferredCompilationData(child, templateDataBuilder.GetNextTemplateIndex()));
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(data.index)));
                        break;
                    }

                }
            }

        }

        private void InitializeElementAttributes(Expression system, ReadOnlySizedArray<AttrInfo> attributes) {

            for (int i = 0; i < attributes.size; i++) {
                ref AttrInfo attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                context.AddStatement((attr.flags & AttributeFlags.Const) != 0
                    ? ExpressionFactory.CallInstanceUnchecked(system, MemberData.ElementSystem_InitializeStaticAttribute, ExpressionUtil.GetStringConstant(attr.key), ExpressionUtil.GetStringConstant(attr.value))
                    : ExpressionFactory.CallInstanceUnchecked(system, MemberData.ElementSystem_InitializeDynamicAttribute, ExpressionUtil.GetStringConstant(attr.key)));
            }

        }

        // context vars can be determined from parse results within the template because the are only exposed via expose: which is only valid on slots
        // injected ones are only available for slot overrides and we should know which slot we override at parse time

        public struct DeferredCompilationData {

            public readonly int index;
            public readonly TemplateNode target;
            public readonly SizedArray<ContextVariableDefinition> contextStack;

            public DeferredCompilationData(TemplateNode target, int index, SizedArray<ContextVariableDefinition> contextStack = default) {
                this.target = target;
                this.index = index;
                this.contextStack = contextStack;
            }

        }

    }

}