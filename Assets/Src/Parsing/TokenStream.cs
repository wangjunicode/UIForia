using System.Collections.Generic;

namespace Src {

    public class TokenStream {
        private int ptr;
        private int savedPtr;

        private readonly Stack<int> stack;
        private readonly List<DslToken> tokens;

        public TokenStream(List<DslToken> tokens) {
            this.tokens = tokens;
            savedPtr = 0;
            stack = new Stack<int>();
            stack.Push(0);
        }

        public DslToken Current => tokens[ptr];
        public DslToken Next => tokens[ptr     + 1];
        public DslToken Previous => tokens[ptr - 1];

        public bool HasMoreTokens => (ptr + 1) < tokens.Count;

        public bool Is(TokenType tokenType) {
            return Current.tokenType == tokenType;
        }

        public DslToken Peek(int count) {
            return tokens[ptr + count];
        }

        public void Consume(int count) {
            ptr += count;
        }

        public void Save() {
            stack.Push(ptr);
        }

        public void Restore() {
            ptr = stack.Pop();
        }
    }

}