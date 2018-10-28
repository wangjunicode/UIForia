using System;
using Rendering;
using Shapes2D;
using Src.Extensions;
using Src.Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

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
        
        private static readonly int s_ColorKey;
        private static readonly int s_ClipRectKey;
        private static readonly int s_SizeKey;
        private static readonly int s_BorderSizeKey;
        private static readonly int s_BorderRadiusKey;
        private static readonly int s_BorderColorKey;
        private static readonly int s_FillOffsetScaleKey;
        
        private static readonly Material s_BaseMaterial;

        static StandardRenderer() {
            s_BaseMaterial = Resources.Load<Material>("Materials/UIForia");
            s_ColorKey = Shader.PropertyToID("_Color");
            s_ClipRectKey = Shader.PropertyToID("_ClipRect");
            s_BorderColorKey = Shader.PropertyToID("_BorderColor");
            s_BorderSizeKey = Shader.PropertyToID("_BorderSize");
            s_BorderRadiusKey = Shader.PropertyToID("_BorderRadius");
            s_FillOffsetScaleKey = Shader.PropertyToID("_FillOffsetScale");
            s_SizeKey = Shader.PropertyToID("_Size");
        }

        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                UIElement element = data.element;

                data.renderPosition = new Vector3(data.renderPosition.x, data.renderPosition.y, -data.renderPosition.z);
                if (data.isMaterialProvider) {
                    data.material = ((IMaterialProvider) element).GetMaterial();
                }
                else if (data.material == null) {
                    data.material = new Material(s_BaseMaterial);
                }

                if (data.material == null) {
                    continue;
                }

                if (data.isMeshProvider) {
                    data.mesh = ((IMeshProvider) element).GetMesh();
                }
                else if (data.mesh == null) {
                    data.mesh = MeshUtil.CreateStandardUIMesh(element.layoutResult.actualSize, Color.white);
                }

                if (data.mesh == null) {
                    continue;
                }

                ComputedStyle style = data.element.ComputedStyle;
                Size size = element.layoutResult.actualSize;
                Material material = data.material;
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

                       
                        switch (style.GradientType) {
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
                Vector2 fillScale = style.BackgroundFillScale;
                Vector2 fillOffset = style.BackgroundFillOffset;
                material.SetColor(s_ColorKey, style.BackgroundColor);
                material.SetVector(s_ClipRectKey, data.clipVector);
                material.SetVector(s_FillOffsetScaleKey, new Vector4(fillOffset.x, fillOffset.y, fillScale.x, fillScale.y));
                material.SetVector(s_BorderSizeKey, style.ResolvedBorder);
                material.SetVector(s_BorderRadiusKey, style.ResolvedBorderRadius);
                material.SetVector(s_BorderColorKey, style.BorderColor);
                material.SetVector(s_SizeKey, new Vector4(size.width, size.height, 0, 0));
                Graphics.DrawMesh(data.mesh, origin + data.renderPosition, Quaternion.identity, material, 0, camera, 0, null, false, false, false);
            }
        }

    }

}