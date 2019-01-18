using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Rendering;
using UIForia.Util;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityScript.Steps;
using MapAction = System.Action<UIForia.Parsing.StyleParser.StyleParserContext, string, string>;

namespace UIForia.Parsing.StyleParser {

    public struct StyleParserContext {

        public UIStyle targetStyle;
        public List<StyleVariable> variables;
        public List<ImportDefinition> imports;

    }

    public struct StyleVariable {

        public Type type;
        public string name;
        public object value;

        public T GetValue<T>() {
            if (type != typeof(T)) { }

            return (T) value;
        }

    }

    public struct ImportDefinition {

        public string path;
        public string name;
        public ParsedStyleSheet sheet;

    }

    public static class StyleParser {

        private static readonly Dictionary<string, ParsedStyleSheet> s_CompiledStyles;
        private static readonly Dictionary<string, MapAction> s_StylePropertyMappers;
        private static readonly List<string> s_CurrentlyParsingList;


        public static void Reset() {
            s_CompiledStyles.Clear();
            s_CurrentlyParsingList.Clear();
        }

        public static UIStyleGroup GetParsedStyle(string uniqueStyleId, string body, string styleName) {
            uniqueStyleId = uniqueStyleId.Trim();
            styleName = styleName.Trim();
            ParsedStyleSheet sheet = s_CompiledStyles.GetOrDefault(uniqueStyleId);
            if (sheet != null) {
                return sheet.GetStyleGroup(styleName);
            }

            if (!string.IsNullOrEmpty(body) && !string.IsNullOrWhiteSpace(body)) {
                sheet = ParseFromString(body);
                sheet.id = uniqueStyleId;

                s_CompiledStyles[uniqueStyleId] = sheet;
                return sheet.GetStyleGroup(styleName);
            }

            if (File.Exists(UnityEngine.Application.dataPath + "/" + uniqueStyleId)) {
                string contents = File.ReadAllText(UnityEngine.Application.dataPath + "/" + uniqueStyleId);
                sheet = ParseFromString(contents);
                sheet.id = uniqueStyleId;
                s_CompiledStyles[uniqueStyleId] = sheet;
                return sheet.GetStyleGroup(styleName);
            }

            sheet = TryParseStyleFromClassPath(Path.GetFileNameWithoutExtension(uniqueStyleId));

            if (sheet != null) {
                s_CompiledStyles[uniqueStyleId] = sheet;
            }

            return sheet?.GetStyleGroup(styleName);
        }

        private static ParsedStyleSheet TryParseStyleFromClassPath(string path) {
            Type styleType = TypeProcessor.GetRuntimeType(Path.GetFileNameWithoutExtension(path));

            if (styleType == null) return null;
            ParsedStyleSheet sheet = new ParsedStyleSheet();
            List<UIStyleGroup> groups = new List<UIStyleGroup>();

            MethodInfo[] methods = styleType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++) {
                MethodInfo methodInfo = methods[i];

                ExportStyleAttribute attr = (ExportStyleAttribute) methodInfo.GetCustomAttribute(typeof(ExportStyleAttribute));

                if (attr == null) continue;

                if (!methodInfo.IsStatic) {
                    throw new Exception($"Methods annotated with {nameof(ExportStyleAttribute)} must be static");
                }

                if (methods[i].GetParameters().Length != 0) {
                    throw new Exception($"Methods annotated with {nameof(ExportStyleAttribute)} must not accept parameters");
                }

                if (methodInfo.ReturnType == typeof(UIStyle)) {
                    UIStyleGroup group = new UIStyleGroup();
                    group.name = attr.name;
                    group.normal = (UIStyle) methodInfo.Invoke(null, null);
                    groups.Add(group);
                }
                else if (methodInfo.ReturnType == typeof(UIStyleGroup)) {
                    UIStyleGroup group = (UIStyleGroup) methodInfo.Invoke(null, null);
                    group.name = attr.name;
                    groups.Add(group);
                }
                else {
                    throw new Exception($"Methods annotated with {nameof(ExportStyleAttribute)} must return {nameof(UIStyle)} or {nameof(UIStyleGroup)}");
                }
            }

            sheet.styles = groups.ToArray();
            return sheet;
        }

        // var name colon equals value
        private static StyleVariable ResolveVariable(List<StyleVariable> variables, List<ImportDefinition> imports, StyleComponent component) {
            string body = component.body;
            int ptr = 0;
            ParseUtil.ConsumeString(":", body, ref ptr);
            string typeName = ParseUtil.ReadToWhitespace(body, ref ptr);
            ParseUtil.ConsumeString("=", body, ref ptr);
            string value = ParseUtil.ReadToStatementEnd(body, ref ptr);
            StyleVariable retn = new StyleVariable();
            retn.name = "@" + component.name;

            switch (typeName.ToLower()) {
                case "color":
                    retn.type = typeof(Color);
                    retn.value = ParseUtil.ParseColor(variables, value);
                    break;
                case "int":
                    retn.type = typeof(int);
                    retn.value = ParseUtil.ParseInt(variables, value);
                    break;
                case "float":
                    retn.type = typeof(float);
                    retn.value = ParseUtil.ParseFloat(variables, value);
                    break;
                case "fixed-length":
                    retn.type = typeof(UIFixedLength);
                    retn.value = ParseUtil.ParseFixedLength(variables, value);
                    break;
                case "measurement":
                    retn.type = typeof(UIMeasurement);
                    retn.value = ParseUtil.ParseMeasurement(variables, value);
                    break;
                case "layout-type":
                    retn.type = typeof(LayoutType);
                    retn.value = ParseUtil.ParseLayoutType(variables, value);
                    break;
                case "layout-direction":
                    retn.type = typeof(LayoutDirection);
                    retn.value = ParseUtil.ParseLayoutDirection(variables, value);
                    break;
                default:
                    throw new NotImplementedException();
            }

            // if local reference make sure it was defined already
            return retn;
        }

        private static ImportDefinition ResolveImport(List<StyleVariable> variables, StyleComponent component) {
            string body = component.body;
            int ptr = 0;
            string alias = ParseUtil.ReadToWhitespace(body, ref ptr);
            ParseUtil.ConsumeString("=", body, ref ptr);
            string path = ParseUtil.ReadToStatementEnd(body, ref ptr);
            ImportDefinition import = new ImportDefinition();
            ParsedStyleSheet sheet;
            if (!s_CompiledStyles.TryGetValue(path, out sheet)) {
                string filePath = UnityEngine.Application.dataPath + "/" + path;
                if (!File.Exists(filePath)) {
                    throw new ParseException("File at " + path + " does not exist or is a not a .style file");
                }

                if (s_CurrentlyParsingList.Contains(filePath)) {
                    throw new ParseException("Cycle in imports");
                }

                s_CurrentlyParsingList.Add(filePath);

                string content = File.ReadAllText(filePath);

                sheet = ParseFromString(content);
                s_CurrentlyParsingList.Remove(filePath);
                s_CompiledStyles[path] = sheet;
                for (int i = 0; i < sheet.variables.Count; i++) {
                    StyleVariable v = new StyleVariable();
                    v.name = "@" + alias + "." + sheet.variables[i].name;
                    v.value = sheet.variables[i].value;
                    v.type = sheet.variables[i].type;
                    variables.Add(v);
                }
            }

            import.name = alias;
            import.path = path;
            import.sheet = sheet;
            return import;
        }

        public static ParsedStyleSheet ParseFromString(string input) {
            if (input == null) {
                return null;
            }

            ParsedStyleSheet retn = new ParsedStyleSheet();
            List<StyleComponent> output = new List<StyleComponent>();
            int ptr = 0;

            if (input.Length == 0) {
                return retn;
            }

            while (ptr < input.Length) {
                int start = ptr;
                ParseUtil.ConsumeComment(input, ref ptr);
                ptr = ReadStyleDefinition(ptr, input, output);
                ptr = ReadImplicitStyleDefinition(ptr, input, output);
                ptr = ReadVariableDefinition(ptr, input, output);
                ptr = ReadImportDefinition(ptr, input, output);
                ptr = ReadCursorDefinition(ptr, input, output);
                ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);
                if (ptr == start && ptr < input.Length) {
                    throw new ParseException("Style Tokenizer failed on string: " + input);
                }
            }

            List<UIStyleGroup> styleList = ListPool<UIStyleGroup>.Get();
            List<StyleVariable> localVariables = ListPool<StyleVariable>.Get();
            List<StyleVariable> variables = ListPool<StyleVariable>.Get();
            List<ImportDefinition> imports = ListPool<ImportDefinition>.Get();

            for (int i = 0; i < output.Count; i++) {
                StyleComponent current = output[i];
                if (current.type == StyleComponentType.Import) {
                    imports.Add(ResolveImport(variables, current));
                }
            }

            for (int i = 0; i < output.Count; i++) {
                StyleComponent current = output[i];
                if (current.type == StyleComponentType.Variable) {
                    variables.Add(ResolveVariable(variables, imports, current));
                    localVariables.Add(variables[variables.Count - 1]);
                }
            }

            for (int i = 0; i < output.Count; i++) {
                StyleComponent current = output[i];
                switch (current.type) {
                    case StyleComponentType.Style:
                        styleList.Add(ParseStyle(current, variables, imports));
                        break;
                    case StyleComponentType.ImplicitStyle:
                        throw new NotImplementedException();
                    
                    case StyleComponentType.Animation:
                        break;
                    case StyleComponentType.Cursor:
                        break;
                    case StyleComponentType.Texture:
                        break;
                    case StyleComponentType.Font:
                        break;
                    case StyleComponentType.Query:
                        break;
                    case StyleComponentType.Variable:
                    case StyleComponentType.Import:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            retn.styles = styleList.ToArray();
            retn.variables = localVariables;
            
            ListPool<UIStyleGroup>.Release(ref styleList);
            ListPool<ImportDefinition>.Release(ref imports);
            ListPool<StyleVariable>.Release(ref variables);
            return retn;
        }

        private static string ParseEventName(string input, ref int ptr) {
//            string eventName = ReadUntilEnd
            return null;
        }

        private static UIStyleGroup ParseStyle(StyleComponent styleComponent, List<StyleVariable> variables, List<ImportDefinition> imports) {
            int ptr = 0;
            string input = styleComponent.body;

            UIStyle currentStyle = new UIStyle();
            UIStyleGroup styleGroup = new UIStyleGroup();
            styleGroup.name = styleComponent.name;
            styleGroup.normal = currentStyle;

            ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);

            Stack<UIStyle> styleStack = StackPool<UIStyle>.Get();

            styleStack.Push(currentStyle);
            StyleParserContext context = new StyleParserContext();
            context.targetStyle = currentStyle;
            context.variables = variables;
            context.imports = imports;

            while (ptr < input.Length) {

                if (ParseUtil.ConsumeComment(input, ref ptr)) {
                    continue;
                }

                int start = ptr;
                char current = input[ptr];

//                if (current == ':' && ptr + 1 < input.Length && input[ptr + 1] == ':') {
//                    ptr++;
//                    string sectionName = ParseUtil.ReadIdentifier(input, ref ptr);
//                    switch (sectionName.ToLower()) {
//                        case "scrollbar-track":
//                            break;
//                        case "scrollbar-track-vertical":
//                            break;
//                        case "scrollbar-track-horizontal":
//                            break;
//                        case "scrollbar-handle":
//                            break;
//                        case "scrollbar-handle-vertical":
//                            break;
//                        case "scrollbar-handle-horizontal":
//                            break;
//                        case "scrollbar-increment":
//                            break;
//                        case "scrollbar-increment-vertical":
//                            break;
//                        case "scrollbar-increment-horizontal":
//                            break;
//                        case "scrollbar-decrement":
//                            break;
//                        case "scrollbar-decrement-vertical":
//                            break;
//                        case "scrollbar-decrement-horizontal":
//                            break;
//                        case "scrollbar-buttons":
//                            break;
//                        case "scrollbar-buttons-vertical:
//                            break;
//                        case "scrollbar-buttons-horizontal":
//                            break;
//                    }
//                }

                if (current == '[') {
                    string stateName = ParseUtil.ReadBlock(input, ref ptr, '[', ']');
                    ParseUtil.ConsumeString("{", input, ref ptr);
                    switch (stateName.ToLower()) {
                        case "normal":
                            context.targetStyle = styleGroup.normal;
                            break;
                        case "inactive":
                            styleGroup.inactive = styleGroup.inactive ?? new UIStyle();
                            context.targetStyle = styleGroup.inactive;
                            break;
                        case "focused":
                            styleGroup.focused = styleGroup.focused ?? new UIStyle();
                            context.targetStyle = styleGroup.focused;
                            break;
                        case "hover":
                            styleGroup.hover = styleGroup.hover ?? new UIStyle();
                            context.targetStyle = styleGroup.hover;
                            break;
                        case "active":
                            styleGroup.inactive = styleGroup.inactive ?? new UIStyle();
                            context.targetStyle = styleGroup.inactive;
                            break;
                        default:
                            throw new ParseException("Style ‘" + styleGroup.name + "’\n" + "Unknown style state: " + stateName);
                    }
                }
                else if (current == '}') {
                    context.targetStyle = styleGroup.normal;
                    ptr++;
                }
//                else if (current == ':') {
//                    string eventName = ParseEventName(input, ref ptr);
//                    string stateBody = ReadBlock(input, ref ptr, '{', '}');
//                }
                else if (char.IsLetter(current)) {
                    string id = ParseUtil.ReadIdentifier(input, ref ptr);
                    ParseUtil.ConsumeString("=", input, ref ptr);
                    string value = ParseUtil.ReadToStatementEnd(input, ref ptr);

                    if (value == null) {
                        throw new ParseException("Style ‘" + styleGroup.name + "’\n" + "Unexpected end of input");
                    }

                    MapAction action;
                    // I think this should be replaced with a huge switch statement
                    if (s_StylePropertyMappers.TryGetValue(id.ToLower(), out action)) {
                        action(context, id, value);
                    }
                    else {
                        Debug.Log("Style ‘" + styleGroup.name + "’\n" + "Encountered unknown style property name: " + id);
                        return styleGroup;
                    }
                }
                else {
                    throw new ParseException("Style ‘" + styleGroup.name + "’\n" + ParseUtil.ProduceErrorMessage(input, ptr));
                }

                ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);
                if (ptr == start && ptr < input.Length) {
                    throw new ParseException("Style Tokenizer failed on string: " + input);
                }
            }


            return styleGroup;
        }

        private static int ReadVariableDefinition(int ptr, string input, List<StyleComponent> output) {
            int start = ptr;

            ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);
            if (!ParseUtil.TryReadCharacters(input, "var ", ref ptr)) {
                return start;
            }

            StyleComponent retn = new StyleComponent(
                StyleComponentType.Variable,
                ParseUtil.ReadIdentifierOrThrow(input, ref ptr),
                null,
                ParseUtil.ReadToStatementEnd(input, ref ptr)
            );

            output.Add(retn);
            return ptr;
        }

        private static int ReadImportDefinition(int ptr, string input, List<StyleComponent> output) {
            int start = ptr;

            ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);
            if (!ParseUtil.TryReadCharacters(input, "import ", ref ptr)) {
                return start;
            }

            StyleComponent retn = new StyleComponent(
                StyleComponentType.Import,
                ParseUtil.ReadIdentifierOrThrow(input, ref ptr),
                null,
                ParseUtil.ReadToStatementEnd(input, ref ptr)
            );

            output.Add(retn);
            return ptr;
        }

        // style space string (space*) (colon? identifier+) (space*) open-brace (space*) .* (space*) close-brace
        private static int ReadStyleDefinition(int ptr, string input, List<StyleComponent> output) {
            int start = ptr;

            ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);

            // if we get past here we can assume what we are parsing is actually a style definition
            if (!ParseUtil.TryReadCharacters(input, "style ", ref ptr)) {
                return start;
            }

            StyleComponent retn = new StyleComponent(
                StyleComponentType.Style,
                ParseUtil.ReadIdentifierOrThrow(input, ref ptr),
                ReadInheritanceList(input, ref ptr),
                ParseUtil.ReadBlockOrThrow(input, ref ptr, '{', '}')
            );

            output.Add(retn);

            return ptr;
        }
        
        // style space string (space*) (colon? identifier+) (space*) open-brace (space*) .* (space*) close-brace
        private static int ReadImplicitStyleDefinition(int ptr, string input, List<StyleComponent> output) {
            int start = ptr;

            ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);

            // if we get past here we can assume what we are parsing is actually a style definition
            if (!ParseUtil.TryReadCharacters(input, "implicit style ", ref ptr)) {
                return start;
            }

            StyleComponent retn = new StyleComponent(
                StyleComponentType.ImplicitStyle,
                ParseUtil.ReadIdentifierOrThrow(input, ref ptr),
                ReadInheritanceList(input, ref ptr),
                ParseUtil.ReadBlockOrThrow(input, ref ptr, '{', '}')
            );

            output.Add(retn);

            return ptr;
        }

        private static int ReadCursorDefinition(int ptr, string input, List<StyleComponent> output) {
            int start = ptr;
            ptr = ParseUtil.ConsumeWhiteSpace(ptr, input);
            if (!ParseUtil.TryReadCharacters(input, "cursor ", ref ptr)) {
                return start;
            }

            StyleComponent retn = new StyleComponent(
                StyleComponentType.Cursor,
                ParseUtil.ReadIdentifierOrThrow(input, ref ptr),
                null,
                ParseUtil.ReadBlockOrThrow(input, ref ptr, '{', '}')
            );

            output.Add(retn);

            return ptr;
        }

        private static string[] ReadInheritanceList(string input, ref int ptr) {
            return null;
        }

        static StyleParser() {
            s_CurrentlyParsingList = new List<string>();
            s_StylePropertyMappers = new Dictionary<string, MapAction>();
            // todo -- get rid of this, or at least don't allocate so many actions. 
            
            StylePropertyMapper[] styleIdentifiers = {
                new StylePropertyMapper("Overflow", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("OverflowX", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("OverflowY", StylePropertyMappers.DisplayMapper),

                new StylePropertyMapper("BackgroundColor", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("BorderColor", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("BackgroundImage", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("Opacity", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("Cursor", StylePropertyMappers.DisplayMapper),
                new StylePropertyMapper("Visibility", StylePropertyMappers.DisplayMapper), 

                new StylePropertyMapper("GridItemColStart", StylePropertyMappers.GridItemMapper),
                new StylePropertyMapper("GridItemColSpan", StylePropertyMappers.GridItemMapper),
                new StylePropertyMapper("GridItemRowStart", StylePropertyMappers.GridItemMapper),
                new StylePropertyMapper("GridItemRowSpan", StylePropertyMappers.GridItemMapper),
                new StylePropertyMapper("GridItemColSelfAlignment", StylePropertyMappers.GridItemMapper),
                new StylePropertyMapper("GridItemRowSelfAlignment", StylePropertyMappers.GridItemMapper),

                new StylePropertyMapper("GridLayoutDirection", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutDensity", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutColTemplate", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutRowTemplate", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutMainAxisAutoSize", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutCrossAxisAutoSize", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutColGap", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutRowGap", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutColAlignment", StylePropertyMappers.GridLayoutMapper),
                new StylePropertyMapper("GridLayoutRowAlignment", StylePropertyMappers.GridLayoutMapper),

                new StylePropertyMapper("FlexLayoutWrap", StylePropertyMappers.FlexLayoutMapper),
                new StylePropertyMapper("FlexLayoutDirection", StylePropertyMappers.FlexLayoutMapper),
                new StylePropertyMapper("FlexLayoutMainAxisAlignment", StylePropertyMappers.FlexLayoutMapper),
                new StylePropertyMapper("FlexLayoutCrossAxisAlignment", StylePropertyMappers.FlexLayoutMapper),

                new StylePropertyMapper("FlexItemSelfAlignment", StylePropertyMappers.FlexItemMapper),
                new StylePropertyMapper("FlexItemOrder", StylePropertyMappers.FlexItemMapper),
                new StylePropertyMapper("FlexItemGrow", StylePropertyMappers.FlexItemMapper),
                new StylePropertyMapper("FlexItemShrink", StylePropertyMappers.FlexItemMapper),

                new StylePropertyMapper("Margin", StylePropertyMappers.MarginMapper),
                new StylePropertyMapper("MarginTop", StylePropertyMappers.MarginMapper),
                new StylePropertyMapper("MarginRight", StylePropertyMappers.MarginMapper),
                new StylePropertyMapper("MarginBottom", StylePropertyMappers.MarginMapper),
                new StylePropertyMapper("MarginLeft", StylePropertyMappers.MarginMapper),

                new StylePropertyMapper("Border", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("BorderTop", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("BorderRight", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("BorderBottom", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("BorderLeft", StylePropertyMappers.PaddingBorderMapper),

                new StylePropertyMapper("Padding", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("PaddingTop", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("PaddingRight", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("PaddingBottom", StylePropertyMappers.PaddingBorderMapper),
                new StylePropertyMapper("PaddingLeft", StylePropertyMappers.PaddingBorderMapper),

                new StylePropertyMapper("BorderRadius", StylePropertyMappers.BorderRadiusMapper),
                new StylePropertyMapper("BorderRadiusTopLeft", StylePropertyMappers.BorderRadiusMapper),
                new StylePropertyMapper("BorderRadiusTopRight", StylePropertyMappers.BorderRadiusMapper),
                new StylePropertyMapper("BorderRadiusBottomLeft", StylePropertyMappers.BorderRadiusMapper),
                new StylePropertyMapper("BorderRadiusBottomRight", StylePropertyMappers.BorderRadiusMapper),

                new StylePropertyMapper("TransformPosition", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformPositionX", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformPositionY", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformScale", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformScaleX", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformScaleY", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformPivot", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformPivotX", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformPivotY", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformRotation", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformBehavior", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformBehaviorX", StylePropertyMappers.TransformMapper),
                new StylePropertyMapper("TransformBehaviorY", StylePropertyMappers.TransformMapper),

                new StylePropertyMapper("MinWidth", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("MaxWidth", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("PreferredWidth", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("MinHeight", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("MaxHeight", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("PreferredHeight", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("PreferredSize", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("MinSize", StylePropertyMappers.SizeMapper),
                new StylePropertyMapper("MaxSize", StylePropertyMappers.SizeMapper),

                new StylePropertyMapper("LayoutType", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("LayoutBehavior", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("AnchorTop", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("AnchorRight", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("AnchorBottom", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("AnchorLeft", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("AnchorTarget", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("ZIndex", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("RenderLayer", StylePropertyMappers.LayoutMapper),
                new StylePropertyMapper("RenderLayerOffset", StylePropertyMappers.LayoutMapper),

                new StylePropertyMapper("TextColor", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextFontAsset", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextFontSize", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextFontStyle", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextAlignment", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextWhitespaceMode", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextWrapMode", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextHorizontalOverflow", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextVerticalOverflow", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextIndentFirstLine", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextIndentNewLine", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextLayoutStyle", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextAutoSize", StylePropertyMappers.TextMapper),
                new StylePropertyMapper("TextTransform", StylePropertyMappers.TextMapper),

                new StylePropertyMapper("BackgroundFillType", null),
                new StylePropertyMapper("BackgroundShapeType", null),
                new StylePropertyMapper("BackgroundSecondaryColor", null),
                new StylePropertyMapper("BackgroundGradientStart", null),
                new StylePropertyMapper("BackgroundGradientAxis", null),
                new StylePropertyMapper("BackgroundGradientType", null),
                new StylePropertyMapper("BackgroundFillRotation", null),
                new StylePropertyMapper("BackgroundFillOffset", null),
                new StylePropertyMapper("BackgroundGridSize", null),
                new StylePropertyMapper("BackgroundLineSize", null),

                new StylePropertyMapper("ScrollbarVerticalTrackSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalTrackColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalTrackBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalTrackBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalTrackBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalTrackImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarVerticalHandleSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalHandleColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalHandleBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalHandleBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalHandleBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalHandleImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarVerticalIncrementSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalIncrementColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalIncrementBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalIncrementBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalIncrementBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalIncrementImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarVerticalDecrementSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalDecrementColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalDecrementBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalDecrementBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalDecrementBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarVerticalDecrementImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarHorizontalTrackSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalTrackColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalTrackBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalTrackBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalTrackBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalTrackImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarHorizontalHandleSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalHandleColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalHandleBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalHandleBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalHandleBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalHandleImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarHorizontalIncrementSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalIncrementColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalIncrementBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalIncrementBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalIncrementBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalIncrementImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarHorizontalDecrementSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalDecrementColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalDecrementBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalDecrementBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalDecrementBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHorizontalDecrementImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarTrackSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarTrackColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarTrackBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarTrackBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarTrackBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarTrackImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarHandleSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHandleColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHandleBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHandleBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHandleBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarHandleImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarIncrementSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarIncrementColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarIncrementBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarIncrementBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarIncrementBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarIncrementImage", StylePropertyMappers.ScrollMapper),

                new StylePropertyMapper("ScrollbarDecrementSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarDecrementColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarDecrementBorderRadius", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarDecrementBorderSize", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarDecrementBorderColor", StylePropertyMappers.ScrollMapper),
                new StylePropertyMapper("ScrollbarDecrementImage", StylePropertyMappers.ScrollMapper),
            };


            s_CompiledStyles = new Dictionary<string, ParsedStyleSheet>();
            for (int i = 0; i < styleIdentifiers.Length; i++) {
                s_StylePropertyMappers[styleIdentifiers[i].propertyName.ToLower()] = styleIdentifiers[i].mapFn;
            }
        }

        private struct StylePropertyMapper {

            public readonly string propertyName;
            public readonly MapAction mapFn;

            public StylePropertyMapper(string propertyName, MapAction mapFn) {
                this.propertyName = propertyName;
                this.mapFn = mapFn;
            }

        }

        public struct StyleComponent {

            public readonly string name;
            public readonly string body;
            public readonly StyleComponentType type;
            public readonly string[] inheritance;

            public StyleComponent(StyleComponentType type, string name, string[] inheritance, string body) {
                this.type = type;
                this.name = name;
                this.body = body;
                this.inheritance = inheritance;
            }

        }

        public enum StyleComponentType {

            Style,
            Animation,
            Cursor,
            Texture,
            Font,
            Query,
            Variable,
            Import,
            ImplicitStyle

        }

    }

}