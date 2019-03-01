using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing.Expression.Tokenizer {

    public struct TokenStream {

        private int ptr;

        private Stack<int> stack;
        private List<ExpressionToken> tokens;

        public TokenStream(List<ExpressionToken> tokens) {
            ptr = 0;
            this.tokens = tokens;
            stack = StackPool<int>.Get();
        }

        public int CurrentIndex => ptr;

        public ExpressionToken Current {
            [DebuggerStepThrough] get { return (ptr >= tokens.Count || tokens.Count == 0) ? ExpressionToken.Invalid : tokens[ptr]; }
        }

        public ExpressionToken Next {
            [DebuggerStepThrough] get { return (ptr + 1 >= tokens.Count) ? ExpressionToken.Invalid : tokens[ptr + 1]; }
        }

        public ExpressionToken Previous {
//            [DebuggerStepThrough]
            get { return (ptr - 1 < 0 || tokens.Count == 0) ? ExpressionToken.Invalid : tokens[ptr - 1]; }
        }

        public ExpressionToken Last {
            [DebuggerStepThrough] get { return (tokens.Count == 0) ? ExpressionToken.Invalid : tokens[tokens.Count - 1]; }
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

            return retn + $" (idx: {ptr}, {Current.value} -> {Current.expressionTokenType})";
        }

        [DebuggerStepThrough]
        public int FindNextIndex(ExpressionTokenType targetExpressionTokenType) {
            int i = 0;
            int counter = 0;
            while (HasTokenAt(i)) {
                ExpressionTokenType expressionToken = Peek(i);
                if (expressionToken == ExpressionTokenType.ParenOpen) {
                    counter++;
                }
                else if (expressionToken == ExpressionTokenType.ParenClose) {
                    counter--;
                }
                else if (expressionToken == targetExpressionTokenType && counter == 0) {
                    return i;
                }

                i++;
            }

            return -1;
        }
        
        [DebuggerStepThrough]
        public int FindNextIndexAtSameLevel(ExpressionTokenType targetExpressionTokenType) {
            int i = 0;
            int level = 0;
            while (HasTokenAt(i)) {
                ExpressionTokenType expressionToken = Peek(i);
                if (expressionToken == ExpressionTokenType.ParenOpen || expressionToken == ExpressionTokenType.ArrayAccessOpen || expressionToken == ExpressionTokenType.LessThan) {
                    level++;
                }
                else if (expressionToken == ExpressionTokenType.ParenClose || expressionToken == ExpressionTokenType.ArrayAccessClose || expressionToken == ExpressionTokenType.GreaterThan) {
                    level--;
                }
                else if (expressionToken == targetExpressionTokenType && level == 0) {
                    return i;
                }

                i++;
            }

            return -1;
        }

        [DebuggerStepThrough]
        public int FindMatchingIndex(ExpressionTokenType braceOpen, ExpressionTokenType braceClose) {
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
            List<ExpressionToken> subStreamTokens = tokens.GetRange(ptr, advance);
            Advance(advance);
            return new TokenStream(subStreamTokens);
        }

        [DebuggerStepThrough]
        public ExpressionTokenType Peek(int i) {
            return tokens[ptr + i];
        }

        [DebuggerStepThrough]
        public bool HasTokenAt(int p0) {
            return ptr + p0 < tokens.Count;
        }

        public void Release() {
            StackPool<int>.Release(stack);
            ListPool<ExpressionToken>.Release(ref tokens);
            stack = null;
            tokens = null;
        }

        public void Rewind(int count = 1) {
            ptr -= count;
        }

    }

}