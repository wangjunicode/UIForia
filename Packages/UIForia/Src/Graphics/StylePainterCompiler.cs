using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    public class StylePainterCompiler {

        private LinqCompiler linqCompiler;

        public StylePainterCompiler() {
            linqCompiler = new LinqCompiler();
        }

        private static string MakeVariableName(string name) {
            return "__var__" + name;
        }

        private static readonly PainterVariableDeclaration[] s_EmptyVariables = { };
        
        public LambdaExpression Compile(StylePainterDefinition painter, string source) {
            
            PainterVariableDeclaration[] variables = painter.definedVariables;
            
            variables = variables ?? s_EmptyVariables;
            
            linqCompiler.Reset();
            linqCompiler.SetSignature(
                new Parameter<StylePainterContext>("ctx", ParameterFlags.NeverNull)
            );

            ParameterExpression context = linqCompiler.GetParameter("ctx");

            linqCompiler.SetImplicitContext(context, ParameterFlags.NeverNull);

            linqCompiler.resolveAlias = (alias, compiler) => {

                if (variables != null) {

                    for (int i = 0; i < variables.Length; i++) {

                        ref PainterVariableDeclaration variable = ref variables[i];

                        if (variable.name != alias) {
                            continue;
                        }

                        return compiler.GetVariable(MakeVariableName(variable.name));
                    }

                }

                Debug.Log($"No variable declared in painter {painter.fileName}:{painter.painterName} with the name {alias}.");
                
                return null;
            };

            linqCompiler.AddNamespace("UIForia.Rendering");
            linqCompiler.AddNamespace("UnityEngine");
            linqCompiler.AddNamespace("Unity.Mathematics");

            BlockNode ast = new TypeBodyParser().ParseMethodBody(source);

            if (variables != null) {
                for (int i = 0; i < variables.Length; i++) {

                    Type type = variables[i].type;

                    ParameterExpression varExpr = linqCompiler.AddVariable(type, MakeVariableName(variables[i].name));

                    if (type.IsEnum) {

                        linqCompiler.Assign(varExpr, Expression.Convert(Expression.Call(context, s_GetIntVariable, Expression.Constant(variables[i].propertyId)), type));

                        continue;
                    }

                    if (type == typeof(float)) {
                        linqCompiler.Assign(varExpr, Expression.Call(context, s_GetFloatVariable, Expression.Constant(variables[i].propertyId)));
                    }
                    else if (type == typeof(int)) {
                        linqCompiler.Assign(varExpr, Expression.Call(context, s_GetIntVariable, Expression.Constant(variables[i].propertyId)));
                    }
                    else if (type == typeof(float2)) {
                        linqCompiler.Assign(varExpr, Expression.Call(context, s_GetVector2Variable, Expression.Constant(variables[i].propertyId)));
                    }
                    else if (type == typeof(Texture)) {
                        linqCompiler.Assign(varExpr, Expression.Call(context, s_GetTextureVariable, Expression.Constant(variables[i].propertyId)));
                    }
                    else if (type == typeof(Color32)) {
                        linqCompiler.Assign(varExpr, Expression.Call(context, s_GetColorVariable, Expression.Constant(variables[i].propertyId)));
                    }
                    else {
                        throw new CompileException("Invalid variable type: " + type + " used with variable: " + variables[i].name);
                    }

                }
            }

            for (int i = 0; i < ast.statements.size; i++) {
                linqCompiler.Statement(ast.statements.array[i]);
            }

            return linqCompiler.BuildLambda();

        }

        private static readonly MethodInfo s_GetFloatVariable = typeof(StylePainterContext).GetMethod(nameof(StylePainterContext.__GetFloatVariable));
        private static readonly MethodInfo s_GetIntVariable = typeof(StylePainterContext).GetMethod(nameof(StylePainterContext.__GetIntVariable));
        private static readonly MethodInfo s_GetColorVariable = typeof(StylePainterContext).GetMethod(nameof(StylePainterContext.__GetColorVariable));
        private static readonly MethodInfo s_GetVector2Variable = typeof(StylePainterContext).GetMethod(nameof(StylePainterContext.__GetVector2Variable));
        private static readonly MethodInfo s_GetTextureVariable = typeof(StylePainterContext).GetMethod(nameof(StylePainterContext.__GetTextureVariable));

    }

}