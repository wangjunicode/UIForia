using System;
using System.Collections.Generic;

namespace UIForia {

    public class InvalidArgumentException : System.Exception {

        public InvalidArgumentException(string message = null) : base(message) { }

    }

    public class ParseException : System.Exception {

        public ParseException(string message = null) : base(message) { }

    }

    public class CompileException : System.Exception {

        public CompileException(string message = null) : base(message) { }

    }
    
    public class GameApplication : Application { }

}