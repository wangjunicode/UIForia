using System.Linq.Expressions;

namespace UIForia.Compilers {

    public struct Variable {

        public ParameterExpression expression;
        public int references;

        public Variable(ParameterExpression expression) {
            this.expression = expression;
            this.references = 0;
        }

    }
    
    public struct VariableGroup {

        public Variable targetElement;
        public Variable bindingNode;
        public Variable slotUsage;

    }

}