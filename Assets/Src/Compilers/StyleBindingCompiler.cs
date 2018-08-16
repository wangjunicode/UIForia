using Rendering;
using Src.StyleBindings;
using UnityEngine;

namespace Src.Compilers {

    public class StyleBindingCompiler {

        private readonly ContextDefinition context;
        private readonly ExpressionCompiler compiler;
        private readonly ExpressionParser parser;
        
        public StyleBindingCompiler(ContextDefinition context) {
            compiler = new ExpressionCompiler(context);
            parser = new ExpressionParser();
        }

        public Binding CompileBackgroundColorBinding(StyleStateType styleState, string input) {
            // todo use special parser to recognize color formats
            ExpressionNode node = parser.Parse(input);
            Expression expression = compiler.Compile(node);
            return new BackgroundColorBinding(styleState, expression);
        }

    }

}