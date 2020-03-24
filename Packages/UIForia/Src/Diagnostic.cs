namespace UIForia {

    public struct Diagnostic {

        public string message;
        public string filePath;
        public int lineNumber;
        public int columnNumber;
        public DiagnosticType diagnosticType;

    }
    
    public enum DiagnosticType {

        ParseError,
        ParseWarning

    }


}