using System;
using System.Collections.Generic;
using UIForia.Expressions;
using UIForia.Parsing.Style.AstNodes;

namespace UIForia.Exceptions {
    public class CompileException : Exception {

        private string fileName = "";

        public CompileException(string message = null) : base(message) {
        }

        public CompileException(StyleASTNode node, string message = null) :
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") {
        }
        
        public CompileException(string fileName, StyleASTNode node, string message = null) :
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") {
            this.fileName = fileName;
        }

        public void SetFileName(string name) {
            this.fileName = "Error in file " + name + ": ";
        }

        public override string Message => fileName + base.Message;
    }

    public static class CompileExceptions {

        public static CompileException TooManyArgumentsException(string methodName, int argumentCount) {
            return new CompileException($"Expressions only support functions with up to 4 arguments. {methodName} is supplying {argumentCount} ");
        }

        public static CompileException MethodNotFound(Type definingType, string identifier, List<Expression> arguments = null) {
            return new CompileException($"Type {definingType} does not define a method '{identifier}' with matching signature");
        }

        public static Exception FieldOrPropertyNotFound(Type definingType, string propertyName) {
            return new CompileException($"Type {definingType} does not define a property or field with the name {propertyName}");
        }

    }
}
