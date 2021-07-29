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

    internal enum PropertyType {
        float2 = 1,
        Single = 2,
        half = 3,
        Byte = 4,
        UInt16 = 5,
        AspectRatio = 6,
        FontAssetId = 7,
        PainterId = 8,
        Int32 = 9,
        TextureInfo = 10,
        UIColor = 11,
        GridItemPlacement = 12,
        GridLayoutTemplate = 13,
        UIMeasurement = 14,
        UIFixedLength = 15,
        UIOffset = 16,
        UIAngle = 17,
        UIFontSize = 18,
        UISpaceSize = 19,
        UISizeConstraint = 20,
        Enum = 21
    }

    internal unsafe partial class StyleDatabase {

        internal UIForia.Util.Unsafe.DataList<Unity.Mathematics.float2> propertyTable_float2;
        internal UIForia.Util.Unsafe.DataList<float> propertyTable_Single;
        internal UIForia.Util.Unsafe.DataList<Unity.Mathematics.half> propertyTable_half;
        internal UIForia.Util.Unsafe.DataList<byte> propertyTable_Byte;
        internal UIForia.Util.Unsafe.DataList<ushort> propertyTable_UInt16;
        internal UIForia.Util.Unsafe.DataList<UIForia.AspectRatio> propertyTable_AspectRatio;
        internal UIForia.Util.Unsafe.DataList<UIForia.FontAssetId> propertyTable_FontAssetId;
        internal UIForia.Util.Unsafe.DataList<UIForia.PainterId> propertyTable_PainterId;
        internal UIForia.Util.Unsafe.DataList<int> propertyTable_Int32;
        internal UIForia.Util.Unsafe.DataList<UIForia.Style.TextureInfo> propertyTable_TextureInfo;
        internal UIForia.Util.Unsafe.DataList<UIForia.UIColor> propertyTable_UIColor;
        internal UIForia.Util.Unsafe.DataList<UIForia.GridItemPlacement> propertyTable_GridItemPlacement;
        internal UIForia.Util.Unsafe.DataList<UIForia.GridLayoutTemplate> propertyTable_GridLayoutTemplate;
        internal UIForia.Util.Unsafe.DataList<UIForia.UIMeasurement> propertyTable_UIMeasurement;
        internal UIForia.Util.Unsafe.DataList<UIForia.UIFixedLength> propertyTable_UIFixedLength;
        internal UIForia.Util.Unsafe.DataList<UIForia.UIOffset> propertyTable_UIOffset;
        internal UIForia.Util.Unsafe.DataList<UIForia.UIAngle> propertyTable_UIAngle;
        internal UIForia.Util.Unsafe.DataList<UIForia.UIFontSize> propertyTable_UIFontSize;
        internal UIForia.Util.Unsafe.DataList<UIForia.UISpaceSize> propertyTable_UISpaceSize;
        internal UIForia.Util.Unsafe.DataList<UIForia.UISizeConstraint> propertyTable_UISizeConstraint;
        internal UIForia.Util.Unsafe.DataList<EnumValue> propertyTable_EnumValue;


        private static Type[] s_EnumTypeIds = {
            typeof(UIForia.LayoutType),
            typeof(UIForia.LayoutBehavior),
            typeof(UIForia.Text.WhitespaceMode),
            typeof(UIForia.Text.TextTransform),
            typeof(UIForia.Text.FontStyle),
            typeof(UIForia.Text.TextAlignment),
            typeof(UIForia.Text.VerticalAlignment),
            typeof(UIForia.Text.TextOverflow),
            typeof(UIForia.SpaceCollapse),
            typeof(UIForia.LayoutFillOrder),
            typeof(UIForia.Layout.GridLayoutDensity),
            typeof(UIForia.Visibility),
            typeof(UIForia.Overflow),
            typeof(UIForia.ClipBehavior),
            typeof(UIForia.ClipBounds),
            typeof(UIForia.PointerEvents),
            typeof(UIForia.AlignmentDirection),
            typeof(UIForia.AlignmentTarget),
            typeof(UIForia.AlignmentBoundary),
            typeof(UIForia.Rendering.UnderlayType),
            typeof(UIForia.Rendering.BackgroundFit),
            typeof(UIForia.Rendering.MeshType),
            typeof(UIForia.Rendering.MeshFillDirection),
            typeof(UIForia.Rendering.MeshFillOrigin)
        };

        partial void AddProperty(PropertyId propertyId, byte * valueBuffer, int offset, ref int location) {
            location = AddProperty(propertyId, valueBuffer, offset);
        }

        private int AddProperty(PropertyId propertyId, byte * buffer, int offset) {
            switch(propertyTypeById[propertyId.index]) {
                case PropertyType.float2:
                    return AddToPropertyTable(ref propertyTable_float2, buffer, offset);
                case PropertyType.Single:
                    return AddToPropertyTable(ref propertyTable_Single, buffer, offset);
                case PropertyType.half:
                    return AddToPropertyTable(ref propertyTable_half, buffer, offset);
                case PropertyType.Byte:
                    return AddToPropertyTable(ref propertyTable_Byte, buffer, offset);
                case PropertyType.UInt16:
                    return AddToPropertyTable(ref propertyTable_UInt16, buffer, offset);
                case PropertyType.AspectRatio:
                    return AddToPropertyTable(ref propertyTable_AspectRatio, buffer, offset);
                case PropertyType.FontAssetId:
                    return AddToPropertyTable(ref propertyTable_FontAssetId, buffer, offset);
                case PropertyType.PainterId:
                    return AddToPropertyTable(ref propertyTable_PainterId, buffer, offset);
                case PropertyType.Int32:
                    return AddToPropertyTable(ref propertyTable_Int32, buffer, offset);
                case PropertyType.TextureInfo:
                    return AddToPropertyTable(ref propertyTable_TextureInfo, buffer, offset);
                case PropertyType.UIColor:
                    return AddToPropertyTable(ref propertyTable_UIColor, buffer, offset);
                case PropertyType.GridItemPlacement:
                    return AddToPropertyTable(ref propertyTable_GridItemPlacement, buffer, offset);
                case PropertyType.GridLayoutTemplate:
                    return AddToPropertyTable(ref propertyTable_GridLayoutTemplate, buffer, offset);
                case PropertyType.UIMeasurement:
                    return AddToPropertyTable(ref propertyTable_UIMeasurement, buffer, offset);
                case PropertyType.UIFixedLength:
                    return AddToPropertyTable(ref propertyTable_UIFixedLength, buffer, offset);
                case PropertyType.UIOffset:
                    return AddToPropertyTable(ref propertyTable_UIOffset, buffer, offset);
                case PropertyType.UIAngle:
                    return AddToPropertyTable(ref propertyTable_UIAngle, buffer, offset);
                case PropertyType.UIFontSize:
                    return AddToPropertyTable(ref propertyTable_UIFontSize, buffer, offset);
                case PropertyType.UISpaceSize:
                    return AddToPropertyTable(ref propertyTable_UISpaceSize, buffer, offset);
                case PropertyType.UISizeConstraint:
                    return AddToPropertyTable(ref propertyTable_UISizeConstraint, buffer, offset);
                case PropertyType.Enum:
                    return AddToPropertyTable(ref propertyTable_EnumValue, buffer, offset);

            }
            return -1;
        }

        partial void InitializeGenerated() {
            propertyTable_float2 = new UIForia.Util.Unsafe.DataList<Unity.Mathematics.float2>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_Single = new UIForia.Util.Unsafe.DataList<float>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_half = new UIForia.Util.Unsafe.DataList<Unity.Mathematics.half>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_Byte = new UIForia.Util.Unsafe.DataList<byte>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UInt16 = new UIForia.Util.Unsafe.DataList<ushort>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_AspectRatio = new UIForia.Util.Unsafe.DataList<UIForia.AspectRatio>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_FontAssetId = new UIForia.Util.Unsafe.DataList<UIForia.FontAssetId>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_PainterId = new UIForia.Util.Unsafe.DataList<UIForia.PainterId>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_Int32 = new UIForia.Util.Unsafe.DataList<int>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_TextureInfo = new UIForia.Util.Unsafe.DataList<UIForia.Style.TextureInfo>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UIColor = new UIForia.Util.Unsafe.DataList<UIForia.UIColor>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_GridItemPlacement = new UIForia.Util.Unsafe.DataList<UIForia.GridItemPlacement>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_GridLayoutTemplate = new UIForia.Util.Unsafe.DataList<UIForia.GridLayoutTemplate>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UIMeasurement = new UIForia.Util.Unsafe.DataList<UIForia.UIMeasurement>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UIFixedLength = new UIForia.Util.Unsafe.DataList<UIForia.UIFixedLength>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UIOffset = new UIForia.Util.Unsafe.DataList<UIForia.UIOffset>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UIAngle = new UIForia.Util.Unsafe.DataList<UIForia.UIAngle>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UIFontSize = new UIForia.Util.Unsafe.DataList<UIForia.UIFontSize>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UISpaceSize = new UIForia.Util.Unsafe.DataList<UIForia.UISpaceSize>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_UISizeConstraint = new UIForia.Util.Unsafe.DataList<UIForia.UISizeConstraint>(32, Unity.Collections.Allocator.Persistent);
            propertyTable_EnumValue = new UIForia.Util.Unsafe.DataList<EnumValue>(32, Unity.Collections.Allocator.Persistent);
            defaultValueIndices = new UIForia.Util.Unsafe.DataList<int>(123, Unity.Collections.Allocator.Persistent);
            defaultValueIndices.size = 123;
            defaultValueIndices[(int)PropertyId.Invalid] = 0;
            defaultValueIndices[(int)PropertyId.AlignmentBoundaryX] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.AlignmentBoundaryY] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.AlignmentDirectionX] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.AlignmentDirectionY] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.AlignmentOffsetX] = AddToPropertyTable(ref propertyTable_UIOffset, default);
            defaultValueIndices[(int)PropertyId.AlignmentOffsetY] = AddToPropertyTable(ref propertyTable_UIOffset, default);
            defaultValueIndices[(int)PropertyId.AlignmentOriginX] = AddToPropertyTable(ref propertyTable_UIOffset, default);
            defaultValueIndices[(int)PropertyId.AlignmentOriginY] = AddToPropertyTable(ref propertyTable_UIOffset, default);
            defaultValueIndices[(int)PropertyId.AlignmentTargetX] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.AlignmentTargetY] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.AspectRatio] = AddToPropertyTable(ref propertyTable_AspectRatio, default);
            defaultValueIndices[(int)PropertyId.BackgroundColor] = AddToPropertyTable(ref propertyTable_UIColor, default);
            defaultValueIndices[(int)PropertyId.BackgroundFit] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(BackgroundFit.Fill));
            defaultValueIndices[(int)PropertyId.BackgroundImage] = AddToPropertyTable(ref propertyTable_TextureInfo, default);
            defaultValueIndices[(int)PropertyId.BackgroundImageOffsetX] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BackgroundImageOffsetY] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BackgroundImageRotation] = AddToPropertyTable(ref propertyTable_UIAngle, default);
            defaultValueIndices[(int)PropertyId.BackgroundImageScaleX] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.BackgroundImageScaleY] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.BackgroundImageTileX] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.BackgroundImageTileY] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.BackgroundRectMaxX] = AddToPropertyTable(ref propertyTable_UIFixedLength, new UIFixedLength(1, UIFixedUnit.Percent));
            defaultValueIndices[(int)PropertyId.BackgroundRectMaxY] = AddToPropertyTable(ref propertyTable_UIFixedLength, new UIFixedLength(1, UIFixedUnit.Percent));
            defaultValueIndices[(int)PropertyId.BackgroundRectMinX] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BackgroundRectMinY] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BackgroundTint] = AddToPropertyTable(ref propertyTable_UIColor, default);
            defaultValueIndices[(int)PropertyId.BorderBottom] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BorderColorBottom] = AddToPropertyTable(ref propertyTable_UIColor, default);
            defaultValueIndices[(int)PropertyId.BorderColorLeft] = AddToPropertyTable(ref propertyTable_UIColor, default);
            defaultValueIndices[(int)PropertyId.BorderColorRight] = AddToPropertyTable(ref propertyTable_UIColor, default);
            defaultValueIndices[(int)PropertyId.BorderColorTop] = AddToPropertyTable(ref propertyTable_UIColor, default);
            defaultValueIndices[(int)PropertyId.BorderLeft] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BorderRight] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.BorderTop] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.ClipBehavior] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.ClipBounds] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.CollapseSpaceHorizontal] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.CollapseSpaceVertical] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.CornerBevelBottomLeft] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerBevelBottomRight] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerBevelTopLeft] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerBevelTopRight] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerRadiusBottomLeft] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerRadiusBottomRight] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerRadiusTopLeft] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.CornerRadiusTopRight] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.GridItemX] = AddToPropertyTable(ref propertyTable_GridItemPlacement, new GridItemPlacement(-1, 1));
            defaultValueIndices[(int)PropertyId.GridItemY] = AddToPropertyTable(ref propertyTable_GridItemPlacement, new GridItemPlacement(-1, 1));
            defaultValueIndices[(int)PropertyId.GridLayoutColGap] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.GridLayoutColTemplate] = AddToPropertyTable(ref propertyTable_GridLayoutTemplate, default);
            defaultValueIndices[(int)PropertyId.GridLayoutDensity] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(GridLayoutDensity.Sparse));
            defaultValueIndices[(int)PropertyId.GridLayoutRowGap] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.GridLayoutRowTemplate] = AddToPropertyTable(ref propertyTable_GridLayoutTemplate, default);
            defaultValueIndices[(int)PropertyId.Layer] = AddToPropertyTable(ref propertyTable_UInt16, default);
            defaultValueIndices[(int)PropertyId.LayoutBehavior] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.LayoutFillOrder] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.LayoutType] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(LayoutType.FlexVertical));
            defaultValueIndices[(int)PropertyId.MarginBottom] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.MarginLeft] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.MarginRight] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.MarginTop] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.MaxHeight] = AddToPropertyTable(ref propertyTable_UISizeConstraint, new UISizeConstraint(float.MaxValue));
            defaultValueIndices[(int)PropertyId.MaxWidth] = AddToPropertyTable(ref propertyTable_UISizeConstraint, new UISizeConstraint(float.MaxValue));
            defaultValueIndices[(int)PropertyId.MeshFillAmount] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.MeshFillDirection] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.MeshFillOffsetX] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.MeshFillOffsetY] = AddToPropertyTable(ref propertyTable_UIFixedLength, default);
            defaultValueIndices[(int)PropertyId.MeshFillOrigin] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.MeshFillRadius] = AddToPropertyTable(ref propertyTable_UIFixedLength, new UIFixedLength(float.MaxValue));
            defaultValueIndices[(int)PropertyId.MeshFillRotation] = AddToPropertyTable(ref propertyTable_UIAngle, default);
            defaultValueIndices[(int)PropertyId.MeshType] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.MinHeight] = AddToPropertyTable(ref propertyTable_UISizeConstraint, new UISizeConstraint(0));
            defaultValueIndices[(int)PropertyId.MinWidth] = AddToPropertyTable(ref propertyTable_UISizeConstraint, new UISizeConstraint(0));
            defaultValueIndices[(int)PropertyId.Opacity] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.OverflowX] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.OverflowY] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.PaddingBottom] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.PaddingLeft] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.PaddingRight] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.PaddingTop] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.Painter] = AddToPropertyTable(ref propertyTable_PainterId, default);
            defaultValueIndices[(int)PropertyId.PointerEvents] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.PreferredHeight] = AddToPropertyTable(ref propertyTable_UIMeasurement, new UIMeasurement(1, UIMeasurementUnit.Content));
            defaultValueIndices[(int)PropertyId.PreferredWidth] = AddToPropertyTable(ref propertyTable_UIMeasurement, new UIMeasurement(1, UIMeasurementUnit.StretchContent));
            defaultValueIndices[(int)PropertyId.SelectionBackgroundColor] = AddToPropertyTable(ref propertyTable_UIColor, new Color32(173, 214, 255,  255));
            defaultValueIndices[(int)PropertyId.SelectionTextColor] = AddToPropertyTable(ref propertyTable_UIColor, Color.black);
            defaultValueIndices[(int)PropertyId.SpaceBetweenHorizontal] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.SpaceBetweenVertical] = AddToPropertyTable(ref propertyTable_UISpaceSize, default);
            defaultValueIndices[(int)PropertyId.TextAlignment] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(UIForia.Text.TextAlignment.Left));
            defaultValueIndices[(int)PropertyId.TextColor] = AddToPropertyTable(ref propertyTable_UIColor, Color.black);
            defaultValueIndices[(int)PropertyId.TextFaceDilate] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextFontAsset] = AddToPropertyTable(ref propertyTable_FontAssetId, new FontAssetId(1));
            defaultValueIndices[(int)PropertyId.TextFontSize] = AddToPropertyTable(ref propertyTable_UIFontSize, 18f);
            defaultValueIndices[(int)PropertyId.TextFontStyle] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(Text.FontStyle.Normal));
            defaultValueIndices[(int)PropertyId.TextGlowColor] = AddToPropertyTable(ref propertyTable_UIColor, ColorUtil.UnsetValue);
            defaultValueIndices[(int)PropertyId.TextGlowInner] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextGlowOffset] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextGlowOuter] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextGlowPower] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextLineHeight] = AddToPropertyTable(ref propertyTable_half, (half)1.4f);
            defaultValueIndices[(int)PropertyId.TextOutlineColor] = AddToPropertyTable(ref propertyTable_UIColor, Color.black);
            defaultValueIndices[(int)PropertyId.TextOutlineSoftness] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextOutlineWidth] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextOverflow] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.TextTransform] = AddToPropertyTable(ref propertyTable_Int32, (System.Int32)(TextTransform.None));
            defaultValueIndices[(int)PropertyId.TextUnderlayColor] = AddToPropertyTable(ref propertyTable_UIColor, ColorUtil.UnsetValue);
            defaultValueIndices[(int)PropertyId.TextUnderlayDilate] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextUnderlaySoftness] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextUnderlayType] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.TextUnderlayX] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextUnderlayY] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TextVerticalAlignment] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(default));
            defaultValueIndices[(int)PropertyId.TextWhitespaceMode] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(WhitespaceMode.CollapseWhitespace | WhitespaceMode.Trim));
            defaultValueIndices[(int)PropertyId.TransformPivotX] = AddToPropertyTable(ref propertyTable_UIFixedLength, new UIFixedLength(0.5f, UIFixedUnit.Percent));
            defaultValueIndices[(int)PropertyId.TransformPivotY] = AddToPropertyTable(ref propertyTable_UIFixedLength, new UIFixedLength(0.5f, UIFixedUnit.Percent));
            defaultValueIndices[(int)PropertyId.TransformPositionX] = AddToPropertyTable(ref propertyTable_UIOffset, default);
            defaultValueIndices[(int)PropertyId.TransformPositionY] = AddToPropertyTable(ref propertyTable_UIOffset, default);
            defaultValueIndices[(int)PropertyId.TransformRotation] = AddToPropertyTable(ref propertyTable_half, (half)default);
            defaultValueIndices[(int)PropertyId.TransformScaleX] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.TransformScaleY] = AddToPropertyTable(ref propertyTable_half, (half)1f);
            defaultValueIndices[(int)PropertyId.Visibility] = AddToPropertyTable(ref propertyTable_Byte, (System.Byte)(Visibility.Visible));
            defaultValueIndices[(int)PropertyId.ZIndex] = AddToPropertyTable(ref propertyTable_UInt16, default);
            propertyTypeById = new UIForia.Util.Unsafe.DataList<PropertyType>(123, Unity.Collections.Allocator.Persistent);
            propertyTypeById.size = 123;
            propertyTypeById[(int)PropertyId.AlignmentBoundaryX] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.AlignmentBoundaryY] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.AlignmentDirectionX] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.AlignmentDirectionY] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.AlignmentOffsetX] = PropertyType.UIOffset;
            propertyTypeById[(int)PropertyId.AlignmentOffsetY] = PropertyType.UIOffset;
            propertyTypeById[(int)PropertyId.AlignmentOriginX] = PropertyType.UIOffset;
            propertyTypeById[(int)PropertyId.AlignmentOriginY] = PropertyType.UIOffset;
            propertyTypeById[(int)PropertyId.AlignmentTargetX] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.AlignmentTargetY] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.AspectRatio] = PropertyType.AspectRatio;
            propertyTypeById[(int)PropertyId.BackgroundColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.BackgroundFit] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.BackgroundImage] = PropertyType.TextureInfo;
            propertyTypeById[(int)PropertyId.BackgroundImageOffsetX] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BackgroundImageOffsetY] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BackgroundImageRotation] = PropertyType.UIAngle;
            propertyTypeById[(int)PropertyId.BackgroundImageScaleX] = PropertyType.half;
            propertyTypeById[(int)PropertyId.BackgroundImageScaleY] = PropertyType.half;
            propertyTypeById[(int)PropertyId.BackgroundImageTileX] = PropertyType.half;
            propertyTypeById[(int)PropertyId.BackgroundImageTileY] = PropertyType.half;
            propertyTypeById[(int)PropertyId.BackgroundRectMaxX] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BackgroundRectMaxY] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BackgroundRectMinX] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BackgroundRectMinY] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BackgroundTint] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.BorderBottom] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BorderColorBottom] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.BorderColorLeft] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.BorderColorRight] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.BorderColorTop] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.BorderLeft] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BorderRight] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.BorderTop] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.ClipBehavior] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.ClipBounds] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.CollapseSpaceHorizontal] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.CollapseSpaceVertical] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.CornerBevelBottomLeft] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerBevelBottomRight] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerBevelTopLeft] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerBevelTopRight] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerRadiusBottomLeft] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerRadiusBottomRight] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerRadiusTopLeft] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.CornerRadiusTopRight] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.GridItemX] = PropertyType.GridItemPlacement;
            propertyTypeById[(int)PropertyId.GridItemY] = PropertyType.GridItemPlacement;
            propertyTypeById[(int)PropertyId.GridLayoutColGap] = PropertyType.half;
            propertyTypeById[(int)PropertyId.GridLayoutColTemplate] = PropertyType.GridLayoutTemplate;
            propertyTypeById[(int)PropertyId.GridLayoutDensity] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.GridLayoutRowGap] = PropertyType.half;
            propertyTypeById[(int)PropertyId.GridLayoutRowTemplate] = PropertyType.GridLayoutTemplate;
            propertyTypeById[(int)PropertyId.Layer] = PropertyType.UInt16;
            propertyTypeById[(int)PropertyId.LayoutBehavior] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.LayoutFillOrder] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.LayoutType] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.MarginBottom] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.MarginLeft] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.MarginRight] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.MarginTop] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.MaxHeight] = PropertyType.UISizeConstraint;
            propertyTypeById[(int)PropertyId.MaxWidth] = PropertyType.UISizeConstraint;
            propertyTypeById[(int)PropertyId.MeshFillAmount] = PropertyType.half;
            propertyTypeById[(int)PropertyId.MeshFillDirection] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.MeshFillOffsetX] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.MeshFillOffsetY] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.MeshFillOrigin] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.MeshFillRadius] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.MeshFillRotation] = PropertyType.UIAngle;
            propertyTypeById[(int)PropertyId.MeshType] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.MinHeight] = PropertyType.UISizeConstraint;
            propertyTypeById[(int)PropertyId.MinWidth] = PropertyType.UISizeConstraint;
            propertyTypeById[(int)PropertyId.Opacity] = PropertyType.half;
            propertyTypeById[(int)PropertyId.OverflowX] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.OverflowY] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.PaddingBottom] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.PaddingLeft] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.PaddingRight] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.PaddingTop] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.Painter] = PropertyType.PainterId;
            propertyTypeById[(int)PropertyId.PointerEvents] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.PreferredHeight] = PropertyType.UIMeasurement;
            propertyTypeById[(int)PropertyId.PreferredWidth] = PropertyType.UIMeasurement;
            propertyTypeById[(int)PropertyId.SelectionBackgroundColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.SelectionTextColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.SpaceBetweenHorizontal] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.SpaceBetweenVertical] = PropertyType.UISpaceSize;
            propertyTypeById[(int)PropertyId.TextAlignment] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.TextColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.TextFaceDilate] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextFontAsset] = PropertyType.FontAssetId;
            propertyTypeById[(int)PropertyId.TextFontSize] = PropertyType.UIFontSize;
            propertyTypeById[(int)PropertyId.TextFontStyle] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.TextGlowColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.TextGlowInner] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextGlowOffset] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextGlowOuter] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextGlowPower] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextLineHeight] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextOutlineColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.TextOutlineSoftness] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextOutlineWidth] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextOverflow] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.TextTransform] = PropertyType.Int32;
            propertyTypeById[(int)PropertyId.TextUnderlayColor] = PropertyType.UIColor;
            propertyTypeById[(int)PropertyId.TextUnderlayDilate] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextUnderlaySoftness] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextUnderlayType] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.TextUnderlayX] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextUnderlayY] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TextVerticalAlignment] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.TextWhitespaceMode] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.TransformPivotX] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.TransformPivotY] = PropertyType.UIFixedLength;
            propertyTypeById[(int)PropertyId.TransformPositionX] = PropertyType.UIOffset;
            propertyTypeById[(int)PropertyId.TransformPositionY] = PropertyType.UIOffset;
            propertyTypeById[(int)PropertyId.TransformRotation] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TransformScaleX] = PropertyType.half;
            propertyTypeById[(int)PropertyId.TransformScaleY] = PropertyType.half;
            propertyTypeById[(int)PropertyId.Visibility] = PropertyType.Byte;
            propertyTypeById[(int)PropertyId.ZIndex] = PropertyType.UInt16;

        }

        partial void DisposeGenerated() {
            propertyTable_float2.Dispose();
            propertyTable_Single.Dispose();
            propertyTable_half.Dispose();
            propertyTable_Byte.Dispose();
            propertyTable_UInt16.Dispose();
            propertyTable_AspectRatio.Dispose();
            propertyTable_FontAssetId.Dispose();
            propertyTable_PainterId.Dispose();
            propertyTable_Int32.Dispose();
            propertyTable_TextureInfo.Dispose();
            propertyTable_UIColor.Dispose();
            propertyTable_GridItemPlacement.Dispose();
            propertyTable_GridLayoutTemplate.Dispose();
            propertyTable_UIMeasurement.Dispose();
            propertyTable_UIFixedLength.Dispose();
            propertyTable_UIOffset.Dispose();
            propertyTable_UIAngle.Dispose();
            propertyTable_UIFontSize.Dispose();
            propertyTable_UISpaceSize.Dispose();
            propertyTable_UISizeConstraint.Dispose();
            propertyTable_EnumValue.Dispose();

        }

        internal void CopyTablePointers(ref PropertyTables propertyTables) {
            propertyTables.propertyTable_float2 = propertyTable_float2;
            propertyTables.propertyTable_Single = propertyTable_Single;
            propertyTables.propertyTable_half = propertyTable_half;
            propertyTables.propertyTable_Byte = propertyTable_Byte;
            propertyTables.propertyTable_UInt16 = propertyTable_UInt16;
            propertyTables.propertyTable_AspectRatio = propertyTable_AspectRatio;
            propertyTables.propertyTable_FontAssetId = propertyTable_FontAssetId;
            propertyTables.propertyTable_PainterId = propertyTable_PainterId;
            propertyTables.propertyTable_Int32 = propertyTable_Int32;
            propertyTables.propertyTable_TextureInfo = propertyTable_TextureInfo;
            propertyTables.propertyTable_UIColor = propertyTable_UIColor;
            propertyTables.propertyTable_GridItemPlacement = propertyTable_GridItemPlacement;
            propertyTables.propertyTable_GridLayoutTemplate = propertyTable_GridLayoutTemplate;
            propertyTables.propertyTable_UIMeasurement = propertyTable_UIMeasurement;
            propertyTables.propertyTable_UIFixedLength = propertyTable_UIFixedLength;
            propertyTables.propertyTable_UIOffset = propertyTable_UIOffset;
            propertyTables.propertyTable_UIAngle = propertyTable_UIAngle;
            propertyTables.propertyTable_UIFontSize = propertyTable_UIFontSize;
            propertyTables.propertyTable_UISpaceSize = propertyTable_UISpaceSize;
            propertyTables.propertyTable_UISizeConstraint = propertyTable_UISizeConstraint;
            propertyTables.propertyTable_EnumValue = propertyTable_EnumValue;

        }

        static partial void TryParseCustomPropertyTypeName_Generated(CharSpan typeName, ref bool valid, ref CustomPropertyType propertyType) {
            if(typeName == "float2") {
                valid = true;
                propertyType = CustomPropertyType.Float2;
            }
            if(typeName == "float") {
                valid = true;
                propertyType = CustomPropertyType.Float;
            }
            if(typeName == "half") {
                valid = true;
                propertyType = CustomPropertyType.Half;
            }
            if(typeName == "byte") {
                valid = true;
                propertyType = CustomPropertyType.Byte;
            }
            if(typeName == "ushort") {
                valid = true;
                propertyType = CustomPropertyType.UShort;
            }
            if(typeName == "int") {
                valid = true;
                propertyType = CustomPropertyType.Int;
            }
            if(typeName == "Texture") {
                valid = true;
                propertyType = CustomPropertyType.Texture;
            }
            if(typeName == "Color") {
                valid = true;
                propertyType = CustomPropertyType.Color;
            }
     
        }

        bool TryRegisterCustomProperty_Float2(ModuleAndNameKey key, Unity.Mathematics.float2 defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.float2);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_float2, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.Float2Parser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_Float(ModuleAndNameKey key, float defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.Single);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_Single, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.FloatParser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_Half(ModuleAndNameKey key, Unity.Mathematics.half defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.half);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_half, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.HalfParser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_Byte(ModuleAndNameKey key, byte defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.Byte);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_Byte, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.ByteParser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_UShort(ModuleAndNameKey key, ushort defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.UInt16);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_UInt16, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.UShortParser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_Int(ModuleAndNameKey key, int defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.Int32);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_Int32, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.IntParser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_Texture(ModuleAndNameKey key, UIForia.Style.TextureInfo defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.TextureInfo);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_TextureInfo, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.TextureInfoParser.Instance); 
            return true;
        }

        bool TryRegisterCustomProperty_Color(ModuleAndNameKey key, UIForia.UIColor defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.UIColor);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_UIColor, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(UIForia.Style.UIColorParser.Instance); 
            return true;
        }



        partial void RegisterCustomProperty_Generated(ModuleAndNameKey key, CustomPropertyType propertyType, ref PropertyParseContext context, ref bool valid) {
            switch(propertyType) {
                case CustomPropertyType.Float2: {
                    if(UIForia.Style.Float2Parser.Instance.TryParse(ref context, out Unity.Mathematics.float2 defaultValue)) {
                        valid = TryRegisterCustomProperty_Float2(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.Float: {
                    if(UIForia.Style.FloatParser.Instance.TryParse(ref context, out float defaultValue)) {
                        valid = TryRegisterCustomProperty_Float(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.Half: {
                    if(UIForia.Style.HalfParser.Instance.TryParse(ref context, out Unity.Mathematics.half defaultValue)) {
                        valid = TryRegisterCustomProperty_Half(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.Byte: {
                    if(UIForia.Style.ByteParser.Instance.TryParse(ref context, out byte defaultValue)) {
                        valid = TryRegisterCustomProperty_Byte(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.UShort: {
                    if(UIForia.Style.UShortParser.Instance.TryParse(ref context, out ushort defaultValue)) {
                        valid = TryRegisterCustomProperty_UShort(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.Int: {
                    if(UIForia.Style.IntParser.Instance.TryParse(ref context, out int defaultValue)) {
                        valid = TryRegisterCustomProperty_Int(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.Texture: {
                    if(UIForia.Style.TextureInfoParser.Instance.TryParse(ref context, out UIForia.Style.TextureInfo defaultValue)) {
                        valid = TryRegisterCustomProperty_Texture(key, defaultValue);
                    }
                    break;
                }
                case CustomPropertyType.Color: {
                    if(UIForia.Style.UIColorParser.Instance.TryParse(ref context, out UIForia.UIColor defaultValue)) {
                        valid = TryRegisterCustomProperty_Color(key, defaultValue);
                    }
                    break;
                }

            }
        }
    }


    internal unsafe partial struct StyleTables {
         public UIForia.AlignmentBoundary* AlignmentBoundaryX;
         public UIForia.AlignmentBoundary* AlignmentBoundaryY;
         public UIForia.AlignmentDirection* AlignmentDirectionX;
         public UIForia.AlignmentDirection* AlignmentDirectionY;
         public UIForia.UIOffset* AlignmentOffsetX;
         public UIForia.UIOffset* AlignmentOffsetY;
         public UIForia.UIOffset* AlignmentOriginX;
         public UIForia.UIOffset* AlignmentOriginY;
         public UIForia.AlignmentTarget* AlignmentTargetX;
         public UIForia.AlignmentTarget* AlignmentTargetY;
         public UIForia.AspectRatio* AspectRatio;
         public UIForia.UIColor* BackgroundColor;
         public UIForia.Rendering.BackgroundFit* BackgroundFit;
         public UIForia.Style.TextureInfo* BackgroundImage;
         public UIForia.UIFixedLength* BackgroundImageOffsetX;
         public UIForia.UIFixedLength* BackgroundImageOffsetY;
         public UIForia.UIAngle* BackgroundImageRotation;
         public Unity.Mathematics.half* BackgroundImageScaleX;
         public Unity.Mathematics.half* BackgroundImageScaleY;
         public Unity.Mathematics.half* BackgroundImageTileX;
         public Unity.Mathematics.half* BackgroundImageTileY;
         public UIForia.UIFixedLength* BackgroundRectMaxX;
         public UIForia.UIFixedLength* BackgroundRectMaxY;
         public UIForia.UIFixedLength* BackgroundRectMinX;
         public UIForia.UIFixedLength* BackgroundRectMinY;
         public UIForia.UIColor* BackgroundTint;
         public UIForia.UIFixedLength* BorderBottom;
         public UIForia.UIColor* BorderColorBottom;
         public UIForia.UIColor* BorderColorLeft;
         public UIForia.UIColor* BorderColorRight;
         public UIForia.UIColor* BorderColorTop;
         public UIForia.UIFixedLength* BorderLeft;
         public UIForia.UIFixedLength* BorderRight;
         public UIForia.UIFixedLength* BorderTop;
         public UIForia.ClipBehavior* ClipBehavior;
         public UIForia.ClipBounds* ClipBounds;
         public UIForia.SpaceCollapse* CollapseSpaceHorizontal;
         public UIForia.SpaceCollapse* CollapseSpaceVertical;
         public UIForia.UIFixedLength* CornerBevelBottomLeft;
         public UIForia.UIFixedLength* CornerBevelBottomRight;
         public UIForia.UIFixedLength* CornerBevelTopLeft;
         public UIForia.UIFixedLength* CornerBevelTopRight;
         public UIForia.UIFixedLength* CornerRadiusBottomLeft;
         public UIForia.UIFixedLength* CornerRadiusBottomRight;
         public UIForia.UIFixedLength* CornerRadiusTopLeft;
         public UIForia.UIFixedLength* CornerRadiusTopRight;
         public UIForia.GridItemPlacement* GridItemX;
         public UIForia.GridItemPlacement* GridItemY;
         public Unity.Mathematics.half* GridLayoutColGap;
         public UIForia.GridLayoutTemplate* GridLayoutColTemplate;
         public UIForia.Layout.GridLayoutDensity* GridLayoutDensity;
         public Unity.Mathematics.half* GridLayoutRowGap;
         public UIForia.GridLayoutTemplate* GridLayoutRowTemplate;
         public ushort* Layer;
         public UIForia.LayoutBehavior* LayoutBehavior;
         public UIForia.LayoutFillOrder* LayoutFillOrder;
         public UIForia.LayoutType* LayoutType;
         public UIForia.UISpaceSize* MarginBottom;
         public UIForia.UISpaceSize* MarginLeft;
         public UIForia.UISpaceSize* MarginRight;
         public UIForia.UISpaceSize* MarginTop;
         public UIForia.UISizeConstraint* MaxHeight;
         public UIForia.UISizeConstraint* MaxWidth;
         public Unity.Mathematics.half* MeshFillAmount;
         public UIForia.Rendering.MeshFillDirection* MeshFillDirection;
         public UIForia.UIFixedLength* MeshFillOffsetX;
         public UIForia.UIFixedLength* MeshFillOffsetY;
         public UIForia.Rendering.MeshFillOrigin* MeshFillOrigin;
         public UIForia.UIFixedLength* MeshFillRadius;
         public UIForia.UIAngle* MeshFillRotation;
         public UIForia.Rendering.MeshType* MeshType;
         public UIForia.UISizeConstraint* MinHeight;
         public UIForia.UISizeConstraint* MinWidth;
         public Unity.Mathematics.half* Opacity;
         public UIForia.Overflow* OverflowX;
         public UIForia.Overflow* OverflowY;
         public UIForia.UISpaceSize* PaddingBottom;
         public UIForia.UISpaceSize* PaddingLeft;
         public UIForia.UISpaceSize* PaddingRight;
         public UIForia.UISpaceSize* PaddingTop;
         public UIForia.PainterId* Painter;
         public UIForia.PointerEvents* PointerEvents;
         public UIForia.UIMeasurement* PreferredHeight;
         public UIForia.UIMeasurement* PreferredWidth;
         public UIForia.UIColor* SelectionBackgroundColor;
         public UIForia.UIColor* SelectionTextColor;
         public UIForia.UISpaceSize* SpaceBetweenHorizontal;
         public UIForia.UISpaceSize* SpaceBetweenVertical;
         public UIForia.Text.TextAlignment* TextAlignment;
         public UIForia.UIColor* TextColor;
         public Unity.Mathematics.half* TextFaceDilate;
         public UIForia.FontAssetId* TextFontAsset;
         public UIForia.UIFontSize* TextFontSize;
         public UIForia.Text.FontStyle* TextFontStyle;
         public UIForia.UIColor* TextGlowColor;
         public Unity.Mathematics.half* TextGlowInner;
         public Unity.Mathematics.half* TextGlowOffset;
         public Unity.Mathematics.half* TextGlowOuter;
         public Unity.Mathematics.half* TextGlowPower;
         public Unity.Mathematics.half* TextLineHeight;
         public UIForia.UIColor* TextOutlineColor;
         public Unity.Mathematics.half* TextOutlineSoftness;
         public Unity.Mathematics.half* TextOutlineWidth;
         public UIForia.Text.TextOverflow* TextOverflow;
         public UIForia.Text.TextTransform* TextTransform;
         public UIForia.UIColor* TextUnderlayColor;
         public Unity.Mathematics.half* TextUnderlayDilate;
         public Unity.Mathematics.half* TextUnderlaySoftness;
         public UIForia.Rendering.UnderlayType* TextUnderlayType;
         public Unity.Mathematics.half* TextUnderlayX;
         public Unity.Mathematics.half* TextUnderlayY;
         public UIForia.Text.VerticalAlignment* TextVerticalAlignment;
         public UIForia.Text.WhitespaceMode* TextWhitespaceMode;
         public UIForia.UIFixedLength* TransformPivotX;
         public UIForia.UIFixedLength* TransformPivotY;
         public UIForia.UIOffset* TransformPositionX;
         public UIForia.UIOffset* TransformPositionY;
         public Unity.Mathematics.half* TransformRotation;
         public Unity.Mathematics.half* TransformScaleX;
         public Unity.Mathematics.half* TransformScaleY;
         public UIForia.Visibility* Visibility;
         public ushort* ZIndex;

    }

    internal unsafe partial struct PropertyTables {
        public DataList<Unity.Mathematics.float2> propertyTable_float2;
        public DataList<float> propertyTable_Single;
        public DataList<Unity.Mathematics.half> propertyTable_half;
        public DataList<byte> propertyTable_Byte;
        public DataList<ushort> propertyTable_UInt16;
        public DataList<UIForia.AspectRatio> propertyTable_AspectRatio;
        public DataList<UIForia.FontAssetId> propertyTable_FontAssetId;
        public DataList<UIForia.PainterId> propertyTable_PainterId;
        public DataList<int> propertyTable_Int32;
        public DataList<UIForia.Style.TextureInfo> propertyTable_TextureInfo;
        public DataList<UIForia.UIColor> propertyTable_UIColor;
        public DataList<UIForia.GridItemPlacement> propertyTable_GridItemPlacement;
        public DataList<UIForia.GridLayoutTemplate> propertyTable_GridLayoutTemplate;
        public DataList<UIForia.UIMeasurement> propertyTable_UIMeasurement;
        public DataList<UIForia.UIFixedLength> propertyTable_UIFixedLength;
        public DataList<UIForia.UIOffset> propertyTable_UIOffset;
        public DataList<UIForia.UIAngle> propertyTable_UIAngle;
        public DataList<UIForia.UIFontSize> propertyTable_UIFontSize;
        public DataList<UIForia.UISpaceSize> propertyTable_UISpaceSize;
        public DataList<UIForia.UISizeConstraint> propertyTable_UISizeConstraint;
        public DataList<EnumValue> propertyTable_EnumValue;

    }

    internal unsafe partial struct PropertySolverGroup_LayoutBehaviorTypeFontSize {

        private int elementCapacity;

        public PropertySolverInfo_Byte LayoutType;
        public PropertySolverInfo_Byte LayoutBehavior;
        public PropertySolverInfo_UIFontSize TextFontSize;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 3;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 71: // LayoutType
                    outputIdx =  0;
                    return;
                case 69: // LayoutBehavior
                    outputIdx =  1;
                    return;
                case 7: // TextFontSize
                    outputIdx =  2;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->LayoutType, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->LayoutBehavior, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextFontSize, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            LayoutType.enumTypeId = 1;
            LayoutBehavior.enumTypeId = 2;
            TextFontSize.enumTypeId = 0;
            TextFontSize.isImplicitInherit = true;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref LayoutType, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->LayoutType, 0, (PropertyId)71, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref LayoutBehavior, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->LayoutBehavior, 1, (PropertyId)69, bumpAllocator, false);
            PropertySolver_UIFontSize.Invoke(ref TextFontSize, parameters, ref context, propertyTables->propertyTable_UIFontSize, styleTables->TextFontSize, 2, (PropertyId)7, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_TextMeasurement {

        private int elementCapacity;

        public PropertySolverInfo_Byte TextWhitespaceMode;
        public PropertySolverInfo_Byte TextFontStyle;
        public PropertySolverInfo_Byte TextAlignment;
        public PropertySolverInfo_Byte TextVerticalAlignment;
        public PropertySolverInfo_Byte TextOverflow;
        public PropertySolverInfo_FontAssetId TextFontAsset;
        public PropertySolverInfo_Int32 TextTransform;
        public PropertySolverInfo_half TextUnderlayDilate;
        public PropertySolverInfo_half TextUnderlayY;
        public PropertySolverInfo_half TextUnderlayX;
        public PropertySolverInfo_half TextGlowPower;
        public PropertySolverInfo_half TextGlowOuter;
        public PropertySolverInfo_half TextOutlineSoftness;
        public PropertySolverInfo_half TextGlowOffset;
        public PropertySolverInfo_half TextUnderlaySoftness;
        public PropertySolverInfo_half TextOutlineWidth;
        public PropertySolverInfo_half TextLineHeight;
        public PropertySolverInfo_half TextGlowInner;
        public PropertySolverInfo_half TextFaceDilate;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 19;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 13: // TextWhitespaceMode
                    outputIdx =  0;
                    return;
                case 8: // TextFontStyle
                    outputIdx =  1;
                    return;
                case 4: // TextAlignment
                    outputIdx =  2;
                    return;
                case 12: // TextVerticalAlignment
                    outputIdx =  3;
                    return;
                case 10: // TextOverflow
                    outputIdx =  4;
                    return;
                case 6: // TextFontAsset
                    outputIdx =  5;
                    return;
                case 11: // TextTransform
                    outputIdx =  6;
                    return;
                case 111: // TextUnderlayDilate
                    outputIdx =  7;
                    return;
                case 115: // TextUnderlayY
                    outputIdx =  8;
                    return;
                case 114: // TextUnderlayX
                    outputIdx =  9;
                    return;
                case 106: // TextGlowPower
                    outputIdx =  10;
                    return;
                case 105: // TextGlowOuter
                    outputIdx =  11;
                    return;
                case 108: // TextOutlineSoftness
                    outputIdx =  12;
                    return;
                case 104: // TextGlowOffset
                    outputIdx =  13;
                    return;
                case 112: // TextUnderlaySoftness
                    outputIdx =  14;
                    return;
                case 109: // TextOutlineWidth
                    outputIdx =  15;
                    return;
                case 9: // TextLineHeight
                    outputIdx =  16;
                    return;
                case 103: // TextGlowInner
                    outputIdx =  17;
                    return;
                case 101: // TextFaceDilate
                    outputIdx =  18;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->TextWhitespaceMode, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextFontStyle, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextAlignment, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextVerticalAlignment, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextOverflow, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextFontAsset, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextTransform, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextUnderlayDilate, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextUnderlayY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextUnderlayX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextGlowPower, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextGlowOuter, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextOutlineSoftness, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextGlowOffset, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextUnderlaySoftness, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextOutlineWidth, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextLineHeight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextGlowInner, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextFaceDilate, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            TextWhitespaceMode.enumTypeId = 3;
            TextWhitespaceMode.isImplicitInherit = true;
            TextFontStyle.enumTypeId = 5;
            TextFontStyle.isImplicitInherit = true;
            TextAlignment.enumTypeId = 6;
            TextAlignment.isImplicitInherit = true;
            TextVerticalAlignment.enumTypeId = 7;
            TextVerticalAlignment.isImplicitInherit = true;
            TextOverflow.enumTypeId = 8;
            TextOverflow.isImplicitInherit = true;
            TextFontAsset.enumTypeId = 0;
            TextFontAsset.isImplicitInherit = true;
            TextTransform.enumTypeId = 4;
            TextTransform.isImplicitInherit = true;
            TextUnderlayDilate.enumTypeId = 0;
            TextUnderlayY.enumTypeId = 0;
            TextUnderlayX.enumTypeId = 0;
            TextGlowPower.enumTypeId = 0;
            TextGlowOuter.enumTypeId = 0;
            TextOutlineSoftness.enumTypeId = 0;
            TextGlowOffset.enumTypeId = 0;
            TextUnderlaySoftness.enumTypeId = 0;
            TextOutlineWidth.enumTypeId = 0;
            TextLineHeight.enumTypeId = 0;
            TextLineHeight.isImplicitInherit = true;
            TextGlowInner.enumTypeId = 0;
            TextFaceDilate.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref TextWhitespaceMode, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->TextWhitespaceMode, 0, (PropertyId)13, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref TextFontStyle, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->TextFontStyle, 1, (PropertyId)8, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref TextAlignment, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->TextAlignment, 2, (PropertyId)4, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref TextVerticalAlignment, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->TextVerticalAlignment, 3, (PropertyId)12, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref TextOverflow, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->TextOverflow, 4, (PropertyId)10, bumpAllocator, false);
            PropertySolver_FontAssetId.Invoke(ref TextFontAsset, parameters, ref context, propertyTables->propertyTable_FontAssetId, styleTables->TextFontAsset, 5, (PropertyId)6, bumpAllocator, false);
            PropertySolver_Int32.Invoke(ref TextTransform, parameters, ref context, propertyTables->propertyTable_Int32, (Int32*)styleTables->TextTransform, 6, (PropertyId)11, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextUnderlayDilate, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextUnderlayDilate, 7, (PropertyId)111, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextUnderlayY, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextUnderlayY, 8, (PropertyId)115, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextUnderlayX, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextUnderlayX, 9, (PropertyId)114, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextGlowPower, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextGlowPower, 10, (PropertyId)106, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextGlowOuter, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextGlowOuter, 11, (PropertyId)105, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextOutlineSoftness, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextOutlineSoftness, 12, (PropertyId)108, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextGlowOffset, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextGlowOffset, 13, (PropertyId)104, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextUnderlaySoftness, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextUnderlaySoftness, 14, (PropertyId)112, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextOutlineWidth, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextOutlineWidth, 15, (PropertyId)109, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextLineHeight, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextLineHeight, 16, (PropertyId)9, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextGlowInner, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextGlowInner, 17, (PropertyId)103, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TextFaceDilate, parameters, ref context, propertyTables->propertyTable_half, styleTables->TextFaceDilate, 18, (PropertyId)101, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_HorizontalSpacing {

        private int elementCapacity;

        public PropertySolverInfo_Byte CollapseSpaceHorizontal;
        public PropertySolverInfo_UISpaceSize MarginRight;
        public PropertySolverInfo_UISpaceSize MarginLeft;
        public PropertySolverInfo_UISpaceSize PaddingRight;
        public PropertySolverInfo_UISpaceSize PaddingLeft;
        public PropertySolverInfo_UISpaceSize SpaceBetweenHorizontal;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 6;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 52: // CollapseSpaceHorizontal
                    outputIdx =  0;
                    return;
                case 74: // MarginRight
                    outputIdx =  1;
                    return;
                case 73: // MarginLeft
                    outputIdx =  2;
                    return;
                case 92: // PaddingRight
                    outputIdx =  3;
                    return;
                case 91: // PaddingLeft
                    outputIdx =  4;
                    return;
                case 99: // SpaceBetweenHorizontal
                    outputIdx =  5;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->CollapseSpaceHorizontal, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MarginRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MarginLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PaddingRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PaddingLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->SpaceBetweenHorizontal, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            CollapseSpaceHorizontal.enumTypeId = 9;
            MarginRight.enumTypeId = 0;
            MarginLeft.enumTypeId = 0;
            PaddingRight.enumTypeId = 0;
            PaddingLeft.enumTypeId = 0;
            SpaceBetweenHorizontal.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref CollapseSpaceHorizontal, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->CollapseSpaceHorizontal, 0, (PropertyId)52, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref MarginRight, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->MarginRight, 1, (PropertyId)74, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref MarginLeft, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->MarginLeft, 2, (PropertyId)73, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref PaddingRight, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->PaddingRight, 3, (PropertyId)92, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref PaddingLeft, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->PaddingLeft, 4, (PropertyId)91, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref SpaceBetweenHorizontal, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->SpaceBetweenHorizontal, 5, (PropertyId)99, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_VerticalSpacing {

        private int elementCapacity;

        public PropertySolverInfo_Byte CollapseSpaceVertical;
        public PropertySolverInfo_UISpaceSize MarginTop;
        public PropertySolverInfo_UISpaceSize MarginBottom;
        public PropertySolverInfo_UISpaceSize PaddingTop;
        public PropertySolverInfo_UISpaceSize PaddingBottom;
        public PropertySolverInfo_UISpaceSize SpaceBetweenVertical;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 6;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 53: // CollapseSpaceVertical
                    outputIdx =  0;
                    return;
                case 75: // MarginTop
                    outputIdx =  1;
                    return;
                case 72: // MarginBottom
                    outputIdx =  2;
                    return;
                case 93: // PaddingTop
                    outputIdx =  3;
                    return;
                case 90: // PaddingBottom
                    outputIdx =  4;
                    return;
                case 100: // SpaceBetweenVertical
                    outputIdx =  5;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->CollapseSpaceVertical, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MarginTop, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MarginBottom, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PaddingTop, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PaddingBottom, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->SpaceBetweenVertical, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            CollapseSpaceVertical.enumTypeId = 9;
            MarginTop.enumTypeId = 0;
            MarginBottom.enumTypeId = 0;
            PaddingTop.enumTypeId = 0;
            PaddingBottom.enumTypeId = 0;
            SpaceBetweenVertical.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref CollapseSpaceVertical, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->CollapseSpaceVertical, 0, (PropertyId)53, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref MarginTop, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->MarginTop, 1, (PropertyId)75, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref MarginBottom, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->MarginBottom, 2, (PropertyId)72, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref PaddingTop, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->PaddingTop, 3, (PropertyId)93, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref PaddingBottom, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->PaddingBottom, 4, (PropertyId)90, bumpAllocator, false);
            PropertySolver_UISpaceSize.Invoke(ref SpaceBetweenVertical, parameters, ref context, propertyTables->propertyTable_UISpaceSize, styleTables->SpaceBetweenVertical, 5, (PropertyId)100, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_LayoutSizes {

        private int elementCapacity;

        public PropertySolverInfo_AspectRatio AspectRatio;
        public PropertySolverInfo_TextureInfo BackgroundImage;
        public PropertySolverInfo_UIMeasurement PreferredWidth;
        public PropertySolverInfo_UIMeasurement PreferredHeight;
        public PropertySolverInfo_UISizeConstraint MinWidth;
        public PropertySolverInfo_UISizeConstraint MinHeight;
        public PropertySolverInfo_UISizeConstraint MaxWidth;
        public PropertySolverInfo_UISizeConstraint MaxHeight;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 8;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 26: // AspectRatio
                    outputIdx =  0;
                    return;
                case 29: // BackgroundImage
                    outputIdx =  1;
                    return;
                case 96: // PreferredWidth
                    outputIdx =  2;
                    return;
                case 95: // PreferredHeight
                    outputIdx =  3;
                    return;
                case 87: // MinWidth
                    outputIdx =  4;
                    return;
                case 86: // MinHeight
                    outputIdx =  5;
                    return;
                case 77: // MaxWidth
                    outputIdx =  6;
                    return;
                case 76: // MaxHeight
                    outputIdx =  7;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->AspectRatio, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImage, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PreferredWidth, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PreferredHeight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MinWidth, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MinHeight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MaxWidth, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MaxHeight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            AspectRatio.enumTypeId = 0;
            BackgroundImage.enumTypeId = 0;
            PreferredWidth.enumTypeId = 0;
            PreferredHeight.enumTypeId = 0;
            MinWidth.enumTypeId = 0;
            MinHeight.enumTypeId = 0;
            MaxWidth.enumTypeId = 0;
            MaxHeight.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_AspectRatio.Invoke(ref AspectRatio, parameters, ref context, propertyTables->propertyTable_AspectRatio, styleTables->AspectRatio, 0, (PropertyId)26, bumpAllocator, false);
            PropertySolver_TextureInfo.Invoke(ref BackgroundImage, parameters, ref context, propertyTables->propertyTable_TextureInfo, styleTables->BackgroundImage, 1, (PropertyId)29, bumpAllocator, false);
            PropertySolver_UIMeasurement.Invoke(ref PreferredWidth, parameters, ref context, propertyTables->propertyTable_UIMeasurement, styleTables->PreferredWidth, 2, (PropertyId)96, bumpAllocator, true);
            PropertySolver_UIMeasurement.Invoke(ref PreferredHeight, parameters, ref context, propertyTables->propertyTable_UIMeasurement, styleTables->PreferredHeight, 3, (PropertyId)95, bumpAllocator, true);
            PropertySolver_UISizeConstraint.Invoke(ref MinWidth, parameters, ref context, propertyTables->propertyTable_UISizeConstraint, styleTables->MinWidth, 4, (PropertyId)87, bumpAllocator, false);
            PropertySolver_UISizeConstraint.Invoke(ref MinHeight, parameters, ref context, propertyTables->propertyTable_UISizeConstraint, styleTables->MinHeight, 5, (PropertyId)86, bumpAllocator, false);
            PropertySolver_UISizeConstraint.Invoke(ref MaxWidth, parameters, ref context, propertyTables->propertyTable_UISizeConstraint, styleTables->MaxWidth, 6, (PropertyId)77, bumpAllocator, false);
            PropertySolver_UISizeConstraint.Invoke(ref MaxHeight, parameters, ref context, propertyTables->propertyTable_UISizeConstraint, styleTables->MaxHeight, 7, (PropertyId)76, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_LayoutSetup {

        private int elementCapacity;

        public PropertySolverInfo_Byte LayoutFillOrder;
        public PropertySolverInfo_Byte GridLayoutDensity;
        public PropertySolverInfo_GridItemPlacement GridItemX;
        public PropertySolverInfo_GridItemPlacement GridItemY;
        public PropertySolverInfo_GridLayoutTemplate GridLayoutColTemplate;
        public PropertySolverInfo_GridLayoutTemplate GridLayoutRowTemplate;
        public PropertySolverInfo_UIFixedLength BorderTop;
        public PropertySolverInfo_UIFixedLength BorderRight;
        public PropertySolverInfo_UIFixedLength BorderBottom;
        public PropertySolverInfo_UIFixedLength BorderLeft;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 10;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 70: // LayoutFillOrder
                    outputIdx =  0;
                    return;
                case 66: // GridLayoutDensity
                    outputIdx =  1;
                    return;
                case 62: // GridItemX
                    outputIdx =  2;
                    return;
                case 63: // GridItemY
                    outputIdx =  3;
                    return;
                case 65: // GridLayoutColTemplate
                    outputIdx =  4;
                    return;
                case 68: // GridLayoutRowTemplate
                    outputIdx =  5;
                    return;
                case 49: // BorderTop
                    outputIdx =  6;
                    return;
                case 48: // BorderRight
                    outputIdx =  7;
                    return;
                case 42: // BorderBottom
                    outputIdx =  8;
                    return;
                case 47: // BorderLeft
                    outputIdx =  9;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->LayoutFillOrder, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridLayoutDensity, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridItemX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridItemY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridLayoutColTemplate, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridLayoutRowTemplate, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderTop, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderBottom, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            LayoutFillOrder.enumTypeId = 10;
            GridLayoutDensity.enumTypeId = 11;
            GridItemX.enumTypeId = 0;
            GridItemY.enumTypeId = 0;
            GridLayoutColTemplate.enumTypeId = 0;
            GridLayoutRowTemplate.enumTypeId = 0;
            BorderTop.enumTypeId = 0;
            BorderRight.enumTypeId = 0;
            BorderBottom.enumTypeId = 0;
            BorderLeft.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref LayoutFillOrder, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->LayoutFillOrder, 0, (PropertyId)70, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref GridLayoutDensity, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->GridLayoutDensity, 1, (PropertyId)66, bumpAllocator, false);
            PropertySolver_GridItemPlacement.Invoke(ref GridItemX, parameters, ref context, propertyTables->propertyTable_GridItemPlacement, styleTables->GridItemX, 2, (PropertyId)62, bumpAllocator, false);
            PropertySolver_GridItemPlacement.Invoke(ref GridItemY, parameters, ref context, propertyTables->propertyTable_GridItemPlacement, styleTables->GridItemY, 3, (PropertyId)63, bumpAllocator, false);
            PropertySolver_GridLayoutTemplate.Invoke(ref GridLayoutColTemplate, parameters, ref context, propertyTables->propertyTable_GridLayoutTemplate, styleTables->GridLayoutColTemplate, 4, (PropertyId)65, bumpAllocator, false);
            PropertySolver_GridLayoutTemplate.Invoke(ref GridLayoutRowTemplate, parameters, ref context, propertyTables->propertyTable_GridLayoutTemplate, styleTables->GridLayoutRowTemplate, 5, (PropertyId)68, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BorderTop, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BorderTop, 6, (PropertyId)49, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BorderRight, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BorderRight, 7, (PropertyId)48, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BorderBottom, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BorderBottom, 8, (PropertyId)42, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BorderLeft, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BorderLeft, 9, (PropertyId)47, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_LayoutAndText {

        private int elementCapacity;

        public PropertySolverInfo_UIFixedLength BackgroundRectMinX;
        public PropertySolverInfo_UIFixedLength BackgroundRectMinY;
        public PropertySolverInfo_UIFixedLength BackgroundRectMaxX;
        public PropertySolverInfo_UIFixedLength BackgroundRectMaxY;
        public PropertySolverInfo_half GridLayoutColGap;
        public PropertySolverInfo_half GridLayoutRowGap;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 6;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 39: // BackgroundRectMinX
                    outputIdx =  0;
                    return;
                case 40: // BackgroundRectMinY
                    outputIdx =  1;
                    return;
                case 37: // BackgroundRectMaxX
                    outputIdx =  2;
                    return;
                case 38: // BackgroundRectMaxY
                    outputIdx =  3;
                    return;
                case 64: // GridLayoutColGap
                    outputIdx =  4;
                    return;
                case 67: // GridLayoutRowGap
                    outputIdx =  5;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundRectMinX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundRectMinY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundRectMaxX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundRectMaxY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridLayoutColGap, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->GridLayoutRowGap, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            BackgroundRectMinX.enumTypeId = 0;
            BackgroundRectMinY.enumTypeId = 0;
            BackgroundRectMaxX.enumTypeId = 0;
            BackgroundRectMaxY.enumTypeId = 0;
            GridLayoutColGap.enumTypeId = 0;
            GridLayoutRowGap.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_UIFixedLength.Invoke(ref BackgroundRectMinX, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BackgroundRectMinX, 0, (PropertyId)39, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BackgroundRectMinY, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BackgroundRectMinY, 1, (PropertyId)40, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BackgroundRectMaxX, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BackgroundRectMaxX, 2, (PropertyId)37, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BackgroundRectMaxY, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BackgroundRectMaxY, 3, (PropertyId)38, bumpAllocator, false);
            PropertySolver_half.Invoke(ref GridLayoutColGap, parameters, ref context, propertyTables->propertyTable_half, styleTables->GridLayoutColGap, 4, (PropertyId)64, bumpAllocator, false);
            PropertySolver_half.Invoke(ref GridLayoutRowGap, parameters, ref context, propertyTables->propertyTable_half, styleTables->GridLayoutRowGap, 5, (PropertyId)67, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_ClippingAndTransformation {

        private int elementCapacity;

        public PropertySolverInfo_Byte Visibility;
        public PropertySolverInfo_Byte OverflowX;
        public PropertySolverInfo_Byte OverflowY;
        public PropertySolverInfo_Byte ClipBehavior;
        public PropertySolverInfo_Byte ClipBounds;
        public PropertySolverInfo_Byte PointerEvents;
        public PropertySolverInfo_Byte AlignmentTargetY;
        public PropertySolverInfo_Byte AlignmentBoundaryY;
        public PropertySolverInfo_Byte AlignmentTargetX;
        public PropertySolverInfo_Byte AlignmentBoundaryX;
        public PropertySolverInfo_Byte AlignmentDirectionX;
        public PropertySolverInfo_Byte AlignmentDirectionY;
        public PropertySolverInfo_UIFixedLength TransformPivotY;
        public PropertySolverInfo_UIFixedLength TransformPivotX;
        public PropertySolverInfo_UIOffset AlignmentOriginY;
        public PropertySolverInfo_UIOffset AlignmentOriginX;
        public PropertySolverInfo_UIOffset AlignmentOffsetY;
        public PropertySolverInfo_UIOffset AlignmentOffsetX;
        public PropertySolverInfo_UIOffset TransformPositionY;
        public PropertySolverInfo_UIOffset TransformPositionX;
        public PropertySolverInfo_UInt16 ZIndex;
        public PropertySolverInfo_UInt16 Layer;
        public PropertySolverInfo_half TransformScaleY;
        public PropertySolverInfo_half TransformScaleX;
        public PropertySolverInfo_half TransformRotation;
        public PropertySolverInfo_half Opacity;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 26;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 14: // Visibility
                    outputIdx =  0;
                    return;
                case 88: // OverflowX
                    outputIdx =  1;
                    return;
                case 89: // OverflowY
                    outputIdx =  2;
                    return;
                case 50: // ClipBehavior
                    outputIdx =  3;
                    return;
                case 51: // ClipBounds
                    outputIdx =  4;
                    return;
                case 3: // PointerEvents
                    outputIdx =  5;
                    return;
                case 25: // AlignmentTargetY
                    outputIdx =  6;
                    return;
                case 17: // AlignmentBoundaryY
                    outputIdx =  7;
                    return;
                case 24: // AlignmentTargetX
                    outputIdx =  8;
                    return;
                case 16: // AlignmentBoundaryX
                    outputIdx =  9;
                    return;
                case 18: // AlignmentDirectionX
                    outputIdx =  10;
                    return;
                case 19: // AlignmentDirectionY
                    outputIdx =  11;
                    return;
                case 117: // TransformPivotY
                    outputIdx =  12;
                    return;
                case 116: // TransformPivotX
                    outputIdx =  13;
                    return;
                case 23: // AlignmentOriginY
                    outputIdx =  14;
                    return;
                case 22: // AlignmentOriginX
                    outputIdx =  15;
                    return;
                case 21: // AlignmentOffsetY
                    outputIdx =  16;
                    return;
                case 20: // AlignmentOffsetX
                    outputIdx =  17;
                    return;
                case 119: // TransformPositionY
                    outputIdx =  18;
                    return;
                case 118: // TransformPositionX
                    outputIdx =  19;
                    return;
                case 15: // ZIndex
                    outputIdx =  20;
                    return;
                case 1: // Layer
                    outputIdx =  21;
                    return;
                case 122: // TransformScaleY
                    outputIdx =  22;
                    return;
                case 121: // TransformScaleX
                    outputIdx =  23;
                    return;
                case 120: // TransformRotation
                    outputIdx =  24;
                    return;
                case 2: // Opacity
                    outputIdx =  25;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->Visibility, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->OverflowX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->OverflowY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->ClipBehavior, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->ClipBounds, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->PointerEvents, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentTargetY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentBoundaryY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentTargetX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentBoundaryX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentDirectionX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentDirectionY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformPivotY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformPivotX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentOriginY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentOriginX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentOffsetY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->AlignmentOffsetX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformPositionY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformPositionX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->ZIndex, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->Layer, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformScaleY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformScaleX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TransformRotation, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->Opacity, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            Visibility.enumTypeId = 12;
            Visibility.isImplicitInherit = true;
            OverflowX.enumTypeId = 13;
            OverflowY.enumTypeId = 13;
            ClipBehavior.enumTypeId = 14;
            ClipBounds.enumTypeId = 15;
            PointerEvents.enumTypeId = 16;
            PointerEvents.isImplicitInherit = true;
            AlignmentTargetY.enumTypeId = 18;
            AlignmentBoundaryY.enumTypeId = 19;
            AlignmentTargetX.enumTypeId = 18;
            AlignmentBoundaryX.enumTypeId = 19;
            AlignmentDirectionX.enumTypeId = 17;
            AlignmentDirectionY.enumTypeId = 17;
            TransformPivotY.enumTypeId = 0;
            TransformPivotX.enumTypeId = 0;
            AlignmentOriginY.enumTypeId = 0;
            AlignmentOriginX.enumTypeId = 0;
            AlignmentOffsetY.enumTypeId = 0;
            AlignmentOffsetX.enumTypeId = 0;
            TransformPositionY.enumTypeId = 0;
            TransformPositionX.enumTypeId = 0;
            ZIndex.enumTypeId = 0;
            ZIndex.isImplicitInherit = true;
            Layer.enumTypeId = 0;
            Layer.isImplicitInherit = true;
            TransformScaleY.enumTypeId = 0;
            TransformScaleX.enumTypeId = 0;
            TransformRotation.enumTypeId = 0;
            Opacity.enumTypeId = 0;
            Opacity.isImplicitInherit = true;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref Visibility, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->Visibility, 0, (PropertyId)14, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref OverflowX, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->OverflowX, 1, (PropertyId)88, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref OverflowY, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->OverflowY, 2, (PropertyId)89, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref ClipBehavior, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->ClipBehavior, 3, (PropertyId)50, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref ClipBounds, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->ClipBounds, 4, (PropertyId)51, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref PointerEvents, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->PointerEvents, 5, (PropertyId)3, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref AlignmentTargetY, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->AlignmentTargetY, 6, (PropertyId)25, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref AlignmentBoundaryY, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->AlignmentBoundaryY, 7, (PropertyId)17, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref AlignmentTargetX, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->AlignmentTargetX, 8, (PropertyId)24, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref AlignmentBoundaryX, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->AlignmentBoundaryX, 9, (PropertyId)16, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref AlignmentDirectionX, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->AlignmentDirectionX, 10, (PropertyId)18, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref AlignmentDirectionY, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->AlignmentDirectionY, 11, (PropertyId)19, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref TransformPivotY, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->TransformPivotY, 12, (PropertyId)117, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref TransformPivotX, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->TransformPivotX, 13, (PropertyId)116, bumpAllocator, false);
            PropertySolver_UIOffset.Invoke(ref AlignmentOriginY, parameters, ref context, propertyTables->propertyTable_UIOffset, styleTables->AlignmentOriginY, 14, (PropertyId)23, bumpAllocator, false);
            PropertySolver_UIOffset.Invoke(ref AlignmentOriginX, parameters, ref context, propertyTables->propertyTable_UIOffset, styleTables->AlignmentOriginX, 15, (PropertyId)22, bumpAllocator, false);
            PropertySolver_UIOffset.Invoke(ref AlignmentOffsetY, parameters, ref context, propertyTables->propertyTable_UIOffset, styleTables->AlignmentOffsetY, 16, (PropertyId)21, bumpAllocator, false);
            PropertySolver_UIOffset.Invoke(ref AlignmentOffsetX, parameters, ref context, propertyTables->propertyTable_UIOffset, styleTables->AlignmentOffsetX, 17, (PropertyId)20, bumpAllocator, false);
            PropertySolver_UIOffset.Invoke(ref TransformPositionY, parameters, ref context, propertyTables->propertyTable_UIOffset, styleTables->TransformPositionY, 18, (PropertyId)119, bumpAllocator, false);
            PropertySolver_UIOffset.Invoke(ref TransformPositionX, parameters, ref context, propertyTables->propertyTable_UIOffset, styleTables->TransformPositionX, 19, (PropertyId)118, bumpAllocator, false);
            PropertySolver_UInt16.Invoke(ref ZIndex, parameters, ref context, propertyTables->propertyTable_UInt16, styleTables->ZIndex, 20, (PropertyId)15, bumpAllocator, false);
            PropertySolver_UInt16.Invoke(ref Layer, parameters, ref context, propertyTables->propertyTable_UInt16, styleTables->Layer, 21, (PropertyId)1, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TransformScaleY, parameters, ref context, propertyTables->propertyTable_half, styleTables->TransformScaleY, 22, (PropertyId)122, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TransformScaleX, parameters, ref context, propertyTables->propertyTable_half, styleTables->TransformScaleX, 23, (PropertyId)121, bumpAllocator, false);
            PropertySolver_half.Invoke(ref TransformRotation, parameters, ref context, propertyTables->propertyTable_half, styleTables->TransformRotation, 24, (PropertyId)120, bumpAllocator, false);
            PropertySolver_half.Invoke(ref Opacity, parameters, ref context, propertyTables->propertyTable_half, styleTables->Opacity, 25, (PropertyId)2, bumpAllocator, false);
        }
    }

    internal unsafe partial struct PropertySolverGroup_Rendering {

        private int elementCapacity;

        public PropertySolverInfo_Byte TextUnderlayType;
        public PropertySolverInfo_Byte MeshFillOrigin;
        public PropertySolverInfo_Byte MeshFillDirection;
        public PropertySolverInfo_Byte MeshType;
        public PropertySolverInfo_Byte BackgroundFit;
        public PropertySolverInfo_PainterId Painter;
        public PropertySolverInfo_UIAngle MeshFillRotation;
        public PropertySolverInfo_UIAngle BackgroundImageRotation;
        public PropertySolverInfo_UIColor BorderColorLeft;
        public PropertySolverInfo_UIColor BorderColorBottom;
        public PropertySolverInfo_UIColor BorderColorRight;
        public PropertySolverInfo_UIColor BorderColorTop;
        public PropertySolverInfo_UIColor TextColor;
        public PropertySolverInfo_UIColor TextOutlineColor;
        public PropertySolverInfo_UIColor SelectionTextColor;
        public PropertySolverInfo_UIColor BackgroundTint;
        public PropertySolverInfo_UIColor SelectionBackgroundColor;
        public PropertySolverInfo_UIColor BackgroundColor;
        public PropertySolverInfo_UIColor TextGlowColor;
        public PropertySolverInfo_UIColor TextUnderlayColor;
        public PropertySolverInfo_UIFixedLength CornerRadiusBottomRight;
        public PropertySolverInfo_UIFixedLength MeshFillRadius;
        public PropertySolverInfo_UIFixedLength CornerBevelBottomLeft;
        public PropertySolverInfo_UIFixedLength CornerBevelBottomRight;
        public PropertySolverInfo_UIFixedLength CornerBevelTopRight;
        public PropertySolverInfo_UIFixedLength CornerBevelTopLeft;
        public PropertySolverInfo_UIFixedLength MeshFillOffsetY;
        public PropertySolverInfo_UIFixedLength BackgroundImageOffsetY;
        public PropertySolverInfo_UIFixedLength CornerRadiusTopRight;
        public PropertySolverInfo_UIFixedLength CornerRadiusTopLeft;
        public PropertySolverInfo_UIFixedLength MeshFillOffsetX;
        public PropertySolverInfo_UIFixedLength BackgroundImageOffsetX;
        public PropertySolverInfo_UIFixedLength CornerRadiusBottomLeft;
        public PropertySolverInfo_half BackgroundImageTileY;
        public PropertySolverInfo_half MeshFillAmount;
        public PropertySolverInfo_half BackgroundImageTileX;
        public PropertySolverInfo_half BackgroundImageScaleY;
        public PropertySolverInfo_half BackgroundImageScaleX;

        partial void GetPropertyCount(ref int propertyCount) {
            propertyCount = 38;
        }


        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx) {
            switch(propertyId) {
                case 113: // TextUnderlayType
                    outputIdx =  0;
                    return;
                case 82: // MeshFillOrigin
                    outputIdx =  1;
                    return;
                case 79: // MeshFillDirection
                    outputIdx =  2;
                    return;
                case 85: // MeshType
                    outputIdx =  3;
                    return;
                case 28: // BackgroundFit
                    outputIdx =  4;
                    return;
                case 94: // Painter
                    outputIdx =  5;
                    return;
                case 84: // MeshFillRotation
                    outputIdx =  6;
                    return;
                case 32: // BackgroundImageRotation
                    outputIdx =  7;
                    return;
                case 44: // BorderColorLeft
                    outputIdx =  8;
                    return;
                case 43: // BorderColorBottom
                    outputIdx =  9;
                    return;
                case 45: // BorderColorRight
                    outputIdx =  10;
                    return;
                case 46: // BorderColorTop
                    outputIdx =  11;
                    return;
                case 5: // TextColor
                    outputIdx =  12;
                    return;
                case 107: // TextOutlineColor
                    outputIdx =  13;
                    return;
                case 98: // SelectionTextColor
                    outputIdx =  14;
                    return;
                case 41: // BackgroundTint
                    outputIdx =  15;
                    return;
                case 97: // SelectionBackgroundColor
                    outputIdx =  16;
                    return;
                case 27: // BackgroundColor
                    outputIdx =  17;
                    return;
                case 102: // TextGlowColor
                    outputIdx =  18;
                    return;
                case 110: // TextUnderlayColor
                    outputIdx =  19;
                    return;
                case 59: // CornerRadiusBottomRight
                    outputIdx =  20;
                    return;
                case 83: // MeshFillRadius
                    outputIdx =  21;
                    return;
                case 54: // CornerBevelBottomLeft
                    outputIdx =  22;
                    return;
                case 55: // CornerBevelBottomRight
                    outputIdx =  23;
                    return;
                case 57: // CornerBevelTopRight
                    outputIdx =  24;
                    return;
                case 56: // CornerBevelTopLeft
                    outputIdx =  25;
                    return;
                case 81: // MeshFillOffsetY
                    outputIdx =  26;
                    return;
                case 31: // BackgroundImageOffsetY
                    outputIdx =  27;
                    return;
                case 61: // CornerRadiusTopRight
                    outputIdx =  28;
                    return;
                case 60: // CornerRadiusTopLeft
                    outputIdx =  29;
                    return;
                case 80: // MeshFillOffsetX
                    outputIdx =  30;
                    return;
                case 30: // BackgroundImageOffsetX
                    outputIdx =  31;
                    return;
                case 58: // CornerRadiusBottomLeft
                    outputIdx =  32;
                    return;
                case 36: // BackgroundImageTileY
                    outputIdx =  33;
                    return;
                case 78: // MeshFillAmount
                    outputIdx =  34;
                    return;
                case 35: // BackgroundImageTileX
                    outputIdx =  35;
                    return;
                case 34: // BackgroundImageScaleY
                    outputIdx =  36;
                    return;
                case 33: // BackgroundImageScaleX
                    outputIdx =  37;
                    return;
                default: 
                    outputIdx = -1;
                    return;
}
        }

        private void EnsureTableCapacities(StyleTables * styleTables, int requiredElementCount) {
            // requiredElementCapacity = MathUtil.NextMultiple();
            if (requiredElementCount <= elementCapacity) return;
             // todo -- use one larger allocation for this, but need to make sure alignments are maintained
            TypedUnsafe.ResizeCleared(ref styleTables->TextUnderlayType, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillOrigin, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillDirection, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshType, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundFit, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->Painter, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillRotation, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageRotation, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderColorLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderColorBottom, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderColorRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BorderColorTop, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextOutlineColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->SelectionTextColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundTint, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->SelectionBackgroundColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextGlowColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->TextUnderlayColor, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerRadiusBottomRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillRadius, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerBevelBottomLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerBevelBottomRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerBevelTopRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerBevelTopLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillOffsetY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageOffsetY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerRadiusTopRight, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerRadiusTopLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillOffsetX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageOffsetX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->CornerRadiusBottomLeft, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageTileY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->MeshFillAmount, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageTileX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageScaleY, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            TypedUnsafe.ResizeCleared(ref styleTables->BackgroundImageScaleX, elementCapacity, requiredElementCount, Unity.Collections.Allocator.Persistent);
            elementCapacity = requiredElementCount;
        }

        partial void InitializeSolvers() {
            TextUnderlayType.enumTypeId = 20;
            MeshFillOrigin.enumTypeId = 24;
            MeshFillDirection.enumTypeId = 23;
            MeshType.enumTypeId = 22;
            BackgroundFit.enumTypeId = 21;
            Painter.enumTypeId = 0;
            MeshFillRotation.enumTypeId = 0;
            BackgroundImageRotation.enumTypeId = 0;
            BorderColorLeft.enumTypeId = 0;
            BorderColorBottom.enumTypeId = 0;
            BorderColorRight.enumTypeId = 0;
            BorderColorTop.enumTypeId = 0;
            TextColor.enumTypeId = 0;
            TextColor.isImplicitInherit = true;
            TextOutlineColor.enumTypeId = 0;
            SelectionTextColor.enumTypeId = 0;
            BackgroundTint.enumTypeId = 0;
            SelectionBackgroundColor.enumTypeId = 0;
            BackgroundColor.enumTypeId = 0;
            TextGlowColor.enumTypeId = 0;
            TextUnderlayColor.enumTypeId = 0;
            CornerRadiusBottomRight.enumTypeId = 0;
            MeshFillRadius.enumTypeId = 0;
            CornerBevelBottomLeft.enumTypeId = 0;
            CornerBevelBottomRight.enumTypeId = 0;
            CornerBevelTopRight.enumTypeId = 0;
            CornerBevelTopLeft.enumTypeId = 0;
            MeshFillOffsetY.enumTypeId = 0;
            BackgroundImageOffsetY.enumTypeId = 0;
            CornerRadiusTopRight.enumTypeId = 0;
            CornerRadiusTopLeft.enumTypeId = 0;
            MeshFillOffsetX.enumTypeId = 0;
            BackgroundImageOffsetX.enumTypeId = 0;
            CornerRadiusBottomLeft.enumTypeId = 0;
            BackgroundImageTileY.enumTypeId = 0;
            MeshFillAmount.enumTypeId = 0;
            BackgroundImageTileX.enumTypeId = 0;
            BackgroundImageScaleY.enumTypeId = 0;
            BackgroundImageScaleX.enumTypeId = 0;
        }

        partial void SolveProperties(in SolverParameters parameters, StyleTables * styleTables, PropertyTables * propertyTables, UIForia.Util.Unsafe.BumpAllocator * bumpAllocator) {
            context.Setup(parameters);
            EnsureTableCapacities(styleTables, parameters.maxElementId);
            PropertySolver_Byte.Invoke(ref TextUnderlayType, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->TextUnderlayType, 0, (PropertyId)113, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref MeshFillOrigin, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->MeshFillOrigin, 1, (PropertyId)82, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref MeshFillDirection, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->MeshFillDirection, 2, (PropertyId)79, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref MeshType, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->MeshType, 3, (PropertyId)85, bumpAllocator, false);
            PropertySolver_Byte.Invoke(ref BackgroundFit, parameters, ref context, propertyTables->propertyTable_Byte, (Byte*)styleTables->BackgroundFit, 4, (PropertyId)28, bumpAllocator, false);
            PropertySolver_PainterId.Invoke(ref Painter, parameters, ref context, propertyTables->propertyTable_PainterId, styleTables->Painter, 5, (PropertyId)94, bumpAllocator, false);
            PropertySolver_UIAngle.Invoke(ref MeshFillRotation, parameters, ref context, propertyTables->propertyTable_UIAngle, styleTables->MeshFillRotation, 6, (PropertyId)84, bumpAllocator, false);
            PropertySolver_UIAngle.Invoke(ref BackgroundImageRotation, parameters, ref context, propertyTables->propertyTable_UIAngle, styleTables->BackgroundImageRotation, 7, (PropertyId)32, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref BorderColorLeft, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->BorderColorLeft, 8, (PropertyId)44, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref BorderColorBottom, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->BorderColorBottom, 9, (PropertyId)43, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref BorderColorRight, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->BorderColorRight, 10, (PropertyId)45, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref BorderColorTop, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->BorderColorTop, 11, (PropertyId)46, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref TextColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->TextColor, 12, (PropertyId)5, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref TextOutlineColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->TextOutlineColor, 13, (PropertyId)107, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref SelectionTextColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->SelectionTextColor, 14, (PropertyId)98, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref BackgroundTint, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->BackgroundTint, 15, (PropertyId)41, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref SelectionBackgroundColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->SelectionBackgroundColor, 16, (PropertyId)97, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref BackgroundColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->BackgroundColor, 17, (PropertyId)27, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref TextGlowColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->TextGlowColor, 18, (PropertyId)102, bumpAllocator, false);
            PropertySolver_UIColor.Invoke(ref TextUnderlayColor, parameters, ref context, propertyTables->propertyTable_UIColor, styleTables->TextUnderlayColor, 19, (PropertyId)110, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerRadiusBottomRight, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerRadiusBottomRight, 20, (PropertyId)59, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref MeshFillRadius, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->MeshFillRadius, 21, (PropertyId)83, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerBevelBottomLeft, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerBevelBottomLeft, 22, (PropertyId)54, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerBevelBottomRight, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerBevelBottomRight, 23, (PropertyId)55, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerBevelTopRight, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerBevelTopRight, 24, (PropertyId)57, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerBevelTopLeft, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerBevelTopLeft, 25, (PropertyId)56, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref MeshFillOffsetY, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->MeshFillOffsetY, 26, (PropertyId)81, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BackgroundImageOffsetY, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BackgroundImageOffsetY, 27, (PropertyId)31, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerRadiusTopRight, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerRadiusTopRight, 28, (PropertyId)61, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerRadiusTopLeft, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerRadiusTopLeft, 29, (PropertyId)60, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref MeshFillOffsetX, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->MeshFillOffsetX, 30, (PropertyId)80, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref BackgroundImageOffsetX, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->BackgroundImageOffsetX, 31, (PropertyId)30, bumpAllocator, false);
            PropertySolver_UIFixedLength.Invoke(ref CornerRadiusBottomLeft, parameters, ref context, propertyTables->propertyTable_UIFixedLength, styleTables->CornerRadiusBottomLeft, 32, (PropertyId)58, bumpAllocator, false);
            PropertySolver_half.Invoke(ref BackgroundImageTileY, parameters, ref context, propertyTables->propertyTable_half, styleTables->BackgroundImageTileY, 33, (PropertyId)36, bumpAllocator, false);
            PropertySolver_half.Invoke(ref MeshFillAmount, parameters, ref context, propertyTables->propertyTable_half, styleTables->MeshFillAmount, 34, (PropertyId)78, bumpAllocator, false);
            PropertySolver_half.Invoke(ref BackgroundImageTileX, parameters, ref context, propertyTables->propertyTable_half, styleTables->BackgroundImageTileX, 35, (PropertyId)35, bumpAllocator, false);
            PropertySolver_half.Invoke(ref BackgroundImageScaleY, parameters, ref context, propertyTables->propertyTable_half, styleTables->BackgroundImageScaleY, 36, (PropertyId)34, bumpAllocator, false);
            PropertySolver_half.Invoke(ref BackgroundImageScaleX, parameters, ref context, propertyTables->propertyTable_half, styleTables->BackgroundImageScaleX, 37, (PropertyId)33, bumpAllocator, false);
        }
    }


}