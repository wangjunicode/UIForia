using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine.UI;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        private CompiledTemplateData templateData;
        private Dictionary<Type, CompiledTemplate> templateMap;
        private TemplateCache templateCache;
        
        public TemplateCompiler2(TemplateSettings settings) {
            this.templateCache = new TemplateCache(settings);
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
        }

        public void CompileTemplates(RootTemplateNode rootTemplateNode, CompiledTemplateData compiledTemplateData) {
            this.templateData = compiledTemplateData;

            ProcessedType processedType = rootTemplateNode.processedType;

        }

        public class TemplateCompilationContext {

            public ParameterExpression bindingNodeExpr;
            public ProcessedType elementType;
            public ProcessedType rootType;
            public ProcessedType internalRootType;
            private ParameterExpression scopeParam;

        }
        
        private CompiledTemplate GetCompiledTemplate(ProcessedType processedType) {
            
            if (templateMap.TryGetValue(processedType.rawType, out CompiledTemplate retn)) {
                return retn;
            }
            
            CompiledTemplate compiledTemplate = Compile(ast);
            
            templateMap[processedType.rawType] = compiledTemplate;

            return compiledTemplate;
        }
        
        private CompiledTemplate Compile(RootTemplateNode rootTemplateNode) {
            CompiledTemplate retn = templateData.CreateTemplate(rootTemplateNode.filePath);
            LightList<string> namespaces = LightList<string>.Get();
            
            if (rootTemplateNode.usings != null) {
                for (int i = 0; i < rootTemplateNode.usings.size; i++) {
                    namespaces.Add(rootTemplateNode.usings[i].namespaceName);
                }
            }

            CompilationContext ctx = new CompilationContext();
            
           
            
            LightList<string>.Release(ref namespaces);

        }

        private Expression VisitChildren(CompilationContext ctx, TemplateNode2 templateNode) {
            for (int i = 0; i < templateNode.ChildCount; i++) {
                TemplateNode2 child = templateNode[i];
                switch (child) {
                    case TextNode textNode:
                        break;
                    case ContainerNode containerNode:
                    
                        return CompileContainerNode(ctx, containerNode);
                    
                    case SlotDefinitionNode slotDefinitionNode:
                        // still need to compile default slot if there is one
                        // if there is an override, compile a usage of it
                        // this usage must be unique, 1 per usage.
                        break;
                    case SlotOverrideNode slotOverrideNode:
                        break;
                    case TerminalNode terminalNode:
                        break;
                    case ExpandedTemplateNode expandedTemplateNode:
                        return CompileExpandedNode(ctx, expandedTemplateNode);
                }
            }    
        }
        
        private Expression CompileContainerNode(CompilationContext ctx, ContainerNode containerNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = containerNode.processedType;
            
            ctx.Comment("new " + processedType.rawType);
            
            ctx.Assign(nodeExpr, CreateElement(ctx, containerNode));
            
            VisitChildren(ctx, containerNode);

            return nodeExpr;

//            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
//                Expression.Constant(processedType.id),
//                ctx.ParentExpr,
//                Expression.Constant(templateNode.children.size),
//                Expression.Constant(trueAttrCount),
//                Expression.Constant(ctx.compiledTemplate.templateId)
//            ));

//            OutputAttributes(ctx, templateNode);
//            CompileElementData(templateNode, ctx);
        }

        private Expression CompileExpandedNode(CompilationContext ctx, ExpandedTemplateNode expandedTemplateNode) {
            throw new NotImplementedException();
        }

    }

}