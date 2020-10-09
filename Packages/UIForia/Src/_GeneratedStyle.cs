using UIForia.Util;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UIForia.Rendering;
using UIForia;
using UIForia.Layout;
using UIForia.UIInput;

namespace UIForia.Style {

    public static partial class PropertyParsers {

        static PropertyParsers() {

            s_ParserTable = new IStylePropertyParser[13];
            s_ParserTable[0] = new UIForia.Style.ColorParser();
            s_ParserTable[1] = new UIForia.Style.EnumParser<UIForia.Rendering.BackgroundFit>();
            s_ParserTable[2] = new UIForia.Style.StringParser();
            s_ParserTable[3] = new UIForia.Style.FixedLengthParser();
            s_ParserTable[4] = new UIForia.Style.FloatParser();
            s_ParserTable[5] = new UIForia.Style.EnumParser<UIForia.Layout.ClipBehavior>();
            s_ParserTable[6] = new UIForia.Style.EnumParser<UIForia.Rendering.ClipBounds>();
            s_ParserTable[7] = new UIForia.Style.CursorStyleParser();
            s_ParserTable[8] = new UIForia.Style.MeasurementParser();
            s_ParserTable[9] = new UIForia.Style.EnumParser<UIForia.Rendering.Overflow>();
            s_ParserTable[10] = new UIForia.Style.StringParser();
            s_ParserTable[11] = new UIForia.Style.EnumParser<UIForia.UIInput.PointerEvents>();
            s_ParserTable[12] = new UIForia.Style.EnumParser<UIForia.Rendering.Visibility>();
          
            s_parseEntries = new PropertyParseEntry[46];
            s_parseEntries[0] = new PropertyParseEntry("BackgroundColor",  PropertyId.BackgroundColor, s_ParserTable[0]);
            s_parseEntries[1] = new PropertyParseEntry("BackgroundFit",  PropertyId.BackgroundFit, s_ParserTable[1]);
            s_parseEntries[2] = new PropertyParseEntry("BackgroundImage",  PropertyId.BackgroundImage, s_ParserTable[2]);
            s_parseEntries[3] = new PropertyParseEntry("BackgroundImageOffsetX",  PropertyId.BackgroundImageOffsetX, s_ParserTable[3]);
            s_parseEntries[4] = new PropertyParseEntry("BackgroundImageOffsetY",  PropertyId.BackgroundImageOffsetY, s_ParserTable[3]);
            s_parseEntries[5] = new PropertyParseEntry("BackgroundImageRotation",  PropertyId.BackgroundImageRotation, s_ParserTable[4]);
            s_parseEntries[6] = new PropertyParseEntry("BackgroundImageScaleX",  PropertyId.BackgroundImageScaleX, s_ParserTable[4]);
            s_parseEntries[7] = new PropertyParseEntry("BackgroundImageScaleY",  PropertyId.BackgroundImageScaleY, s_ParserTable[4]);
            s_parseEntries[8] = new PropertyParseEntry("BackgroundImageTileX",  PropertyId.BackgroundImageTileX, s_ParserTable[4]);
            s_parseEntries[9] = new PropertyParseEntry("BackgroundImageTileY",  PropertyId.BackgroundImageTileY, s_ParserTable[4]);
            s_parseEntries[10] = new PropertyParseEntry("BackgroundTint",  PropertyId.BackgroundTint, s_ParserTable[0]);
            s_parseEntries[11] = new PropertyParseEntry("BorderBottom",  PropertyId.BorderBottom, s_ParserTable[3]);
            s_parseEntries[12] = new PropertyParseEntry("BorderColorBottom",  PropertyId.BorderColorBottom, s_ParserTable[0]);
            s_parseEntries[13] = new PropertyParseEntry("BorderColorLeft",  PropertyId.BorderColorLeft, s_ParserTable[0]);
            s_parseEntries[14] = new PropertyParseEntry("BorderColorRight",  PropertyId.BorderColorRight, s_ParserTable[0]);
            s_parseEntries[15] = new PropertyParseEntry("BorderColorTop",  PropertyId.BorderColorTop, s_ParserTable[0]);
            s_parseEntries[16] = new PropertyParseEntry("BorderLeft",  PropertyId.BorderLeft, s_ParserTable[3]);
            s_parseEntries[17] = new PropertyParseEntry("BorderRadiusBottomLeft",  PropertyId.BorderRadiusBottomLeft, s_ParserTable[3]);
            s_parseEntries[18] = new PropertyParseEntry("BorderRadiusBottomRight",  PropertyId.BorderRadiusBottomRight, s_ParserTable[3]);
            s_parseEntries[19] = new PropertyParseEntry("BorderRadiusTopLeft",  PropertyId.BorderRadiusTopLeft, s_ParserTable[3]);
            s_parseEntries[20] = new PropertyParseEntry("BorderRadiusTopRight",  PropertyId.BorderRadiusTopRight, s_ParserTable[3]);
            s_parseEntries[21] = new PropertyParseEntry("BorderRight",  PropertyId.BorderRight, s_ParserTable[3]);
            s_parseEntries[22] = new PropertyParseEntry("BorderTop",  PropertyId.BorderTop, s_ParserTable[3]);
            s_parseEntries[23] = new PropertyParseEntry("ClipBehavior",  PropertyId.ClipBehavior, s_ParserTable[5]);
            s_parseEntries[24] = new PropertyParseEntry("ClipBounds",  PropertyId.ClipBounds, s_ParserTable[6]);
            s_parseEntries[25] = new PropertyParseEntry("Cursor",  PropertyId.Cursor, s_ParserTable[7]);
            s_parseEntries[26] = new PropertyParseEntry("MarginBottom",  PropertyId.MarginBottom, s_ParserTable[3]);
            s_parseEntries[27] = new PropertyParseEntry("MarginLeft",  PropertyId.MarginLeft, s_ParserTable[3]);
            s_parseEntries[28] = new PropertyParseEntry("MarginRight",  PropertyId.MarginRight, s_ParserTable[3]);
            s_parseEntries[29] = new PropertyParseEntry("MarginTop",  PropertyId.MarginTop, s_ParserTable[3]);
            s_parseEntries[30] = new PropertyParseEntry("MaxHeight",  PropertyId.MaxHeight, s_ParserTable[8]);
            s_parseEntries[31] = new PropertyParseEntry("MaxWidth",  PropertyId.MaxWidth, s_ParserTable[8]);
            s_parseEntries[32] = new PropertyParseEntry("MinHeight",  PropertyId.MinHeight, s_ParserTable[8]);
            s_parseEntries[33] = new PropertyParseEntry("MinWidth",  PropertyId.MinWidth, s_ParserTable[8]);
            s_parseEntries[34] = new PropertyParseEntry("Opacity",  PropertyId.Opacity, s_ParserTable[4]);
            s_parseEntries[35] = new PropertyParseEntry("OverflowX",  PropertyId.OverflowX, s_ParserTable[9]);
            s_parseEntries[36] = new PropertyParseEntry("OverflowY",  PropertyId.OverflowY, s_ParserTable[9]);
            s_parseEntries[37] = new PropertyParseEntry("PaddingBottom",  PropertyId.PaddingBottom, s_ParserTable[3]);
            s_parseEntries[38] = new PropertyParseEntry("PaddingLeft",  PropertyId.PaddingLeft, s_ParserTable[3]);
            s_parseEntries[39] = new PropertyParseEntry("PaddingRight",  PropertyId.PaddingRight, s_ParserTable[3]);
            s_parseEntries[40] = new PropertyParseEntry("PaddingTop",  PropertyId.PaddingTop, s_ParserTable[3]);
            s_parseEntries[41] = new PropertyParseEntry("Painter",  PropertyId.Painter, s_ParserTable[10]);
            s_parseEntries[42] = new PropertyParseEntry("PointerEvents",  PropertyId.PointerEvents, s_ParserTable[11]);
            s_parseEntries[43] = new PropertyParseEntry("PreferredHeight",  PropertyId.PreferredHeight, s_ParserTable[8]);
            s_parseEntries[44] = new PropertyParseEntry("PreferredWidth",  PropertyId.PreferredWidth, s_ParserTable[8]);
            s_parseEntries[45] = new PropertyParseEntry("Visibility",  PropertyId.Visibility, s_ParserTable[12]);

            s_PropertyNames = new string[] {
                "BackgroundColor",
                "BackgroundFit",
                "BackgroundImage",
                "BackgroundImageOffsetX",
                "BackgroundImageOffsetY",
                "BackgroundImageRotation",
                "BackgroundImageScaleX",
                "BackgroundImageScaleY",
                "BackgroundImageTileX",
                "BackgroundImageTileY",
                "BackgroundTint",
                "BorderBottom",
                "BorderColorBottom",
                "BorderColorLeft",
                "BorderColorRight",
                "BorderColorTop",
                "BorderLeft",
                "BorderRadiusBottomLeft",
                "BorderRadiusBottomRight",
                "BorderRadiusTopLeft",
                "BorderRadiusTopRight",
                "BorderRight",
                "BorderTop",
                "ClipBehavior",
                "ClipBounds",
                "Cursor",
                "MarginBottom",
                "MarginLeft",
                "MarginRight",
                "MarginTop",
                "MaxHeight",
                "MaxWidth",
                "MinHeight",
                "MinWidth",
                "Opacity",
                "OverflowX",
                "OverflowY",
                "PaddingBottom",
                "PaddingLeft",
                "PaddingRight",
                "PaddingTop",
                "Painter",
                "PointerEvents",
                "PreferredHeight",
                "PreferredWidth",
                "Visibility"
            };

            s_ShorthandNames = new string[] {
                "MaxSize",
                "MinSize",
                "PreferredSize"
            };
            s_ShorthandEntries = new ShorthandEntry[3];
            s_ShorthandEntries[0] = new ShorthandEntry("MaxSize",  0, new UIForia.Style.MaxSizeParser());
            s_ShorthandEntries[1] = new ShorthandEntry("MinSize",  1, new UIForia.Style.MinSizeParser());
            s_ShorthandEntries[2] = new ShorthandEntry("PreferredSize",  2, new UIForia.Style.PreferredSizeParser());

        }
    }

    public partial struct PropertyId {
    
        public static readonly PropertyId BackgroundColor = new PropertyId(0, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundFit = new PropertyId(1, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId BackgroundImage = new PropertyId(2, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId BackgroundImageOffsetX = new PropertyId(3, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundImageOffsetY = new PropertyId(4, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundImageRotation = new PropertyId(5, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundImageScaleX = new PropertyId(6, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundImageScaleY = new PropertyId(7, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundImageTileX = new PropertyId(8, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundImageTileY = new PropertyId(9, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BackgroundTint = new PropertyId(10, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderBottom = new PropertyId(11, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderColorBottom = new PropertyId(12, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderColorLeft = new PropertyId(13, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderColorRight = new PropertyId(14, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderColorTop = new PropertyId(15, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderLeft = new PropertyId(16, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderRadiusBottomLeft = new PropertyId(17, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderRadiusBottomRight = new PropertyId(18, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderRadiusTopLeft = new PropertyId(19, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderRadiusTopRight = new PropertyId(20, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderRight = new PropertyId(21, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId BorderTop = new PropertyId(22, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId ClipBehavior = new PropertyId(23, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId ClipBounds = new PropertyId(24, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId Cursor = new PropertyId(25, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId MarginBottom = new PropertyId(26, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MarginLeft = new PropertyId(27, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MarginRight = new PropertyId(28, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MarginTop = new PropertyId(29, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MaxHeight = new PropertyId(30, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MaxWidth = new PropertyId(31, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MinHeight = new PropertyId(32, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId MinWidth = new PropertyId(33, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId Opacity = new PropertyId(34, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Inherited | PropertyTypeFlags.Animated);
        public static readonly PropertyId OverflowX = new PropertyId(35, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId OverflowY = new PropertyId(36, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId PaddingBottom = new PropertyId(37, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId PaddingLeft = new PropertyId(38, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId PaddingRight = new PropertyId(39, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId PaddingTop = new PropertyId(40, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId Painter = new PropertyId(41, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId PointerEvents = new PropertyId(42, PropertyTypeFlags.BuiltIn);
        public static readonly PropertyId PreferredHeight = new PropertyId(43, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId PreferredWidth = new PropertyId(44, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Animated);
        public static readonly PropertyId Visibility = new PropertyId(45, PropertyTypeFlags.BuiltIn | PropertyTypeFlags.Inherited);

    } 

    public partial struct StyleProperty2 {

        public static StyleProperty2 BackgroundColor(UnityEngine.Color32 value) {
            return new StyleProperty2(PropertyId.BackgroundColor, ColorUtil.ColorToInt(value));
        }

        public static StyleProperty2 BackgroundFit(UIForia.Rendering.BackgroundFit value) {
            return new StyleProperty2(PropertyId.BackgroundFit, (int)value);
        }

        public static StyleProperty2 BackgroundImage(UnityEngine.Texture2D value) {
            return new StyleProperty2(PropertyId.BackgroundImage, GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject());
        }

        public static StyleProperty2 BackgroundImageOffsetX(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BackgroundImageOffsetX, value.value, (int)value.unit);
        }

        public static StyleProperty2 BackgroundImageOffsetY(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BackgroundImageOffsetY, value.value, (int)value.unit);
        }

        public static StyleProperty2 BackgroundImageRotation(float value) {
            return new StyleProperty2(PropertyId.BackgroundImageRotation, value);
        }

        public static StyleProperty2 BackgroundImageScaleX(float value) {
            return new StyleProperty2(PropertyId.BackgroundImageScaleX, value);
        }

        public static StyleProperty2 BackgroundImageScaleY(float value) {
            return new StyleProperty2(PropertyId.BackgroundImageScaleY, value);
        }

        public static StyleProperty2 BackgroundImageTileX(float value) {
            return new StyleProperty2(PropertyId.BackgroundImageTileX, value);
        }

        public static StyleProperty2 BackgroundImageTileY(float value) {
            return new StyleProperty2(PropertyId.BackgroundImageTileY, value);
        }

        public static StyleProperty2 BackgroundTint(UnityEngine.Color32 value) {
            return new StyleProperty2(PropertyId.BackgroundTint, ColorUtil.ColorToInt(value));
        }

        public static StyleProperty2 BorderBottom(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderBottom, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderColorBottom(UnityEngine.Color32 value) {
            return new StyleProperty2(PropertyId.BorderColorBottom, ColorUtil.ColorToInt(value));
        }

        public static StyleProperty2 BorderColorLeft(UnityEngine.Color32 value) {
            return new StyleProperty2(PropertyId.BorderColorLeft, ColorUtil.ColorToInt(value));
        }

        public static StyleProperty2 BorderColorRight(UnityEngine.Color32 value) {
            return new StyleProperty2(PropertyId.BorderColorRight, ColorUtil.ColorToInt(value));
        }

        public static StyleProperty2 BorderColorTop(UnityEngine.Color32 value) {
            return new StyleProperty2(PropertyId.BorderColorTop, ColorUtil.ColorToInt(value));
        }

        public static StyleProperty2 BorderLeft(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderLeft, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderRadiusBottomLeft(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderRadiusBottomLeft, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderRadiusBottomRight(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderRadiusBottomRight, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderRadiusTopLeft(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderRadiusTopLeft, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderRadiusTopRight(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderRadiusTopRight, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderRight(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderRight, value.value, (int)value.unit);
        }

        public static StyleProperty2 BorderTop(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.BorderTop, value.value, (int)value.unit);
        }

        public static StyleProperty2 ClipBehavior(UIForia.Layout.ClipBehavior value) {
            return new StyleProperty2(PropertyId.ClipBehavior, (int)value);
        }

        public static StyleProperty2 ClipBounds(UIForia.Rendering.ClipBounds value) {
            return new StyleProperty2(PropertyId.ClipBounds, (int)value);
        }

        public static StyleProperty2 Cursor(UIForia.Rendering.CursorStyle value) {
            return new StyleProperty2(PropertyId.Cursor, GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject());
        }

        public static StyleProperty2 MarginBottom(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.MarginBottom, value.value, (int)value.unit);
        }

        public static StyleProperty2 MarginLeft(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.MarginLeft, value.value, (int)value.unit);
        }

        public static StyleProperty2 MarginRight(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.MarginRight, value.value, (int)value.unit);
        }

        public static StyleProperty2 MarginTop(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.MarginTop, value.value, (int)value.unit);
        }

        public static StyleProperty2 MaxHeight(UIForia.Rendering.UIMeasurement value) {
            return new StyleProperty2(PropertyId.MaxHeight, value.value, (int)value.unit);
        }

        public static StyleProperty2 MaxWidth(UIForia.Rendering.UIMeasurement value) {
            return new StyleProperty2(PropertyId.MaxWidth, value.value, (int)value.unit);
        }

        public static StyleProperty2 MinHeight(UIForia.Rendering.UIMeasurement value) {
            return new StyleProperty2(PropertyId.MinHeight, value.value, (int)value.unit);
        }

        public static StyleProperty2 MinWidth(UIForia.Rendering.UIMeasurement value) {
            return new StyleProperty2(PropertyId.MinWidth, value.value, (int)value.unit);
        }

        public static StyleProperty2 Opacity(float value) {
            return new StyleProperty2(PropertyId.Opacity, value);
        }

        public static StyleProperty2 OverflowX(UIForia.Rendering.Overflow value) {
            return new StyleProperty2(PropertyId.OverflowX, (int)value);
        }

        public static StyleProperty2 OverflowY(UIForia.Rendering.Overflow value) {
            return new StyleProperty2(PropertyId.OverflowY, (int)value);
        }

        public static StyleProperty2 PaddingBottom(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.PaddingBottom, value.value, (int)value.unit);
        }

        public static StyleProperty2 PaddingLeft(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.PaddingLeft, value.value, (int)value.unit);
        }

        public static StyleProperty2 PaddingRight(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.PaddingRight, value.value, (int)value.unit);
        }

        public static StyleProperty2 PaddingTop(UIForia.UIFixedLength value) {
            return new StyleProperty2(PropertyId.PaddingTop, value.value, (int)value.unit);
        }

        public static StyleProperty2 Painter(string value) {
            return new StyleProperty2(PropertyId.Painter, GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject());
        }

        public static StyleProperty2 PointerEvents(UIForia.UIInput.PointerEvents value) {
            return new StyleProperty2(PropertyId.PointerEvents, (int)value);
        }

        public static StyleProperty2 PreferredHeight(UIForia.Rendering.UIMeasurement value) {
            return new StyleProperty2(PropertyId.PreferredHeight, value.value, (int)value.unit);
        }

        public static StyleProperty2 PreferredWidth(UIForia.Rendering.UIMeasurement value) {
            return new StyleProperty2(PropertyId.PreferredWidth, value.value, (int)value.unit);
        }

        public static StyleProperty2 Visibility(UIForia.Rendering.Visibility value) {
            return new StyleProperty2(PropertyId.Visibility, (int)value);
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UnityEngine.Color32 value) {
            switch(propertyId.id) {
                case 393216: // BackgroundColor
                case 393226: // BackgroundTint
                case 393228: // BorderColorBottom
                case 393229: // BorderColorLeft
                case 393230: // BorderColorRight
                case 393231: // BorderColorTop
                    return new StyleProperty2(propertyId, ColorUtil.ColorToInt(value));
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type Color32");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Rendering.BackgroundFit value) {
            switch(propertyId.id) {
                case 262145: // BackgroundFit
                    return new StyleProperty2(propertyId, (int)value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type BackgroundFit");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UnityEngine.Texture2D value) {
            switch(propertyId.id) {
                case 262146: // BackgroundImage
                    return new StyleProperty2(propertyId, GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject());
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type Texture2D");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.UIFixedLength value) {
            switch(propertyId.id) {
                case 393219: // BackgroundImageOffsetX
                case 393220: // BackgroundImageOffsetY
                case 393227: // BorderBottom
                case 393232: // BorderLeft
                case 393233: // BorderRadiusBottomLeft
                case 393234: // BorderRadiusBottomRight
                case 393235: // BorderRadiusTopLeft
                case 393236: // BorderRadiusTopRight
                case 393237: // BorderRight
                case 393238: // BorderTop
                case 393242: // MarginBottom
                case 393243: // MarginLeft
                case 393244: // MarginRight
                case 393245: // MarginTop
                case 393253: // PaddingBottom
                case 393254: // PaddingLeft
                case 393255: // PaddingRight
                case 393256: // PaddingTop
                    return new StyleProperty2(propertyId, value.value, (int)value.unit);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type UIFixedLength");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, float value) {
            switch(propertyId.id) {
                case 393221: // BackgroundImageRotation
                case 393222: // BackgroundImageScaleX
                case 393223: // BackgroundImageScaleY
                case 393224: // BackgroundImageTileX
                case 393225: // BackgroundImageTileY
                case 458786: // Opacity
                    return new StyleProperty2(propertyId, value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type Single");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Layout.ClipBehavior value) {
            switch(propertyId.id) {
                case 262167: // ClipBehavior
                    return new StyleProperty2(propertyId, (int)value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type ClipBehavior");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Rendering.ClipBounds value) {
            switch(propertyId.id) {
                case 262168: // ClipBounds
                    return new StyleProperty2(propertyId, (int)value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type ClipBounds");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Rendering.CursorStyle value) {
            switch(propertyId.id) {
                case 262169: // Cursor
                    return new StyleProperty2(propertyId, GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject());
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type CursorStyle");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Rendering.UIMeasurement value) {
            switch(propertyId.id) {
                case 393246: // MaxHeight
                case 393247: // MaxWidth
                case 393248: // MinHeight
                case 393249: // MinWidth
                case 393259: // PreferredHeight
                case 393260: // PreferredWidth
                    return new StyleProperty2(propertyId, value.value, (int)value.unit);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type UIMeasurement");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Rendering.Overflow value) {
            switch(propertyId.id) {
                case 262179: // OverflowX
                case 262180: // OverflowY
                    return new StyleProperty2(propertyId, (int)value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type Overflow");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, string value) {
            switch(propertyId.id) {
                case 262185: // Painter
                    return new StyleProperty2(propertyId, GCHandle.Alloc(value, GCHandleType.Pinned).AddrOfPinnedObject());
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type String");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.UIInput.PointerEvents value) {
            switch(propertyId.id) {
                case 262186: // PointerEvents
                    return new StyleProperty2(propertyId, (int)value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type PointerEvents");
            }
        }

        public static StyleProperty2 FromValue(PropertyId propertyId, UIForia.Rendering.Visibility value) {
            switch(propertyId.id) {
                case 327725: // Visibility
                    return new StyleProperty2(propertyId, (int)value);
                default:
                    throw new Exception($"Tried to create a {nameof(StyleProperty2)} from value but the given propertyId `{propertyId}` is not compatible with the value type Visibility");
            }
        }

        public UnityEngine.Color32 AsColor {
            get {
                return ColorUtil.ColorFromInt(int0);
            }
        }

        public UIForia.Rendering.BackgroundFit AsBackgroundFit {
            get {
                return (UIForia.Rendering.BackgroundFit)int0;
            }
        }

        public UnityEngine.Texture2D AsTexture2D {
            get {
                return (UnityEngine.Texture2D)GCHandle.FromIntPtr(ptr).Target;
            }
        }

        public UIForia.UIFixedLength AsUIFixedLength {
            get {
                return new UIFixedLength(float0, (UIFixedUnit)int1);
            }
        }

        public float AsFloat {
            get {
                return float0;
            }
        }

        public UIForia.Layout.ClipBehavior AsClipBehavior {
            get {
                return (UIForia.Layout.ClipBehavior)int0;
            }
        }

        public UIForia.Rendering.ClipBounds AsClipBounds {
            get {
                return (UIForia.Rendering.ClipBounds)int0;
            }
        }

        public UIForia.Rendering.CursorStyle AsCursorStyle {
            get {
                return (UIForia.Rendering.CursorStyle)GCHandle.FromIntPtr(ptr).Target;
            }
        }

        public UIForia.Rendering.UIMeasurement AsUIMeasurement {
            get {
                return new UIMeasurement(float0, (UIMeasurementUnit)int1);
            }
        }

        public UIForia.Rendering.Overflow AsOverflow {
            get {
                return (UIForia.Rendering.Overflow)int0;
            }
        }

        public string AsString {
            get {
                return (string)GCHandle.FromIntPtr(ptr).Target;
            }
        }

        public UIForia.UIInput.PointerEvents AsPointerEvents {
            get {
                return (UIForia.UIInput.PointerEvents)int0;
            }
        }

        public UIForia.Rendering.Visibility AsVisibility {
            get {
                return (UIForia.Rendering.Visibility)int0;
            }
        }


    }
  
    public partial class DefaultStyleValue {

    

    }
  
}