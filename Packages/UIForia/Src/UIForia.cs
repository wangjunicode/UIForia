using System;
using UIForia.Parsing.Style.Tokenizer;

namespace UIForia {

    public class InvalidArgumentException : Exception {

        public InvalidArgumentException(string message = null) : base(message) {
        }

    }

    public class ParseException : Exception {

        public ParseException(StyleToken token, string message = null) : base($"Parse error at line {token.line} column {token.column}: {message}") {
        }

        public ParseException(string message = null) : base(message) {
        }

    }

    public class CompileException : Exception {

        public CompileException(string message = null) : base(message) {
        }

    }

    public class GameApplication : Application {
    }

}
