using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UIForia.Compilers;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace UIForia.Editor {

    public struct StylePropertyType {

        public string name;
        public Type type;
        public StyleFlags flags;

        public StylePropertyType(string name, Type type, StyleFlags flags = 0) {
            this.name = name;
            this.type = type;
            this.flags = flags;
            if (type.IsClass) {
                flags |= StyleFlags.RequireGCHandleFree;
            }
        }

    }

    [Flags]
    public enum StyleFlags {

        Inherited = 1 << 0,
        Animated = 1 << 1,
        RequireGCHandleFree = 1 << 2

    }

    public class StyleParseContext {

        public struct StyleParseEntry {

            public readonly string name;
            public readonly string value;

            public StyleParseEntry(string name, string value) {
                this.name = name;
                this.value = value;
            }

        }

        private List<StyleParseEntry> variables;

        public void AddVariable(string variable, string value) {
            variables = variables ?? new List<StyleParseEntry>();
            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].name == variable) {
                    throw new ParseException("Duplicate variable");
                }
            }

            variables.Add(new StyleParseEntry(variable, value));
        }

        public string Resolve(CharSpan span) {
            for (int i = 0; i < variables.Count; i++) {
                if (StringUtil.EqualsRangeUnsafe(variables[i].name, span)) {
                    return variables[i].value;
                }
            }

            return null;
        }

    }

   

    public class StylePropertyGroupType {

        public readonly string groupName;

        public StylePropertyGroupType(string groupName) {
            this.groupName = groupName;
        }

    }

    public class EnumParser {

        // public bool TryParse(out StyleProperty2 styleProperty) {
        //     int refId = context.GetObjectId("objectKey");
        //     PropertyData data = new PropertyData(refId);
        //     styleProperty = new StyleProperty2(new PropertyId(), data);
        //     return true;
        // }

    }
    // <ScrollView> {
    //     LayoutType = "ScrollView" !Important;
    // }

    public class FloatParser : IStylePropertyParser {

        public bool TryParse(CharStream stream, out PropertyData propertyData) {
            if (stream.TryParseFloat(out float value)) {
                propertyData = new PropertyData(value);
                return true;
            }

            propertyData = default;
            return false;
        }

    }

    public class IntParser : IStylePropertyParser {

        public bool TryParse(CharStream stream, out PropertyData propertyData) {
            if (stream.TryParseInt(out int value)) {
                propertyData = new PropertyData(value);
                return true;
            }

            propertyData = default;
            return false;
        }

    }

    // [CustomLayoutType("LayoutTypeName")]
    public class StyleCodeGen {

        protected readonly Dictionary<Type, Type> parserMap;
        protected readonly Dictionary<Type, string> styleUnpackers;
        protected readonly List<StylePropertyType> stylePropertyTypes;
        
        public StyleCodeGen() {
            
            this.stylePropertyTypes = new List<StylePropertyType>();
            this.styleUnpackers = new Dictionary<Type, string>();
            this.parserMap = new Dictionary<Type, Type>();

            this.styleUnpackers.Add(typeof(float), "float0");
            this.styleUnpackers.Add(typeof(int), "int0");

            this.stylePropertyTypes.Add(new StylePropertyType("Opacity", typeof(float), StyleFlags.Inherited | StyleFlags.Animated));
            
            this.SetStyleTypeParser<FloatParser>(typeof(float));
            this.SetStyleTypeParser<IntParser>(typeof(int));
            
        }

        public void SetStyleUnpacker<T>(string code) {
            if (styleUnpackers.TryGetValue(typeof(T), out string current)) {
                throw new Exception("Duplicate style unpacker for type " + typeof(T));
            }
            else {
                styleUnpackers[typeof(T)] = code;
            }
        }

        private void GeneratePropertyParsersCode(StringBuilder builder) {
            builder.Clear();
            builder.Append("public static partial class PropertyParsers {\n\n");

            builder.Append("static PropertyParsers() {\n");
            builder.Append("s_parseEntries = new PropertyParseEntry[");
            builder.Append(stylePropertyTypes.Count);
            builder.Append("];\n\n");
            
            for(int i = 0; i < stylePropertyTypes.Count; i++) {
                builder.Append("s_parseEntries = new PropertyParseEntry(");
                builder.Append(stylePropertyTypes[i].name);
                builder.Append(", ");
                builder.Append("new PropertyId(");
                builder.Append(i);
                builder.Append(", (PropertyId)");
                builder.Append((int)stylePropertyTypes[i].flags);
                builder.Append("), s_ParserTable[");
                builder.Append(GetParserIndexForType(stylePropertyTypes[i].type));
                builder.Append("]);\n");
            }
            
            string content = builder.ToString();
        }

        private int GetParserIndexForType(Type type) {
            throw new NotImplementedException();
        }

        [MenuItem("UIForia Dev/Regenerate Styles 2")]
        public static void Generate() {

            StyleCodeGen styleGenerator = new StyleCodeGen();
            
            styleGenerator.GenerateStyleCode();

            
            // generate an enum 

            // generate static StyleProperty function for setting values

            // generate list of string names somewhere

            // maybe generate map name to type

            // new StylePropertyData(object)

            // new StylePropertyData(struct) 

            // generator uses extension methods to create StyleProperties

        }

        private void GenerateParsers() {
            
            StringBuilder builder = new StringBuilder(4096);

            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;
                string name = stylePropertyTypes[i].name;
                StyleFlags flags = stylePropertyTypes[i].flags;

                if (!parserMap.ContainsKey(type)) {
                            
                }
                
                
            }
            
            
        }
        
        private void GenerateStyleCode() {
            
            StringBuilder builder = new StringBuilder(4096);

            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;
                string name = stylePropertyTypes[i].name;
                StyleFlags flags = stylePropertyTypes[i].flags;

                if (!parserMap.ContainsKey(type)) {
                    
                }
                
                
                
            }

            builder.Append("    public static class StylePropertyId2 {\n\n");
            
            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                builder.Append("        public const int ");
                builder.Append(stylePropertyTypes[i].name);
                builder.Append(" = ");
                builder.Append("new PropertyId(");
                builder.Append(i + 1);
                builder.Append(", ");
                builder.Append((int) stylePropertyTypes[i].flags);
                builder.Append(");\n");
            }

            builder.Append("\n    }\n");

            string content = builder.ToString();

            Debug.Log(content);
            builder.Clear();

            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                string name = stylePropertyTypes[i].name;

                builder.Append("public static StyleProperty2 ");
                builder.Append(stylePropertyTypes[i].name);
                builder.Append("(");
                builder.Append(TypeNameGenerator.GetTypeName(stylePropertyTypes[i].type));

                if (!stylePropertyTypes[i].type.IsClass) {
                    builder.Append("?");
                }

                builder.Append(" ");
                builder.Append(char.ToLowerInvariant(name[0]));
                builder.Append(name, 1, name.Length - 1);
                builder.Append(") {\n");
                builder.Append("return new StyleProperty(");
                builder.Append("id, ");
                builder.Append("StylePropertyConverter.ConverterType(value)");
                builder.Append("}");
            }
        }

        public void AddStyleProperty(StylePropertyType propertyType) {
            
        }

        private void SetStyleTypeParser<T>(Type type) where T : IStylePropertyParser, new() {
            if (parserMap.TryGetValue(typeof(T), out Type parserType)) {
                if (parserType != type) {
                    throw new Exception("Duplicate style type parser for type " + type.Name);
                }
            }

            parserMap[typeof(T)] = type;
        }

    }

}