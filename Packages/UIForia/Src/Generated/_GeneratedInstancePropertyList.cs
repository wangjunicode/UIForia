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

    internal unsafe partial struct PropertyContainer {
        
        [System.Runtime.InteropServices.FieldOffset(8)] internal fixed byte bytes[12];

    }
    
    public unsafe partial struct StylePropertyList {    

        [PropertySetterMethod(16)]
        public void SetAlignmentBoundaryX(UIForia.AlignmentBoundary value) {
            SetProperty(16, 0, &value, sizeof(UIForia.AlignmentBoundary));
        }

        [PropertySetterVariableMethod(16)]
        public void SetAlignmentBoundaryX(string variableName, UIForia.AlignmentBoundary value) {
            SetProperty(16, ResolveVariableId(variableName), &value, sizeof(UIForia.AlignmentBoundary));
        }

        [PropertySetterMethod(17)]
        public void SetAlignmentBoundaryY(UIForia.AlignmentBoundary value) {
            SetProperty(17, 0, &value, sizeof(UIForia.AlignmentBoundary));
        }

        [PropertySetterVariableMethod(17)]
        public void SetAlignmentBoundaryY(string variableName, UIForia.AlignmentBoundary value) {
            SetProperty(17, ResolveVariableId(variableName), &value, sizeof(UIForia.AlignmentBoundary));
        }

        [PropertySetterMethod(18)]
        public void SetAlignmentDirectionX(UIForia.AlignmentDirection value) {
            SetProperty(18, 0, &value, sizeof(UIForia.AlignmentDirection));
        }

        [PropertySetterVariableMethod(18)]
        public void SetAlignmentDirectionX(string variableName, UIForia.AlignmentDirection value) {
            SetProperty(18, ResolveVariableId(variableName), &value, sizeof(UIForia.AlignmentDirection));
        }

        [PropertySetterMethod(19)]
        public void SetAlignmentDirectionY(UIForia.AlignmentDirection value) {
            SetProperty(19, 0, &value, sizeof(UIForia.AlignmentDirection));
        }

        [PropertySetterVariableMethod(19)]
        public void SetAlignmentDirectionY(string variableName, UIForia.AlignmentDirection value) {
            SetProperty(19, ResolveVariableId(variableName), &value, sizeof(UIForia.AlignmentDirection));
        }

        [PropertySetterMethod(20)]
        public void SetAlignmentOffsetX(UIForia.UIOffset value) {
            SetProperty(20, 0, &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterVariableMethod(20)]
        public void SetAlignmentOffsetX(string variableName, UIForia.UIOffset value) {
            SetProperty(20, ResolveVariableId(variableName), &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterMethod(21)]
        public void SetAlignmentOffsetY(UIForia.UIOffset value) {
            SetProperty(21, 0, &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterVariableMethod(21)]
        public void SetAlignmentOffsetY(string variableName, UIForia.UIOffset value) {
            SetProperty(21, ResolveVariableId(variableName), &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterMethod(22)]
        public void SetAlignmentOriginX(UIForia.UIOffset value) {
            SetProperty(22, 0, &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterVariableMethod(22)]
        public void SetAlignmentOriginX(string variableName, UIForia.UIOffset value) {
            SetProperty(22, ResolveVariableId(variableName), &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterMethod(23)]
        public void SetAlignmentOriginY(UIForia.UIOffset value) {
            SetProperty(23, 0, &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterVariableMethod(23)]
        public void SetAlignmentOriginY(string variableName, UIForia.UIOffset value) {
            SetProperty(23, ResolveVariableId(variableName), &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterMethod(24)]
        public void SetAlignmentTargetX(UIForia.AlignmentTarget value) {
            SetProperty(24, 0, &value, sizeof(UIForia.AlignmentTarget));
        }

        [PropertySetterVariableMethod(24)]
        public void SetAlignmentTargetX(string variableName, UIForia.AlignmentTarget value) {
            SetProperty(24, ResolveVariableId(variableName), &value, sizeof(UIForia.AlignmentTarget));
        }

        [PropertySetterMethod(25)]
        public void SetAlignmentTargetY(UIForia.AlignmentTarget value) {
            SetProperty(25, 0, &value, sizeof(UIForia.AlignmentTarget));
        }

        [PropertySetterVariableMethod(25)]
        public void SetAlignmentTargetY(string variableName, UIForia.AlignmentTarget value) {
            SetProperty(25, ResolveVariableId(variableName), &value, sizeof(UIForia.AlignmentTarget));
        }

        [PropertySetterMethod(26)]
        public void SetAspectRatio(UIForia.AspectRatio value) {
            SetProperty(26, 0, &value, sizeof(UIForia.AspectRatio));
        }

        [PropertySetterVariableMethod(26)]
        public void SetAspectRatio(string variableName, UIForia.AspectRatio value) {
            SetProperty(26, ResolveVariableId(variableName), &value, sizeof(UIForia.AspectRatio));
        }

        [PropertySetterMethod(27)]
        public void SetBackgroundColor(UIForia.UIColor value) {
            SetProperty(27, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(27)]
        public void SetBackgroundColor(string variableName, UIForia.UIColor value) {
            SetProperty(27, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(27)]
        public void SetBackgroundColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(27, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(27)]
        public void SetBackgroundColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(27, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(28)]
        public void SetBackgroundFit(UIForia.Rendering.BackgroundFit value) {
            SetProperty(28, 0, &value, sizeof(UIForia.Rendering.BackgroundFit));
        }

        [PropertySetterVariableMethod(28)]
        public void SetBackgroundFit(string variableName, UIForia.Rendering.BackgroundFit value) {
            SetProperty(28, ResolveVariableId(variableName), &value, sizeof(UIForia.Rendering.BackgroundFit));
        }

        [PropertySetterMethod(29)]
        public void SetBackgroundImage(UIForia.Style.TextureInfo value) {
            SetProperty(29, 0, &value, sizeof(UIForia.Style.TextureInfo));
        }

        [PropertySetterVariableMethod(29)]
        public void SetBackgroundImage(string variableName, UIForia.Style.TextureInfo value) {
            SetProperty(29, ResolveVariableId(variableName), &value, sizeof(UIForia.Style.TextureInfo));
        }

        [PropertySetterMethod(30)]
        public void SetBackgroundImageOffsetX(UIForia.UIFixedLength value) {
            SetProperty(30, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(30)]
        public void SetBackgroundImageOffsetX(string variableName, UIForia.UIFixedLength value) {
            SetProperty(30, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(31)]
        public void SetBackgroundImageOffsetY(UIForia.UIFixedLength value) {
            SetProperty(31, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(31)]
        public void SetBackgroundImageOffsetY(string variableName, UIForia.UIFixedLength value) {
            SetProperty(31, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(32)]
        public void SetBackgroundImageRotation(UIForia.UIAngle value) {
            SetProperty(32, 0, &value, sizeof(UIForia.UIAngle));
        }

        [PropertySetterVariableMethod(32)]
        public void SetBackgroundImageRotation(string variableName, UIForia.UIAngle value) {
            SetProperty(32, ResolveVariableId(variableName), &value, sizeof(UIForia.UIAngle));
        }

        [PropertySetterMethod(33)]
        public void SetBackgroundImageScaleX(Unity.Mathematics.half value) {
            SetProperty(33, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(33)]
        public void SetBackgroundImageScaleX(string variableName, Unity.Mathematics.half value) {
            SetProperty(33, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(33)]
        public void SetBackgroundImageScaleX(float value) {
            half castValue = (half)value;
            SetProperty(33, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(33)]
        public void SetBackgroundImageScaleX(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(33, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(34)]
        public void SetBackgroundImageScaleY(Unity.Mathematics.half value) {
            SetProperty(34, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(34)]
        public void SetBackgroundImageScaleY(string variableName, Unity.Mathematics.half value) {
            SetProperty(34, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(34)]
        public void SetBackgroundImageScaleY(float value) {
            half castValue = (half)value;
            SetProperty(34, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(34)]
        public void SetBackgroundImageScaleY(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(34, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(35)]
        public void SetBackgroundImageTileX(Unity.Mathematics.half value) {
            SetProperty(35, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(35)]
        public void SetBackgroundImageTileX(string variableName, Unity.Mathematics.half value) {
            SetProperty(35, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(35)]
        public void SetBackgroundImageTileX(float value) {
            half castValue = (half)value;
            SetProperty(35, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(35)]
        public void SetBackgroundImageTileX(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(35, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(36)]
        public void SetBackgroundImageTileY(Unity.Mathematics.half value) {
            SetProperty(36, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(36)]
        public void SetBackgroundImageTileY(string variableName, Unity.Mathematics.half value) {
            SetProperty(36, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(36)]
        public void SetBackgroundImageTileY(float value) {
            half castValue = (half)value;
            SetProperty(36, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(36)]
        public void SetBackgroundImageTileY(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(36, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(37)]
        public void SetBackgroundRectMaxX(UIForia.UIFixedLength value) {
            SetProperty(37, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(37)]
        public void SetBackgroundRectMaxX(string variableName, UIForia.UIFixedLength value) {
            SetProperty(37, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(38)]
        public void SetBackgroundRectMaxY(UIForia.UIFixedLength value) {
            SetProperty(38, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(38)]
        public void SetBackgroundRectMaxY(string variableName, UIForia.UIFixedLength value) {
            SetProperty(38, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(39)]
        public void SetBackgroundRectMinX(UIForia.UIFixedLength value) {
            SetProperty(39, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(39)]
        public void SetBackgroundRectMinX(string variableName, UIForia.UIFixedLength value) {
            SetProperty(39, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(40)]
        public void SetBackgroundRectMinY(UIForia.UIFixedLength value) {
            SetProperty(40, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(40)]
        public void SetBackgroundRectMinY(string variableName, UIForia.UIFixedLength value) {
            SetProperty(40, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(41)]
        public void SetBackgroundTint(UIForia.UIColor value) {
            SetProperty(41, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(41)]
        public void SetBackgroundTint(string variableName, UIForia.UIColor value) {
            SetProperty(41, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(41)]
        public void SetBackgroundTint(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(41, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(41)]
        public void SetBackgroundTint(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(41, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(42)]
        public void SetBorderBottom(UIForia.UIFixedLength value) {
            SetProperty(42, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(42)]
        public void SetBorderBottom(string variableName, UIForia.UIFixedLength value) {
            SetProperty(42, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(43)]
        public void SetBorderColorBottom(UIForia.UIColor value) {
            SetProperty(43, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(43)]
        public void SetBorderColorBottom(string variableName, UIForia.UIColor value) {
            SetProperty(43, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(43)]
        public void SetBorderColorBottom(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(43, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(43)]
        public void SetBorderColorBottom(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(43, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(44)]
        public void SetBorderColorLeft(UIForia.UIColor value) {
            SetProperty(44, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(44)]
        public void SetBorderColorLeft(string variableName, UIForia.UIColor value) {
            SetProperty(44, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(44)]
        public void SetBorderColorLeft(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(44, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(44)]
        public void SetBorderColorLeft(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(44, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(45)]
        public void SetBorderColorRight(UIForia.UIColor value) {
            SetProperty(45, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(45)]
        public void SetBorderColorRight(string variableName, UIForia.UIColor value) {
            SetProperty(45, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(45)]
        public void SetBorderColorRight(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(45, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(45)]
        public void SetBorderColorRight(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(45, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(46)]
        public void SetBorderColorTop(UIForia.UIColor value) {
            SetProperty(46, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(46)]
        public void SetBorderColorTop(string variableName, UIForia.UIColor value) {
            SetProperty(46, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(46)]
        public void SetBorderColorTop(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(46, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(46)]
        public void SetBorderColorTop(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(46, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(47)]
        public void SetBorderLeft(UIForia.UIFixedLength value) {
            SetProperty(47, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(47)]
        public void SetBorderLeft(string variableName, UIForia.UIFixedLength value) {
            SetProperty(47, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(48)]
        public void SetBorderRight(UIForia.UIFixedLength value) {
            SetProperty(48, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(48)]
        public void SetBorderRight(string variableName, UIForia.UIFixedLength value) {
            SetProperty(48, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(49)]
        public void SetBorderTop(UIForia.UIFixedLength value) {
            SetProperty(49, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(49)]
        public void SetBorderTop(string variableName, UIForia.UIFixedLength value) {
            SetProperty(49, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(50)]
        public void SetClipBehavior(UIForia.ClipBehavior value) {
            SetProperty(50, 0, &value, sizeof(UIForia.ClipBehavior));
        }

        [PropertySetterVariableMethod(50)]
        public void SetClipBehavior(string variableName, UIForia.ClipBehavior value) {
            SetProperty(50, ResolveVariableId(variableName), &value, sizeof(UIForia.ClipBehavior));
        }

        [PropertySetterMethod(51)]
        public void SetClipBounds(UIForia.ClipBounds value) {
            SetProperty(51, 0, &value, sizeof(UIForia.ClipBounds));
        }

        [PropertySetterVariableMethod(51)]
        public void SetClipBounds(string variableName, UIForia.ClipBounds value) {
            SetProperty(51, ResolveVariableId(variableName), &value, sizeof(UIForia.ClipBounds));
        }

        [PropertySetterMethod(52)]
        public void SetCollapseSpaceHorizontal(UIForia.SpaceCollapse value) {
            SetProperty(52, 0, &value, sizeof(UIForia.SpaceCollapse));
        }

        [PropertySetterVariableMethod(52)]
        public void SetCollapseSpaceHorizontal(string variableName, UIForia.SpaceCollapse value) {
            SetProperty(52, ResolveVariableId(variableName), &value, sizeof(UIForia.SpaceCollapse));
        }

        [PropertySetterMethod(53)]
        public void SetCollapseSpaceVertical(UIForia.SpaceCollapse value) {
            SetProperty(53, 0, &value, sizeof(UIForia.SpaceCollapse));
        }

        [PropertySetterVariableMethod(53)]
        public void SetCollapseSpaceVertical(string variableName, UIForia.SpaceCollapse value) {
            SetProperty(53, ResolveVariableId(variableName), &value, sizeof(UIForia.SpaceCollapse));
        }

        [PropertySetterMethod(54)]
        public void SetCornerBevelBottomLeft(UIForia.UIFixedLength value) {
            SetProperty(54, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(54)]
        public void SetCornerBevelBottomLeft(string variableName, UIForia.UIFixedLength value) {
            SetProperty(54, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(55)]
        public void SetCornerBevelBottomRight(UIForia.UIFixedLength value) {
            SetProperty(55, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(55)]
        public void SetCornerBevelBottomRight(string variableName, UIForia.UIFixedLength value) {
            SetProperty(55, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(56)]
        public void SetCornerBevelTopLeft(UIForia.UIFixedLength value) {
            SetProperty(56, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(56)]
        public void SetCornerBevelTopLeft(string variableName, UIForia.UIFixedLength value) {
            SetProperty(56, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(57)]
        public void SetCornerBevelTopRight(UIForia.UIFixedLength value) {
            SetProperty(57, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(57)]
        public void SetCornerBevelTopRight(string variableName, UIForia.UIFixedLength value) {
            SetProperty(57, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(58)]
        public void SetCornerRadiusBottomLeft(UIForia.UIFixedLength value) {
            SetProperty(58, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(58)]
        public void SetCornerRadiusBottomLeft(string variableName, UIForia.UIFixedLength value) {
            SetProperty(58, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(59)]
        public void SetCornerRadiusBottomRight(UIForia.UIFixedLength value) {
            SetProperty(59, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(59)]
        public void SetCornerRadiusBottomRight(string variableName, UIForia.UIFixedLength value) {
            SetProperty(59, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(60)]
        public void SetCornerRadiusTopLeft(UIForia.UIFixedLength value) {
            SetProperty(60, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(60)]
        public void SetCornerRadiusTopLeft(string variableName, UIForia.UIFixedLength value) {
            SetProperty(60, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(61)]
        public void SetCornerRadiusTopRight(UIForia.UIFixedLength value) {
            SetProperty(61, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(61)]
        public void SetCornerRadiusTopRight(string variableName, UIForia.UIFixedLength value) {
            SetProperty(61, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(62)]
        public void SetGridItemX(UIForia.GridItemPlacement value) {
            SetProperty(62, 0, &value, sizeof(UIForia.GridItemPlacement));
        }

        [PropertySetterVariableMethod(62)]
        public void SetGridItemX(string variableName, UIForia.GridItemPlacement value) {
            SetProperty(62, ResolveVariableId(variableName), &value, sizeof(UIForia.GridItemPlacement));
        }

        [PropertySetterMethod(63)]
        public void SetGridItemY(UIForia.GridItemPlacement value) {
            SetProperty(63, 0, &value, sizeof(UIForia.GridItemPlacement));
        }

        [PropertySetterVariableMethod(63)]
        public void SetGridItemY(string variableName, UIForia.GridItemPlacement value) {
            SetProperty(63, ResolveVariableId(variableName), &value, sizeof(UIForia.GridItemPlacement));
        }

        [PropertySetterMethod(64)]
        public void SetGridLayoutColGap(Unity.Mathematics.half value) {
            SetProperty(64, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(64)]
        public void SetGridLayoutColGap(string variableName, Unity.Mathematics.half value) {
            SetProperty(64, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(64)]
        public void SetGridLayoutColGap(float value) {
            half castValue = (half)value;
            SetProperty(64, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(64)]
        public void SetGridLayoutColGap(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(64, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(65)]
        public void SetGridLayoutColTemplate(UIForia.GridLayoutTemplate value) {
            SetProperty(65, 0, &value, sizeof(UIForia.GridLayoutTemplate));
        }

        [PropertySetterVariableMethod(65)]
        public void SetGridLayoutColTemplate(string variableName, UIForia.GridLayoutTemplate value) {
            SetProperty(65, ResolveVariableId(variableName), &value, sizeof(UIForia.GridLayoutTemplate));
        }

        [PropertySetterMethod(66)]
        public void SetGridLayoutDensity(UIForia.Layout.GridLayoutDensity value) {
            SetProperty(66, 0, &value, sizeof(UIForia.Layout.GridLayoutDensity));
        }

        [PropertySetterVariableMethod(66)]
        public void SetGridLayoutDensity(string variableName, UIForia.Layout.GridLayoutDensity value) {
            SetProperty(66, ResolveVariableId(variableName), &value, sizeof(UIForia.Layout.GridLayoutDensity));
        }

        [PropertySetterMethod(67)]
        public void SetGridLayoutRowGap(Unity.Mathematics.half value) {
            SetProperty(67, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(67)]
        public void SetGridLayoutRowGap(string variableName, Unity.Mathematics.half value) {
            SetProperty(67, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(67)]
        public void SetGridLayoutRowGap(float value) {
            half castValue = (half)value;
            SetProperty(67, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(67)]
        public void SetGridLayoutRowGap(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(67, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(68)]
        public void SetGridLayoutRowTemplate(UIForia.GridLayoutTemplate value) {
            SetProperty(68, 0, &value, sizeof(UIForia.GridLayoutTemplate));
        }

        [PropertySetterVariableMethod(68)]
        public void SetGridLayoutRowTemplate(string variableName, UIForia.GridLayoutTemplate value) {
            SetProperty(68, ResolveVariableId(variableName), &value, sizeof(UIForia.GridLayoutTemplate));
        }

        [PropertySetterMethod(1)]
        public void SetLayer(ushort value) {
            SetProperty(1, 0, &value, sizeof(ushort));
        }

        [PropertySetterVariableMethod(1)]
        public void SetLayer(string variableName, ushort value) {
            SetProperty(1, ResolveVariableId(variableName), &value, sizeof(ushort));
        }

        [PropertySetterMethod(1)]
        public void SetLayer(int value) {
            SetProperty(1, 0, &value, sizeof(ushort));
        }

        [PropertySetterVariableMethod(1)]
        public void SetLayer(string variableName, int value) {
            SetProperty(1, ResolveVariableId(variableName), &value, sizeof(ushort));
        }

        [PropertySetterMethod(69)]
        public void SetLayoutBehavior(UIForia.LayoutBehavior value) {
            SetProperty(69, 0, &value, sizeof(UIForia.LayoutBehavior));
        }

        [PropertySetterVariableMethod(69)]
        public void SetLayoutBehavior(string variableName, UIForia.LayoutBehavior value) {
            SetProperty(69, ResolveVariableId(variableName), &value, sizeof(UIForia.LayoutBehavior));
        }

        [PropertySetterMethod(70)]
        public void SetLayoutFillOrder(UIForia.LayoutFillOrder value) {
            SetProperty(70, 0, &value, sizeof(UIForia.LayoutFillOrder));
        }

        [PropertySetterVariableMethod(70)]
        public void SetLayoutFillOrder(string variableName, UIForia.LayoutFillOrder value) {
            SetProperty(70, ResolveVariableId(variableName), &value, sizeof(UIForia.LayoutFillOrder));
        }

        [PropertySetterMethod(71)]
        public void SetLayoutType(UIForia.LayoutType value) {
            SetProperty(71, 0, &value, sizeof(UIForia.LayoutType));
        }

        [PropertySetterVariableMethod(71)]
        public void SetLayoutType(string variableName, UIForia.LayoutType value) {
            SetProperty(71, ResolveVariableId(variableName), &value, sizeof(UIForia.LayoutType));
        }

        [PropertySetterMethod(72)]
        public void SetMarginBottom(UIForia.UISpaceSize value) {
            SetProperty(72, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(72)]
        public void SetMarginBottom(string variableName, UIForia.UISpaceSize value) {
            SetProperty(72, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(73)]
        public void SetMarginLeft(UIForia.UISpaceSize value) {
            SetProperty(73, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(73)]
        public void SetMarginLeft(string variableName, UIForia.UISpaceSize value) {
            SetProperty(73, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(74)]
        public void SetMarginRight(UIForia.UISpaceSize value) {
            SetProperty(74, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(74)]
        public void SetMarginRight(string variableName, UIForia.UISpaceSize value) {
            SetProperty(74, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(75)]
        public void SetMarginTop(UIForia.UISpaceSize value) {
            SetProperty(75, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(75)]
        public void SetMarginTop(string variableName, UIForia.UISpaceSize value) {
            SetProperty(75, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(76)]
        public void SetMaxHeight(UIForia.UISizeConstraint value) {
            SetProperty(76, 0, &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterVariableMethod(76)]
        public void SetMaxHeight(string variableName, UIForia.UISizeConstraint value) {
            SetProperty(76, ResolveVariableId(variableName), &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterMethod(77)]
        public void SetMaxWidth(UIForia.UISizeConstraint value) {
            SetProperty(77, 0, &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterVariableMethod(77)]
        public void SetMaxWidth(string variableName, UIForia.UISizeConstraint value) {
            SetProperty(77, ResolveVariableId(variableName), &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterMethod(78)]
        public void SetMeshFillAmount(Unity.Mathematics.half value) {
            SetProperty(78, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(78)]
        public void SetMeshFillAmount(string variableName, Unity.Mathematics.half value) {
            SetProperty(78, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(78)]
        public void SetMeshFillAmount(float value) {
            half castValue = (half)value;
            SetProperty(78, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(78)]
        public void SetMeshFillAmount(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(78, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(79)]
        public void SetMeshFillDirection(UIForia.Rendering.MeshFillDirection value) {
            SetProperty(79, 0, &value, sizeof(UIForia.Rendering.MeshFillDirection));
        }

        [PropertySetterVariableMethod(79)]
        public void SetMeshFillDirection(string variableName, UIForia.Rendering.MeshFillDirection value) {
            SetProperty(79, ResolveVariableId(variableName), &value, sizeof(UIForia.Rendering.MeshFillDirection));
        }

        [PropertySetterMethod(80)]
        public void SetMeshFillOffsetX(UIForia.UIFixedLength value) {
            SetProperty(80, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(80)]
        public void SetMeshFillOffsetX(string variableName, UIForia.UIFixedLength value) {
            SetProperty(80, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(81)]
        public void SetMeshFillOffsetY(UIForia.UIFixedLength value) {
            SetProperty(81, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(81)]
        public void SetMeshFillOffsetY(string variableName, UIForia.UIFixedLength value) {
            SetProperty(81, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(82)]
        public void SetMeshFillOrigin(UIForia.Rendering.MeshFillOrigin value) {
            SetProperty(82, 0, &value, sizeof(UIForia.Rendering.MeshFillOrigin));
        }

        [PropertySetterVariableMethod(82)]
        public void SetMeshFillOrigin(string variableName, UIForia.Rendering.MeshFillOrigin value) {
            SetProperty(82, ResolveVariableId(variableName), &value, sizeof(UIForia.Rendering.MeshFillOrigin));
        }

        [PropertySetterMethod(83)]
        public void SetMeshFillRadius(UIForia.UIFixedLength value) {
            SetProperty(83, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(83)]
        public void SetMeshFillRadius(string variableName, UIForia.UIFixedLength value) {
            SetProperty(83, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(84)]
        public void SetMeshFillRotation(UIForia.UIAngle value) {
            SetProperty(84, 0, &value, sizeof(UIForia.UIAngle));
        }

        [PropertySetterVariableMethod(84)]
        public void SetMeshFillRotation(string variableName, UIForia.UIAngle value) {
            SetProperty(84, ResolveVariableId(variableName), &value, sizeof(UIForia.UIAngle));
        }

        [PropertySetterMethod(85)]
        public void SetMeshType(UIForia.Rendering.MeshType value) {
            SetProperty(85, 0, &value, sizeof(UIForia.Rendering.MeshType));
        }

        [PropertySetterVariableMethod(85)]
        public void SetMeshType(string variableName, UIForia.Rendering.MeshType value) {
            SetProperty(85, ResolveVariableId(variableName), &value, sizeof(UIForia.Rendering.MeshType));
        }

        [PropertySetterMethod(86)]
        public void SetMinHeight(UIForia.UISizeConstraint value) {
            SetProperty(86, 0, &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterVariableMethod(86)]
        public void SetMinHeight(string variableName, UIForia.UISizeConstraint value) {
            SetProperty(86, ResolveVariableId(variableName), &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterMethod(87)]
        public void SetMinWidth(UIForia.UISizeConstraint value) {
            SetProperty(87, 0, &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterVariableMethod(87)]
        public void SetMinWidth(string variableName, UIForia.UISizeConstraint value) {
            SetProperty(87, ResolveVariableId(variableName), &value, sizeof(UIForia.UISizeConstraint));
        }

        [PropertySetterMethod(2)]
        public void SetOpacity(Unity.Mathematics.half value) {
            SetProperty(2, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(2)]
        public void SetOpacity(string variableName, Unity.Mathematics.half value) {
            SetProperty(2, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(2)]
        public void SetOpacity(float value) {
            half castValue = (half)value;
            SetProperty(2, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(2)]
        public void SetOpacity(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(2, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(88)]
        public void SetOverflowX(UIForia.Overflow value) {
            SetProperty(88, 0, &value, sizeof(UIForia.Overflow));
        }

        [PropertySetterVariableMethod(88)]
        public void SetOverflowX(string variableName, UIForia.Overflow value) {
            SetProperty(88, ResolveVariableId(variableName), &value, sizeof(UIForia.Overflow));
        }

        [PropertySetterMethod(89)]
        public void SetOverflowY(UIForia.Overflow value) {
            SetProperty(89, 0, &value, sizeof(UIForia.Overflow));
        }

        [PropertySetterVariableMethod(89)]
        public void SetOverflowY(string variableName, UIForia.Overflow value) {
            SetProperty(89, ResolveVariableId(variableName), &value, sizeof(UIForia.Overflow));
        }

        [PropertySetterMethod(90)]
        public void SetPaddingBottom(UIForia.UISpaceSize value) {
            SetProperty(90, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(90)]
        public void SetPaddingBottom(string variableName, UIForia.UISpaceSize value) {
            SetProperty(90, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(91)]
        public void SetPaddingLeft(UIForia.UISpaceSize value) {
            SetProperty(91, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(91)]
        public void SetPaddingLeft(string variableName, UIForia.UISpaceSize value) {
            SetProperty(91, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(92)]
        public void SetPaddingRight(UIForia.UISpaceSize value) {
            SetProperty(92, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(92)]
        public void SetPaddingRight(string variableName, UIForia.UISpaceSize value) {
            SetProperty(92, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(93)]
        public void SetPaddingTop(UIForia.UISpaceSize value) {
            SetProperty(93, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(93)]
        public void SetPaddingTop(string variableName, UIForia.UISpaceSize value) {
            SetProperty(93, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(94)]
        public void SetPainter(UIForia.PainterId value) {
            SetProperty(94, 0, &value, sizeof(UIForia.PainterId));
        }

        [PropertySetterVariableMethod(94)]
        public void SetPainter(string variableName, UIForia.PainterId value) {
            SetProperty(94, ResolveVariableId(variableName), &value, sizeof(UIForia.PainterId));
        }

        [PropertySetterMethod(3)]
        public void SetPointerEvents(UIForia.PointerEvents value) {
            SetProperty(3, 0, &value, sizeof(UIForia.PointerEvents));
        }

        [PropertySetterVariableMethod(3)]
        public void SetPointerEvents(string variableName, UIForia.PointerEvents value) {
            SetProperty(3, ResolveVariableId(variableName), &value, sizeof(UIForia.PointerEvents));
        }

        [PropertySetterMethod(95)]
        public void SetPreferredHeight(UIForia.UIMeasurement value) {
            SetProperty(95, 0, &value, sizeof(UIForia.UIMeasurement));
        }

        [PropertySetterVariableMethod(95)]
        public void SetPreferredHeight(string variableName, UIForia.UIMeasurement value) {
            SetProperty(95, ResolveVariableId(variableName), &value, sizeof(UIForia.UIMeasurement));
        }

        [PropertySetterMethod(96)]
        public void SetPreferredWidth(UIForia.UIMeasurement value) {
            SetProperty(96, 0, &value, sizeof(UIForia.UIMeasurement));
        }

        [PropertySetterVariableMethod(96)]
        public void SetPreferredWidth(string variableName, UIForia.UIMeasurement value) {
            SetProperty(96, ResolveVariableId(variableName), &value, sizeof(UIForia.UIMeasurement));
        }

        [PropertySetterMethod(97)]
        public void SetSelectionBackgroundColor(UIForia.UIColor value) {
            SetProperty(97, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(97)]
        public void SetSelectionBackgroundColor(string variableName, UIForia.UIColor value) {
            SetProperty(97, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(97)]
        public void SetSelectionBackgroundColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(97, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(97)]
        public void SetSelectionBackgroundColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(97, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(98)]
        public void SetSelectionTextColor(UIForia.UIColor value) {
            SetProperty(98, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(98)]
        public void SetSelectionTextColor(string variableName, UIForia.UIColor value) {
            SetProperty(98, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(98)]
        public void SetSelectionTextColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(98, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(98)]
        public void SetSelectionTextColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(98, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(99)]
        public void SetSpaceBetweenHorizontal(UIForia.UISpaceSize value) {
            SetProperty(99, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(99)]
        public void SetSpaceBetweenHorizontal(string variableName, UIForia.UISpaceSize value) {
            SetProperty(99, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(100)]
        public void SetSpaceBetweenVertical(UIForia.UISpaceSize value) {
            SetProperty(100, 0, &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterVariableMethod(100)]
        public void SetSpaceBetweenVertical(string variableName, UIForia.UISpaceSize value) {
            SetProperty(100, ResolveVariableId(variableName), &value, sizeof(UIForia.UISpaceSize));
        }

        [PropertySetterMethod(4)]
        public void SetTextAlignment(UIForia.Text.TextAlignment value) {
            SetProperty(4, 0, &value, sizeof(UIForia.Text.TextAlignment));
        }

        [PropertySetterVariableMethod(4)]
        public void SetTextAlignment(string variableName, UIForia.Text.TextAlignment value) {
            SetProperty(4, ResolveVariableId(variableName), &value, sizeof(UIForia.Text.TextAlignment));
        }

        [PropertySetterMethod(5)]
        public void SetTextColor(UIForia.UIColor value) {
            SetProperty(5, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(5)]
        public void SetTextColor(string variableName, UIForia.UIColor value) {
            SetProperty(5, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(5)]
        public void SetTextColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(5, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(5)]
        public void SetTextColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(5, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(101)]
        public void SetTextFaceDilate(Unity.Mathematics.half value) {
            SetProperty(101, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(101)]
        public void SetTextFaceDilate(string variableName, Unity.Mathematics.half value) {
            SetProperty(101, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(101)]
        public void SetTextFaceDilate(float value) {
            half castValue = (half)value;
            SetProperty(101, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(101)]
        public void SetTextFaceDilate(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(101, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(6)]
        public void SetTextFontAsset(UIForia.FontAssetId value) {
            SetProperty(6, 0, &value, sizeof(UIForia.FontAssetId));
        }

        [PropertySetterVariableMethod(6)]
        public void SetTextFontAsset(string variableName, UIForia.FontAssetId value) {
            SetProperty(6, ResolveVariableId(variableName), &value, sizeof(UIForia.FontAssetId));
        }

        [PropertySetterMethod(7)]
        public void SetTextFontSize(UIForia.UIFontSize value) {
            SetProperty(7, 0, &value, sizeof(UIForia.UIFontSize));
        }

        [PropertySetterVariableMethod(7)]
        public void SetTextFontSize(string variableName, UIForia.UIFontSize value) {
            SetProperty(7, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFontSize));
        }

        [PropertySetterMethod(8)]
        public void SetTextFontStyle(UIForia.Text.FontStyle value) {
            SetProperty(8, 0, &value, sizeof(UIForia.Text.FontStyle));
        }

        [PropertySetterVariableMethod(8)]
        public void SetTextFontStyle(string variableName, UIForia.Text.FontStyle value) {
            SetProperty(8, ResolveVariableId(variableName), &value, sizeof(UIForia.Text.FontStyle));
        }

        [PropertySetterMethod(102)]
        public void SetTextGlowColor(UIForia.UIColor value) {
            SetProperty(102, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(102)]
        public void SetTextGlowColor(string variableName, UIForia.UIColor value) {
            SetProperty(102, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(102)]
        public void SetTextGlowColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(102, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(102)]
        public void SetTextGlowColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(102, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(103)]
        public void SetTextGlowInner(Unity.Mathematics.half value) {
            SetProperty(103, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(103)]
        public void SetTextGlowInner(string variableName, Unity.Mathematics.half value) {
            SetProperty(103, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(103)]
        public void SetTextGlowInner(float value) {
            half castValue = (half)value;
            SetProperty(103, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(103)]
        public void SetTextGlowInner(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(103, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(104)]
        public void SetTextGlowOffset(Unity.Mathematics.half value) {
            SetProperty(104, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(104)]
        public void SetTextGlowOffset(string variableName, Unity.Mathematics.half value) {
            SetProperty(104, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(104)]
        public void SetTextGlowOffset(float value) {
            half castValue = (half)value;
            SetProperty(104, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(104)]
        public void SetTextGlowOffset(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(104, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(105)]
        public void SetTextGlowOuter(Unity.Mathematics.half value) {
            SetProperty(105, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(105)]
        public void SetTextGlowOuter(string variableName, Unity.Mathematics.half value) {
            SetProperty(105, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(105)]
        public void SetTextGlowOuter(float value) {
            half castValue = (half)value;
            SetProperty(105, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(105)]
        public void SetTextGlowOuter(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(105, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(106)]
        public void SetTextGlowPower(Unity.Mathematics.half value) {
            SetProperty(106, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(106)]
        public void SetTextGlowPower(string variableName, Unity.Mathematics.half value) {
            SetProperty(106, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(106)]
        public void SetTextGlowPower(float value) {
            half castValue = (half)value;
            SetProperty(106, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(106)]
        public void SetTextGlowPower(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(106, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(9)]
        public void SetTextLineHeight(Unity.Mathematics.half value) {
            SetProperty(9, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(9)]
        public void SetTextLineHeight(string variableName, Unity.Mathematics.half value) {
            SetProperty(9, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(9)]
        public void SetTextLineHeight(float value) {
            half castValue = (half)value;
            SetProperty(9, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(9)]
        public void SetTextLineHeight(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(9, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(107)]
        public void SetTextOutlineColor(UIForia.UIColor value) {
            SetProperty(107, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(107)]
        public void SetTextOutlineColor(string variableName, UIForia.UIColor value) {
            SetProperty(107, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(107)]
        public void SetTextOutlineColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(107, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(107)]
        public void SetTextOutlineColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(107, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(108)]
        public void SetTextOutlineSoftness(Unity.Mathematics.half value) {
            SetProperty(108, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(108)]
        public void SetTextOutlineSoftness(string variableName, Unity.Mathematics.half value) {
            SetProperty(108, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(108)]
        public void SetTextOutlineSoftness(float value) {
            half castValue = (half)value;
            SetProperty(108, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(108)]
        public void SetTextOutlineSoftness(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(108, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(109)]
        public void SetTextOutlineWidth(Unity.Mathematics.half value) {
            SetProperty(109, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(109)]
        public void SetTextOutlineWidth(string variableName, Unity.Mathematics.half value) {
            SetProperty(109, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(109)]
        public void SetTextOutlineWidth(float value) {
            half castValue = (half)value;
            SetProperty(109, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(109)]
        public void SetTextOutlineWidth(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(109, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(10)]
        public void SetTextOverflow(UIForia.Text.TextOverflow value) {
            SetProperty(10, 0, &value, sizeof(UIForia.Text.TextOverflow));
        }

        [PropertySetterVariableMethod(10)]
        public void SetTextOverflow(string variableName, UIForia.Text.TextOverflow value) {
            SetProperty(10, ResolveVariableId(variableName), &value, sizeof(UIForia.Text.TextOverflow));
        }

        [PropertySetterMethod(11)]
        public void SetTextTransform(UIForia.Text.TextTransform value) {
            SetProperty(11, 0, &value, sizeof(UIForia.Text.TextTransform));
        }

        [PropertySetterVariableMethod(11)]
        public void SetTextTransform(string variableName, UIForia.Text.TextTransform value) {
            SetProperty(11, ResolveVariableId(variableName), &value, sizeof(UIForia.Text.TextTransform));
        }

        [PropertySetterMethod(110)]
        public void SetTextUnderlayColor(UIForia.UIColor value) {
            SetProperty(110, 0, &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterVariableMethod(110)]
        public void SetTextUnderlayColor(string variableName, UIForia.UIColor value) {
            SetProperty(110, ResolveVariableId(variableName), &value, sizeof(UIForia.UIColor));
        }

        [PropertySetterMethod(110)]
        public void SetTextUnderlayColor(UnityEngine.Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(110, 0, &castValue, sizeof(UIColor));
        }

        [PropertySetterVariableMethod(110)]
        public void SetTextUnderlayColor(string variableName, Color value) {
            UIColor castValue = (UIColor)value;
            SetProperty(110, ResolveVariableId(variableName), &castValue, sizeof(UIColor));
        }

        [PropertySetterMethod(111)]
        public void SetTextUnderlayDilate(Unity.Mathematics.half value) {
            SetProperty(111, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(111)]
        public void SetTextUnderlayDilate(string variableName, Unity.Mathematics.half value) {
            SetProperty(111, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(111)]
        public void SetTextUnderlayDilate(float value) {
            half castValue = (half)value;
            SetProperty(111, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(111)]
        public void SetTextUnderlayDilate(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(111, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(112)]
        public void SetTextUnderlaySoftness(Unity.Mathematics.half value) {
            SetProperty(112, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(112)]
        public void SetTextUnderlaySoftness(string variableName, Unity.Mathematics.half value) {
            SetProperty(112, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(112)]
        public void SetTextUnderlaySoftness(float value) {
            half castValue = (half)value;
            SetProperty(112, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(112)]
        public void SetTextUnderlaySoftness(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(112, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(113)]
        public void SetTextUnderlayType(UIForia.Rendering.UnderlayType value) {
            SetProperty(113, 0, &value, sizeof(UIForia.Rendering.UnderlayType));
        }

        [PropertySetterVariableMethod(113)]
        public void SetTextUnderlayType(string variableName, UIForia.Rendering.UnderlayType value) {
            SetProperty(113, ResolveVariableId(variableName), &value, sizeof(UIForia.Rendering.UnderlayType));
        }

        [PropertySetterMethod(114)]
        public void SetTextUnderlayX(Unity.Mathematics.half value) {
            SetProperty(114, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(114)]
        public void SetTextUnderlayX(string variableName, Unity.Mathematics.half value) {
            SetProperty(114, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(114)]
        public void SetTextUnderlayX(float value) {
            half castValue = (half)value;
            SetProperty(114, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(114)]
        public void SetTextUnderlayX(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(114, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(115)]
        public void SetTextUnderlayY(Unity.Mathematics.half value) {
            SetProperty(115, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(115)]
        public void SetTextUnderlayY(string variableName, Unity.Mathematics.half value) {
            SetProperty(115, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(115)]
        public void SetTextUnderlayY(float value) {
            half castValue = (half)value;
            SetProperty(115, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(115)]
        public void SetTextUnderlayY(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(115, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(12)]
        public void SetTextVerticalAlignment(UIForia.Text.VerticalAlignment value) {
            SetProperty(12, 0, &value, sizeof(UIForia.Text.VerticalAlignment));
        }

        [PropertySetterVariableMethod(12)]
        public void SetTextVerticalAlignment(string variableName, UIForia.Text.VerticalAlignment value) {
            SetProperty(12, ResolveVariableId(variableName), &value, sizeof(UIForia.Text.VerticalAlignment));
        }

        [PropertySetterMethod(13)]
        public void SetTextWhitespaceMode(UIForia.Text.WhitespaceMode value) {
            SetProperty(13, 0, &value, sizeof(UIForia.Text.WhitespaceMode));
        }

        [PropertySetterVariableMethod(13)]
        public void SetTextWhitespaceMode(string variableName, UIForia.Text.WhitespaceMode value) {
            SetProperty(13, ResolveVariableId(variableName), &value, sizeof(UIForia.Text.WhitespaceMode));
        }

        [PropertySetterMethod(116)]
        public void SetTransformPivotX(UIForia.UIFixedLength value) {
            SetProperty(116, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(116)]
        public void SetTransformPivotX(string variableName, UIForia.UIFixedLength value) {
            SetProperty(116, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(117)]
        public void SetTransformPivotY(UIForia.UIFixedLength value) {
            SetProperty(117, 0, &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterVariableMethod(117)]
        public void SetTransformPivotY(string variableName, UIForia.UIFixedLength value) {
            SetProperty(117, ResolveVariableId(variableName), &value, sizeof(UIForia.UIFixedLength));
        }

        [PropertySetterMethod(118)]
        public void SetTransformPositionX(UIForia.UIOffset value) {
            SetProperty(118, 0, &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterVariableMethod(118)]
        public void SetTransformPositionX(string variableName, UIForia.UIOffset value) {
            SetProperty(118, ResolveVariableId(variableName), &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterMethod(119)]
        public void SetTransformPositionY(UIForia.UIOffset value) {
            SetProperty(119, 0, &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterVariableMethod(119)]
        public void SetTransformPositionY(string variableName, UIForia.UIOffset value) {
            SetProperty(119, ResolveVariableId(variableName), &value, sizeof(UIForia.UIOffset));
        }

        [PropertySetterMethod(120)]
        public void SetTransformRotation(Unity.Mathematics.half value) {
            SetProperty(120, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(120)]
        public void SetTransformRotation(string variableName, Unity.Mathematics.half value) {
            SetProperty(120, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(120)]
        public void SetTransformRotation(float value) {
            half castValue = (half)value;
            SetProperty(120, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(120)]
        public void SetTransformRotation(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(120, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(121)]
        public void SetTransformScaleX(Unity.Mathematics.half value) {
            SetProperty(121, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(121)]
        public void SetTransformScaleX(string variableName, Unity.Mathematics.half value) {
            SetProperty(121, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(121)]
        public void SetTransformScaleX(float value) {
            half castValue = (half)value;
            SetProperty(121, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(121)]
        public void SetTransformScaleX(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(121, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(122)]
        public void SetTransformScaleY(Unity.Mathematics.half value) {
            SetProperty(122, 0, &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(122)]
        public void SetTransformScaleY(string variableName, Unity.Mathematics.half value) {
            SetProperty(122, ResolveVariableId(variableName), &value, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(122)]
        public void SetTransformScaleY(float value) {
            half castValue = (half)value;
            SetProperty(122, 0,  &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterVariableMethod(122)]
        public void SetTransformScaleY(string variableName, float value) {
            half castValue = (half)value;
            SetProperty(122, ResolveVariableId(variableName), &castValue, sizeof(Unity.Mathematics.half));
        }

        [PropertySetterMethod(14)]
        public void SetVisibility(UIForia.Visibility value) {
            SetProperty(14, 0, &value, sizeof(UIForia.Visibility));
        }

        [PropertySetterVariableMethod(14)]
        public void SetVisibility(string variableName, UIForia.Visibility value) {
            SetProperty(14, ResolveVariableId(variableName), &value, sizeof(UIForia.Visibility));
        }

        [PropertySetterMethod(15)]
        public void SetZIndex(ushort value) {
            SetProperty(15, 0, &value, sizeof(ushort));
        }

        [PropertySetterVariableMethod(15)]
        public void SetZIndex(string variableName, ushort value) {
            SetProperty(15, ResolveVariableId(variableName), &value, sizeof(ushort));
        }

        [PropertySetterMethod(15)]
        public void SetZIndex(int value) {
            SetProperty(15, 0, &value, sizeof(ushort));
        }

        [PropertySetterVariableMethod(15)]
        public void SetZIndex(string variableName, int value) {
            SetProperty(15, ResolveVariableId(variableName), &value, sizeof(ushort));
        }

       
    }

}
