using System;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UnityEngine;

namespace UIForia.Graphics {

    public class ShapeCompiler {

        public Action<ShapeContext, UIElement> Compile(string source) {

            LinqCompiler linqCompiler = new LinqCompiler();
            string[] lines = source.Split('\n');

            linqCompiler.SetSignature(
                new Parameter<ShapeContext>("ctx", ParameterFlags.NeverNull),
                new Parameter<UIElement>("element", ParameterFlags.NeverNull)
            );

            linqCompiler.SetImplicitContext(linqCompiler.GetParameter("ctx"), ParameterFlags.NeverNull);

            linqCompiler.resolveAlias = (s, compiler) => { return null; };

            linqCompiler.AddNamespace("UIForia.Rendering");

            BlockNode ast = new TypeBodyParser().ParseMethodBody(source);

            for (int i = 0; i < ast.statements.size; i++) {
                linqCompiler.Statement(ast.statements.array[i]);
            }
            
            // for (int i = 0; i < lines.Length; i++) {
            //     string s = lines[i].Trim();
            //
            //     if (s.Length != 0 && s[s.Length - 1] == ';') {
            //         s = s.Substring(0, s.Length - 1);
            //     }
            //
            //     if (!string.IsNullOrWhiteSpace(s)) {
            //         ASTNode ast = ExpressionParser.Parse(s);
            //         linqCompiler.Statement(ast);
            //     }
            //
            // }

            LambdaExpression retn = linqCompiler.BuildLambda();
            Debug.Log(retn.ToCSharpCode());
            return null;
        }

    }

}