using System.Collections.Generic;

namespace Src {

    public class TokenStream {

        private int ptr;

        private readonly Stack<int> stack;
        private readonly List<DslToken> tokens;

        public TokenStream(List<DslToken> tokens) {
            this.tokens = tokens;
            stack = new Stack<int>();
        }

        public DslToken Current => tokens[ptr];
        public DslToken Next => tokens[ptr + 1];
        public DslToken Previous => tokens[ptr - 1];
        public DslToken Last => tokens[tokens.Count - 1];

        public bool HasMoreTokens => ptr < tokens.Count;

        public void Advance(int count = 1) {
            ptr += count;
        }

        public void Save() {
            stack.Push(ptr);
        }

        public void Restore() {
            ptr = stack.Pop();
        }

        public void Chop() {
            tokens.RemoveAt(tokens.Count - 1);
        }

        public override string ToString() {
            string retn = string.Empty;
            for (int i = 0; i < tokens.Count; i++) {
                retn += tokens[i].value;
            }

            return retn;
        }

        public TokenStream AdvanceAndReturnSubStream(int advance) {
            List<DslToken> subStreamTokens = tokens.GetRange(ptr, advance);
            Advance(advance);
            return new TokenStream(subStreamTokens);
        }

    }

}