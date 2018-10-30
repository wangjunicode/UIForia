using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Extensions;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using TMPro;
using UIForia;
using UnityEngine;

namespace Src.Parsing.StyleParser {

    public static class ParseUtil {

        public static string ReadToWhitespace(string input, ref int ptr) {
            int start = ptr;
            while (ptr < input.Length && !char.IsWhiteSpace(input[ptr])) {
                ptr++;
            }

            string retn = input.Substring(start, ptr - start);
            ptr = ConsumeWhiteSpace(ptr, input);
            return retn;
        }

        public static string ReadToStatementEnd(string input, ref int ptr) {
            int start = ptr;
            while (ptr < input.Length) {
                char current = input[ptr];
                if (current == ';' || current == '\n') {
                    ptr++;
                    return input.Substring(start, ptr - start - 1);
                }

                ptr++;
            }

            if (ptr == input.Length) {
                return input.Substring(start, ptr - start);
            }

            ptr = start;
            return null;
        }

        public static int ReadInt(string input, ref int ptr) {
            int start = ptr;

            if (!char.IsDigit(input[ptr])) {
                throw new ParseException("Cannot parse integer because character in string (" + input + ") at index " + ptr + "(" + input[ptr] + ") is not a digit");
            }

            while (ptr < input.Length && char.IsDigit(input[ptr])) {
                ptr++;
            }

            return int.Parse(input.Substring(start, ptr - start));
        }

        public static string ReadIdentifierOrThrow(string input, ref int ptr) {
            string retn = ReadIdentifier(input, ref ptr);
            if (retn == null) {
                throw new ParseException("Expected a valid style identifier while parsing style: " + input.Substring(ptr));
            }

            return retn;
        }

        public static string ReadBlockOrThrow(string input, ref int ptr, char open, char close) {
            string retn = ReadBlock(input, ref ptr, open, close);
            if (retn == null) {
                throw new ParseException("Expected a '{' '}' delimited block while parsing style: " + input.Substring(ptr));
            }

            return retn;
        }

        public static string ReadBlock(string input, ref int ptr, char open, char close) {
            int start = ptr;

            ptr = ConsumeWhiteSpace(ptr, input);

            if (input[ptr] != open) {
                return null;
            }

            int counter = 0;
            while (ptr < input.Length) {
                char current = input[ptr];
                if (current == open) {
                    counter++;
                }
                else if (current == close) {
                    counter--;
                    if (counter == 0) {
                        ptr++;
                        return input.Substring(start + 1, ptr - start - 2);
                    }
                }

                ptr++;
            }

            ptr = start;
            return null;
        }

        public static string ReadIdentifier(string input, ref int ptr) {
            char current = input[ptr];
            int startIndex = ptr;

            if (current == '-' || !IsIdentifierCharacter(current)) {
                return null;
            }

            if (char.IsDigit(current)) {
                return null;
            }

            ptr++;

            while (ptr < input.Length && IsIdentifierCharacter(current)) {
                ptr++;
                current = input[ptr];
            }

            string retn = input.Substring(startIndex, ptr - startIndex);
            ptr = ConsumeWhiteSpace(ptr, input);
            return retn;
        }

        public static bool IsIdentifierCharacter(char character) {
            return !char.IsWhiteSpace(character) && (char.IsLetterOrDigit(character) || character == '-' || character == '_');
        }

        public static int ConsumeWhiteSpace(int start, string input) {
            int ptr = start;
            if (ptr >= input.Length) return input.Length;
            while (ptr < input.Length && char.IsWhiteSpace(input[ptr])) {
                ptr++;
            }

            return ptr;
        }

        public static bool TryReadCharacters(string input, string match, ref int ptr) {
            if (ptr + match.Length > input.Length) return false;
            for (int i = 0; i < match.Length; i++) {
                if (input[ptr + i] != match[i]) {
                    return false;
                }
            }

            ptr = ptr + match.Length;
            ptr = ConsumeWhiteSpace(ptr, input);
            return true;
        }

        public static void ConsumeString(string match, string input, ref int ptr) {
            ptr = ConsumeWhiteSpace(ptr, input);
            if (ptr + match.Length > input.Length) {
                throw new ParseException("Expected " + match + " but was not found when parsing: " + input.Substring(ptr, input.Length - ptr));
            }

            for (int i = 0; i < match.Length; i++) {
                if (input[ptr + i] != match[i]) {
                    throw new ParseException("Expected " + match + " but was not found when parsing: " + input.Substring(ptr, input.Length - ptr));
                }
            }

            ptr = ptr + match.Length;
            ptr = ConsumeWhiteSpace(ptr, input);
        }

        public static Color ParseColor(List<StyleVariable> variables, string propertyValue) {
            Color color;
            if (TryResolveVariable(variables, propertyValue, out color)) {
                return color;
            }

            return ParseColor(propertyValue);
        }

        public static Color ParseColor(string propertyValue) {
            Color color;
            if (!ColorUtility.TryParseHtmlString(propertyValue, out color)) {
                if (propertyValue.StartsWith("rgba")) {
                    int ptr = 0;
                    ConsumeString("rgba", propertyValue, ref ptr);
                    ConsumeString("(", propertyValue, ref ptr);
                    byte r = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(",", propertyValue, ref ptr);
                    byte g = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(",", propertyValue, ref ptr);
                    byte b = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(",", propertyValue, ref ptr);
                    byte a = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(")", propertyValue, ref ptr);
                    color = new Color32(r, g, b, a);
                }
                else if (propertyValue.StartsWith("rgb")) {
                    int ptr = 0;
                    ConsumeString("rgb", propertyValue, ref ptr);
                    ConsumeString("(", propertyValue, ref ptr);
                    byte r = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(",", propertyValue, ref ptr);
                    byte g = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(",", propertyValue, ref ptr);
                    byte b = (byte) ReadInt(propertyValue, ref ptr);
                    ConsumeString(")", propertyValue, ref ptr);
                    color = new Color32(r, g, b, 255);
                }
                else {
                    throw new ParseException("Unable to parse color from: " + propertyValue);
                }
            }

            return color;
        }

        public static UIMeasurement ParseMeasurement(List<StyleVariable> variables, string propertyValue) {
            UIMeasurement measurement;
            if (TryResolveVariable(variables, propertyValue, out measurement)) {
                return measurement;
            }

            int ptr = 0;
            float value = ParseFloat(propertyValue, ref ptr);
            ptr = ConsumeWhiteSpace(ptr, propertyValue);
            if (ptr == propertyValue.Length) {
                return new UIMeasurement(value, UIMeasurementUnit.Pixel);
            }

            UIMeasurementUnit unit = ParseMeasurementUnit(propertyValue, ref ptr);
            if (unit == UIMeasurementUnit.Unset) {
                throw new ParseException("Unknown measurement unit: " + propertyValue);
            }

            if (propertyValue.IndexOf('%') != -1) {
                value = value * 0.01f;
            }

            return new UIMeasurement(value, unit);
        }

        public static UIMeasurementUnit ParseMeasurementUnit(string input, ref int ptr) {
            ptr = ConsumeWhiteSpace(ptr, input);
            if (TryReadCharacters(input, "px", ref ptr)) {
                return UIMeasurementUnit.Pixel;
            }

            if (TryReadCharacters(input, "cnt", ref ptr)) {
                return UIMeasurementUnit.Content;
            }

            if (TryReadCharacters(input, "aw", ref ptr)) {
                return UIMeasurementUnit.AnchorWidth;
            }

            if (TryReadCharacters(input, "ah", ref ptr)) {
                return UIMeasurementUnit.AnchorHeight;
            }

            if (TryReadCharacters(input, "psz", ref ptr)) {
                return UIMeasurementUnit.ParentSize;
            }

            if (TryReadCharacters(input, "%", ref ptr)) {
                return UIMeasurementUnit.ParentSize;
            }

            if (TryReadCharacters(input, "pca", ref ptr)) {
                return UIMeasurementUnit.ParentContentArea;
            }

            if (TryReadCharacters(input, "%cnt", ref ptr)) {
                return UIMeasurementUnit.ParentContentArea;
            }

            if (TryReadCharacters(input, "em", ref ptr)) {
                return UIMeasurementUnit.Em;
            }

            if (TryReadCharacters(input, "vw", ref ptr)) {
                return UIMeasurementUnit.ViewportWidth;
            }

            if (TryReadCharacters(input, "vh", ref ptr)) {
                return UIMeasurementUnit.ViewportHeight;
            }

            return UIMeasurementUnit.Unset;
        }

        // 10, 10f, 10.0, -10 px, em, %, vw, vh
        public static UIFixedLength ParseFixedLength(List<StyleVariable> variables, string propertyValue) {
            UIFixedLength length;
            if (TryResolveVariable(variables, propertyValue, out length)) {
                return length;
            }

            int ptr = 0;
            float value = ParseFloat(propertyValue, ref ptr);
            ptr = ConsumeWhiteSpace(ptr, propertyValue);
            if (ptr == propertyValue.Length) {
                return new UIFixedLength(value, UIFixedUnit.Pixel);
            }

            UIFixedUnit unit = ParseFixedUnit(propertyValue, ref ptr);
            if (unit == UIFixedUnit.Unset) {
                throw new ParseException("Unknown fixed length unit: " + propertyValue);
            }

            if (unit == UIFixedUnit.Percent) {
                value = (float) Math.Round(value * 0.01f, 2);
            }

            return new UIFixedLength(value, unit);
        }

        public static UIFixedUnit ParseFixedUnit(string input, ref int ptr) {
            ptr = ConsumeWhiteSpace(ptr, input);
            if (TryReadCharacters(input, "px", ref ptr)) {
                return UIFixedUnit.Pixel;
            }

            if (TryReadCharacters(input, "%", ref ptr)) {
                return UIFixedUnit.Percent;
            }

            if (TryReadCharacters(input, "em", ref ptr)) {
                return UIFixedUnit.Em;
            }

            if (TryReadCharacters(input, "vw", ref ptr)) {
                return UIFixedUnit.ViewportWidth;
            }

            if (TryReadCharacters(input, "vh", ref ptr)) {
                return UIFixedUnit.ViewportHeight;
            }

            return UIFixedUnit.Unset;
        }

        public static FixedLengthRect ParseFixedLengthRect(List<StyleVariable> variables, string input) {
            int ptr = ConsumeWhiteSpace(0, input);

            int idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIFixedLength val0 = ParseFixedLength(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr == input.Length) {
                return new FixedLengthRect(val0, val0, val0, val0);
            }

            idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIFixedLength val1 = ParseFixedLength(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr == input.Length) {
                return new FixedLengthRect(val0, val1, val0, val1);
            }

            idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIFixedLength val2 = ParseFixedLength(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr == input.Length) {
                throw new ParseException("Fixed length rect requires 1, 2, or 4 arguments but 3 were provided");
            }

            idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIFixedLength val3 = ParseFixedLength(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr != input.Length) {
                throw new ParseException("Fixed length rect requires 1, 2, or 4 arguments but more were provided");
            }

            return new FixedLengthRect(val0, val1, val2, val3);
        }

        public static ContentBoxRect ParseMeasurementRect(List<StyleVariable> variables, string input) {
            int ptr = ConsumeWhiteSpace(0, input);

            int idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIMeasurement val0 = ParseMeasurement(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr == input.Length) {
                return new ContentBoxRect(val0, val0, val0, val0);
            }

            idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIMeasurement val1 = ParseMeasurement(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr == input.Length) {
                return new ContentBoxRect(val0, val1, val0, val1);
            }

            idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIMeasurement val2 = ParseMeasurement(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr == input.Length) {
                throw new ParseException("measurement length rect requires 1, 2, or 4 arguments but 3 were provided");
            }

            idx = FindCharacterOrStatementEnd(input, ',', ptr);
            UIMeasurement val3 = ParseMeasurement(variables, input.Substring(ptr, idx - ptr));
            ptr = ConsumeWhiteSpace(idx, input);
            if (ptr != input.Length) {
                throw new ParseException("measurement length rect requires 1, 2, or 4 arguments but more were provided");
            }

            return new ContentBoxRect(val0, val1, val2, val3);
        }

        public static int FindCharacterOrStatementEnd(string input, char targetChar, int ptr) {
            while (ptr < input.Length) {
                char current = input[ptr];
                ptr++;
                if (current == targetChar || current == ';' || current == '\n') {
                    break;
                }
            }

            return ptr;
        }

        public static float ParseFloat(string input, ref int ptr) {
            bool foundDot = false;
            int startIndex = ptr;

            if (input[ptr] != '-' && !char.IsDigit(input[ptr])) {
                throw new ParseException("Unable to parse float from: " + input.Substring(startIndex));
            }

            // 1
            // 1.4
            // 1.4f

            if (input[ptr] == '-') {
                ptr++;
            }

            while (ptr < input.Length && (char.IsDigit(input[ptr]) || (!foundDot && input[ptr] == '.'))) {
                if (input[ptr] == '.') {
                    foundDot = true;
                }

                ptr++;
            }

            if (ptr < input.Length && input[ptr] == 'f') {
                ptr++;
            }

            ptr = ConsumeWhiteSpace(ptr, input);
            return float.Parse(input.Substring(startIndex, ptr - startIndex));
        }

        public static float ParseFloat(List<StyleVariable> variables, string propertyValue) {
            float val;
            if (TryResolveVariable(variables, propertyValue, out val)) {
                return val;
            }

            propertyValue = propertyValue.Trim();
            if (propertyValue.EndsWith("f")) {
                propertyValue = propertyValue.Substring(0, propertyValue.Length - 1);
            }

            return float.Parse(propertyValue);
        }

        private static bool TryResolveVariable<T>(List<StyleVariable> variables, string propertyValue, out T retn) {
            if (propertyValue[0] != '@') {
                retn = default(T);
                return false;
            }

            if (variables != null) {
                for (int i = 0; i < variables.Count; i++) {
                    if (variables[i].name == propertyValue) {
                        retn = variables[i].GetValue<T>();
                        return true;
                    }
                }
            }

            throw new ParseException("Unable to resolve local variable: " + propertyValue);
        }

        public static int ParseInt(List<StyleVariable> variables, string propertyValue) {
            int val;
            if (TryResolveVariable(variables, propertyValue, out val)) {
                return val;
            }

            return int.Parse(propertyValue);
        }

        public static CrossAxisAlignment ParseCrossAxisAlignment(List<StyleVariable> variables, string propertyValue) {
            CrossAxisAlignment alignment;
            if (TryResolveVariable(variables, propertyValue, out alignment)) {
                return alignment;
            }

            switch (propertyValue.ToLower()) {
                case "default":
                case "start":
                    return CrossAxisAlignment.Start;
                case "end":
                    return CrossAxisAlignment.End;
                case "center":
                    return CrossAxisAlignment.Center;
                case "stretch":
                    return CrossAxisAlignment.Stretch;
                default:
                    throw new ParseException("Unknown value for CrossAxisAlignment: " + propertyValue);
            }
        }

        public static MainAxisAlignment ParseMainAxisAlignment(List<StyleVariable> variables, string propertyValue) {
            MainAxisAlignment alignment;
            if (TryResolveVariable(variables, propertyValue, out alignment)) {
                return alignment;
            }

            switch (propertyValue.ToLower()) {
                case "default":
                case "start":
                    return MainAxisAlignment.Start;
                case "end":
                    return MainAxisAlignment.End;
                case "center":
                    return MainAxisAlignment.Center;
                case "spacearound":
                case "space-around":
                    return MainAxisAlignment.SpaceAround;
                case "spacebetween":
                case "space-between":
                    return MainAxisAlignment.SpaceBetween;
                default:
                    throw new ParseException($"Unknown value for {nameof(MainAxisAlignment)}: {propertyValue}");
            }
        }

        public static LayoutWrap ParseLayoutWrap(List<StyleVariable> variables, string propertyValue) {
            LayoutWrap wrap;
            if (TryResolveVariable(variables, propertyValue, out wrap)) {
                return wrap;
            }

            switch (propertyValue.ToLower()) {
                case "default":
                case "none":
                    return LayoutWrap.None;
                case "wrap":
                    return LayoutWrap.Wrap;
                case "reverse":
                    return LayoutWrap.Reverse;
                case "wrap-reverse":
                    return LayoutWrap.WrapReverse;
                default:
                    throw new ParseException($"Unknown value for {nameof(LayoutWrap)}: {propertyValue}");
            }
        }

        public static TransformBehavior ParseTransformBehavior(List<StyleVariable> variables, string propertyValue) {
            TransformBehavior behavior;
            if (TryResolveVariable(variables, propertyValue, out behavior)) {
                return behavior;
            }

            switch (propertyValue.ToLower()) {
                case "none":
                case "default":
                    return TransformBehavior.Default;
                case "layoutoffset":
                    return TransformBehavior.LayoutOffset;
                case "anchorminoffset":
                    return TransformBehavior.AnchorMinOffset;
                case "anchormaxoffset":
                    return TransformBehavior.AnchorMaxOffset;
                default:
                    throw new ParseException($"Unknown value for {nameof(TransformBehavior)}: {propertyValue}");
            }
        }

        public static LayoutType ParseLayoutType(List<StyleVariable> variables, string propertyValue) {
            LayoutType layoutType;
            if (TryResolveVariable(variables, propertyValue, out layoutType)) {
                return layoutType;
            }

            switch (propertyValue.ToLower()) {
                case "default":
                case "flex":
                    return LayoutType.Flex;
                case "flow":
                    return LayoutType.Flow;
                case "radial":
                    return LayoutType.Radial;
                case "grid":
                    return LayoutType.Grid;
                default:
                    throw new ParseException($"Unknown value for {nameof(LayoutType)}: {propertyValue}");
            }
        }

        public static LayoutBehavior ParseLayoutBehavior(List<StyleVariable> variables, string propertyValue) {
            LayoutBehavior layoutType;
            if (TryResolveVariable(variables, propertyValue, out layoutType)) {
                return layoutType;
            }

            switch (propertyValue.ToLower()) {
                case "default":
                case "normal":
                    return LayoutBehavior.Normal;
                case "none":
                case "ignored":
                case "ignore":
                    return LayoutBehavior.Ignored;
                default:
                    throw new ParseException($"Unknown value for {nameof(LayoutBehavior)}: {propertyValue}");
            }
        }

        public static RenderLayer ParseRenderLayer(List<StyleVariable> variables, string propertyValue) {
            RenderLayer layer;
            if (TryResolveVariable(variables, propertyValue, out layer)) {
                return layer;
            }

            switch (propertyValue.ToLower()) {
                case "default":
                case "normal":
                    return RenderLayer.Default;
                case "parent":
                    return RenderLayer.Parent;
                case "screen":
                    return RenderLayer.Screen;
                case "modal":
                    return RenderLayer.Modal;
                case "view":
                    return RenderLayer.View;
                case "template":
                    return RenderLayer.Template;
                default:
                    throw new ParseException($"Unknown value for {nameof(RenderLayer)}: {propertyValue}");
            }
        }

        public static TextUtil.FontStyle ParseFontStyle(List<StyleVariable> variables, string propertyValue) {
            TextUtil.FontStyle fontStyle;
            if (TryResolveVariable(variables, propertyValue, out fontStyle)) {
                return fontStyle;
            }

            TextUtil.FontStyle style = TextUtil.FontStyle.Normal;

            if (propertyValue.Contains("bold")) {
                style |= TextUtil.FontStyle.Bold;
            }

            if (propertyValue.Contains("italic")) {
                style |= TextUtil.FontStyle.Italic;
            }

            if (propertyValue.Contains("highlight")) {
                style |= TextUtil.FontStyle.Highlight;
            }

            if (propertyValue.Contains("smallcaps")) {
                style |= TextUtil.FontStyle.SmallCaps;
            }

            if (propertyValue.Contains("superscript")) {
                style |= TextUtil.FontStyle.Superscript;
            }

            if (propertyValue.Contains("subscript")) {
                style |= TextUtil.FontStyle.Subscript;
            }

            if (propertyValue.Contains("underline")) {
                style |= TextUtil.FontStyle.Underline;
            }

            if (propertyValue.Contains("strikethrough")) {
                style |= TextUtil.FontStyle.StrikeThrough;
            }

            if ((style & TextUtil.FontStyle.Superscript) != 0 && (style & TextUtil.FontStyle.Subscript) != 0) {
                throw new ParseException("Font style cannot be both superscript and subscript");
            }

            return style;
        }

        public static TMP_FontAsset ParseFont(List<StyleVariable> variables, string propertyValue) {
            TMP_FontAsset font;
            if (TryResolveVariable(variables, propertyValue, out font)) {
                return font;
            }

            propertyValue = propertyValue.ToLower();
            if (propertyValue == "unset" || propertyValue == "default" || propertyValue == "null") {
                return null;
            }

            return Resources.Load<TMP_FontAsset>(propertyValue);
        }

        public static Overflow ParseOverflow(List<StyleVariable> variables, string propertyValue) {
            Overflow val;
            if (TryResolveVariable(variables, propertyValue, out val)) {
                return val;
            }

            switch (propertyValue.ToLower()) {
                case "none":
                    return Overflow.None;
                case "scroll":
                    return Overflow.Scroll;
                case "scrollandhide":
                    return Overflow.ScrollAndAutoHide;
                case "hidden":
                    return Overflow.Hidden;
                default:
                    throw new ParseException("Unknown value for " + nameof(Overflow) + ": " + propertyValue);
            }
        }

        public static LayoutDirection ParseLayoutDirection(List<StyleVariable> variables, string propertyValue) {
            LayoutDirection direction;
            if (TryResolveVariable(variables, propertyValue, out direction)) {
                return direction;
            }

            switch (propertyValue.ToLower()) {
                case "row":
                case "vertical":
                case "layoutdirection.row":
                    return LayoutDirection.Row;
                case "col":
                case "column":
                case "horizontal":
                case "layoutdirection.column":
                    return LayoutDirection.Column;
                default:
                    throw new ParseException("Unknown value for " + nameof(LayoutDirection) + ": " + propertyValue);
            }
        }

        public static GridLayoutDensity ParseDensity(List<StyleVariable> variables, string propertyValue) {
            GridLayoutDensity val;
            if (TryResolveVariable(variables, propertyValue, out val)) {
                return val;
            }

            switch (propertyValue.ToLower()) {
                case "dense":
                    return GridLayoutDensity.Dense;
                case "sparse":
                    return GridLayoutDensity.Sparse;
                default:
                    throw new ParseException("Unknown value for " + nameof(GridLayoutDensity) + ": " + propertyValue);
            }
        }

        public static GridTrackSize ParseGridTrackSize(List<StyleVariable> variables, string propertyValue) {
            GridTrackSize val;
            if (TryResolveVariable(variables, propertyValue, out val)) {
                return val;
            }

            int ptr = 0;
            return ParseGridTrackSize(propertyValue, ref ptr);
        }

        public static GridTrackSize ParseGridTrackSize(string input, ref int ptr) {
            float value = ParseFloat(input, ref ptr);
            ptr = ConsumeWhiteSpace(ptr, input);
            if (ptr == input.Length) {
                return new GridTrackSize(value, GridTemplateUnit.Pixel);
            }

            GridTemplateUnit unit = ParseGridTemplateUnit(input, ref ptr);
            if (unit == GridTemplateUnit.Unset) {
                throw new ParseException("Unknown grid track unit: " + input);
            }

            ptr = ConsumeWhiteSpace(ptr, input);
            return new GridTrackSize(value, unit);
        }

        public static GridTemplateUnit ParseGridTemplateUnit(string input, ref int ptr) {
            ptr = ConsumeWhiteSpace(ptr, input);
            if (TryReadCharacters(input, "px", ref ptr)) {
                return GridTemplateUnit.Pixel;
            }

            if (TryReadCharacters(input, "psz", ref ptr)) {
                return GridTemplateUnit.Container;
            }

            if (TryReadCharacters(input, "pca", ref ptr)) {
                return GridTemplateUnit.ContainerContentArea;
            }

            if (TryReadCharacters(input, "em", ref ptr)) {
                return GridTemplateUnit.Em;
            }

            if (TryReadCharacters(input, "fr", ref ptr)) {
                return GridTemplateUnit.Flex;
            }

            if (TryReadCharacters(input, "mn", ref ptr)) {
                return GridTemplateUnit.MinContent;
            }

            if (TryReadCharacters(input, "mx", ref ptr)) {
                return GridTemplateUnit.MaxContent;
            }

            if (TryReadCharacters(input, "vw", ref ptr)) {
                return GridTemplateUnit.ViewportWidth;
            }

            if (TryReadCharacters(input, "vh", ref ptr)) {
                return GridTemplateUnit.ViewportHeight;
            }

            return GridTemplateUnit.Unset;
        }

        public static IReadOnlyList<GridTrackSize> ParseGridTemplate(List<StyleVariable> variables, string propertyValue) {
            IReadOnlyList<GridTrackSize> readonlyVal;
            if (TryResolveVariable(variables, propertyValue, out readonlyVal)) {
                return readonlyVal;
            }

            List<GridTrackSize> val = new List<GridTrackSize>();
            int ptr = 0;
            while (ptr != propertyValue.Length) {
                val.Add(ParseGridTrackSize(propertyValue, ref ptr));
                if (ptr == propertyValue.Length) {
                    break;
                }

                ConsumeString(",", propertyValue, ref ptr);
            }

            return val;
        }

        public static AnchorTarget ParseAnchorTarget(List<StyleVariable> variables, string propertyValue) {
            switch (propertyValue.ToLower()) {
                case "parent":
                    return AnchorTarget.Parent;
                case "parentcontentarea":
                    return AnchorTarget.ParentContentArea;
                case "screen":
                    return AnchorTarget.Screen;
                case "viewport":
                    return AnchorTarget.Viewport;
                default:
                    throw new ParseException($"Unknown value for {nameof(AnchorTarget)}: {propertyValue}");
            }
        }

    }

}