using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Src.Parsing {

    public class TextElementParser {

        private string remaining;

        private bool inString;
        private bool inExpression;

        private readonly List<string> output;
        private readonly StringBuilder builder;

        public TextElementParser() {
            this.output = new List<string>();
            this.builder = new StringBuilder();
        }

        public string[] Parse(string input) {
            remaining = input.Trim();
            int ptr = 0;

            while (ptr < remaining.Length) {
                ptr = HandleOpenBrace(ptr);
                ptr = HandleCloseBrace(ptr);
                
                if (ptr < remaining.Length) {
                    builder.Append(remaining[ptr]);
                }

                ptr++;
            }

            string final = builder.ToString();
            // we don't want to add lone ' characters
            // we also don't care about '' since there is no content
            // whitespace is valid with ' ' 
            if (final.Length >= 3) {
                output.Add(final);
            }

            string[] retn = output.Where((s) => s.Length > 0).ToArray();
            output.Clear();
            builder.Clear();
            remaining = null;
            return retn;
        }

        private int HandleOpenBrace(int ptr) {
            char current = remaining[ptr];

            if (current != '{') return ptr;

            if (inExpression) {
                Abort("Can't part nested expression");
            }

            inExpression = true;
            if (ptr - 1 >= 0 && remaining[ptr - 1] != '}') {
                builder.Append('\'');
            }

            output.Add(builder.ToString());
            builder.Clear();
            builder.Append('{');
            return ptr + 1;
        }

        private int HandleCloseBrace(int ptr) {
            char current = remaining[ptr];

            if (current != '}') return ptr;

            if (!inExpression) {
                Abort("Encountered '}' but not inside an expression");
            }

            inExpression = false;
            builder.Append('}');
   
            output.Add(builder.ToString());
            builder.Clear();
            
            if (ptr + 1 < remaining.Length - 1 && remaining[ptr + 1] != '{' && remaining[ptr + 1] != '\'') {
                builder.Append('\'');
            }
            
            return ptr + 1;
        }

        private void Abort(string message = "unable to parse template") {
            throw new InvalidTemplateException(remaining, message);
        }

    }

}