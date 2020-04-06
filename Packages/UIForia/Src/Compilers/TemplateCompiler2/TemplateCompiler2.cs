using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Util;
using UnityEditor.Graphs;
using UnityEngine;
using SlotType = UIForia.Parsing.SlotType;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        public readonly CompilationContext2 context;
        public readonly TemplateDataBuilder templateDataBuilder;
        private SizedArray<DeferredCompilationData> deferredData;

        [ThreadStatic] private static Dictionary<int, Expression> s_IntExpression;
        [ThreadStatic] private static Dictionary<Type, Expression> s_DefaultExpressions;
        [ThreadStatic] private static Dictionary<string, Expression> s_StringExpression;

        public TemplateCompiler2() {
            this.deferredData = new SizedArray<DeferredCompilationData>(8);
            this.context = new CompilationContext2();
            this.templateDataBuilder = new TemplateDataBuilder();
        }

        public TemplateExpressionSet CompileTemplate(ProcessedType processedType) {

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

                // case SlotNode slotNode:
                //     CompileSlotNode(slotNode, data.index);
                //     break;

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

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, GetIntConstant(attrCount), GetIntConstant(childCount)));

            InitializeElementAttributes(systemParam, slotNode.attributes);
            SetupChildren(systemParam, slotNode.children);

            templateDataBuilder.SetSlotTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));

        }

        private void CompileSlotOverride(SlotNode slotNode, int slotId) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = slotNode.CountRealAttributes();
            int childCount = slotNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, GetIntConstant(attrCount), GetIntConstant(childCount)));

            InitializeElementAttributes(systemParam, slotNode.attributes);
            SetupChildren(systemParam, slotNode.children);

            templateDataBuilder.SetSlotTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));
        }

        private void CompileSlotForward(SlotNode slotNode, int slotId) { }

        private void CompileEntryPoint(ProcessedType processedType) {

            context.Setup<UIElement>();

            TemplateRootNode templateRootNode = processedType.templateRootNode;

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");
            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");
            
            context.Assign(elementParam, ExpressionFactory.New(processedType.GetConstructor()));

            int attrCount = templateRootNode.CountRealAttributes();
            int childCount = templateRootNode.ChildCount;

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeEntryPoint, elementParam, GetIntConstant(attrCount), GetIntConstant(childCount)));

            InitializeElementAttributes(systemParam, templateRootNode.attributes);

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

        private void CompileExpandedNode(ExpandedNode expandedNode, int templateId) {
            context.Setup();

            ProcessedType processedType = ResolveProcessedType(expandedNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = expandedNode.CountRealAttributes();
            int childCount = expandedNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, GetIntConstant(attrCount), GetIntConstant(childCount)));

            TemplateRootNode toExpand = processedType.templateRootNode;

            SizedArray<AttributeDefinition> attributes = AttributeMerger.MergeExpandedAttributes(toExpand.attributes, expandedNode.attributes);

            // setup slot overrides

            for (int i = 0; i < expandedNode.slotOverrideNodes.size; i++) {

                SlotNode overrider = expandedNode.slotOverrideNodes.array[i];

                if (!toExpand.TryGetSlotNode(overrider.slotName, out SlotNode _)) {
                    // todo -- diagnostic
                    Debug.LogError("Cannot find to slot to override with name : " + overrider.slotName);
                    continue;
                }

                deferredData.Add(new DeferredCompilationData(overrider, templateDataBuilder.GetSlotIndex(overrider.slotName)));

                if (overrider.slotType == SlotType.Override) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_OverrideSlot, GetStringConstant(overrider.slotName), GetIntConstant(i)));
                }
                else if (overrider.slotType == SlotType.Forward) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_ForwardSlot, GetStringConstant(overrider.slotName), GetIntConstant(i)));
                }
                else {
                    // todo -- diagnostics
                    Debug.Log("Invalid slot");
                }

            }

            InitializeElementAttributes(systemParam, attributes);

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_HydrateElement, Expression.Constant(processedType.rawType)));

            templateDataBuilder.SetElementTemplate(expandedNode, templateId, context.Build(expandedNode.GetTagName()));

        }

        private void CompileContainerElement(ContainerNode containerNode, int templateId) {
            context.Setup();

            ProcessedType processedType = ResolveProcessedType(containerNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = containerNode.CountRealAttributes();
            int childCount = containerNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, GetIntConstant(attrCount), GetIntConstant(childCount)));

            InitializeElementAttributes(systemParam, containerNode.attributes);

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

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, GetIntConstant(attrCount), GetIntConstant(childCount)));

            InitializeElementAttributes(systemParam, textNode.attributes);
            SetupChildren(systemParam, textNode.children);

            templateDataBuilder.SetElementTemplate(textNode, templateId, context.Build(textNode.GetTagName()));

        }

        private void SetupChildren(Expression systemParam, ReadOnlySizedArray<TemplateNode> children) {

            for (int i = 0; i < children.size; i++) {

                TemplateNode child = children.array[i];

                Expression childNew = ExpressionFactory.New(child.processedType.GetConstructor());

                switch (child) {
                    case SlotNode slotNode: {
                        DeferredCompilationData data = deferredData.Add(new DeferredCompilationData(child, templateDataBuilder.GetSlotIndex(slotNode.slotName)));
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddSlotChild, childNew, GetStringConstant(slotNode.slotName), GetIntConstant(data.index)));
                        break;
                    }

                    case ExpandedNode _: {
                        DeferredCompilationData data = deferredData.Add(new DeferredCompilationData(child, templateDataBuilder.GetTemplateIndex(child.processedType)));
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddHydratedChild, childNew, GetIntConstant(data.index)));
                        break;
                    }

                    default: {
                        DeferredCompilationData data = deferredData.Add(new DeferredCompilationData(child, templateDataBuilder.GetTemplateIndex(child.processedType)));
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddChild, childNew, GetIntConstant(data.index)));
                        break;
                    }

                }
            }

        }

        private void InitializeElementAttributes(Expression system, ReadOnlySizedArray<AttributeDefinition> attributes) {

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                context.AddStatement((attr.flags & AttributeFlags.Const) != 0
                    ? ExpressionFactory.CallInstanceUnchecked(system, MemberData.ElementSystem_InitializeStaticAttribute, GetStringConstant(attr.key), GetStringConstant(attr.value))
                    : ExpressionFactory.CallInstanceUnchecked(system, MemberData.ElementSystem_InitializeDynamicAttribute, GetStringConstant(attr.key)));
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

        public static Expression GetIntConstant(int value) {
            s_IntExpression = s_IntExpression ?? new Dictionary<int, Expression>();
            if (s_IntExpression.TryGetValue(value, out Expression retn)) {
                return retn;
            }

            retn = Expression.Constant(value);
            s_IntExpression[value] = retn;
            return retn;
        }

        public static Expression GetStringConstant(string value) {
            s_StringExpression = s_StringExpression ?? new Dictionary<string, Expression>();
            if (s_StringExpression.TryGetValue(value, out Expression retn)) {
                return retn;
            }

            retn = Expression.Constant(value);
            s_StringExpression[value] = retn;
            return retn;
        }

        public static Expression GetDefaultExpression(Type type) {
            s_DefaultExpressions = s_DefaultExpressions ?? new Dictionary<Type, Expression>();
            if (s_DefaultExpressions.TryGetValue(type, out Expression retn)) {
                return retn;
            }

            retn = Expression.Default(type);
            s_DefaultExpressions[type] = retn;
            return retn;
        }

    }

}