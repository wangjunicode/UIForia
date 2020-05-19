using System.Collections.Generic;

namespace UIForia {

    public class Diagnostics {

        public List<Diagnostic> diagnosticList;

        private DiagnosticType type;

        public Diagnostics() {
            this.diagnosticList = new List<Diagnostic>();
        }

        public bool HasErrors { get; private set; }

        public void LogError(string message) {
            HasErrors = true;
            diagnosticList.Add(new Diagnostic() {
                message = message,
                diagnosticType = DiagnosticType.ModuleError
            });
        }

        public void LogParseError(string filePath, string message, int lineNumber = 0, int columnNumber = 0) {
            HasErrors = true;
            diagnosticList.Add(new Diagnostic() {
                message = message,
                filePath = filePath,
                lineNumber = lineNumber,
                columnNumber = columnNumber,
                diagnosticType = DiagnosticType.ParseError
            });
        }

        public void Clear() {
            HasErrors = false;
            diagnosticList.Clear();
        }

    }

}