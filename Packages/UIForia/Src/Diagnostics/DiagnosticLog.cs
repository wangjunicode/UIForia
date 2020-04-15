namespace UIForia {

    public class DiagnosticLog {

        public virtual void ReportCompileError() { }

        public virtual void ReportTemplateParseError() { }

        public virtual void ReportExpressionParseError() { }

        public virtual void ReportStyleParseError() { }

        public virtual void ReportRuntimeError() { }
        
    }

    public struct Diagnostic {
    
        public string message;
        public string filePath;
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
    
        ParseError,
        ParseWarning
    
    }

}