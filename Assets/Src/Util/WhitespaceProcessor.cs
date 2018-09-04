using System.Text;

namespace Src.Util {

    public static class WhitespaceProcessor {

        private static readonly StringBuilder builder = new StringBuilder();

        public static string ProcessWrap(string input) {
            bool collapsing = false;

            input = input.Trim();
            for (int i = 0; i < input.Length; i++) {
                char current = input[i];
                bool isWhiteSpace = IsWhitespace(current);
                if (collapsing) {
                    if (!isWhiteSpace) {
                        builder.Append(current);
                        collapsing = false;
                    }
                }
                else if (isWhiteSpace) {
                    collapsing = true;
                    builder.Append(' ');
                }
                else {
                    builder.Append(current);
                }
            }


            string retn = builder.ToString();
            builder.Clear();
            return retn;
        }

        private static bool IsWhitespace(char character) {
            return character == ' ' || character == '\n' || character == '\r' || character == '\t' || character == '\0' || character == '\v';
        }

    }

}