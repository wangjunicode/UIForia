using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UIForia.Compilers;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.UIInput;
using UIForia.Util;
using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

    public struct TypeHandler {

        public Type parserType;
        public string packer;
        public string unpacker;
        public string typeName;

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
        protected readonly List<StylePropertyDescription> stylePropertyDescriptions;
        protected readonly List<StyleShorthandDescription> styleShorthandDescriptions;
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
            this.stylePropertyDescriptions = new List<StylePropertyDescription>();
            this.parserMap = new Dictionary<Type, ParserTableEntry>();
            this.styleShorthandDescriptions = new List<StyleShorthandDescription>();

            this.builtInPhase = true;

            AddNamespace("UIForia.Util");
            AddNamespace("System");
            AddNamespace("System.Runtime.InteropServices");

            SetTypeHandler<float>(new TypeHandler() {
                unpacker = nameof(StyleProperty2.float0),
                parserType = typeof(FloatParser),
                typeName = "Float"
            });

            SetTypeHandler<int>(new TypeHandler() {
                unpacker = nameof(StyleProperty2.int0),
                parserType = typeof(IntParser),
                typeName = "Int"
            });

            SetTypeHandler<string>(new TypeHandler() {
                parserType = typeof(StringParser)
            });

            SetTypeHandler<Texture2D>(new TypeHandler() {
                parserType = typeof(StringParser) // todo -- make URL parser
            });

            SetTypeHandler<Color32>(new TypeHandler() {
                packer = "ColorUtil.ColorToInt(value)",
                unpacker = "ColorUtil.ColorFromInt(int0)",
                parserType = typeof(ColorParser),
                typeName = "Color"
            });

            SetTypeHandler<UIMeasurement>(new TypeHandler() {
                packer = "value.value, (int)value.unit",
                unpacker = "new UIMeasurement(float0, (UIMeasurementUnit)int1)",
                parserType = typeof(MeasurementParser),
            });

            SetTypeHandler<UIFixedLength>(new TypeHandler() {
                packer = "value.value, (int)value.unit",
                unpacker = "new UIFixedLength(float0, (UIFixedUnit)int1)",
                parserType = typeof(FixedLengthParser),
            });

            SetTypeHandler<CursorStyle>(new TypeHandler() {
                parserType = typeof(CursorStyleParser)
            });
            

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

            AddStyleShorthand<MinSizeParser>("MinSize");
            AddStyleShorthand<MaxSizeParser>("MaxSize");
            AddStyleShorthand<PreferredSizeParser>("PreferredSize");

            builtInPhase = false;
        }

        protected struct ParserTableEntry {

            public int? index;
            public Type parserType;

        }

        private void SetTypeHandler<T>(TypeHandler handler) {
            typeHandlerMap[typeof(T)] = handler;
            parserMap[typeof(T)] = new ParserTableEntry() {
                parserType = handler.parserType
            };
        }

        private void ValidateParserTypes() {
            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                Type type = stylePropertyDescriptions[i].type;

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


        private string BuildAsValueAccessors(StringBuilder builder) {
            builder.Clear();

            List<Type> types = new List<Type>();

            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                Type type = stylePropertyDescriptions[i].type;
                if (!types.Contains(type)) {
                    types.Add(type);
                }
            }

            for (int i = 0; i < types.Count; i++) {
                Type targetType = types[i];

                TypeHandler handler = default;

                string unpacker = null;
                string typeName = null;
                if (typeHandlerMap.TryGetValue(targetType, out handler)) {
                    unpacker = handler.unpacker;
                    typeName = handler.typeName;
                }

                if (unpacker == null) {
                    if (targetType.IsEnum) {
                        unpacker = "(" + TypeNameGenerator.GetTypeName(targetType) + ")" + nameof(StyleProperty2.int0);
                    }
                    else if (targetType.IsClass) {
                        unpacker = "(" + TypeNameGenerator.GetTypeName(targetType) + ")GCHandle.FromIntPtr(" + nameof(StyleProperty2.ptr) + ").Target";
                    }
                    else {
                        throw new Exception("Unable to unpack " + targetType);
                    }
                }

                if (typeName == null) {
                    typeName = targetType.Name;
                }

                builder.Append(s_Indent8);
                builder.Append("public ");
                builder.Append(TypeNameGenerator.GetTypeName(targetType));
                builder.Append(" As");
                builder.Append(typeName);
                builder.Append(" {\n");
                builder.Append(s_Indent12);
                builder.Append("get {\n");
                builder.Append(s_Indent16);
                builder.Append("return ");
                builder.Append(unpacker);
                builder.Append(";\n");
                builder.Append(s_Indent12);
                builder.Append("}\n");
                builder.Append(s_Indent8);
                builder.Append("}\n");
                builder.Append("\n");
            }


            return builder.ToString();
        }

        private string BuildFromValueFunctions(StringBuilder builder) {
            builder.Clear();
            List<Type> types = new List<Type>();
            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                Type type = stylePropertyDescriptions[i].type;
                if (!types.Contains(type)) {
                    types.Add(type);
                }
            }

            List<StylePropertyDescription> propertyTypes = new List<StylePropertyDescription>();

            for (int i = 0; i < types.Count; i++) {
                propertyTypes.Clear();

                Type targetType = types[i];

                for (int j = 0; j < stylePropertyDescriptions.Count; j++) {
                    StylePropertyDescription property = stylePropertyDescriptions[j];
                    if (property.type == targetType) {
                        propertyTypes.Add(property);
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
                for (int j = 0; j < propertyTypes.Count; j++) {
                    builder.Append(s_Indent16);
                    builder.Append("case ");
                    builder.Append(propertyTypes[j].propertyId.id);
                    builder.Append(": // ");
                    builder.Append(propertyTypes[j].name);
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

            return builder.ToString();
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

        private string BuildParserTable(StringBuilder builder, out int count) {
            builder.Clear();
            ValidateParserTypes();

            int idx = 0;

            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                Type type = stylePropertyDescriptions[i].type;

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

            count = idx;
            return builder.ToString();
        }


        private string BuildPropertyIds(StringBuilder builder) {
            List<string> tmp = new List<string>();

            builder.Clear();
            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                propertyIds.Add(new PropertyId(i, stylePropertyDescriptions[i].flags));
                stylePropertyDescriptions[i].propertyId = new PropertyId(i, stylePropertyDescriptions[i].flags);
                builder.Append("        public static readonly PropertyId ");
                builder.Append(stylePropertyDescriptions[i].name);
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

            return builder.ToString();
        }

        private string BuildPropertyNameList(StringBuilder builder) {
            builder.Clear();
            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                builder.Append("                \"");
                builder.Append(stylePropertyDescriptions[i].name);
                builder.Append("\"");
                if (i != stylePropertyDescriptions.Count - 1) {
                    builder.Append(",\n");
                }
            }

            return builder.ToString();
        }

        private string BuildPropertyList(StringBuilder builder) {
            builder.Clear();
            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                Type type = stylePropertyDescriptions[i].type;
                string name = stylePropertyDescriptions[i].name;

                parserMap.TryGetValue(type, out ParserTableEntry entry);

                string result = StyleGeneratorConstants.k_PropertyEntryTemplate
                    .Replace("::INDEX::", i.ToString())
                    .Replace("::PROPERTY_NAME::", name)
                    .Replace("::PARSER_ID::", entry.index.ToString());

                builder.Append(result);
            }

            return builder.ToString();
        }

        private string BuildShorthandEntries(StringBuilder builder) {
            builder.Clear();
            for (ushort i = 0; i < styleShorthandDescriptions.Count; i++) {
                string name = styleShorthandDescriptions[i].name;
                
                string result = StyleGeneratorConstants.k_ShorthandEntryTemplate
                    .Replace("::INDEX::", i.ToString())
                    .Replace("::SHORTHAND_NAME::", name)
                    .Replace("::PARSER_TYPE::", TypeNameGenerator.GetTypeName(styleShorthandDescriptions[i].parserType));

                builder.Append(result);
            }

            return builder.ToString();
        }

        private string BuildStylePackers(StringBuilder builder) {
            builder.Clear();

            string line0 = $"        public static {nameof(StyleProperty2)} ::PROPERTY_NAME::(::ARGUMENT_TYPE:: value) {{\n";
            string line1 = $"            return new {nameof(StyleProperty2)}({nameof(PropertyId)}.::PROPERTY_NAME::, ::CONVERT::);\n";
            string line2 = "        }\n\n";

            for (int i = 0; i < stylePropertyDescriptions.Count; i++) {
                StylePropertyDescription property = stylePropertyDescriptions[i];

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

            return builder.ToString();
        }

        public void AddNamespace(string namespaceName) {
            if (!namespaces.Contains(namespaceName)) {
                namespaces.Add(namespaceName);
            }
        }

        private string BuildUsings(StringBuilder builder) {
            builder.Clear();
            for (ushort i = 0; i < stylePropertyDescriptions.Count; i++) {
                Type type = stylePropertyDescriptions[i].type;
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

            return builder.ToString();
        }

        private string BuildShorthandNames(StringBuilder builder) {
            builder.Clear();

            for (int i = 0; i < styleShorthandDescriptions.Count; i++) {
                builder.Append("                \"");
                builder.Append(styleShorthandDescriptions[i].name);
                builder.Append("\"");
                if (i != styleShorthandDescriptions.Count - 1) {
                    builder.Append(",\n");
                }
            }

            return builder.ToString();
        }

        private string GenerateStyleCode() {
            StringBuilder builder = new StringBuilder(4096);

            stylePropertyDescriptions.Sort((a, b) => string.CompareOrdinal(a.loweredName, b.loweredName));
            styleShorthandDescriptions.Sort((a, b) => string.CompareOrdinal(a.loweredName, b.loweredName));

            return StyleGeneratorConstants.k_PropertyParserClass
                .Replace("::USINGS::", BuildUsings(builder))
                .Replace("::PARSER_TABLE_CREATION::", BuildParserTable(builder, out int parserCount))
                .Replace("::PARSE_TABLE_COUNT::", parserCount.ToString())
                .Replace("::PROPERTY_NAME_COUNT::", stylePropertyDescriptions.Count.ToString())
                .Replace("::PROPERTY_ENTRIES::", BuildPropertyList(builder))
                .Replace("::PROPERTY_NAMES::", BuildPropertyNameList(builder))
                .Replace("::PROPERTY_IDS::", BuildPropertyIds(builder))
                .Replace("::STYLE_PROPERTY_PACKERS::", BuildStylePackers(builder))
                .Replace("::STYLE_FROM_VALUE::", BuildFromValueFunctions(builder))
                .Replace("::STYLE_AS_VALUE::", BuildAsValueAccessors(builder))
                .Replace("::SHORTHAND_COUNT::", styleShorthandDescriptions.Count.ToString())
                .Replace("::SHORTHAND_NAMES::", BuildShorthandNames(builder))
                .Replace("::SHORTHAND_ENTRIES::", BuildShorthandEntries(builder))
                .Replace("::DEFAULT_STYLE_VALUES::", "")
                .Trim();
        }

        public void AddStyleProperty(string name, Type type, string defaultValue, PropertyFlags flags = 0) {
            if (builtInPhase) {
                flags |= PropertyFlags.BuiltIn;
            }

            stylePropertyDescriptions.Add(new StylePropertyDescription(name, type, defaultValue, flags));
        }

        public void AddStyleShorthand<TParserType>(string shorthandName) where TParserType : IStyleShorthandParser, new() {
            if (builtInPhase) {
                // flag as built in?
            }

            styleShorthandDescriptions.Add(new StyleShorthandDescription(shorthandName, typeof(TParserType)));
        }
        

    }

}