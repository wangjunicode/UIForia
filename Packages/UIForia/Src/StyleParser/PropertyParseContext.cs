using UIForia.Text;
using UIForia.Util;

namespace UIForia.Style {

    internal struct PropertyParseContext {

        public PropertyId propertyId;
        public CharStream charStream;
        public CharSpan rawValueSpan;
        public StringTagger variableTagger;
        
        internal Diagnostics diagnostics;
        internal string fileName;

        public string FileName => fileName;

        public void LogError(string message) {
            diagnostics.LogError(message);
        }

    }

}