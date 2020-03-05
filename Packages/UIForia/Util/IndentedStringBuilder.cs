using System.Text;

namespace UIForia.Util {

    public class IndentedStringBuilder {

        private int indentLevel;
        private readonly StringBuilder builder;

        public IndentedStringBuilder(uint capacity) {
            this.builder = new StringBuilder((int)capacity);
        }

        public void Indent() {
            indentLevel++;
        }

        public void Outdent() {
            indentLevel--;
            if (indentLevel < 0) indentLevel = 0;
        }

        public void Append(string str) {
            if (indentLevel != 0) {
                for (int i = 0; i < indentLevel * 4; i++) {
                    builder.Append(' ');
                }
            }

            builder.Append(str);
        }

        public void Clear() {
            builder.Clear();
            indentLevel = 0;
        }
        
        public override string ToString() {
            return builder.ToString();
        }

        public void AppendInline(string str) {
            builder.Append(str);
        }
        
        public void NewLine() {
            builder.Append('\n');
        }

    }

}