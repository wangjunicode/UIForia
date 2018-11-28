using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public struct TokenStream {

        private int ptr;

        private Stack<int> stack;
        private List<DslToken> tokens;

        public TokenStream(List<DslToken> tokens) {
            ptr = 0;
            this.tokens = tokens;
            stack = StackPool<int>.Get();
        }

        public int CurrentIndex => ptr;

        public DslToken Current {
            [DebuggerStepThrough] get { return (ptr >= tokens.Count || tokens.Count == 0) ? DslToken.Invalid : tokens[ptr]; }
        }

        public DslToken Next {
            [DebuggerStepThrough] get { return (ptr + 1 >= tokens.Count) ? DslToken.Invalid : tokens[ptr + 1]; }
        }

        public DslToken Previous {
//            [DebuggerStepThrough]
            get { return (ptr - 1 < 0 || tokens.Count == 0) ? DslToken.Invalid : tokens[ptr - 1]; }
        }

        public DslToken Last {
            [DebuggerStepThrough] get { return (tokens.Count == 0) ? DslToken.Invalid : tokens[tokens.Count - 1]; }
        }

        public bool HasMoreTokens {
            [DebuggerStepThrough] get { return ptr < tokens.Count; }
        }

        public bool HasPrevious {
            [DebuggerStepThrough] get { return ptr - 1 >= 0; }
        }

        [DebuggerStepThrough]
        public void Advance(int count = 1) {
            ptr = Mathf.Min(ptr + count, tokens.Count);
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

            return retn + $" (idx: {ptr}, {Current.value} -> {Current.tokenType})";
        }

        [DebuggerStepThrough]
        public int FindNextIndex(TokenType targetTokenType) {
            int i = 0;
            int counter = 0;
            while (HasTokenAt(i)) {
                TokenType token = Peek(i);
                if (token == TokenType.ParenOpen) {
                    counter++;
                }
                else if (token == TokenType.ParenClose) {
                    counter--;
                }
                else if (token == targetTokenType && counter == 0) {
                    return i;
                }

                i++;
            }

            return -1;
        }

        [DebuggerStepThrough]
        public int FindMatchingBraceIndex(TokenType braceOpen, TokenType braceClose) {
            if (Current != braceOpen) {
                return -1;
            }

            Save();

            int i = -1;
            int counter = 0;
            while (ptr != tokens.Count) {
                i++;

                if (Current == braceOpen) {
                    counter++;
                }

                if (Current == braceClose) {
                    counter--;
                    if (counter == 0) {
                        Restore();
                        return i;
                    }
                }

                Advance();
            }

            Restore();
            return -1;
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

        public void Release() {
            StackPool<int>.Release(stack);
            ListPool<DslToken>.Release(ref tokens);
            stack = null;
            tokens = null;
        }

        public void Rewind(int count = 1) {
            ptr -= count;
        }

    }

}