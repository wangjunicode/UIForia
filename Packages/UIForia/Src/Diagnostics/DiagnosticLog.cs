using System;

namespace UIForia {
    
    public struct Diagnostic {
    
        public string message;
        public string filePath;
        public Exception exception;
        public int lineNumber;
        public int columnNumber;
        public DiagnosticType diagnosticType;
        public Severity severity;

    }

    public enum Severity : ushort {

        Info,
        Error,
        Warning,

    }
    
    public enum DiagnosticType : ushort {
    
        ModuleLog,
        ModuleError,
        ModuleException,
        ParseError,
        ParseWarning,

        ModuleInfo

    }

}