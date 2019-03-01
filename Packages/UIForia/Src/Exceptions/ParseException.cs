using System;
using UIForia.Parsing.Style.Tokenizer;

namespace UIForia.Exceptions {
    public class ParseException : Exception {

        private string fileName = "";

        public readonly StyleToken token;
        
        public ParseException(StyleToken token, string message = null) : 
            base($"Parse error at line {token.line}, column {token.column}, token type '{token.styleTokenType}' -> {token.value}\n{message}") {
            this.token = token;
        }
        
        public ParseException(string message = null) : base(message) {
        }

        public void SetFileName(string name) {
            this.fileName = "Error in file " + name + ": ";
        }

        public override string Message => fileName + base.Message;
    }
}