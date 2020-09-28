namespace UIForia.Parsing {

    public abstract class TemplateParser {

        public string FilePath { get; private set; }

        private ErrorContext errorContext;

        private static TemplateParser[] s_TemplateParsers;

        public string extension { get; internal set; }

        internal void Setup(string filePath, Diagnostics diagnostics) {
            this.diagnostics = diagnostics;
            this.FilePath = filePath;
            OnSetup();
        }

        internal void Reset() {
            OnReset();
        }

        private Diagnostics diagnostics;

        protected bool ReportParseError(string message) {
            diagnostics.LogError(message, FilePath, errorContext.lineNumber, errorContext.colNumber);
            return false;
        }

        protected void SetErrorContext(int line, int column) {
            errorContext.lineNumber = line;
            errorContext.colNumber = column;
        }

        public abstract bool TryParse(string contents, out TemplateFileShell templateShell);

        public virtual void OnSetup() { }

        public virtual void OnReset() { }

        // internal static TemplateParser GetParserForFileType(string extension) {
        //     s_TemplateParsers = s_TemplateParsers ?? TypeProcessor.CreateTemplateParsers();
        //     for (int i = 0; i < s_TemplateParsers.Length; i++) {
        //         if (s_TemplateParsers[i].extension == extension) {
        //             return s_TemplateParsers[i];
        //         }
        //     }
        //
        //     return null;
        // }

    }

}