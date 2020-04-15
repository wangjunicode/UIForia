using System;
using System.Linq.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

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