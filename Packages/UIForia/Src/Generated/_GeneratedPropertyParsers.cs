using UIForia.Util;
using System;
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;
using UIForia;
using UIForia.Rendering;
using UIForia.Layout;
using UIForia.Text;
// ReSharper disable RedundantNameQualifier
// ReSharper disable UnusedMember.Global


namespace UIForia.Style {

    public static partial class PropertyParsers {

        static PropertyParsers() {

            s_ParserTable = new IStylePropertyParser[41];
            s_ParserTable[1] = new UIForia.Style.EnumParser<UIForia.AlignmentBoundary>();
            s_ParserTable[2] = new UIForia.Style.EnumParser<UIForia.AlignmentDirection>();
            s_ParserTable[3] = new UIForia.Style.OffsetMeasurementParser();
            s_ParserTable[4] = new UIForia.Style.EnumParser<UIForia.AlignmentTarget>();
            s_ParserTable[5] = new UIForia.Style.AspectRatioParser();
            s_ParserTable[6] = new UIForia.Style.UIColorParser();
            s_ParserTable[7] = new UIForia.Style.EnumParser<UIForia.Rendering.BackgroundFit>();
            s_ParserTable[8] = new UIForia.Style.TextureInfoParser();
            s_ParserTable[9] = new UIForia.Style.FixedLengthParser();
            s_ParserTable[10] = new UIForia.Style.UIAngleParser();
            s_ParserTable[11] = new UIForia.Style.HalfParser();
            s_ParserTable[12] = new UIForia.Style.EnumParser<UIForia.ClipBehavior>();
            s_ParserTable[13] = new UIForia.Style.EnumParser<UIForia.ClipBounds>();
            s_ParserTable[14] = new UIForia.Style.EnumFlagParser<UIForia.SpaceCollapse>();
            s_ParserTable[15] = new UIForia.Style.GridItemPlacementParser();
            s_ParserTable[16] = new UIForia.Style.GridTemplateParser();
            s_ParserTable[17] = new UIForia.Style.EnumParser<UIForia.Layout.GridLayoutDensity>();
            s_ParserTable[18] = new UIForia.Style.UShortParser();
            s_ParserTable[19] = new UIForia.Style.EnumParser<UIForia.LayoutBehavior>();
            s_ParserTable[20] = new UIForia.Style.EnumParser<UIForia.LayoutFillOrder>();
            s_ParserTable[21] = new UIForia.Style.EnumParser<UIForia.LayoutType>();
            s_ParserTable[22] = new UIForia.Style.SpaceSizeParser();
            s_ParserTable[23] = new UIForia.Style.SizeConstraintParser();
            s_ParserTable[24] = new UIForia.Style.EnumParser<UIForia.Rendering.MeshFillDirection>();
            s_ParserTable[25] = new UIForia.Style.EnumParser<UIForia.Rendering.MeshFillOrigin>();
            s_ParserTable[26] = new UIForia.Style.EnumParser<UIForia.Rendering.MeshType>();
            s_ParserTable[27] = new UIForia.Style.EnumParser<UIForia.Overflow>();
            s_ParserTable[28] = new UIForia.Style.PainterParser();
            s_ParserTable[29] = new UIForia.Style.EnumParser<UIForia.PointerEvents>();
            s_ParserTable[30] = new UIForia.Style.MeasurementParser();
            s_ParserTable[31] = new UIForia.Style.EnumParser<UIForia.Text.TextAlignment>();
            s_ParserTable[32] = new UIForia.Style.FontAssetParser();
            s_ParserTable[33] = new UIForia.Style.FontSizeParser();
            s_ParserTable[34] = new UIForia.Style.EnumFlagParser<UIForia.Text.FontStyle>();
            s_ParserTable[35] = new UIForia.Style.EnumParser<UIForia.Text.TextOverflow>();
            s_ParserTable[36] = new UIForia.Style.EnumParser<UIForia.Text.TextTransform>();
            s_ParserTable[37] = new UIForia.Style.EnumParser<UIForia.Rendering.UnderlayType>();
            s_ParserTable[38] = new UIForia.Style.EnumParser<UIForia.Text.VerticalAlignment>();
            s_ParserTable[39] = new UIForia.Style.EnumFlagParser<UIForia.Text.WhitespaceMode>();
            s_ParserTable[40] = new UIForia.Style.EnumFlagParser<UIForia.Visibility>();
          
            s_parseEntries = new PropertyParseEntry[122 + 1];
            s_parseEntries[1] = new PropertyParseEntry("Layer", PropertyId.Layer, s_ParserTable[18]);
            s_parseEntries[2] = new PropertyParseEntry("Opacity", PropertyId.Opacity, s_ParserTable[11]);
            s_parseEntries[3] = new PropertyParseEntry("PointerEvents", PropertyId.PointerEvents, s_ParserTable[29]);
            s_parseEntries[4] = new PropertyParseEntry("TextAlignment", PropertyId.TextAlignment, s_ParserTable[31]);
            s_parseEntries[5] = new PropertyParseEntry("TextColor", PropertyId.TextColor, s_ParserTable[6]);
            s_parseEntries[6] = new PropertyParseEntry("TextFontAsset", PropertyId.TextFontAsset, s_ParserTable[32]);
            s_parseEntries[7] = new PropertyParseEntry("TextFontSize", PropertyId.TextFontSize, s_ParserTable[33]);
            s_parseEntries[8] = new PropertyParseEntry("TextFontStyle", PropertyId.TextFontStyle, s_ParserTable[34]);
            s_parseEntries[9] = new PropertyParseEntry("TextLineHeight", PropertyId.TextLineHeight, s_ParserTable[11]);
            s_parseEntries[10] = new PropertyParseEntry("TextOverflow", PropertyId.TextOverflow, s_ParserTable[35]);
            s_parseEntries[11] = new PropertyParseEntry("TextTransform", PropertyId.TextTransform, s_ParserTable[36]);
            s_parseEntries[12] = new PropertyParseEntry("TextVerticalAlignment", PropertyId.TextVerticalAlignment, s_ParserTable[38]);
            s_parseEntries[13] = new PropertyParseEntry("TextWhitespaceMode", PropertyId.TextWhitespaceMode, s_ParserTable[39]);
            s_parseEntries[14] = new PropertyParseEntry("Visibility", PropertyId.Visibility, s_ParserTable[40]);
            s_parseEntries[15] = new PropertyParseEntry("ZIndex", PropertyId.ZIndex, s_ParserTable[18]);
            s_parseEntries[16] = new PropertyParseEntry("AlignmentBoundaryX", PropertyId.AlignmentBoundaryX, s_ParserTable[1]);
            s_parseEntries[17] = new PropertyParseEntry("AlignmentBoundaryY", PropertyId.AlignmentBoundaryY, s_ParserTable[1]);
            s_parseEntries[18] = new PropertyParseEntry("AlignmentDirectionX", PropertyId.AlignmentDirectionX, s_ParserTable[2]);
            s_parseEntries[19] = new PropertyParseEntry("AlignmentDirectionY", PropertyId.AlignmentDirectionY, s_ParserTable[2]);
            s_parseEntries[20] = new PropertyParseEntry("AlignmentOffsetX", PropertyId.AlignmentOffsetX, s_ParserTable[3]);
            s_parseEntries[21] = new PropertyParseEntry("AlignmentOffsetY", PropertyId.AlignmentOffsetY, s_ParserTable[3]);
            s_parseEntries[22] = new PropertyParseEntry("AlignmentOriginX", PropertyId.AlignmentOriginX, s_ParserTable[3]);
            s_parseEntries[23] = new PropertyParseEntry("AlignmentOriginY", PropertyId.AlignmentOriginY, s_ParserTable[3]);
            s_parseEntries[24] = new PropertyParseEntry("AlignmentTargetX", PropertyId.AlignmentTargetX, s_ParserTable[4]);
            s_parseEntries[25] = new PropertyParseEntry("AlignmentTargetY", PropertyId.AlignmentTargetY, s_ParserTable[4]);
            s_parseEntries[26] = new PropertyParseEntry("AspectRatio", PropertyId.AspectRatio, s_ParserTable[5]);
            s_parseEntries[27] = new PropertyParseEntry("BackgroundColor", PropertyId.BackgroundColor, s_ParserTable[6]);
            s_parseEntries[28] = new PropertyParseEntry("BackgroundFit", PropertyId.BackgroundFit, s_ParserTable[7]);
            s_parseEntries[29] = new PropertyParseEntry("BackgroundImage", PropertyId.BackgroundImage, s_ParserTable[8]);
            s_parseEntries[30] = new PropertyParseEntry("BackgroundImageOffsetX", PropertyId.BackgroundImageOffsetX, s_ParserTable[9]);
            s_parseEntries[31] = new PropertyParseEntry("BackgroundImageOffsetY", PropertyId.BackgroundImageOffsetY, s_ParserTable[9]);
            s_parseEntries[32] = new PropertyParseEntry("BackgroundImageRotation", PropertyId.BackgroundImageRotation, s_ParserTable[10]);
            s_parseEntries[33] = new PropertyParseEntry("BackgroundImageScaleX", PropertyId.BackgroundImageScaleX, s_ParserTable[11]);
            s_parseEntries[34] = new PropertyParseEntry("BackgroundImageScaleY", PropertyId.BackgroundImageScaleY, s_ParserTable[11]);
            s_parseEntries[35] = new PropertyParseEntry("BackgroundImageTileX", PropertyId.BackgroundImageTileX, s_ParserTable[11]);
            s_parseEntries[36] = new PropertyParseEntry("BackgroundImageTileY", PropertyId.BackgroundImageTileY, s_ParserTable[11]);
            s_parseEntries[37] = new PropertyParseEntry("BackgroundRectMaxX", PropertyId.BackgroundRectMaxX, s_ParserTable[9]);
            s_parseEntries[38] = new PropertyParseEntry("BackgroundRectMaxY", PropertyId.BackgroundRectMaxY, s_ParserTable[9]);
            s_parseEntries[39] = new PropertyParseEntry("BackgroundRectMinX", PropertyId.BackgroundRectMinX, s_ParserTable[9]);
            s_parseEntries[40] = new PropertyParseEntry("BackgroundRectMinY", PropertyId.BackgroundRectMinY, s_ParserTable[9]);
            s_parseEntries[41] = new PropertyParseEntry("BackgroundTint", PropertyId.BackgroundTint, s_ParserTable[6]);
            s_parseEntries[42] = new PropertyParseEntry("BorderBottom", PropertyId.BorderBottom, s_ParserTable[9]);
            s_parseEntries[43] = new PropertyParseEntry("BorderColorBottom", PropertyId.BorderColorBottom, s_ParserTable[6]);
            s_parseEntries[44] = new PropertyParseEntry("BorderColorLeft", PropertyId.BorderColorLeft, s_ParserTable[6]);
            s_parseEntries[45] = new PropertyParseEntry("BorderColorRight", PropertyId.BorderColorRight, s_ParserTable[6]);
            s_parseEntries[46] = new PropertyParseEntry("BorderColorTop", PropertyId.BorderColorTop, s_ParserTable[6]);
            s_parseEntries[47] = new PropertyParseEntry("BorderLeft", PropertyId.BorderLeft, s_ParserTable[9]);
            s_parseEntries[48] = new PropertyParseEntry("BorderRight", PropertyId.BorderRight, s_ParserTable[9]);
            s_parseEntries[49] = new PropertyParseEntry("BorderTop", PropertyId.BorderTop, s_ParserTable[9]);
            s_parseEntries[50] = new PropertyParseEntry("ClipBehavior", PropertyId.ClipBehavior, s_ParserTable[12]);
            s_parseEntries[51] = new PropertyParseEntry("ClipBounds", PropertyId.ClipBounds, s_ParserTable[13]);
            s_parseEntries[52] = new PropertyParseEntry("CollapseSpaceHorizontal", PropertyId.CollapseSpaceHorizontal, s_ParserTable[14]);
            s_parseEntries[53] = new PropertyParseEntry("CollapseSpaceVertical", PropertyId.CollapseSpaceVertical, s_ParserTable[14]);
            s_parseEntries[54] = new PropertyParseEntry("CornerBevelBottomLeft", PropertyId.CornerBevelBottomLeft, s_ParserTable[9]);
            s_parseEntries[55] = new PropertyParseEntry("CornerBevelBottomRight", PropertyId.CornerBevelBottomRight, s_ParserTable[9]);
            s_parseEntries[56] = new PropertyParseEntry("CornerBevelTopLeft", PropertyId.CornerBevelTopLeft, s_ParserTable[9]);
            s_parseEntries[57] = new PropertyParseEntry("CornerBevelTopRight", PropertyId.CornerBevelTopRight, s_ParserTable[9]);
            s_parseEntries[58] = new PropertyParseEntry("CornerRadiusBottomLeft", PropertyId.CornerRadiusBottomLeft, s_ParserTable[9]);
            s_parseEntries[59] = new PropertyParseEntry("CornerRadiusBottomRight", PropertyId.CornerRadiusBottomRight, s_ParserTable[9]);
            s_parseEntries[60] = new PropertyParseEntry("CornerRadiusTopLeft", PropertyId.CornerRadiusTopLeft, s_ParserTable[9]);
            s_parseEntries[61] = new PropertyParseEntry("CornerRadiusTopRight", PropertyId.CornerRadiusTopRight, s_ParserTable[9]);
            s_parseEntries[62] = new PropertyParseEntry("GridItemX", PropertyId.GridItemX, s_ParserTable[15]);
            s_parseEntries[63] = new PropertyParseEntry("GridItemY", PropertyId.GridItemY, s_ParserTable[15]);
            s_parseEntries[64] = new PropertyParseEntry("GridLayoutColGap", PropertyId.GridLayoutColGap, s_ParserTable[11]);
            s_parseEntries[65] = new PropertyParseEntry("GridLayoutColTemplate", PropertyId.GridLayoutColTemplate, s_ParserTable[16]);
            s_parseEntries[66] = new PropertyParseEntry("GridLayoutDensity", PropertyId.GridLayoutDensity, s_ParserTable[17]);
            s_parseEntries[67] = new PropertyParseEntry("GridLayoutRowGap", PropertyId.GridLayoutRowGap, s_ParserTable[11]);
            s_parseEntries[68] = new PropertyParseEntry("GridLayoutRowTemplate", PropertyId.GridLayoutRowTemplate, s_ParserTable[16]);
            s_parseEntries[69] = new PropertyParseEntry("LayoutBehavior", PropertyId.LayoutBehavior, s_ParserTable[19]);
            s_parseEntries[70] = new PropertyParseEntry("LayoutFillOrder", PropertyId.LayoutFillOrder, s_ParserTable[20]);
            s_parseEntries[71] = new PropertyParseEntry("LayoutType", PropertyId.LayoutType, s_ParserTable[21]);
            s_parseEntries[72] = new PropertyParseEntry("MarginBottom", PropertyId.MarginBottom, s_ParserTable[22]);
            s_parseEntries[73] = new PropertyParseEntry("MarginLeft", PropertyId.MarginLeft, s_ParserTable[22]);
            s_parseEntries[74] = new PropertyParseEntry("MarginRight", PropertyId.MarginRight, s_ParserTable[22]);
            s_parseEntries[75] = new PropertyParseEntry("MarginTop", PropertyId.MarginTop, s_ParserTable[22]);
            s_parseEntries[76] = new PropertyParseEntry("MaxHeight", PropertyId.MaxHeight, s_ParserTable[23]);
            s_parseEntries[77] = new PropertyParseEntry("MaxWidth", PropertyId.MaxWidth, s_ParserTable[23]);
            s_parseEntries[78] = new PropertyParseEntry("MeshFillAmount", PropertyId.MeshFillAmount, s_ParserTable[11]);
            s_parseEntries[79] = new PropertyParseEntry("MeshFillDirection", PropertyId.MeshFillDirection, s_ParserTable[24]);
            s_parseEntries[80] = new PropertyParseEntry("MeshFillOffsetX", PropertyId.MeshFillOffsetX, s_ParserTable[9]);
            s_parseEntries[81] = new PropertyParseEntry("MeshFillOffsetY", PropertyId.MeshFillOffsetY, s_ParserTable[9]);
            s_parseEntries[82] = new PropertyParseEntry("MeshFillOrigin", PropertyId.MeshFillOrigin, s_ParserTable[25]);
            s_parseEntries[83] = new PropertyParseEntry("MeshFillRadius", PropertyId.MeshFillRadius, s_ParserTable[9]);
            s_parseEntries[84] = new PropertyParseEntry("MeshFillRotation", PropertyId.MeshFillRotation, s_ParserTable[10]);
            s_parseEntries[85] = new PropertyParseEntry("MeshType", PropertyId.MeshType, s_ParserTable[26]);
            s_parseEntries[86] = new PropertyParseEntry("MinHeight", PropertyId.MinHeight, s_ParserTable[23]);
            s_parseEntries[87] = new PropertyParseEntry("MinWidth", PropertyId.MinWidth, s_ParserTable[23]);
            s_parseEntries[88] = new PropertyParseEntry("OverflowX", PropertyId.OverflowX, s_ParserTable[27]);
            s_parseEntries[89] = new PropertyParseEntry("OverflowY", PropertyId.OverflowY, s_ParserTable[27]);
            s_parseEntries[90] = new PropertyParseEntry("PaddingBottom", PropertyId.PaddingBottom, s_ParserTable[22]);
            s_parseEntries[91] = new PropertyParseEntry("PaddingLeft", PropertyId.PaddingLeft, s_ParserTable[22]);
            s_parseEntries[92] = new PropertyParseEntry("PaddingRight", PropertyId.PaddingRight, s_ParserTable[22]);
            s_parseEntries[93] = new PropertyParseEntry("PaddingTop", PropertyId.PaddingTop, s_ParserTable[22]);
            s_parseEntries[94] = new PropertyParseEntry("Painter", PropertyId.Painter, s_ParserTable[28]);
            s_parseEntries[95] = new PropertyParseEntry("PreferredHeight", PropertyId.PreferredHeight, s_ParserTable[30]);
            s_parseEntries[96] = new PropertyParseEntry("PreferredWidth", PropertyId.PreferredWidth, s_ParserTable[30]);
            s_parseEntries[97] = new PropertyParseEntry("SelectionBackgroundColor", PropertyId.SelectionBackgroundColor, s_ParserTable[6]);
            s_parseEntries[98] = new PropertyParseEntry("SelectionTextColor", PropertyId.SelectionTextColor, s_ParserTable[6]);
            s_parseEntries[99] = new PropertyParseEntry("SpaceBetweenHorizontal", PropertyId.SpaceBetweenHorizontal, s_ParserTable[22]);
            s_parseEntries[100] = new PropertyParseEntry("SpaceBetweenVertical", PropertyId.SpaceBetweenVertical, s_ParserTable[22]);
            s_parseEntries[101] = new PropertyParseEntry("TextFaceDilate", PropertyId.TextFaceDilate, s_ParserTable[11]);
            s_parseEntries[102] = new PropertyParseEntry("TextGlowColor", PropertyId.TextGlowColor, s_ParserTable[6]);
            s_parseEntries[103] = new PropertyParseEntry("TextGlowInner", PropertyId.TextGlowInner, s_ParserTable[11]);
            s_parseEntries[104] = new PropertyParseEntry("TextGlowOffset", PropertyId.TextGlowOffset, s_ParserTable[11]);
            s_parseEntries[105] = new PropertyParseEntry("TextGlowOuter", PropertyId.TextGlowOuter, s_ParserTable[11]);
            s_parseEntries[106] = new PropertyParseEntry("TextGlowPower", PropertyId.TextGlowPower, s_ParserTable[11]);
            s_parseEntries[107] = new PropertyParseEntry("TextOutlineColor", PropertyId.TextOutlineColor, s_ParserTable[6]);
            s_parseEntries[108] = new PropertyParseEntry("TextOutlineSoftness", PropertyId.TextOutlineSoftness, s_ParserTable[11]);
            s_parseEntries[109] = new PropertyParseEntry("TextOutlineWidth", PropertyId.TextOutlineWidth, s_ParserTable[11]);
            s_parseEntries[110] = new PropertyParseEntry("TextUnderlayColor", PropertyId.TextUnderlayColor, s_ParserTable[6]);
            s_parseEntries[111] = new PropertyParseEntry("TextUnderlayDilate", PropertyId.TextUnderlayDilate, s_ParserTable[11]);
            s_parseEntries[112] = new PropertyParseEntry("TextUnderlaySoftness", PropertyId.TextUnderlaySoftness, s_ParserTable[11]);
            s_parseEntries[113] = new PropertyParseEntry("TextUnderlayType", PropertyId.TextUnderlayType, s_ParserTable[37]);
            s_parseEntries[114] = new PropertyParseEntry("TextUnderlayX", PropertyId.TextUnderlayX, s_ParserTable[11]);
            s_parseEntries[115] = new PropertyParseEntry("TextUnderlayY", PropertyId.TextUnderlayY, s_ParserTable[11]);
            s_parseEntries[116] = new PropertyParseEntry("TransformPivotX", PropertyId.TransformPivotX, s_ParserTable[9]);
            s_parseEntries[117] = new PropertyParseEntry("TransformPivotY", PropertyId.TransformPivotY, s_ParserTable[9]);
            s_parseEntries[118] = new PropertyParseEntry("TransformPositionX", PropertyId.TransformPositionX, s_ParserTable[3]);
            s_parseEntries[119] = new PropertyParseEntry("TransformPositionY", PropertyId.TransformPositionY, s_ParserTable[3]);
            s_parseEntries[120] = new PropertyParseEntry("TransformRotation", PropertyId.TransformRotation, s_ParserTable[11]);
            s_parseEntries[121] = new PropertyParseEntry("TransformScaleX", PropertyId.TransformScaleX, s_ParserTable[11]);
            s_parseEntries[122] = new PropertyParseEntry("TransformScaleY", PropertyId.TransformScaleY, s_ParserTable[11]);

            s_PropertyNames = new [] {
                "Invalid",
                "Layer",
                "Opacity",
                "PointerEvents",
                "TextAlignment",
                "TextColor",
                "TextFontAsset",
                "TextFontSize",
                "TextFontStyle",
                "TextLineHeight",
                "TextOverflow",
                "TextTransform",
                "TextVerticalAlignment",
                "TextWhitespaceMode",
                "Visibility",
                "ZIndex",
                "AlignmentBoundaryX",
                "AlignmentBoundaryY",
                "AlignmentDirectionX",
                "AlignmentDirectionY",
                "AlignmentOffsetX",
                "AlignmentOffsetY",
                "AlignmentOriginX",
                "AlignmentOriginY",
                "AlignmentTargetX",
                "AlignmentTargetY",
                "AspectRatio",
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
                "BackgroundRectMaxX",
                "BackgroundRectMaxY",
                "BackgroundRectMinX",
                "BackgroundRectMinY",
                "BackgroundTint",
                "BorderBottom",
                "BorderColorBottom",
                "BorderColorLeft",
                "BorderColorRight",
                "BorderColorTop",
                "BorderLeft",
                "BorderRight",
                "BorderTop",
                "ClipBehavior",
                "ClipBounds",
                "CollapseSpaceHorizontal",
                "CollapseSpaceVertical",
                "CornerBevelBottomLeft",
                "CornerBevelBottomRight",
                "CornerBevelTopLeft",
                "CornerBevelTopRight",
                "CornerRadiusBottomLeft",
                "CornerRadiusBottomRight",
                "CornerRadiusTopLeft",
                "CornerRadiusTopRight",
                "GridItemX",
                "GridItemY",
                "GridLayoutColGap",
                "GridLayoutColTemplate",
                "GridLayoutDensity",
                "GridLayoutRowGap",
                "GridLayoutRowTemplate",
                "LayoutBehavior",
                "LayoutFillOrder",
                "LayoutType",
                "MarginBottom",
                "MarginLeft",
                "MarginRight",
                "MarginTop",
                "MaxHeight",
                "MaxWidth",
                "MeshFillAmount",
                "MeshFillDirection",
                "MeshFillOffsetX",
                "MeshFillOffsetY",
                "MeshFillOrigin",
                "MeshFillRadius",
                "MeshFillRotation",
                "MeshType",
                "MinHeight",
                "MinWidth",
                "OverflowX",
                "OverflowY",
                "PaddingBottom",
                "PaddingLeft",
                "PaddingRight",
                "PaddingTop",
                "Painter",
                "PreferredHeight",
                "PreferredWidth",
                "SelectionBackgroundColor",
                "SelectionTextColor",
                "SpaceBetweenHorizontal",
                "SpaceBetweenVertical",
                "TextFaceDilate",
                "TextGlowColor",
                "TextGlowInner",
                "TextGlowOffset",
                "TextGlowOuter",
                "TextGlowPower",
                "TextOutlineColor",
                "TextOutlineSoftness",
                "TextOutlineWidth",
                "TextUnderlayColor",
                "TextUnderlayDilate",
                "TextUnderlaySoftness",
                "TextUnderlayType",
                "TextUnderlayX",
                "TextUnderlayY",
                "TransformPivotX",
                "TransformPivotY",
                "TransformPositionX",
                "TransformPositionY",
                "TransformRotation",
                "TransformScaleX",
                "TransformScaleY"
            };

            s_ShorthandNames = new string[] {
                "AlignmentBoundary",
                "AlignmentDirection",
                "AlignmentOffset",
                "AlignmentOrigin",
                "AlignmentTarget",
                "BackgroundImageOffset",
                "BackgroundImageScale",
                "BackgroundImageTile",
                "Border",
                "BorderColor",
                "CollapseSpace",
                "CornerBevel",
                "CornerRadius",
                "Margin",
                "MaxSize",
                "MeshFillOffset",
                "MinSize",
                "Overflow",
                "Padding",
                "PreferredSize",
                "SpaceBetween"
            };
            s_ShorthandEntries = new ShorthandEntry[21];
            s_ShorthandEntries[0] = new ShorthandEntry("AlignmentBoundary", 0, new UIForia.Style.AlignmentBoundaryShortHandParser("AlignmentBoundary"));
            s_ShorthandEntries[1] = new ShorthandEntry("AlignmentDirection", 1, new UIForia.Style.AlignmentDirectionShortHandParser("AlignmentDirection"));
            s_ShorthandEntries[2] = new ShorthandEntry("AlignmentOffset", 2, new UIForia.Style.AlignmentOffsetShortHandParser("AlignmentOffset"));
            s_ShorthandEntries[3] = new ShorthandEntry("AlignmentOrigin", 3, new UIForia.Style.AlignmentOriginShortHandParser("AlignmentOrigin"));
            s_ShorthandEntries[4] = new ShorthandEntry("AlignmentTarget", 4, new UIForia.Style.AlignmentTargetShortHandParser("AlignmentTarget"));
            s_ShorthandEntries[5] = new ShorthandEntry("BackgroundImageOffset", 5, new UIForia.Style.BackgroundImageOffsetShortHandParser("BackgroundImageOffset"));
            s_ShorthandEntries[6] = new ShorthandEntry("BackgroundImageScale", 6, new UIForia.Style.BackgroundImageScaleShortHandParser("BackgroundImageScale"));
            s_ShorthandEntries[7] = new ShorthandEntry("BackgroundImageTile", 7, new UIForia.Style.BackgroundImageTileShortHandParser("BackgroundImageTile"));
            s_ShorthandEntries[8] = new ShorthandEntry("Border", 8, new UIForia.Style.BorderShortHandParser("Border"));
            s_ShorthandEntries[9] = new ShorthandEntry("BorderColor", 9, new UIForia.Style.BorderColorShortHandParser("BorderColor"));
            s_ShorthandEntries[10] = new ShorthandEntry("CollapseSpace", 10, new UIForia.Style.CollapseSpaceShortHandParser("CollapseSpace"));
            s_ShorthandEntries[11] = new ShorthandEntry("CornerBevel", 11, new UIForia.Style.CornerBevelShortHandParser("CornerBevel"));
            s_ShorthandEntries[12] = new ShorthandEntry("CornerRadius", 12, new UIForia.Style.CornerRadiusShortHandParser("CornerRadius"));
            s_ShorthandEntries[13] = new ShorthandEntry("Margin", 13, new UIForia.Style.MarginShortHandParser("Margin"));
            s_ShorthandEntries[14] = new ShorthandEntry("MaxSize", 14, new UIForia.Style.MaxSizeShortHandParser("MaxSize"));
            s_ShorthandEntries[15] = new ShorthandEntry("MeshFillOffset", 15, new UIForia.Style.MeshFillOffsetShortHandParser("MeshFillOffset"));
            s_ShorthandEntries[16] = new ShorthandEntry("MinSize", 16, new UIForia.Style.MinSizeShortHandParser("MinSize"));
            s_ShorthandEntries[17] = new ShorthandEntry("Overflow", 17, new UIForia.Style.OverflowShortHandParser("Overflow"));
            s_ShorthandEntries[18] = new ShorthandEntry("Padding", 18, new UIForia.Style.PaddingShortHandParser("Padding"));
            s_ShorthandEntries[19] = new ShorthandEntry("PreferredSize", 19, new UIForia.Style.PreferredSizeShortHandParser("PreferredSize"));
            s_ShorthandEntries[20] = new ShorthandEntry("SpaceBetween", 20, new UIForia.Style.SpaceBetweenShortHandParser("SpaceBetween"));
            
            s_NameEntries = new PropertyNameEntry[s_parseEntries.Length];
            for (int i = 0; i < s_NameEntries.Length; i++) {
                s_NameEntries[i].loweredName = s_parseEntries[i].loweredName;
                s_NameEntries[i].propertyId = s_parseEntries[i].propertyId.id;
            }

            Array.Sort(s_NameEntries);
        }


    }

    public partial struct PropertyId {
    
        public static readonly PropertyId Invalid = new PropertyId(0);
        public static readonly PropertyId Layer = new PropertyId(1);
        public static readonly PropertyId Opacity = new PropertyId(2);
        public static readonly PropertyId PointerEvents = new PropertyId(3);
        public static readonly PropertyId TextAlignment = new PropertyId(4);
        public static readonly PropertyId TextColor = new PropertyId(5);
        public static readonly PropertyId TextFontAsset = new PropertyId(6);
        public static readonly PropertyId TextFontSize = new PropertyId(7);
        public static readonly PropertyId TextFontStyle = new PropertyId(8);
        public static readonly PropertyId TextLineHeight = new PropertyId(9);
        public static readonly PropertyId TextOverflow = new PropertyId(10);
        public static readonly PropertyId TextTransform = new PropertyId(11);
        public static readonly PropertyId TextVerticalAlignment = new PropertyId(12);
        public static readonly PropertyId TextWhitespaceMode = new PropertyId(13);
        public static readonly PropertyId Visibility = new PropertyId(14);
        public static readonly PropertyId ZIndex = new PropertyId(15);
        public static readonly PropertyId AlignmentBoundaryX = new PropertyId(16);
        public static readonly PropertyId AlignmentBoundaryY = new PropertyId(17);
        public static readonly PropertyId AlignmentDirectionX = new PropertyId(18);
        public static readonly PropertyId AlignmentDirectionY = new PropertyId(19);
        public static readonly PropertyId AlignmentOffsetX = new PropertyId(20);
        public static readonly PropertyId AlignmentOffsetY = new PropertyId(21);
        public static readonly PropertyId AlignmentOriginX = new PropertyId(22);
        public static readonly PropertyId AlignmentOriginY = new PropertyId(23);
        public static readonly PropertyId AlignmentTargetX = new PropertyId(24);
        public static readonly PropertyId AlignmentTargetY = new PropertyId(25);
        public static readonly PropertyId AspectRatio = new PropertyId(26);
        public static readonly PropertyId BackgroundColor = new PropertyId(27);
        public static readonly PropertyId BackgroundFit = new PropertyId(28);
        public static readonly PropertyId BackgroundImage = new PropertyId(29);
        public static readonly PropertyId BackgroundImageOffsetX = new PropertyId(30);
        public static readonly PropertyId BackgroundImageOffsetY = new PropertyId(31);
        public static readonly PropertyId BackgroundImageRotation = new PropertyId(32);
        public static readonly PropertyId BackgroundImageScaleX = new PropertyId(33);
        public static readonly PropertyId BackgroundImageScaleY = new PropertyId(34);
        public static readonly PropertyId BackgroundImageTileX = new PropertyId(35);
        public static readonly PropertyId BackgroundImageTileY = new PropertyId(36);
        public static readonly PropertyId BackgroundRectMaxX = new PropertyId(37);
        public static readonly PropertyId BackgroundRectMaxY = new PropertyId(38);
        public static readonly PropertyId BackgroundRectMinX = new PropertyId(39);
        public static readonly PropertyId BackgroundRectMinY = new PropertyId(40);
        public static readonly PropertyId BackgroundTint = new PropertyId(41);
        public static readonly PropertyId BorderBottom = new PropertyId(42);
        public static readonly PropertyId BorderColorBottom = new PropertyId(43);
        public static readonly PropertyId BorderColorLeft = new PropertyId(44);
        public static readonly PropertyId BorderColorRight = new PropertyId(45);
        public static readonly PropertyId BorderColorTop = new PropertyId(46);
        public static readonly PropertyId BorderLeft = new PropertyId(47);
        public static readonly PropertyId BorderRight = new PropertyId(48);
        public static readonly PropertyId BorderTop = new PropertyId(49);
        public static readonly PropertyId ClipBehavior = new PropertyId(50);
        public static readonly PropertyId ClipBounds = new PropertyId(51);
        public static readonly PropertyId CollapseSpaceHorizontal = new PropertyId(52);
        public static readonly PropertyId CollapseSpaceVertical = new PropertyId(53);
        public static readonly PropertyId CornerBevelBottomLeft = new PropertyId(54);
        public static readonly PropertyId CornerBevelBottomRight = new PropertyId(55);
        public static readonly PropertyId CornerBevelTopLeft = new PropertyId(56);
        public static readonly PropertyId CornerBevelTopRight = new PropertyId(57);
        public static readonly PropertyId CornerRadiusBottomLeft = new PropertyId(58);
        public static readonly PropertyId CornerRadiusBottomRight = new PropertyId(59);
        public static readonly PropertyId CornerRadiusTopLeft = new PropertyId(60);
        public static readonly PropertyId CornerRadiusTopRight = new PropertyId(61);
        public static readonly PropertyId GridItemX = new PropertyId(62);
        public static readonly PropertyId GridItemY = new PropertyId(63);
        public static readonly PropertyId GridLayoutColGap = new PropertyId(64);
        public static readonly PropertyId GridLayoutColTemplate = new PropertyId(65);
        public static readonly PropertyId GridLayoutDensity = new PropertyId(66);
        public static readonly PropertyId GridLayoutRowGap = new PropertyId(67);
        public static readonly PropertyId GridLayoutRowTemplate = new PropertyId(68);
        public static readonly PropertyId LayoutBehavior = new PropertyId(69);
        public static readonly PropertyId LayoutFillOrder = new PropertyId(70);
        public static readonly PropertyId LayoutType = new PropertyId(71);
        public static readonly PropertyId MarginBottom = new PropertyId(72);
        public static readonly PropertyId MarginLeft = new PropertyId(73);
        public static readonly PropertyId MarginRight = new PropertyId(74);
        public static readonly PropertyId MarginTop = new PropertyId(75);
        public static readonly PropertyId MaxHeight = new PropertyId(76);
        public static readonly PropertyId MaxWidth = new PropertyId(77);
        public static readonly PropertyId MeshFillAmount = new PropertyId(78);
        public static readonly PropertyId MeshFillDirection = new PropertyId(79);
        public static readonly PropertyId MeshFillOffsetX = new PropertyId(80);
        public static readonly PropertyId MeshFillOffsetY = new PropertyId(81);
        public static readonly PropertyId MeshFillOrigin = new PropertyId(82);
        public static readonly PropertyId MeshFillRadius = new PropertyId(83);
        public static readonly PropertyId MeshFillRotation = new PropertyId(84);
        public static readonly PropertyId MeshType = new PropertyId(85);
        public static readonly PropertyId MinHeight = new PropertyId(86);
        public static readonly PropertyId MinWidth = new PropertyId(87);
        public static readonly PropertyId OverflowX = new PropertyId(88);
        public static readonly PropertyId OverflowY = new PropertyId(89);
        public static readonly PropertyId PaddingBottom = new PropertyId(90);
        public static readonly PropertyId PaddingLeft = new PropertyId(91);
        public static readonly PropertyId PaddingRight = new PropertyId(92);
        public static readonly PropertyId PaddingTop = new PropertyId(93);
        public static readonly PropertyId Painter = new PropertyId(94);
        public static readonly PropertyId PreferredHeight = new PropertyId(95);
        public static readonly PropertyId PreferredWidth = new PropertyId(96);
        public static readonly PropertyId SelectionBackgroundColor = new PropertyId(97);
        public static readonly PropertyId SelectionTextColor = new PropertyId(98);
        public static readonly PropertyId SpaceBetweenHorizontal = new PropertyId(99);
        public static readonly PropertyId SpaceBetweenVertical = new PropertyId(100);
        public static readonly PropertyId TextFaceDilate = new PropertyId(101);
        public static readonly PropertyId TextGlowColor = new PropertyId(102);
        public static readonly PropertyId TextGlowInner = new PropertyId(103);
        public static readonly PropertyId TextGlowOffset = new PropertyId(104);
        public static readonly PropertyId TextGlowOuter = new PropertyId(105);
        public static readonly PropertyId TextGlowPower = new PropertyId(106);
        public static readonly PropertyId TextOutlineColor = new PropertyId(107);
        public static readonly PropertyId TextOutlineSoftness = new PropertyId(108);
        public static readonly PropertyId TextOutlineWidth = new PropertyId(109);
        public static readonly PropertyId TextUnderlayColor = new PropertyId(110);
        public static readonly PropertyId TextUnderlayDilate = new PropertyId(111);
        public static readonly PropertyId TextUnderlaySoftness = new PropertyId(112);
        public static readonly PropertyId TextUnderlayType = new PropertyId(113);
        public static readonly PropertyId TextUnderlayX = new PropertyId(114);
        public static readonly PropertyId TextUnderlayY = new PropertyId(115);
        public static readonly PropertyId TransformPivotX = new PropertyId(116);
        public static readonly PropertyId TransformPivotY = new PropertyId(117);
        public static readonly PropertyId TransformPositionX = new PropertyId(118);
        public static readonly PropertyId TransformPositionY = new PropertyId(119);
        public static readonly PropertyId TransformRotation = new PropertyId(120);
        public static readonly PropertyId TransformScaleX = new PropertyId(121);
        public static readonly PropertyId TransformScaleY = new PropertyId(122);

        public const int k_InheritedPropertyCount = 15;

    } 


}