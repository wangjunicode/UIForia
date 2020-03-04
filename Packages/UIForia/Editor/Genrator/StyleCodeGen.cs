using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UIForia.Compilers;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.UIInput;
using UIForia.Util;
using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

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

    public struct TypeHandler {

        public Type parserType;
        public string packer;
        public string unpacker;

    }

    public class StyleCodeGen {

        [MenuItem("UIForia Dev/Regenerate Styles 2")]
        public static void Generate() {
            StyleCodeGen styleGenerator = new StyleCodeGen();

            string output = styleGenerator.GenerateStyleCode();

            string generatedPath = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Src/_GeneratedStyle.cs"));

            File.WriteAllText(generatedPath, output);
        }


        protected readonly List<PropertyId> propertyIds;
        protected readonly List<StylePropertyType> stylePropertyTypes;
        protected readonly Dictionary<Type, ParserTableEntry> parserMap;
        protected readonly Dictionary<Type, TypeHandler> typeHandlerMap;
        protected readonly List<string> namespaces;

        private readonly bool builtInPhase;

        private const string s_Indent8 = "        ";
        private const string s_Indent12 = "            ";
        private const string s_Indent16 = "                ";
        private const string s_Indent20 = "                    ";


        public StyleCodeGen() {
            this.propertyIds = new List<PropertyId>(128);
            this.namespaces = new List<string>();
            this.typeHandlerMap = new Dictionary<Type, TypeHandler>();
            this.stylePropertyTypes = new List<StylePropertyType>();
            this.parserMap = new Dictionary<Type, ParserTableEntry>();

            this.builtInPhase = true;

            AddNamespace("UIForia.Util");
            AddNamespace("System");
            AddNamespace("System.Runtime.InteropServices");

            SetTypeHandler<float>(new TypeHandler() {
                unpacker = nameof(StyleProperty2.float0),
                parserType = typeof(FloatParser)
            });

            SetTypeHandler<int>(new TypeHandler() {
                unpacker = nameof(StyleProperty2.int0),
                parserType = typeof(IntParser)
            });

            SetTypeHandler<string>(new TypeHandler() {
                parserType = typeof(StringParser)
            });

            SetTypeHandler<Texture2D>(new TypeHandler() {
                parserType = typeof(StringParser) // todo -- make URL parser
            });

            SetTypeHandler<Color32>(new TypeHandler() {
                packer = "ColorUtil.ColorToInt(value)",
                unpacker = "ColorUtil.ColorFromInt(value)",
                parserType = typeof(ColorParser)
            });

            SetTypeHandler<UIMeasurement>(new TypeHandler() {
                packer = "value.value, (int)value.unit",
                unpacker = "new UIMeasurement(float0, (UIMeasurementUnit)int1)",
                parserType = typeof(MeasurementParser)
            });

            SetTypeHandler<UIFixedLength>(new TypeHandler() {
                packer = "value.value, (int)value.unit",
                unpacker = "new UIFixedLength(float0, (UIFixedLengthUnit)int1)",
                parserType = typeof(FixedLengthParser)
            });

            SetTypeHandler<CursorStyle>(new TypeHandler() {
                parserType = typeof(CursorStyleParser)
            });
            //
            // SetStyleTypeParser<FloatParser>(typeof(float));
            // SetStyleTypeParser<IntParser>(typeof(int));
            // SetStyleTypeParser<StringParser>(typeof(string));
            // SetStyleTypeParser<ColorParser>(typeof(Color32));
            // SetStyleTypeParser<TextureParser>(typeof(Texture2D));
            // SetStyleTypeParser<MeasurementParser>(typeof(UIMeasurement));
            // SetStyleTypeParser<FixedLengthParser>(typeof(UIFixedLength));
            // SetStyleTypeParser<CursorStyleParser>(typeof(CursorStyle));
            // SetStyleTypeParser<EnumParser<LayoutDirection>>(typeof(LayoutDirection));

            AddStyleProperty("Opacity", typeof(float), "1f", PropertyFlags.Inherited | PropertyFlags.Animated);
            AddStyleProperty("Visibility", typeof(Visibility), "Visibility.Visible", PropertyFlags.Inherited);
            AddStyleProperty("Cursor", typeof(CursorStyle), "null");
            AddStyleProperty("Painter", typeof(string), "null");
            AddStyleProperty("OverflowX", typeof(Overflow), "Overflow.Visible");
            AddStyleProperty("OverflowY", typeof(Overflow), "Overflow.Visible");
            AddStyleProperty("ClipBehavior", typeof(ClipBehavior), "ClipBehavior.Normal");
            AddStyleProperty("ClipBounds", typeof(ClipBounds), "ClipBounds.BorderBox");
            AddStyleProperty("PointerEvents", typeof(PointerEvents), "PointerEvents.Normal");

            // Background
            AddStyleProperty("BackgroundColor", typeof(Color32), "default", PropertyFlags.Animated);
            AddStyleProperty("BackgroundTint", typeof(Color32), "default", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageOffsetX", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageOffsetY", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageScaleX", typeof(float), "1f", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageScaleY", typeof(float), "1f", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageTileX", typeof(float), "1f", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageTileY", typeof(float), "1f", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImageRotation", typeof(float), "0f", PropertyFlags.Animated);
            AddStyleProperty("BackgroundImage", typeof(Texture2D), "null");
            AddStyleProperty("BackgroundFit", typeof(BackgroundFit), "BackgroundFit.Fill");

            // Border
            AddStyleProperty("BorderColorTop", typeof(Color32), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderColorRight", typeof(Color32), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderColorBottom", typeof(Color32), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderColorLeft", typeof(Color32), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderTop", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderRight", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderBottom", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderLeft", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderRadiusTopLeft", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderRadiusTopRight", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderRadiusBottomRight", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("BorderRadiusBottomLeft", typeof(UIFixedLength), "default", PropertyFlags.Animated);

            // Padding
            AddStyleProperty("PaddingTop", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("PaddingRight", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("PaddingBottom", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("PaddingLeft", typeof(UIFixedLength), "default", PropertyFlags.Animated);

            // Margin
            AddStyleProperty("MarginTop", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("MarginRight", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("MarginBottom", typeof(UIFixedLength), "default", PropertyFlags.Animated);
            AddStyleProperty("MarginLeft", typeof(UIFixedLength), "default", PropertyFlags.Animated);

            // Size
            AddStyleProperty("MinWidth", typeof(UIMeasurement), "default", PropertyFlags.Animated);
            AddStyleProperty("MaxWidth", typeof(UIMeasurement), "new UIMeasurement(float.MaxValue)", PropertyFlags.Animated);
            AddStyleProperty("MinHeight", typeof(UIMeasurement), "default", PropertyFlags.Animated);
            AddStyleProperty("MaxHeight", typeof(UIMeasurement), "new UIMeasurement(float.MaxValue)", PropertyFlags.Animated);
            AddStyleProperty("PreferredWidth", typeof(UIMeasurement), "new UIMeasurement(1, UIMeasurementUnit.Content)", PropertyFlags.Animated);
            AddStyleProperty("PreferredHeight", typeof(UIMeasurement), "new UIMeasurement(1, UIMeasurementUnit.Content)", PropertyFlags.Animated);

            builtInPhase = false;
        }


        protected struct ParserTableEntry {

            public int? index;
            public Type parserType;

        }

        
        private void SetTypeHandler<T>(TypeHandler handler) {
            typeHandlerMap[typeof(T)] = handler;
        }

        private void ValidateParserTypes() {
            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;

                if (parserMap.TryGetValue(type, out ParserTableEntry entry)) {
                    continue;
                }

                if (type.IsEnum) {
                    entry.parserType = typeof(EnumParser<>).MakeGenericType(type);
                    parserMap[type] = entry;
                    continue;
                }

                throw new Exception("Missing parser for type " + type);
            }
        }


        private void BuildFromValueFunctions(StringBuilder builder) {
            List<Type> types = new List<Type>();
            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;
                if (!types.Contains(type)) {
                    types.Add(type);
                }
            }

            List<StylePropertyType> ids = new List<StylePropertyType>();

            for (int i = 0; i < types.Count; i++) {
                ids.Clear();

                Type targetType = types[i];

                for (int j = 0; j < stylePropertyTypes.Count; j++) {
                    StylePropertyType property = stylePropertyTypes[j];
                    if (property.type == targetType) {
                        ids.Add(property);
                    }
                }

                builder.Append(s_Indent8);
                builder.Append("public static ");
                builder.Append(nameof(StyleProperty2));
                builder.Append(" FromValue(");
                builder.Append(nameof(PropertyId));
                builder.Append(" propertyId, ");
                builder.Append(TypeNameGenerator.GetTypeName(targetType));
                builder.Append(" value) {\n");
                builder.Append(s_Indent12);
                builder.Append("switch(propertyId.id) {\n");
                for (int j = 0; j < ids.Count; j++) {
                    builder.Append(s_Indent16);
                    builder.Append("case ");
                    builder.Append(ids[j].propertyId.id);
                    builder.Append(": // ");
                    builder.Append(ids[j].name);
                    builder.Append("\n");
                }

                builder.Append(s_Indent20);
                builder.Append("return new ");
                builder.Append(nameof(StyleProperty2));
                builder.Append("(propertyId, ");
                string packer = GetTypePacker(targetType);

                builder.Append(packer);
                builder.Append(");\n");
                builder.Append(s_Indent16);
                builder.Append("default:\n");
                builder.Append(s_Indent20);
                builder.Append($"throw new Exception($\"Tried to create a {{nameof({nameof(StyleProperty2)})}} from value but the given propertyId `{{propertyId}}` is not compatible with the value type {targetType.Name}\");\n");
                builder.Append(s_Indent12);
                builder.Append("}\n");
                builder.Append(s_Indent8);
                builder.Append("}\n\n");
            }
        }

        private string GetTypePacker(Type type) {
            typeHandlerMap.TryGetValue(type, out TypeHandler handler);

            string packer = handler.packer;

            if (handler.packer == null) {
                if (type.IsEnum) {
                    packer = "(int)value";
                }
                else if (type.IsClass) {
                    packer = "GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject()";
                }
                else {
                    packer = "value";
                }
            }

            return packer;
        }

        private int BuildParserTable(StringBuilder builder) {
            ValidateParserTypes();

            int idx = 0;

            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;

                parserMap.TryGetValue(type, out ParserTableEntry entry);

                if (entry.index != null) {
                    continue;
                }

                entry.index = idx;
                parserMap[type] = entry;

                string result = StyleGeneratorConstants.k_ParseEntryTemplate
                    .Replace("::INDEX::", idx.ToString())
                    .Replace("::PARSER_TYPE_NAME::", TypeNameGenerator.GetTypeName(entry.parserType));

                builder.Append(result);
                idx++;
            }

            return idx;
        }


        private void BuildPropertyIds(StringBuilder builder) {
            List<string> tmp = new List<string>();

            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                propertyIds.Add(new PropertyId(i, stylePropertyTypes[i].flags));
                stylePropertyTypes[i].propertyId = new PropertyId(i, stylePropertyTypes[i].flags);
                builder.Append("        public static readonly PropertyId ");
                builder.Append(stylePropertyTypes[i].name);
                builder.Append(" = new PropertyId(");
                builder.Append(propertyIds[i].index);
                builder.Append(", ");
                if (propertyIds[i].flags == 0) {
                    builder.Append("PropertyFlags.None");
                }
                else {
                    tmp.Clear();

                    PropertyFlags flags = propertyIds[i].flags;

                    if ((flags & PropertyFlags.BuiltIn) != 0) {
                        tmp.Add("PropertyFlags.BuiltIn");
                    }

                    if ((flags & PropertyFlags.Inherited) != 0) {
                        tmp.Add("PropertyFlags.Inherited");
                    }

                    if ((flags & PropertyFlags.Animated) != 0) {
                        tmp.Add("PropertyFlags.Animated");
                    }

                    if ((flags & PropertyFlags.RequireDestruction) != 0) {
                        tmp.Add("PropertyFlags.RequireDestruction");
                    }

                    builder.Append(StringUtil.ListToString(tmp, " | "));
                }

                builder.Append(");\n");
            }
        }

        private int BuildPropertyList(StringBuilder builder) {
            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;
                string name = stylePropertyTypes[i].name;

                parserMap.TryGetValue(type, out ParserTableEntry entry);

                string result = StyleGeneratorConstants.k_PropertyEntryTemplate
                    .Replace("::INDEX::", i.ToString())
                    .Replace("::PROPERTY_NAME::", name)
                    .Replace("::PARSER_ID::", entry.index.ToString());

                builder.Append(result);
            }

            return stylePropertyTypes.Count;
        }

        private void BuildStylePackers(StringBuilder builder) {
            string line0 = $"        public static {nameof(StyleProperty2)} ::PROPERTY_NAME::(::ARGUMENT_TYPE:: value) {{\n";
            string line1 = $"            return new {nameof(StyleProperty2)}({nameof(PropertyId)}.::PROPERTY_NAME::, ::CONVERT::);\n";
            string line2 = "        }\n\n";

            for (int i = 0; i < stylePropertyTypes.Count; i++) {
                StylePropertyType property = stylePropertyTypes[i];

                string result = line0
                    .Replace("::PROPERTY_NAME::", property.name)
                    .Replace("::ARGUMENT_TYPE::", TypeNameGenerator.GetTypeName(property.type));

                string result1 = line1
                    .Replace("::PROPERTY_NAME::", property.name)
                    .Replace("::CONVERT::", GetTypePacker(property.type));

                builder.Append(result);
                builder.Append(result1);
                builder.Append(line2);
            }
        }

        public void AddNamespace(string namespaceName) {
            if (!namespaces.Contains(namespaceName)) {
                namespaces.Add(namespaceName);
            }
        }

        private void BuildUsings(StringBuilder builder) {
            for (ushort i = 0; i < stylePropertyTypes.Count; i++) {
                Type type = stylePropertyTypes[i].type;
                if (type.Namespace != "UIForia.Style") {
                    if (!namespaces.Contains(type.Namespace)) {
                        namespaces.Add(type.Namespace);
                    }
                }
            }

            for (int i = 0; i < namespaces.Count; i++) {
                builder.Append("using ");
                builder.Append(namespaces[i]);
                builder.Append(";\n");
            }
        }

        private string GenerateStyleCode() {
            StringBuilder builder = new StringBuilder(4096);

            stylePropertyTypes.Sort((a, b) => a.loweredName.CompareTo(b.loweredName));

            int parserCount = BuildParserTable(builder);

            string parserTable = builder.ToString();

            builder.Clear();

            BuildPropertyIds(builder);

            string propertyIdCode = builder.ToString();

            builder.Clear();

            int propertyCount = BuildPropertyList(builder);

            string propertyEntries = builder.ToString();

            builder.Clear();

            BuildStylePackers(builder);

            string stylePropertyPackers = builder.ToString();

            builder.Clear();

            BuildUsings(builder);

            string usings = builder.ToString();

            builder.Clear();

            BuildFromValueFunctions(builder);

            string styleFromValueFns = builder.ToString();

            return StyleGeneratorConstants.k_PropertyParserClass
                .Replace("::USINGS::", usings)
                .Replace("::PARSE_TABLE_COUNT::", parserCount.ToString())
                .Replace("::PARSER_TABLE_CREATION::", parserTable)
                .Replace("::PROPERTY_NAME_COUNT::", propertyCount.ToString())
                .Replace("::PROPERTY_ENTRIES::", propertyEntries)
                .Replace("::PROPERTY_IDS::", propertyIdCode)
                .Replace("::STYLE_PROPERTY_PACKERS::", stylePropertyPackers)
                .Replace("::STYLE_FROM_VALUE::", styleFromValueFns)
                .Replace("::DEFAULT_STYLE_VALUES::", "")
                .Trim();
        }

        public void AddStyleProperty(string name, Type type, string defaultValue, PropertyFlags flags = 0) {
            if (builtInPhase) {
                flags |= PropertyFlags.BuiltIn;
            }

            stylePropertyTypes.Add(new StylePropertyType(name, type, defaultValue, flags));
        }

        // private void SetStyleTypeParser<T>(Type toBeParsedType) where T : IStylePropertyParser, new() {
        //     Type parserType = typeof(T);
        //
        //     if (parserMap.TryGetValue(toBeParsedType, out ParserTableEntry parserEntry)) {
        //         if (parserEntry.parserType != parserType) {
        //             throw new Exception("Duplicate style type parser for type " + typeof(T).Name);
        //         }
        //
        //         return;
        //     }
        //
        //     parserMap[toBeParsedType] = new ParserTableEntry() {
        //         parserType = parserType
        //     };
        // }

    }

}