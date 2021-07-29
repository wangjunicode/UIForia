using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal abstract class ValueShortHandParser<T> where T : unmanaged {

        protected string shortHandName;

        protected ValueShortHandParser(string shortHandName) {
            this.shortHandName = shortHandName;
        }

        protected unsafe bool TryParseValueOrVariable(ref PropertyParseContext context, ref CharStream stream, out T length, out ushort variableId) {

            variableId = ushort.MaxValue;

            if (stream.TryMatchRange("var")) {

                if (!stream.TryGetParenStream(out CharStream parenStream)) {
                    context.diagnostics.LogError("Variable usage must follow the pattern `var($variableName, defaultValue)`", context.fileName, stream.GetLineInfo());
                    length = default;
                    return false;
                }

                if (!parenStream.TryGetCharSpanTo(',', out CharSpan variableNameSpan)) {
                    context.diagnostics.LogError("Variable usage must follow the pattern `var($variableName, defaultValue)`", context.fileName, stream.GetLineInfo());
                    length = default;
                    return false;
                }

                variableNameSpan = variableNameSpan.Trim();

                if (!variableNameSpan.StartsWith('$')) {
                    context.diagnostics.LogError("Variable names must begin with a `$`", context.fileName, stream.GetLineInfo());
                    length = default;
                    return false;
                }

                variableId = (ushort) context.variableTagger.TagString(variableNameSpan.data, new RangeInt(variableNameSpan.rangeStart, variableNameSpan.Length));
                parenStream.ConsumeWhiteSpace();

                if (!TryParseValue(ref parenStream, out length)) {
                    context.diagnostics.LogError($"Unable to parse {parenStream} as a {typeof(T).GetTypeName()}", context.fileName, parenStream.GetLineInfo());
                    return false;
                }
            }
            else if (!TryParseValue(ref stream, out length)) {
                context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(T).GetTypeName()}", context.fileName, stream.GetLineInfo());
                return false;
            }

            return true;
        }

        protected abstract bool TryParseValue(ref CharStream parenStream, out T value);

    }

}