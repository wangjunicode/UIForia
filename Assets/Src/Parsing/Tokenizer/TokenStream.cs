using System.Collections.Generic;
using System.Diagnostics;

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
        public bool HasPrevious => ptr - 1 >= 0;

        [DebuggerStepThrough]
        public void Advance(int count = 1) {
            ptr += count;
        }

        [DebuggerStepThrough]
        public void Save() {
            stack.Push(ptr);
        }

        [DebuggerStepThrough]
        public void Restore() {
            ptr = stack.Pop();
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public TokenStream AdvanceAndReturnSubStream(int advance) {
            List<DslToken> subStreamTokens = tokens.GetRange(ptr, advance);
            Advance(advance);
            return new TokenStream(subStreamTokens);
        }

        [DebuggerStepThrough]
        public TokenType Peek(int i) {
            return tokens[ptr + i];
        }

        [DebuggerStepThrough]
        public bool HasTokenAt(int p0) {
            return ptr + p0 < tokens.Count;
        }

    }

}