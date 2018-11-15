using System;
using System.Collections.Generic;
using UIForia.Rendering;
using Shapes2D;
using UIForia.Extensions;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Rendering {

    public class StandardInstancedRenderer : ElementRenderer {

        private readonly MaterialPropertyBlock m_Block;

        private readonly LightList<Matrix4x4> m_Matrices;

        private readonly LightList<Vector4> m_ClipRects;
        private readonly LightList<Vector4> m_SizeRotations;
        private readonly LightList<Vector4> m_PrimaryColors;
        private readonly LightList<Vector4> m_SecondaryColors;

        private readonly LightList<Vector4> m_BorderRadii;
        private readonly LightList<Vector4> m_BorderSizes;
        private readonly LightList<Vector4> m_BorderColors;

        private readonly LightList<Vector4> m_GradientAxisAndStart;
        private readonly LightList<Vector4> m_FillOffsetAndScales;
        private readonly LightList<Vector4> m_FillRotationAndTilings;
        private readonly LightList<Vector4> m_GridAndLineSizes;

        private readonly LightList<Vector4> m_TextureIndicesAndUVs;
        private readonly LightList<Vector4> m_TextureTilingAndOffsets;

        private readonly LightList<RenderData> m_SpecialRenderList;
        private readonly LightList<BatchEntry> m_BatchEntries;

        private readonly IntMap<Mesh> m_MeshMap;
        private readonly IntMap<Material> m_MaterialMap;

        private static readonly int s_SizeRotationGradientStartKey;
        private static readonly int s_PrimaryColorKey;
        private static readonly int s_BorderColorKey;
        private static readonly int s_BorderSizeKey;
        private static readonly int s_BorderRadiusKey;
        private static readonly int s_ClipRectKey;
        private static readonly int s_GridAndLineSizeKey;
        private static readonly int s_GradientAxisAndStartKey;

        private static readonly int s_SecondaryColorKey;
        private static readonly int s_FillOffsetAndScaleKey;
        private static readonly int s_FillRotationKey;

        private static readonly Mesh s_Mesh;
        private static readonly Material s_InstancedMaterial;

        static StandardInstancedRenderer() {
            s_Mesh = MeshUtil.CreateStandardUIMesh(new Size(1, 1), Color.white);
            s_InstancedMaterial = new Material(Resources.Load<Material>("UIForia/Materials/UIForiaInstanced"));
            s_InstancedMaterial.enableInstancing = true;

            s_ClipRectKey = Shader.PropertyToID("_ClipRect");
            s_BorderSizeKey = Shader.PropertyToID("_BorderSize");
            s_BorderColorKey = Shader.PropertyToID("_BorderColor");
            s_BorderRadiusKey = Shader.PropertyToID("_BorderRadius");
            s_PrimaryColorKey = Shader.PropertyToID("_PrimaryColor");
            s_SecondaryColorKey = Shader.PropertyToID("_SecondaryColor");
            s_GridAndLineSizeKey = Shader.PropertyToID("_GridAndLineSize");
            s_GradientAxisAndStartKey = Shader.PropertyToID("_GradientAxisAndStart");
            s_FillOffsetAndScaleKey = Shader.PropertyToID("_FillOffsetAndScale");
            s_SizeRotationGradientStartKey = Shader.PropertyToID("_SizeRotationGradientStart");
        }

        public StandardInstancedRenderer() {
            this.m_Block = new MaterialPropertyBlock();
            this.m_SpecialRenderList = new LightList<RenderData>();
            
            this.m_Matrices = new LightList<Matrix4x4>(16);
            this.m_SizeRotations = new LightList<Vector4>(16);
            this.m_ClipRects = new LightList<Vector4>(16);
            
            this.m_PrimaryColors = new LightList<Vector4>(16);
            this.m_SecondaryColors = new LightList<Vector4>(16);
            this.m_FillOffsetAndScales = new LightList<Vector4>(16);
            
            this.m_BorderRadii = new LightList<Vector4>(16);
            this.m_BorderSizes = new LightList<Vector4>(16);
            this.m_BorderColors = new LightList<Vector4>(16);
            
            this.m_GradientAxisAndStart = new LightList<Vector4>(16);
            this.m_GridAndLineSizes = new LightList<Vector4>(16);
            this.m_TextureTilingAndOffsets = new LightList<Vector4>(16);
            this.m_TextureIndicesAndUVs = new LightList<Vector4>(16);
            
            this.m_BatchEntries = new LightList<BatchEntry>(16);
            this.m_MaterialMap = new IntMap<Material>();
            this.m_MeshMap = new IntMap<Mesh>();
        }

        private void RenderBatch(MaterialBatchKey batchKey, BatchEntry[] entries, int start, int end, Vector3 origin, Camera camera) {
            Material material;
            if (!m_MaterialMap.TryGetValue((int) batchKey, out material)) {
                material = CreateMaterial(batchKey);
            }

            int instanceId = 0;

            MaterialBatchKey fillType = batchKey & MaterialBatchKey.FillMask;
            MaterialBatchKey shapeType = batchKey & MaterialBatchKey.ShapeMask;

//            if (end - start >= 3) {
//                entries[0].renderData.renderPosition = new Vector3(entries[0].renderData.renderPosition.x, entries[0].renderData.renderPosition.y, -2);
//                entries[1].renderData.renderPosition = new Vector3(entries[1].renderData.renderPosition.x, entries[1].renderData.renderPosition.y, -1);
//                entries[2].renderData.renderPosition = new Vector3(entries[2].renderData.renderPosition.x, entries[2].renderData.renderPosition.y, 0);
//
//                // things that might be happening: painting 2 things at the same x/y causes culling internally
                // pushing the layer up manually seems to work (but child doesn't go with it)
                // ignored thing takes size of content even though it shouldn't check flexbox impl 
//            }

            for (int i = end - 1; i >= start; i--) {
                RenderData data = entries[i].renderData;
                UIElement element = data.element;
                ComputedStyle style = element.ComputedStyle;
                
                if ((batchKey & MaterialBatchKey.Border) != 0) {
                    m_BorderSizes[instanceId] = style.ResolvedBorder;
                    m_BorderRadii[instanceId] = style.ResolvedBorderRadius;
                    m_BorderColors[instanceId] = style.BorderColor;
                }

                Size size = element.layoutResult.actualSize;
                m_SizeRotations[instanceId] = new Vector4(size.width, size.height, style.BackgroundFillRotation, 0);
                m_ClipRects[instanceId] = data.clipVector;
                m_Matrices[instanceId] = Matrix4x4.TRS(origin + data.renderPosition, Quaternion.identity, Vector3.one);
                m_PrimaryColors[instanceId] = style.BackgroundColor;
                m_SecondaryColors[instanceId] = style.BackgroundColorSecondary;
                m_FillOffsetAndScales[instanceId] = new Vector4(0, 0, 1, 1);
                
                switch (fillType) {
                    case MaterialBatchKey.FillType_Color:
                        break;
                    case MaterialBatchKey.FillType_Textured:
                    case MaterialBatchKey.FillType_MultiTextured:
                        // might separate between textured or not
                        m_TextureTilingAndOffsets[instanceId] = new Vector4(0, 0, 0, 0);
                        m_TextureIndicesAndUVs[instanceId] = new Vector4();
                        break;
                    case MaterialBatchKey.FillType_Grid:
                    case MaterialBatchKey.FillType_Stripes:
                    case MaterialBatchKey.FillType_Checker:
                        m_GridAndLineSizes[instanceId] = new Vector4();
                        break;

                    case MaterialBatchKey.FillType_GradientLinear:
                    case MaterialBatchKey.FillType_GradientRadial:
                    case MaterialBatchKey.FillType_GradientCylindrical:
                        m_GradientAxisAndStart[instanceId] = new Vector4((int)style.BackgroundGradientAxis, style.BackgroundGradientStart);
                        break;
                }

                switch (shapeType) {
                    case MaterialBatchKey.Shape_Ellipse:
                        break;
                    case MaterialBatchKey.Shape_Triangle:
                        break;
                }

                instanceId++;
            }
            
            m_Block.Clear();
            m_Block.SetVectorArray(s_PrimaryColorKey, m_PrimaryColors.List);
            m_Block.SetVectorArray(s_SecondaryColorKey, m_SecondaryColors.List);
            m_Block.SetVectorArray(s_FillOffsetAndScaleKey, m_FillOffsetAndScales.List);
            m_Block.SetVectorArray(s_SizeRotationGradientStartKey, m_SizeRotations.List);
            m_Block.SetVectorArray(s_ClipRectKey, m_ClipRects.List);
            
            if ((batchKey & MaterialBatchKey.Border) != 0) {
                m_Block.SetVectorArray(s_BorderRadiusKey, m_BorderRadii.List);
                m_Block.SetVectorArray(s_BorderSizeKey, m_BorderSizes.List);
                m_Block.SetVectorArray(s_BorderColorKey, m_BorderColors.List);
            }

            if ((batchKey & MaterialBatchKey.Shape_Ellipse) != 0) {
                // set arch & radii
            }

            if ((batchKey & MaterialBatchKey.Shape_Triangle) != 0) {
                // set offset
            }

            if ((batchKey & MaterialBatchKey.FillType_Textured) != 0) {
                // set texture id, uvs, tiling
            }

            if ((batchKey & MaterialBatchKey.__FillType_Gradient) != 0) {
                m_Block.SetVectorArray(s_GradientAxisAndStartKey, m_GradientAxisAndStart.List);
            }
            
            if ((batchKey & MaterialBatchKey.__FillType_GridOrLine) != 0) {
                m_Block.SetVectorArray(s_GridAndLineSizeKey, m_GridAndLineSizes.List);
            }

            Graphics.DrawMeshInstanced(s_Mesh, 0, material, m_Matrices.List, end - start, m_Block, ShadowCastingMode.Off, false, 0, camera, LightProbeUsage.Off);

        }

        private Material CreateMaterial(MaterialBatchKey batchKey) {
            Material material = new Material(s_InstancedMaterial);
            material.enableInstancing = true;

            if ((batchKey & MaterialBatchKey.Border) != 0) {
                material.EnableKeyword("UIFORIA_USE_BORDER");
            }

            if ((batchKey & MaterialBatchKey.FillType_Color) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_COLOR");
            }
            else if ((batchKey & MaterialBatchKey.FillType_GradientLinear) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_LINEAR_GRADIENT");
            }
            else if ((batchKey & MaterialBatchKey.FillType_GradientRadial) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_RADIAL_GRADIENT");
            }
            else if ((batchKey & MaterialBatchKey.FillType_GradientCylindrical) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_Cylindrical_GRADIENT");
            }
            else if ((batchKey & MaterialBatchKey.FillType_Grid) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_GRID");
            }
            else if ((batchKey & MaterialBatchKey.FillType_Stripes) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_STRIPES");
            }
            else if ((batchKey & MaterialBatchKey.FillType_Checker) != 0) {
                material.EnableKeyword("UIFORIA_FILLTYPE_CHECKER");
            }

            if ((batchKey & MaterialBatchKey.Shape_Rectangle) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_RECTANGLE");
                EnsureMeshExists(BackgroundShapeType.Rectangle);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Triangle) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_TRIANGLE");
                EnsureMeshExists(BackgroundShapeType.Triangle);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Ellipse) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_ELLIPSE");
                EnsureMeshExists(BackgroundShapeType.Ellipse);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Diamond) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Diamond);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Pentagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Pentagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Hexagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Hexagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Heptagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Heptagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Octagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Octagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Nonagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Nonagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Decagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Decagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Hendecagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Hendecagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Dodecagon) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Dodecagon);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Star) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Star);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Arrow) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Arrow);
            }
            else if ((batchKey & MaterialBatchKey.Shape_Chevron) != 0) {
                material.EnableKeyword("UIFORIA_SHAPE_POLYGON");
                EnsureMeshExists(BackgroundShapeType.Chevron);
            }
            
            // todo -- other basic shapes like check marks 

            m_MaterialMap.Add((int) batchKey, material);
            return material;
        }

        private void EnsureMeshExists(BackgroundShapeType shapeType) {
            if (!m_MeshMap.ContainsKey((int) shapeType)) {
                Mesh mesh = null;
                switch (shapeType) {
                    case BackgroundShapeType.Ellipse:
                        mesh = MeshUtil.CreateStandardUIMesh(new Size(1, 1), Color.white);
                        break;
                    case BackgroundShapeType.Triangle:
                        mesh = MeshUtil.CreateTriangle(new Size(1, 1));
                        break;
                    case BackgroundShapeType.Diamond:
                        mesh = MeshUtil.CreateDiamond(new Size(1, 1));
                        break;
                    case BackgroundShapeType.Pentagon:
                    case BackgroundShapeType.Hexagon:
                    case BackgroundShapeType.Heptagon:
                    case BackgroundShapeType.Octagon:
                    case BackgroundShapeType.Nonagon:
                    case BackgroundShapeType.Decagon:
                    case BackgroundShapeType.Hendecagon:
                    case BackgroundShapeType.Dodecagon:
                    case BackgroundShapeType.Star:
                    case BackgroundShapeType.Arrow:
                    case BackgroundShapeType.Chevron:
                        throw new NotImplementedException();
                    case BackgroundShapeType.Rectangle:
                        mesh = MeshUtil.CreateStandardUIMesh(new Size(1, 1), Color.white);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, null);
                }

                m_MeshMap.Add((int) shapeType, mesh);
            }
        }

        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            // never need to clear the arrays because we tell the graphics lib how many items to read
            int count = end - start;

            if (count == 0) return;

            m_SizeRotations.EnsureCapacity(count);
            m_Matrices.EnsureCapacity(count);
            m_PrimaryColors.EnsureCapacity(count);
            m_BorderRadii.EnsureCapacity(count);
            m_BorderColors.EnsureCapacity(count);
            m_BorderSizes.EnsureCapacity(count);
            m_ClipRects.EnsureCapacity(count);

            m_BatchEntries.EnsureCapacity(count);

            SortBatches(drawList, start, end);
            if (m_BatchEntries.Count > 0) {
                MaterialBatchKey currentKey = m_BatchEntries[0].batchKey;
                BatchEntry[] entries = m_BatchEntries.List;
                int batchStart = 0;
                for (int i = 0; i < m_BatchEntries.Count; i++) {
                    if (entries[i].batchKey != currentKey) {
                        RenderBatch(currentKey, entries, batchStart, i, origin, camera);
                        batchStart = i;
                        currentKey = entries[i].batchKey;
                    }
                }

                RenderBatch(currentKey, entries, batchStart, m_BatchEntries.Count, origin, camera);
            }

            DefaultNonInstanced.Render(m_SpecialRenderList.List, 0, m_SpecialRenderList.Count, origin, camera);

            m_SpecialRenderList.Clear();
            m_BatchEntries.Clear();
        }

        private void SortBatches(RenderData[] drawList, int start, int end) {
            // compute all material batch keys & then sort by them

            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                ComputedStyle style = data.element.ComputedStyle;

                Texture2D background = style.BackgroundImage;
                // todo! remove this
                if (true || background != null) {
                    // todo -- pack textures into Texture2DArray and use sampling in shader instead of using standard renderer
                    m_SpecialRenderList.Add(data);
                    continue;
                }
//
//                BackgroundFillType fillType = style.BackgroundFillType;
//                BackgroundShapeType shapeType = style.BackgroundShapeType;
//                MaterialBatchKey materialKey = 0;
//
//                if (style.HasBorderRadius || style.BorderColor.IsDefined()) {
//                    materialKey |= MaterialBatchKey.Border;
//                }
//
//                // todo -- replace with mask
//                switch (shapeType) {
//                    case BackgroundShapeType.Triangle:
//                        materialKey |= MaterialBatchKey.Shape_Triangle;
//                        break;
//                    case BackgroundShapeType.Diamond:
//                        materialKey |= MaterialBatchKey.Shape_Diamond;
//                        break;
//                    case BackgroundShapeType.Pentagon:
//                        materialKey |= MaterialBatchKey.Shape_Pentagon;
//                        break;
//                    case BackgroundShapeType.Hexagon:
//                        materialKey |= MaterialBatchKey.Shape_Hexagon;
//                        break;
//                    case BackgroundShapeType.Heptagon:
//                        materialKey |= MaterialBatchKey.Shape_Heptagon;
//                        break;
//                    case BackgroundShapeType.Octagon:
//                        materialKey |= MaterialBatchKey.Shape_Octagon;
//                        break;
//                    case BackgroundShapeType.Nonagon:
//                        materialKey |= MaterialBatchKey.Shape_Nonagon;
//                        break;
//                    case BackgroundShapeType.Decagon:
//                        materialKey |= MaterialBatchKey.Shape_Decagon;
//                        break;
//                    case BackgroundShapeType.Hendecagon:
//                        materialKey |= MaterialBatchKey.Shape_Hendecagon;
//                        break;
//                    case BackgroundShapeType.Dodecagon:
//                        materialKey |= MaterialBatchKey.Shape_Dodecagon;
//                        break;
//                    case BackgroundShapeType.Star:
//                        materialKey |= MaterialBatchKey.Shape_Star;
//                        break;
//                    case BackgroundShapeType.Arrow:
//                        materialKey |= MaterialBatchKey.Shape_Arrow;
//                        break;
//                    case BackgroundShapeType.Chevron:
//                        materialKey |= MaterialBatchKey.Shape_Chevron;
//                        break;
//                    case BackgroundShapeType.Rectangle:
//                        materialKey |= MaterialBatchKey.Shape_Rectangle;
//                        break;
//                    default:
//                        throw new ArgumentOutOfRangeException();
//                }
//
//                switch (fillType) {
//                    case BackgroundFillType.Unset:
//                    case BackgroundFillType.None:
//                    case BackgroundFillType.Normal:
//                        materialKey |= MaterialBatchKey.FillType_Color;
//                        // todo check if textured
//                        break;
//                    case BackgroundFillType.Gradient:
//                        GradientType gradientType = style.BackgroundGradientType;
//                        gradientType = GradientType.Linear;
//                        switch (gradientType) {
//                            case GradientType.Linear:
//                                materialKey |= MaterialBatchKey.FillType_GradientLinear;
//                                break;
//                            case GradientType.Cylindrical:
//                                materialKey |= MaterialBatchKey.FillType_GradientCylindrical;
//                                break;
//                            case GradientType.Radial:
//                                materialKey |= MaterialBatchKey.FillType_GradientRadial;
//                                break;
//                            default:
//                                materialKey |= MaterialBatchKey.FillType_GradientLinear;
//                                break;
//                        }
//
//                        break;
//                    case BackgroundFillType.Grid:
//                        materialKey |= MaterialBatchKey.FillType_Grid;
//                        break;
//                    case BackgroundFillType.Checker:
//                        materialKey |= MaterialBatchKey.FillType_Checker;
//                        break;
//                    case BackgroundFillType.Stripes:
//                        materialKey |= MaterialBatchKey.FillType_Stripes;
//                        break;
//                    default:
//                        throw new ArgumentOutOfRangeException();
//                }
//
//                m_BatchEntries.AddUnchecked(new BatchEntry(materialKey, data));
            }
        }

        [Flags]
        private enum MaterialBatchKey {

            Shape_Triangle = 1 << 0,
            Shape_Diamond = 1 << 1,
            Shape_Pentagon = 1 << 2,
            Shape_Hexagon = 1 << 3,
            Shape_Heptagon = 1 << 4,
            Shape_Octagon = 1 << 5,
            Shape_Nonagon = 1 << 6,
            Shape_Decagon = 1 << 7,
            Shape_Hendecagon = 1 << 8,
            Shape_Dodecagon = 1 << 9,
            Shape_Star = 1 << 10,
            Shape_Arrow = 1 << 11,
            Shape_Chevron = 1 << 12,
            Shape_Rectangle = 1 << 13,
            Shape_Ellipse = 1 << 14,

            FillType_GradientLinear = 1 << 17,
            FillType_GradientRadial = 1 << 18,
            FillType_GradientCylindrical = 1 << 19,

            FillType_Color = 1 << 20,
            FillType_Textured = 1 << 21, 
            FillType_MultiTextured = 1 << 22,
            FillType_Grid = 1 << 23,
            FillType_Checker = 1 << 24,
            FillType_Stripes = 1 << 25,

            Border = 1 << 31,
            
            ShapeMask = 0x0000ffff,
            FillMask = 0x0fff0000,

            __FillType_Gradient = FillType_GradientLinear | FillType_GradientCylindrical | FillType_GradientCylindrical,
            __FillType_GridOrLine = FillType_Checker | FillType_Grid | FillType_Stripes

        }

      

        private struct BatchEntry {

            public readonly MaterialBatchKey batchKey;
            public readonly RenderData renderData;

            public BatchEntry(MaterialBatchKey batchKey, RenderData renderData) {
                this.batchKey = batchKey;
                this.renderData = renderData;
            }

        }

    }

}