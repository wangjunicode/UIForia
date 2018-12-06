
using System;
using System.Collections.Generic;

namespace UIForia.Exceptions {

    public static class CompileExceptions {

        public static CompileException TooManyArgumentsException(string methodName, int argumentCount) {
            return new CompileException($"Expressions only support functions with up to 4 arguments. {methodName} is supplying {argumentCount} ");
        }
        
        public static CompileException MethodNotFound(Type definingType, string identifier, List<Expression> arguments) {
            return new CompileException($"Type {definingType} does not define a method {identifier} with matching signature");
        }

        public static Exception FieldOrPropertyNotFound(Type definingType, string propertyName) {
            return new CompileException($"Type {definingType} does not define a property or field with the name {propertyName}");
        }

    }

}