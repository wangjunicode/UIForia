using UIForia.Util;

namespace UIForia.Compilers {

    public static class TypeParser {

        public static unsafe bool TryParseTypeName(string typeName, out TypeLookup lookup) {
            lookup = default;
            fixed (char* charptr = typeName) {
                CharStream stream = new CharStream(charptr, 0, (uint) typeName.Length);
                return TryParseTypePath(ref stream, ref lookup);
            }
        }

        public static unsafe bool TryParseTypeName(CharSpan span, out TypeLookup lookup) {
            lookup = default;
            CharStream stream = new CharStream(span.data + span.rangeStart, 0, (uint) span.Length);
            return TryParseTypePath(ref stream, ref lookup);
        }

        private static bool TryParseTypePathGenerics(ref CharStream stream, ref TypeLookup retn) {
            if (stream != '<') return false;

            if (!stream.TryGetSubStream('<', '>', out CharStream genericStream)) {
                return false;
            }

            return TryParseTypePathGenericStep(ref genericStream, ref retn);

        }

        private static bool TryParseTypePathGenericStep(ref CharStream stream, ref TypeLookup lookup) {
            TypeLookup arg = default;
            while (stream.HasMoreTokens) {
                uint start = stream.Ptr;
                if (stream.TryParseIdentifier(out CharSpan _, false)) {
                    stream.RewindTo(start);
                    if (!TryParseTypePathHead(ref stream, ref arg)) {
                        return false;
                    }

                    arg.generics = default;
                    continue;
                }

                stream.RewindTo(start);
                if (stream.TryParseCharacter(',')) {
                    lookup.generics = lookup.generics.array != null ? lookup.generics : new SizedArray<TypeLookup>(4);
                    lookup.generics.Add(arg);
                    continue;
                }

                if (stream == '<') {
                    if (TryParseTypePathGenerics(ref stream, ref arg)) {
                        continue;
                    }
                }

                return false;

            }

            lookup.generics = lookup.generics.array != null ? lookup.generics : new SizedArray<TypeLookup>(4);
            lookup.generics.Add(arg);

            return true;
        }

        private static bool TryParseTypePathHead(ref CharStream stream, ref TypeLookup lookup) {

            if (!stream.TryParseMultiDottedIdentifier(out CharSpan identifier)) {
                return false;
            }

            int idx = identifier.LastIndexOf('.');
            if (idx == -1) {
                lookup.typeName = identifier.ToString();
            }
            else {
                unsafe {
                    lookup.typeName = new CharSpan(identifier.data, idx + 1, identifier.rangeEnd).ToString();
                    lookup.namespaceName = new CharSpan(identifier.data, identifier.rangeStart, idx).ToString();
                }
            }

            return true;
        }

        private static bool TryParseTypePath(ref CharStream stream, ref TypeLookup retn) {

            if (!TryParseTypePathHead(ref stream, ref retn)) {
                return false;
            }

            stream.ConsumeWhiteSpaceAndComments();

            if (!stream.HasMoreTokens) {
                return true;
            }

            if (stream == '<' && !TryParseTypePathGenerics(ref stream, ref retn)) {
                return false;
            }

            if (stream == '[' && stream.HasMoreTokens && stream[stream.Ptr + 1] == ']') {
                retn.isArray = true;
                stream.Advance(2);
            }

            stream.ConsumeWhiteSpaceAndComments();

            return !stream.HasMoreTokens;

        }

    }

}