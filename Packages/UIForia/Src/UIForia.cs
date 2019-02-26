using System;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Parsing.Style.Tokenizer;

namespace UIForia {

    public class InvalidArgumentException : Exception {

        public InvalidArgumentException(string message = null) : base(message) {
        }
    }

    public class ParseException : Exception {

        private string fileName = "";
        
        public ParseException(StyleToken token, string message = null) : 
            base($"Parse error at line {token.line}, column {token.column}, token type '{token.styleTokenType}' -> {token.value}\n{message}") {
        }
        
        public ParseException(string message = null) : base(message) {
        }

        public void SetFileName(string name) {
            this.fileName = "Error in file " + name + ": ";
        }

        public override string Message => fileName + base.Message;
    }

    public class CompileException : Exception {

        public CompileException(string message = null) : base(message) {
        }

        public CompileException(StyleASTNode node, string message = null) : 
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") {
        }
    }

    public class GameApplication : Application {
    }
}
