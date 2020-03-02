using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UIForia.Compilers.Style;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    public class StyleSheet2 {

        public int CrunchStyles(IList<int> styleIds) {
            return 0;
        }

        public StyleSheet2 Define(string define0, string define1 = null, string define2 = null, string define3 = null) {
            return this;
        }

        public StyleSheet2 SetScreenParameters(float width, float height, int orientation, float resolution) {
            return this;
        }

        public RuntimeStyleSheet Build() {
            return default;
        }

    }

    public class RuntimeStyleSheet { }

    public class StyleSheetParser {

        private ConcurrentDictionary<string, StyleSheet2> parsedSheets;

        public StyleSheetParser() { }

        public StyleSheet2 Parse(string filePath) {
            if (parsedSheets.TryGetValue(filePath, out StyleSheet2 sheet)) {
                return sheet;
            }

            string file = File.ReadAllText(filePath);

            return default;
        }

        private List<StyleToken> Tokenize(string contents) {
            CharStream stream = new CharStream(contents.ToCharArray());
            while (stream.HasMoreTokens) {
                if (stream.TryMatchRange("style")) {
                    ParseStyle(stream);
                }
                else if (stream.TryMatchRange("export")) {
                    stream.ConsumeWhiteSpace();
                }
                else if (stream.TryMatchRange("import")) { }
                else if (stream.TryMatchRange("const")) { }

                else if (stream.TryMatchRange("animation")) { }

                else if (stream.TryMatchRange("sound")) { }

                else { }
            }

            return null;
        }

        private void ParseStyle(CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan span)) { }

            string styleName = span.ToString();

            if (stream.TryParseCharacter(':', CharStream.WhitespaceHandling.ConsumeAll)) { }
            else if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                ParseStyleBody(bodyStream);
            }
        }

        private void ParseStyleBody(CharStream stream) {
            stream.ConsumeWhiteSpace();

            while (stream.HasMoreTokens) {
                if (stream == '[') {
                    ParseStyleBlock(stream);
                }
            }
        }

        private void ParseStyleBlock(CharStream stream) {
            if (!stream.TryGetSubStream('[', ']', out CharStream blockStream)) {
                // error
            }

            stream.ConsumeWhiteSpace();

            if (!stream.TryGetSubStream('{', '}', out CharStream body)) {
                // error    
            }


            if (stream == '#') { }

            if (stream == '[') { }

            StyleCompileContext ctx = default;
            
            if (stream.TryParseIdentifier(out CharSpan span)) {
                string idName = span.ToLowerString();

                if (idName == "run") { }

                else {
                    // dont want to switch on string table. need to binary search the lower version

                    if (PropertyParsers.TryResolvePropertyId(idName)) {
                        
                    }
                    
                    stream.TryParseCharacter('=', CharStream.WhitespaceHandling.ConsumeAll);

                    // var propertyStream = GetSubstreamToTerminator(';', '\n');
                    //
                    // var parser = GetStyleParser(idName);

                    IStylePropertyParser parser = PropertyParsers.GetParser(idName);
                    
                    
                    // get mutable stream 
                    // replace variables with their values
                    // invoke parser to attempt parsing
                    // release mutable stream
                    // if (parser.TryParse(stream, ctx, out StyleProperty2 property)) {
                    //             
                    // }

                }

                // if (StylePropertyId.TryLowerNameMatch(idName, out StylePropertyDefinition definition)) {
                //     
                //     // definition.Parse(stream.GetSu);
                //      
                // }
            }
        }

    }

    public struct StyleConfiguration {

        public MediaCondition condition;
        public StyleProperty2[] properties;

    }

    public class MediaCondition { }

    public struct StyleBlock {

        public StyleConfiguration[] normal;
        public StyleConfiguration[] hover;
        public StyleConfiguration[] active;
        public StyleConfiguration[] focus;

    }

    public struct StyleToken {

        public CharStream stream;

    }

}