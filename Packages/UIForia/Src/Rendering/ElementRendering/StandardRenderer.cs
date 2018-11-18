using System;
using Shapes2D;
using UIForia.Extensions;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class StandardRenderer : ElementRenderer {

        public const string k_UseBorder = "UIFORIA_USE_BORDER";
        public const string k_FillType_Color = "UIFORIA_FILLTYPE_COLOR";
        public const string k_FillType_Texture = "UIFORIA_FILLTYPE_TEXTURE";
        public const string k_FillType_LinearGradient = "UIFORIA_FILLTYPE_LINEAR_GRADIENT";
        public const string k_FillType_RadialGradient = "UIFORIA_FILLTYPE_RADIAL_GRADIENT";
        public const string k_FillType_CylindricalGradient = "UIFORIA_FILLTYPE_CYLINDRICAL_GRADIENT";
        public const string k_FillType_Grid = "UIFORIA_FILLTYPE_GRID";
        public const string k_FillType_Checker = "UIFORIA_FILLTYPE_CHECKER";
        public const string k_FillType_Stripes = "UIFORIA_FILLTYPE_STRIPES";

        public static readonly int s_ColorKey;
        public static readonly int s_ClipRectKey;
        public static readonly int s_SizeKey;
        public static readonly int s_BorderSizeKey;
        public static readonly int s_BorderRadiusKey;
        public static readonly int s_BorderColorKey;
        public static readonly int s_ContentRectKey;
        public static readonly int s_FillOffsetScaleKey;

        protected static readonly Material s_BaseMaterial;
        private static readonly int s_RotationKey = Shader.PropertyToID("_Rotation");

        static StandardRenderer() {
            s_BaseMaterial = Resources.Load<Material>("UIForia/Materials/UIForia");
            s_ColorKey = Shader.PropertyToID("_Color");
            s_ClipRectKey = Shader.PropertyToID("_ClipRect");
            s_BorderColorKey = Shader.PropertyToID("_BorderColor");
            s_BorderSizeKey = Shader.PropertyToID("_BorderSize");
            s_BorderRadiusKey = Shader.PropertyToID("_BorderRadius");
            s_FillOffsetScaleKey = Shader.PropertyToID("_FillOffsetScale");
            s_ContentRectKey = Shader.PropertyToID("_ContentRect");
            s_SizeKey = Shader.PropertyToID("_Size");
        }

        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                UIElement element = data.element;

                if (data.isMeshProvider) {
                    data.mesh = ((IMeshProvider) element).GetMesh();
                }
                else if (data.mesh == null || element.layoutResult.ActualSizeChanged || element.layoutResult.RotationChanged) {
                    LayoutResult result = element.layoutResult;
                    data.mesh = MeshUtil.ResizeStandardUIMesh(data.mesh, new Size(
                                Mathf.Max(result.actualSize.width, result.allocatedSize.width),
                                Mathf.Max(result.actualSize.height, result.allocatedSize.height)
                            )
                        );
                }

                if (data.mesh == null) {
                    continue;
                }

                if (data.isMaterialProvider) {
                    data.material = ((IMaterialProvider) element).GetMaterial();
                }
                else {
                    data.material = InitDefaultMaterial(data);
                }

                if (data.material == null) {
                    continue;
                }

                data.material.SetFloat(s_RotationKey, element.layoutResult.rotation * Mathf.Deg2Rad);
                Matrix4x4 matrix = Matrix4x4.TRS(
                    origin + data.renderPosition - new Vector3(1, 0, 0),
                    Quaternion.AngleAxis(0, Vector3.forward),
                    new Vector3(1, 1, 1)
                );

                Graphics.DrawMesh(data.mesh, matrix, data.material, 0, camera, 0, null, false, false, false);
            }
        }

        protected static Material InitDefaultMaterial(RenderData data) {
            UIStyleSet style = data.element.style;
            Size size = data.element.layoutResult.actualSize;
            Material material = data.material;

            if (material == null) {
                material = new Material(s_BaseMaterial);
            }

            if (style.HasBorderRadius || style.BorderColor.IsDefined()) {
                material.EnableKeyword(k_UseBorder);
            }
            else {
                material.DisableKeyword(k_UseBorder);
            }

            switch (style.BackgroundFillType) {
                case BackgroundFillType.Unset:
                case BackgroundFillType.None:
                case BackgroundFillType.Normal:
                    if (style.BackgroundImage != null) {
                        material.EnableKeyword(k_FillType_Texture);
                        material.DisableKeyword(k_FillType_Color);
                        material.mainTexture = style.BackgroundImage;
                    }
                    else {
                        material.EnableKeyword(k_FillType_Color);
                    }

                    break;
                case BackgroundFillType.Gradient:

                    switch (style.BackgroundGradientType) {
                        case GradientType.Linear:
                            material.EnableKeyword(k_FillType_LinearGradient);
                            break;
                        case GradientType.Radial:
                            material.EnableKeyword(k_FillType_RadialGradient);
                            break;
                        case GradientType.Cylindrical:
                            material.EnableKeyword(k_FillType_CylindricalGradient);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

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

            // todo -- see if [PerRendererData] can stop us from needing unique materials
            Vector2 fillScale = new Vector2(style.BackgroundFillScaleX, style.BackgroundFillScaleY);
            Vector2 fillOffset = new Vector2(style.BackgroundFillOffsetX, style.BackgroundFillOffsetY);
            Vector2 pivot = data.element.layoutResult.Pivot;
            Rect contentRect = data.element.layoutResult.ContentRect;
            material.SetVector(s_ContentRectKey,
                new Vector4(contentRect.x, contentRect.y, contentRect.width, contentRect.height));
            material.SetColor(s_ColorKey, style.BackgroundColor);
            material.SetVector(s_ClipRectKey, data.clipVector);
            material.SetVector(s_FillOffsetScaleKey, new Vector4(fillOffset.x, fillOffset.y, fillScale.x, fillScale.y));
            material.SetVector(s_BorderSizeKey, style.ResolvedBorder);
            material.SetVector(s_BorderRadiusKey, style.ResolvedBorderRadius);
            material.SetVector(s_BorderColorKey, style.BorderColor);
            material.SetVector(s_SizeKey, new Vector4(size.width, size.height, pivot.x, pivot.y));
            return material;
        }

        public static Material CreateMaterial() {
            return new Material(s_BaseMaterial);
        }

    }

}