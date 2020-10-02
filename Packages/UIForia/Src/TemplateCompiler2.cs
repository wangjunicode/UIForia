using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Compilers {

    public enum BindingVariableKind {

        Local,
        Reference

    }

    public struct BindingVariableDesc {

        public Type variableType;
        public Type originTemplateType;
        public string variableName;
        public int index;
        public BindingVariableKind kind;

    }

    internal class AttributeCompiler {

        public AttributeCompiler(AttributeCompilerContext context) { }

    }

    internal class TemplateCompiler2 {

        private State state;

        private SizedArray<AttrInfo> scratchAttributes;

        private SizedArray<AttributeDefinition2> scratch1;
        private SizedArray<AttributeDefinition2> scratch2;

        private readonly AttributeCompiler attributeCompiler;

        private readonly Dictionary<ProcessedType, TemplateExpressionSet> compiledTemplates;

        public event Action<TemplateExpressionSet> onTemplateCompiled;

        private static readonly Dictionary<Type, MethodInfo> s_CreateVariableMethodCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_ReferenceVariableMethodCache = new Dictionary<Type, MethodInfo>();

        private readonly LightList<State> statePool;
        private readonly StructStack<State> stateStack;
        private readonly AttributeCompilerContext attrCompilerContext;
        private readonly TemplateLinqCompiler typeResolver;
        private Diagnostics diagnostics;

        private Compilation compilation;

        public TemplateCompiler2(Compilation compilation) {
            this.compilation = compilation;
            this.diagnostics = compilation.diagnostics;
            this.stringBuilder = new StringBuilder();
            this.statePool = new LightList<State>();
            this.stateStack = new StructStack<State>();
            this.attrCompilerContext = new AttributeCompilerContext();
            this.attributeCompiler = new AttributeCompiler(attrCompilerContext);
            this.scratchAttributes = new SizedArray<AttrInfo>(16);
            this.compiledTemplates = new Dictionary<ProcessedType, TemplateExpressionSet>();
            this.typeResolver = new TemplateLinqCompiler(attrCompilerContext);
        }
        // private readonly AttributeCompilerContext attrCompilerContext;
        // private readonly TemplateLinqCompiler typeResolver;

        private readonly StringBuilder stringBuilder;

        private ParameterExpression system {
            get => state.systemParam;
        }

        private TemplateCompilerContext context {
            get => state.context;
        }

        private TemplateDataBuilder templateDataBuilder {
            get => state.templateDataBuilder;
        }

        private ProcessedType rootProcessedType {
            get => state.rootProcessedType;
        }

        private TemplateASTNode rootNode {
            get => state.templateFile.templateNodes[state.templateRootAST.templateIndex];
        }

        private TemplateFileShell fileShell {
            get => state.templateFile;
        }

        private struct State {

            public ParameterExpression systemParam;
            public TemplateCompilerContext context;
            public TemplateDataBuilder templateDataBuilder;

            public ProcessedType rootProcessedType;

            public LightStack<StructList<BindingVariableDesc>> variableStack;
            public TemplateASTRoot templateRootAST;
            public TemplateFileShell templateFile;

        }

        private void PushState(ProcessedType processedType) {
            if (state.context == null) {
                state = GetState(processedType);
            }
            else {
                stateStack.Push(state);
                state = GetState(processedType);
            }
        }

        private State GetState(ProcessedType processedType) {
            // compilation.moduleMap.TryGetValue(processedType, out UIModule module);
            TemplateFileShell shell = compilation.templateMap[processedType];
            TemplateASTRoot template = shell.GetRootTemplateForType(processedType);

            if (statePool.size == 0) {
                return new State() {
                    context = new TemplateCompilerContext(diagnostics),
                    templateDataBuilder = new TemplateDataBuilder(),
                    rootProcessedType = processedType,
                    templateRootAST = template,
                    templateFile = shell,
                    variableStack = new LightStack<StructList<BindingVariableDesc>>()
                };
            }

            State retn = statePool.RemoveLast();
            retn.rootProcessedType = processedType;
            retn.templateRootAST = template;
            retn.templateFile = shell;
            return retn;
        }

        private void PopState() {
            statePool.Add(state);
            state = stateStack.size == 0
                ? default
                : stateStack.Pop();
        }

        public TemplateExpressionSet CompileTemplate(ProcessedType processedType) {
            if (compiledTemplates.TryGetValue(processedType, out TemplateExpressionSet retn)) {
                return retn;
            }

            PushState(processedType);

            CompileEntryPoint(processedType);

            retn = templateDataBuilder.Build(processedType);

            compiledTemplates[processedType] = retn;

            PopState();

            onTemplateCompiled?.Invoke(retn); // todo -- replace this with an even per function that is ready to be compiled

            return retn;
        }


        private void CompileEntryPoint(ProcessedType processedType) {
            context.Setup<UIElement>();

            state.systemParam = context.AddParameter<TemplateSystem>("system");
            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");


            Expression ctorExpression = processedType.GetConstructorExpression();

            if (ctorExpression == null) {
                diagnostics.LogError(processedType.rawType + "doesn't define a parameterless public constructor. This is a requirement for it to be used templates");
                return;
            }

            context.Assign(elementParam, ctorExpression);

            int attrCount = CountRealAttributes(rootNode);

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeEntryPoint, elementParam, ExpressionUtil.GetIntConstant(attrCount)));

            // AttributeMerger.ConvertAttributeDefinitions(templateRootNode.attributes, ref scratchAttributes);

            // InitializeElementAttributes(scratchAttributes); // can go through and pre-initialize all attributes in template now that we have a flat list

            // attrCompilerContext.Init(templateRootNode, TemplateNodeType.EntryPoint, processedType, null, state.variableStack);
            // attrCompilerContext.AddContextReference(processedType, templateRootNode);

            bool needPop = false; // CompileBindings(templateRootNode);

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_HydrateEntryPoint));

            context.AddStatement(elementParam);

            templateDataBuilder.SetEntryPoint(context.Build(processedType.rawType.GetTypeName()));

            CompileHydratePoint(rootNode, processedType);

            if (needPop) {
                state.variableStack.Pop().Release();
            }
        }

        private void CompileHydratePoint(TemplateASTNode node, ProcessedType processedType) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<TemplateSystem>("system");

            StructList<ConstructedChildData> childrenSetup = StructList<ConstructedChildData>.Get();

            SetupElementChildren_New(systemParam, node, childrenSetup);

            templateDataBuilder.SetHydratePoint(context.Build(processedType.rawType.GetTypeName()));

            CompileChildren(childrenSetup, null);

            childrenSetup.Release();
        }

        private void CompileChildren(StructList<ConstructedChildData> childData, StructList<AttrInfo> injectedAttributes) {
            for (int i = 0; i < childData.size; i++) {
                ref ConstructedChildData data = ref childData.array[i];
                CompileNode(data, injectedAttributes);
            }
        }

        private string GetRequiredTypeAttribute(int nodeIdx) {
            ref TemplateASTNode node = ref fileShell.templateNodes[nodeIdx];
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (fileShell.attributeList[i].type == AttributeType.RequireType) {
                    return fileShell.GetString(fileShell.attributeList[i].value);
                }
            }

            return null;
        }

        private void SetupElementChildren_New(Expression systemParam, in TemplateASTNode parent, StructList<ConstructedChildData> output) {
            int childIndex = parent.firstChildIndex;

            TemplateASTNode[] templateNodes = fileShell.templateNodes;

            Type requiredType = null; //ResolveRequiredType(fileShell.GetRequiredType(parent.index));

            while (childIndex != -1) {
                ref TemplateASTNode child = ref templateNodes[childIndex];
                childIndex = child.nextSiblingIndex;

                ProcessedType childProcessedType = fileShell.typeMappings[child.index];

                if (requiredType != null && requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    diagnostics.LogError(($"type {childProcessedType.rawType} doesnt match required type of {requiredType}"));
                    continue;
                }

                int outputIndex = templateDataBuilder.GetNextTemplateIndex();

                output.AddUnsafe(new ConstructedChildData(childProcessedType, child.index, outputIndex));

                Expression childNew = childProcessedType.GetConstructorExpression();

                if ((child.templateNodeType & TemplateNodeType.Slot) != 0) {
                    Assert.IsTrue(child.templateNodeType == TemplateNodeType.SlotDefine);
                    string tagName = fileShell.GetString(child.tagNameRange);
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(tagName), ExpressionUtil.GetIntConstant(outputIndex)));
                }
                else if ((child.templateNodeType & TemplateNodeType.Meta) != 0) {
                    // todo -- 
                }
                else {
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(outputIndex)));
                }
            }
        }

        private void CompileNode(in ConstructedChildData constructedChildData, StructList<AttrInfo> injectedAttributes) {
            context.Setup();

            state.systemParam = context.AddParameter<TemplateSystem>("system");

            TemplateExpressionSet innerTemplate = default;
            ProcessedType processedType = constructedChildData.processedType;
            int templateNodeId = constructedChildData.templateNodeId;
            ref TemplateASTNode templateNode = ref fileShell.templateNodes[templateNodeId];

            scratchAttributes.size = 0;
            attrCompilerContext.Init(fileShell, templateNodeId, processedType, null, state.variableStack);

            int attrCount = 0; //CountRealAttributes(scratchAttributes); // todo inject / inherit needs to account for that here
            int styleCount = 0; // todo

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();


            switch (processedType.archetype) {
                case ProcessedType.ElementArchetype.Template:
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeHydratedElement, ExpressionUtil.GetIntConstant(attrCount)));
                    InitializeElementAttributes(scratchAttributes);

                    SetupSlotChildren(templateNodeId, innerTemplate, setupChildren);
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_HydrateElement, Expression.Constant(processedType.rawType)));
                    break;
                case ProcessedType.ElementArchetype.Meta:
                    break;
                case ProcessedType.ElementArchetype.SlotDefine:
                    break;
                case ProcessedType.ElementArchetype.SlotForward:
                    break;
                case ProcessedType.ElementArchetype.SlotOverride:
                    context.AddStatement(ExpressionFactory.CallInstance(
                        system,
                        MemberData.TemplateSystem_InitializeSlotElement,
                        ExpressionUtil.GetIntConstant(attrCount),
                        ExpressionUtil.GetIntConstant(attrCompilerContext.references.size)
                    ));

                    InitializeElementAttributes(scratchAttributes);

                    SetupElementChildren_New(system, templateNode, setupChildren);
                    break;
                case ProcessedType.ElementArchetype.Container: {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount)));
                    InitializeElementAttributes(scratchAttributes);
                    SetupElementChildren_New(system, templateNode, setupChildren);
                    break;
                }
                case ProcessedType.ElementArchetype.Text: {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount)));

                    if (constructedChildData.processedType.IsTextElement && fileShell.IsTextConstant(templateNodeId)) {
                        context.AddStatement(ExpressionFactory.CallInstance(
                                system,
                                MemberData.TemplateSystem_SetText,
                                Expression.Constant("text goes here")) // todo -- need to get text  //fileShell.GetString(templateNode.textContentRange)) // todo
                        );
                    }

                    InitializeElementAttributes(scratchAttributes);

                    SetupElementChildren_New(system, templateNode, setupChildren);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool alteredStateStack = CompileBindings(processedType, templateNodeId);

            string formattedTagName = GetFormattedTagName(constructedChildData.processedType, templateNodeId);

            templateDataBuilder.SetElementTemplate(
                formattedTagName,
                templateNode.GetLineInfo(),
                constructedChildData.outputTemplateIndex,
                context.Build(fileShell.GetString(templateNode.tagNameRange))
            );

            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            CompileChildren(setupChildren, injectedAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

            injectedChildAttributes.Release();
            setupChildren.Release();
        }

        private string GetStringContent(int templateNodeId) {
            fileShell.TryGetTextContent(templateNodeId, out RangeInt textExpressionRange);
            stringBuilder.Clear();

            for (int i = textExpressionRange.start; i < textExpressionRange.end; i++) {
                stringBuilder.Append(fileShell.textContents[i].stringRange);
            }

            return stringBuilder.ToString();
        }

        public string GetFormattedTagName(ProcessedType processedType, int templateNodeIndex) {
            ref TemplateASTNode node = ref fileShell.templateNodes[templateNodeIndex];

            switch (processedType.archetype) { }

            return processedType.tagName; // todo -- totally wrong
            // switch (node.templateNodeType) {
            //
            //     case TemplateNodeType.Unresolved:
            //         return "unresolved";
            //
            //     case TemplateNodeType.SlotDefine:
            //         return "define:" + fileShell.GetString(node.tagNameRange);
            //
            //     case TemplateNodeType.SlotForward:
            //         return "<forward:" + fileShell.GetString(node.tagNameRange);;
            //
            //     case TemplateNodeType.SlotOverride:
            //         return "override:" + fileShell.GetString(node.tagNameRange);;
            //
            //     case TemplateNodeType.Root:
            //         return processedType.tagName;
            //
            //     case TemplateNodeType.Expanded:
            //     case TemplateNodeType.Container:
            //     case TemplateNodeType.Text:{
            //         // todo -- might want to print out a nice generic string if type is generic
            //         string moduleName = GetModuleName(templateNodeIndex);
            //         string tagName = processedType.tagName; //fileShell.GetString(node.tagNameRange);;
            //         if (moduleName != null) {
            //             return moduleName + ":" + tagName;
            //         }
            //
            //         return tagName;
            //     }
            //
            //     case TemplateNodeType.Repeat:
            //         return "Repeat";
            // }
            //
            // return null;
        }

        private bool CompileBindings(ProcessedType processedType, int templateNodeId) {
            return false;
        }

        private Type ResolveRequiredType(string typeExpression) {
            if (string.IsNullOrEmpty(typeExpression)) {
                return null;
            }

            // todo -- list of referenced namespaces from template and implicitly from module
            Type requiredType = TypeResolver.ResolveTypeExpression(rootProcessedType.rawType, null, typeExpression);

            if (requiredType == null) {
                diagnostics.LogError($"Unable to resolve required child type `{typeExpression}`");
                return null;
            }

            if (!requiredType.IsInterface && !typeof(UIElement).IsAssignableFrom(requiredType)) {
                diagnostics.LogError($"When requiring an explicit child type, that type must either be an interface or a subclass of UIElement. {requiredType} was neither");
                return null;
            }

            return requiredType;
        }

        private int CountRealAttributes(TemplateASTNode node) {
            if (node.attributeCount == 0) return 0;
            int count = 0;

            AttributeDefinition3[] attrList = fileShell.attributeList;
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (attrList[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        private void InitializeElementAttributes(ReadOnlySizedArray<AttrInfo> attributes) {
            for (int i = 0; i < attributes.size; i++) {
                ref AttrInfo attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                // todo -- probably want to count & pre-alloc 
                context.AddStatement((attr.flags & AttributeFlags.Const) != 0
                    ? ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeStaticAttribute, ExpressionUtil.GetStringConstant(attr.key), ExpressionUtil.GetStringConstant(attr.value))
                    : ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeDynamicAttribute, ExpressionUtil.GetStringConstant(attr.key)));
            }
        }

        private void SetupSlotChildren(int templateNodeId, TemplateExpressionSet innerTemplate, StructList<ConstructedChildData> setupChildren) {
            return;
            // ExpandedNode expandedNode = default;
            // // only slot definitions can enforce required types or provide injected attributes
            // // injected attribute scope is always the define scope
            //
            // // todo -- maybe parser should handle this after all
            // if (expandedNode.children.size != 0) {
            //
            //     SlotOverrideInfo[] overrideChain = innerTemplate.GetSlotOverrideChain("Children");
            //
            //     if (overrideChain == null) {
            //         // todo -- diagnostic
            //         Debug.LogError("Cannot find a 'Children' slot to override");
            //     }
            //     else {
            //         SlotNode implicitOverride = new SlotNode("Children", default, default, default, SlotType.Override) {
            //             children = expandedNode.children,
            //             parent = expandedNode,
            //             root = expandedNode.root,
            //             // requireType = declaredChildrenSlot.requireType,
            //             // injectedAttributes = declaredChildrenSlot.injectedAttributes
            //         };
            //
            //         // templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, implicitOverride, scratchAttributes, overrideChain);
            //
            //         int idx = templateDataBuilder.GetNextTemplateIndex();
            //         // setupChildren.Add(new ConstructedChildData(implicitOverride.processedType, implicitOverride, idx));
            //         context.AddStatement(ExpressionFactory.CallInstance(
            //                 system,
            //                 MemberData.TemplateSystem_OverrideSlot,
            //                 ExpressionUtil.GetStringConstant("Children"),
            //                 ExpressionUtil.GetIntConstant(idx)
            //             )
            //         );
            //     }
            // }
            //
            // for (int i = 0; i < expandedNode.slotOverrideNodes.size; i++) {
            //
            //     SlotNode overrider = expandedNode.slotOverrideNodes.array[i];
            //
            //     SlotOverrideInfo[] overrideChain = innerTemplate.GetSlotOverrideChain(overrider.slotName);
            //
            //     if (overrideChain == null) {
            //         // todo -- diagnostic
            //         Debug.LogError("Cannot find to slot to override with name : " + overrider.slotName);
            //         continue;
            //     }
            //
            //     // overrider.requireType = toOverride.requireType; // todo -- ignoring definition of forward/override require types atm
            //
            //     AttributeMerger.ConvertAttributeDefinitions(overrider.attributes, ref scratchAttributes);
            //
            //     // templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, overrider, scratchAttributes, overrideChain);
            //
            //     int idx = templateDataBuilder.GetNextTemplateIndex();
            //
            //     setupChildren.Add(new ConstructedChildData(overrider.processedType, -9999, idx));
            //
            //     if (overrider.slotType == SlotType.Override) {
            //         context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_OverrideSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(idx)));
            //     }
            //     else if (overrider.slotType == SlotType.Forward) {
            //         context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_ForwardSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(idx)));
            //     }
            //     else {
            //         // todo -- diagnostics
            //         Debug.Log("Invalid slot");
            //     }
            //
            // }
        }

        private struct ConstructedChildData {

            public readonly int templateNodeId;
            public readonly int outputTemplateIndex;
            public readonly ProcessedType processedType;

            public ConstructedChildData(ProcessedType p, int templateNodeId, int outputTemplateIndex) {
                this.processedType = p;
                this.outputTemplateIndex = outputTemplateIndex;
                this.templateNodeId = templateNodeId;
            }

        }

    }

    public struct BindingIndices {

        public int updateIndex;
        public int lateUpdateIndex;
        public int constIndex;
        public int enableIndex;

    }

}