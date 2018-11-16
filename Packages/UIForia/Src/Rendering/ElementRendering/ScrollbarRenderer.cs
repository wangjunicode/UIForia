using System;
using Shapes2D;
using UIForia;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace Src.Rendering {

    public class ScrollbarRenderer : StandardRenderer {

        public struct MaterialData {

            public BackgroundFillType fillType;
            public Color fillColor;
            public Texture2D backgroundImage;
            public Rect contentRect;
            public Size size;
            public Vector4 clipVector;
            public Vector4 fillOffsetScale;
            public Vector4 borderRadius;
            public Vector4 borderSize;
            public Color borderColor;

        }

        private void SetupMaterial(Material material, ref MaterialData materialData) {
            Vector4 borderRadius = materialData.borderRadius;
            Color borderColor = materialData.borderColor;

            if (borderRadius.x > 0 || borderRadius.y > 0 || borderRadius.z > 0 || borderRadius.w > 0 || borderColor.IsDefined()) {
                material.EnableKeyword(k_UseBorder);
            }
            else {
                material.DisableKeyword(k_UseBorder);
            }

            switch (materialData.fillType) {
                case BackgroundFillType.Unset:
                case BackgroundFillType.None:
                case BackgroundFillType.Normal:
                    if (materialData.backgroundImage != null) {
                        material.EnableKeyword(k_FillType_Texture);
                        material.DisableKeyword(k_FillType_Color);
                        material.mainTexture = materialData.backgroundImage;
                    }
                    else {
                        material.EnableKeyword(k_FillType_Color);
                    }

                    break;
                case BackgroundFillType.Gradient:

//                        switch (style.BackgroundGradientType) {
//                            case GradientType.Linear:
//                                material.EnableKeyword(k_FillType_LinearGradient);
//                                break;
//                            case GradientType.Radial:
//                                material.EnableKeyword(k_FillType_RadialGradient);
//                                break;
//                            case GradientType.Cylindrical:
//                                material.EnableKeyword(k_FillType_CylindricalGradient);
//                                break;
//                            default:
//                                throw new ArgumentOutOfRangeException();
//                        }

                    break;
                case BackgroundFillType.Grid:
                    material.EnableKeyword(k_FillType_Grid);
                    break;
                case BackgroundFillType.Checker:
                    material.EnableKeyword(k_FillType_Checker);
                    break;
                case BackgroundFillType.Stripes:
                    material.EnableKeyword(k_FillType_Stripes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Rect contentRect = materialData.contentRect;
            material.SetVector(s_ContentRectKey, new Vector4(contentRect.x, contentRect.y, contentRect.width, contentRect.height));
            material.SetColor(s_ColorKey, materialData.fillColor);
            material.SetVector(s_ClipRectKey, materialData.clipVector);
            material.SetVector(s_FillOffsetScaleKey, materialData.fillOffsetScale);
            material.SetVector(s_BorderSizeKey, materialData.borderSize);
            material.SetVector(s_BorderRadiusKey, borderRadius);
            material.SetVector(s_BorderColorKey, borderColor);
            material.SetVector(s_SizeKey, new Vector4(materialData.size.width, materialData.size.height, 0, 0));
        }

        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                VirtualScrollbar scrollbar = (VirtualScrollbar) data.element;
                Rect trackRect = scrollbar.trackRect;
                Matrix4x4 matrix;
                Vector3 renderPosition;
                MaterialData materialData;
                UIStyleSet style = scrollbar.targetElement.style;

                scrollbar.trackMesh = MeshUtil.ResizeStandardUIMesh(scrollbar.trackMesh, new Size(trackRect.size));

                if (scrollbar.incrementButtonRect.width != 0 && scrollbar.incrementButtonRect.height != 0) {
                    scrollbar.incrementButtonMesh = MeshUtil.ResizeStandardUIMesh(scrollbar.incrementButtonMesh, new Size(scrollbar.incrementButtonRect.size));

                    materialData = new MaterialData();
                    materialData.size = new Size(scrollbar.incrementButtonRect.size);
                    materialData.contentRect = new Rect();
                    materialData.clipVector = new Vector4(0, 0, 1, 1);
                    GetIncrementMaterialData(style, scrollbar.IsVertical, ref materialData);
                    SetupMaterial(scrollbar.incrementButtonMaterial, ref materialData);

                    renderPosition = new Vector3(scrollbar.incrementButtonRect.x, -scrollbar.incrementButtonRect.y, data.renderPosition.z + 0.2f);

                    matrix = Matrix4x4.TRS(
                        origin + renderPosition - new Vector3(1, 0, 0),
                        Quaternion.AngleAxis(data.element.style.TransformRotation, Vector3.forward),
                        new Vector3(1, 1, 1)
                    );

                    Graphics.DrawMesh(scrollbar.incrementButtonMesh, matrix, scrollbar.incrementButtonMaterial, 0, camera, 0, null, false, false, false);
                }

                if (scrollbar.decrementButtonRect.width != 0 && scrollbar.decrementButtonRect.height != 0) {
                    scrollbar.decrementButtonMesh = MeshUtil.ResizeStandardUIMesh(scrollbar.decrementButtonMesh, new Size(scrollbar.decrementButtonRect.size));

                    materialData = new MaterialData();

                    materialData.size = new Size(scrollbar.decrementButtonRect.size);
                    materialData.contentRect = new Rect();
                    materialData.clipVector = new Vector4(0, 0, 1, 1);
                    GetDecrementMaterialData(style, scrollbar.IsVertical, ref materialData);
                    SetupMaterial(scrollbar.decrementButtonMaterial, ref materialData);

                    renderPosition = new Vector3(scrollbar.decrementButtonRect.x, -scrollbar.decrementButtonRect.y, data.renderPosition.z + 0.2f);

                    matrix = Matrix4x4.TRS(
                        origin + renderPosition - new Vector3(1, 0, 0),
                        Quaternion.AngleAxis(data.element.style.TransformRotation, Vector3.forward),
                        new Vector3(1, 1, 1)
                    );

                    Graphics.DrawMesh(scrollbar.decrementButtonMesh, matrix, scrollbar.decrementButtonMaterial, 0, camera, 0, null, false, false, false);
                }


                scrollbar.trackMesh = MeshUtil.ResizeStandardUIMesh(scrollbar.trackMesh, new Size(scrollbar.trackRect.size));
                materialData = new MaterialData();

                materialData.size = new Size(scrollbar.trackRect.size);
                materialData.contentRect = new Rect();
                materialData.clipVector = new Vector4(0, 0, 1, 1);
                GetTrackMaterialData(style, scrollbar.IsVertical, ref materialData);
                SetupMaterial(scrollbar.trackMaterial, ref materialData);

                renderPosition = new Vector3(trackRect.x, -trackRect.y, data.renderPosition.z);

                matrix = Matrix4x4.TRS(
                    origin + renderPosition - new Vector3(1, 0, 0),
                    Quaternion.AngleAxis(data.element.style.TransformRotation, Vector3.forward),
                    new Vector3(1, 1, 1)
                );

                Graphics.DrawMesh(scrollbar.trackMesh, matrix, scrollbar.trackMaterial, 0, camera, 0, null, false, false, false);

                scrollbar.handleMesh = MeshUtil.ResizeStandardUIMesh(scrollbar.handleMesh, new Size(scrollbar.handleRect.size));

                materialData = new MaterialData();
                materialData.size = new Size(scrollbar.handleRect.size);
                materialData.contentRect = new Rect();
                materialData.clipVector = new Vector4(0, 0, 1, 1);
                GetHandleMaterialData(style, scrollbar.IsVertical, ref materialData);
                SetupMaterial(scrollbar.handleMaterial, ref materialData);

                renderPosition = new Vector3(scrollbar.handleRect.x, -scrollbar.handleRect.y, data.renderPosition.z - 0.2f);

                matrix = Matrix4x4.TRS(
                    origin + renderPosition - new Vector3(1, 0, 0),
                    Quaternion.AngleAxis(data.element.style.TransformRotation, Vector3.forward),
                    new Vector3(1, 1, 1)
                );

                Graphics.DrawMesh(scrollbar.handleMesh, matrix, scrollbar.handleMaterial, 0, camera, 0, null, false, false, false);
            }
        }

        private static void GetHandleMaterialData(UIStyleSet style, bool isVertical, ref MaterialData materialData) {
            if (isVertical) {
                float radius = style.ScrollbarVerticalHandleBorderRadius;
                float borderSize = style.ScrollbarVerticalHandleBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarVerticalHandleColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarVerticalHandleBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
            else {
                float radius = style.ScrollbarHorizontalHandleBorderRadius;
                float borderSize = style.ScrollbarHorizontalHandleBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarHorizontalTrackColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarHorizontalTrackBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
        }

        private static void GetTrackMaterialData(UIStyleSet style, bool isVertical, ref MaterialData materialData) {
            if (isVertical) {
                float radius = style.ScrollbarVerticalTrackBorderRadius;
                float borderSize = style.ScrollbarVerticalTrackBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarVerticalTrackColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarVerticalTrackBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
            else {
                float radius = style.ScrollbarHorizontalTrackBorderRadius;
                float borderSize = style.ScrollbarHorizontalTrackBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarHorizontalHandleColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarHorizontalHandleBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
        }

        private static void GetIncrementMaterialData(UIStyleSet style, bool isVertical, ref MaterialData materialData) {
            if (isVertical) {
                float radius = style.ScrollbarVerticalIncrementBorderRadius;
                float borderSize = style.ScrollbarVerticalIncrementBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarVerticalIncrementColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarVerticalIncrementBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
            else {
                float radius = style.ScrollbarHorizontalIncrementBorderRadius;
                float borderSize = style.ScrollbarHorizontalIncrementBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarHorizontalIncrementColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarHorizontalIncrementBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
        }

        private static void GetDecrementMaterialData(UIStyleSet style, bool isVertical, ref MaterialData materialData) {
            if (isVertical) {
                float radius = style.ScrollbarVerticalDecrementBorderRadius;
                float borderSize = style.ScrollbarVerticalDecrementBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarVerticalDecrementColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarVerticalDecrementBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
            else {
                float radius = style.ScrollbarHorizontalDecrementBorderRadius;
                float borderSize = style.ScrollbarHorizontalDecrementBorderSize;
                materialData.fillType = BackgroundFillType.Normal;
                materialData.fillColor = style.ScrollbarHorizontalDecrementColor;
                materialData.borderRadius = new Vector4(radius, radius, radius, radius);
                materialData.borderColor = style.ScrollbarHorizontalDecrementBorderColor;
                materialData.borderSize = new Vector4(borderSize, borderSize, borderSize, borderSize);
                materialData.fillOffsetScale = new Vector4(0, 0, 1, 1);
            }
        }

    }

}