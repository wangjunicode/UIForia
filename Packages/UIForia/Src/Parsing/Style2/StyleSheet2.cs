using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UIForia.Compilers.Style;
using UIForia.Exceptions;
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

        public StyleSheetParser() {
            parsedSheets = new ConcurrentDictionary<string, StyleSheet2>();
        }

        public StyleSheet2 Parse(string filePath) {
            if (parsedSheets.TryGetValue(filePath, out StyleSheet2 sheet)) {
                return sheet;
            }

            string file = File.ReadAllText(filePath);

            ParseStyleSheet(file);

            return default;
        }

        public StyleSheet2 ParseString(string contents) {
            ParseStyleSheet(contents);
            return default;
        }

        private void ParseStyleSheet(string contents) {
            CharStream stream = new CharStream(contents.ToCharArray());

            while (stream.HasMoreTokens) {
                stream.ConsumeWhiteSpace();

                if (stream.TryMatchRange("style")) {
                    ParseStyle(stream);
                }
                else if (stream.TryMatchRange("export")) {
                    throw new NotImplementedException();
                }
                else if (stream.TryMatchRange("import")) {
                    throw new NotImplementedException();
                }
                else if (stream.TryMatchRange("const")) {
                    throw new NotImplementedException();
                }
                else if (stream.TryMatchRange("animation")) {
                    throw new NotImplementedException();
                }
                else if (stream.TryMatchRange("sound")) {
                    throw new NotImplementedException();
                }
                else {
                    throw new ParseException("Unexpected end of style sheet");
                }
            }
        }

        private void ParseStyle(CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan span)) {
                throw new ParseException("Expected to find an identifier after 'style' token on line " + stream.GetLineNumber());
            }

            string styleName = span.ToString();

            if (stream.TryParseCharacter(':')) {
                // Handle Extension here
            }
            else if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                ParseStyleBody(bodyStream);
            }
        }

        private void ParseStyleBody(CharStream stream) {
            stream.ConsumeWhiteSpace();

            while (stream.HasMoreTokens) {
                if (stream == '[') {
                    // ParseStyleBlock(stream);
                    throw new NotImplementedException();
                }
                else {
                    if (stream.TryParseIdentifier(out CharSpan span)) {
                        string idName = span.ToLowerString();

                        if (idName == "run") { }

                        else if (PropertyParsers.TryResolvePropertyId(idName, out PropertyParsers.PropertyParseEntry entry)) {
                            if (!stream.TryParseCharacter('=')) {
                                throw new ParseException("Expected an equal sign after property name " + span + " on line " + stream.GetLineNumber());
                            }

                            if (!stream.TryGetSubstreamTo(';', '\n', out CharStream propertyStream)) {
                                throw new ParseException("Expected a property value and then a semi colon after '" + span + " =' on line " + stream.GetLineNumber());
                            }

                            if (!entry.parser.TryParse(propertyStream, entry.propertyId, out StyleProperty2 property)) {
                                throw new ParseException("Failed to parse");
                            }

                            currentStyleList.Add(property);

                        }

                        if (stream.Contains('@')) { }

                        // get mutable stream 
                        // replace variables with their values
                        // invoke parser to attempt parsing
                        // release mutable stream
                        // if (parser.TryParse(stream, ctx, out StyleProperty2 property)) {
                        //             
                        // }
                    }

                    throw new NotImplementedException();
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