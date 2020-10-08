using System.Linq.Expressions;

namespace UIForia.Compilers {

    internal struct CompiledExpression {

        public int templateId;
        public int targetIndex;
        public CompileTarget type;
        public LambdaExpression expression;

    }

}