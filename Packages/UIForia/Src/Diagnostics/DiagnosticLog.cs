using System;

namespace UIForia {
    
    public struct DiagnosticEntry {
    
        public string message;
        public string filePath;
        public string category;
        public DateTime timestamp;
        public Exception exception;
        public int lineNumber;
        public int columnNumber;
        public DiagnosticType diagnosticType;

    }

    public enum DiagnosticType : ushort {
    
        Error,
        Exception,
        Info,
        Warning

    }

}