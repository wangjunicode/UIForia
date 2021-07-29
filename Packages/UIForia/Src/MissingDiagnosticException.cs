using System;

namespace UIForia.Compilers {

    public class MissingDiagnosticException : Exception {

        public MissingDiagnosticException() { }

        public MissingDiagnosticException(string message) : base(message) { }

    }

}