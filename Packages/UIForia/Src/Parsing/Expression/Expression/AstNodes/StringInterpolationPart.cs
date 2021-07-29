using System.Diagnostics;
using UnityEngine;

namespace UIForia.Parsing.Expressions.AstNodes {

    [DebuggerDisplay("{DebuggerDisplay()}")]
    public struct StringInterpolationPart {

        public bool isExpression;
        public RangeInt contentRange;
        public string rawContent;

        public StringInterpolationPart(string rawContent, RangeInt contentRange, bool isExpression) {
            this.rawContent = rawContent;
            this.contentRange = contentRange;
            this.isExpression = isExpression;
        }

        public string DebuggerDisplay() {
            string retn = "IsExpression = " + isExpression;
            retn += " `" + rawContent.Substring(contentRange.start, contentRange.length) + "`";
            return retn;
        }

        public string GetString() {
            return rawContent.Substring(contentRange.start, contentRange.length);
        }

    }

}