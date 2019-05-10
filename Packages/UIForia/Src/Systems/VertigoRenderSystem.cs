using System;
using System.Collections.Generic;
using SVGX;
using UIForia;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;
using CompareFunction = Vertigo.CompareFunction;

namespace Packages.UIForia.Src.Systems {

    public class VertigoRenderSystem : IRenderSystem {

        private VertigoContext ctx;
        private Camera camera;
        private ILayoutSystem layoutSystem;
        private CommandBuffer commandBuffer;
        private LightList<UIView> views;

        // todo -- per view
        private IntMap<RenderInfo> renderInfos;

        public VertigoRenderSystem(Camera camera, ILayoutSystem layoutSystem) {
            this.camera = camera;
            this.ctx = new VertigoContext();
            this.layoutSystem = layoutSystem;
            this.views = new LightList<UIView>();
            this.commandBuffer = new CommandBuffer(); // todo -- per view
        }

        public struct RenderInfo {

            public int elementId;
            public Color32 backgroundColor;
            public Color32 backgroundTint;
            public Color32 textColor;
            public Color32 borderColorTop;
            public Color32 borderColorRight;
            public Color32 borderColorBottom;
            public Color32 borderColorLeft;
            public float opacity;

            // todo -- some / all of these can be packed 

            public Rect uvRect;
            public Vector2 uvTiling;
            public Vector2 uvOffset;
            public Vector2 backgroundScale;
            public float backgroundRotation;
            public Vector4 borderRadius;
            public Vector4 borderSize;

            // todo -- border style

            public Color32 textOutlineColor;
            public Color32 textGlowColor;
            public Vector4 clipRect;
            public Visibility visibility;
            public ISVGXPaintable painter;
            public bool isSelfPainting;
            public bool hasUniformBorder;
            public bool requiresRendering;
            public RenderMethod renderMethod;
            public int geometryId;
            public Texture backgroundImage;
            public VertigoMaterial material;

        }

        public enum RenderMethod {

            None,
            Text,
            Painter,
            SelfPainter,
            UniformBorder,
            UniformBorderRadius,
            MixedBorder,
            MixedBorderRadius,
            NoBorderFilled

        }

        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void OnReset() { }

        public void OnUpdate() {
            ctx.Clear();

            for (int i = 0; i < views.Count; i++) {
                RenderView(views[i]);
            }

            ctx.Flush(camera, commandBuffer);
        }

        private VertigoMaterial fillMaterial;
        private GeometryCache geometryCache;

        public class VertigoBatcher : IDrawCallBatcher {

            private GeometryCache geometryCache;

            private struct PendingBatch {

                public GeometryCache cache;
                public Mesh mesh;
                public Matrix4x4 transform;
                public StructList<Matrix4x4> transforms;
                public RangeInt[] geometryRanges;
                public VertigoMaterial material;
                public int pass;

            }

            private struct PendingDrawCall {

                public Mesh mesh;
                public Matrix4x4 matrix;
                public RangeInt geometryRange;
                public RangeInt triangleRange;

            }

            private VertigoMaterial lastMaterial;


            private StructList<PendingBatch> pendingBatches = new StructList<PendingBatch>();
            private StructList<PendingDrawCall> pendingDrawCalls = new StructList<PendingDrawCall>();
            private PendingBatch pendingBatch;
            private List<Vector3> scratchVector3List = new List<Vector3>();
            protected readonly VertigoMesh.MeshPool meshPool;

            private static readonly List<Vector3> s_MeshVector3 = new List<Vector3>(0);
            private static readonly List<Vector4> s_MeshVector4 = new List<Vector4>(0);
            private static readonly List<Color> s_MeshColor = new List<Color>(0);
            private static readonly List<int> s_MeshInt = new List<int>(0);

            private static readonly StructList<Vector3> s_Vector3Scratch = new StructList<Vector3>(128);
            private static readonly StructList<Vector4> s_Vector4Scratch = new StructList<Vector4>(128);
            private static readonly StructList<Color> s_ColorScratch = new StructList<Color>(128);
            private static readonly StructList<int> s_IntScratch = new StructList<int>(256);
            
            private void PromotePendingBatch() {
                
                if (pendingDrawCalls.size == 0) {
                    return;
                }
                
                                
                VertigoMesh vertigoMesh = meshPool.GetDynamic();
                Mesh mesh = vertigoMesh.mesh;

                // geometry and triangle ranges refer to our internal cache
                
                if (pendingDrawCalls.size > 1) {
                    int vertexCount = 0;
                    int triangleCount = 0;
                    for (int i = 0; i < pendingDrawCalls.size; i++) {
                        PendingDrawCall call = pendingDrawCalls.array[i];
                        // maybe don't support this, let Unity do it
                        if (call.mesh != null) {
                            // transform mesh vertices by transform
                            call.mesh.GetVertices(scratchVector3List);
                            for (int j = 0; j < scratchVector3List.Count; j++) {
                                scratchVector3List[j] = call.matrix.MultiplyVector(scratchVector3List[j]);
                            }

                            call.mesh.SetVertices(scratchVector3List);
                            call.mesh.RecalculateBounds();
                            scratchVector3List.Clear();
                        }
                        else {
                            // transform geometry vertices by transform
                            int start = call.geometryRange.start;
                            int end = call.geometryRange.end;
                            Vector3[] positions = geometryCache.positions.array;
                            for (int j = start; j < end; j++) {
                                positions[j] = call.matrix.MultiplyVector(positions[j]);
                            }

                            s_Vector3Scratch.EnsureCapacity(vertexCount);
                            s_Vector4Scratch.EnsureCapacity(vertexCount);
                            s_ColorScratch.EnsureCapacity(vertexCount);
                            s_IntScratch.EnsureCapacity(triangleCount);
                            
                            s_Vector3Scratch.SetFromRange(geometryCache.positions.array, vertexStart, vertexCount);
                            ListAccessor<Vector3>.SetArray(s_MeshVector3, s_Vector3Scratch.array, vertexCount);
                            mesh.SetVertices(s_MeshVector3);

                            s_Vector3Scratch.SetFromRange(geometryCache.normals.array, vertexStart, vertexCount);
                            ListAccessor<Vector3>.SetArray(s_MeshVector3, s_Vector3Scratch.array, vertexCount);
                            mesh.SetNormals(s_MeshVector3);

                            s_Vector4Scratch.SetFromRange(geometryCache.texCoord0.array, vertexStart, vertexCount);
                            ListAccessor<Vector4>.SetArray(s_MeshVector4, s_Vector4Scratch.array, vertexCount);
                            mesh.SetUVs(0, s_MeshVector4);

                            s_Vector4Scratch.SetFromRange(geometryCache.texCoord1.array, vertexStart, vertexCount);
                            ListAccessor<Vector4>.SetArray(s_MeshVector4, s_Vector4Scratch.array, vertexCount);
                            mesh.SetUVs(1, s_MeshVector4);

                            s_ColorScratch.SetFromRange(geometryCache.colors.array, vertexStart, vertexCount);
                            ListAccessor<Color>.SetArray(s_MeshColor, s_ColorScratch.array, vertexCount);
                            mesh.SetColors(s_MeshColor);
                            
                            s_IntScratch.SetFromRange(cache.triangles.array, triangleStart, triangleCount);
                            for (int i = 0; i < s_IntScratch.size; i++) {
                                s_IntScratch.array[i] -= vertexStart;
                            }

                            ListAccessor<int>.SetArray(s_MeshInt, s_IntScratch.array, triangleCount);
                            mesh.SetTriangles(s_MeshInt, 0);
                            
                            // not sure where this comes from
                            vertexCount += call.geometryRange.length;
                            //todo -- recalculate 3d bounds
                        }
                    }

                    // todo -- only copy used channels for each 
                    
                    
                    PendingBatch batch = new PendingBatch();
                    batch.transform = Matrix4x4.identity;
                    batch.mesh = CreateMeshFromBatch();
                    batch.material = lastMaterial;
                    pendingBatches.Add(batch);
                }
                // todo -- culling, partly handled by layout system for now but should eventually be done in batcher
                // if size is 1 no need to transform vertices
                else {
                    PendingDrawCall call = pendingDrawCalls[0];
                    PendingBatch batch = new PendingBatch();
                    batch.transform = call.matrix;
                    batch.mesh = call.mesh != null ? call.mesh : CreateMeshFromBatch();
                    batch.material = lastMaterial;
                    pendingBatches.Add(batch);
                }

                pendingDrawCalls.Clear();
            }

            private Mesh CreateMeshFromBatch() {
                
                return null;
            }

            public void AddDrawCall(GeometryCache inputCache, RangeInt shapeRange, VertigoMaterial material, in Matrix4x4 transform) {
                if (material == lastMaterial) {
                    // all good to batch.
                }
                else if (lastMaterial.shaderName == "Vertigo/Default" && material.shaderName == "Vertigo/Default") {
                    if (!material.PassesMatch(lastMaterial)) {
                        // maybe pass # is an argument to this fn
                    }

                    if (!material.RenderStateMatches(lastMaterial)) {
                        // nope, promote the current batch
                        PromotePendingBatch();
                        pendingBatches.Add(new PendingBatch() {
                            transform = transform,
                            inputCache,
                            shapeRange
                        });
                    }

                    if (!material.TexturesMatch(lastMaterial)) {
                        // nope
                        // create pending batch for last draw call
                    }

                    if (!material.KeywordsMatch(lastMaterial)) {
                        // check if they are additive
                        // if so, clone material & merge keywords?
                    }

                    // are the textures the same? (or missing)
                    // are the fonts the same? (or missing)
                    // is the mask the same? (or missing)
                    // are the keywords only additive?
                    // is the render state the same?
                    // ok! batch em -> might involve re-writing of attributes
                    // if current batch matrix != transform -> update vertices to reflect transform
                    // calculate bounds for culling? maybe keep the bounds from the shapes around for this purpose?
                }

                // if the shader is the same and all the values are the same then go ahead and batch

                // if we do decide to batch ->


                lastMaterial = material;

                GeometryShape shape = geometryCache.shapes[shapeRange.start];
                int vertexStart = shape.vertexStart;
                int vertexCount = shape.vertexCount;
                int triangleStart = shape.triangleStart;
                int triangleCount = shape.triangleCount;

                for (int i = shapeRange.start + 1; i < shapeRange.end; i++) {
                    vertexCount += shape.vertexCount;
                    triangleCount += shape.triangleCount;
                    // add shapes to our cache and be sure we copy offsets properly
                }

                geometryCache.EnsureAdditionalCapacity(vertexCount, triangleCount);
                geometryCache.positions.AddRange(inputCache.positions, vertexStart, vertexCount);
                geometryCache.normals.AddRange(inputCache.normals, vertexStart, vertexCount);
                geometryCache.texCoord0.AddRange(inputCache.texCoord0, vertexStart, vertexCount);
                geometryCache.texCoord1.AddRange(inputCache.texCoord1, vertexStart, vertexCount);
                geometryCache.texCoord2.AddRange(inputCache.texCoord2, vertexStart, vertexCount);
                geometryCache.texCoord3.AddRange(inputCache.texCoord3, vertexStart, vertexCount);
                geometryCache.colors.AddRange(inputCache.colors, vertexStart, vertexCount);
                geometryCache.triangles.AddRange(inputCache.triangles, triangleStart, triangleCount);
                
                pendingDrawCalls.Add(new PendingDrawCall() {
                    geometryRange = new RangeInt(vertexStart, vertexCount),
                    triangleRange = new RangeInt(triangleStart, triangleCount)
                });

            }

            public void AddDrawCall(Mesh mesh, VertigoMaterial material, in Matrix4x4 transform) {
                PromotePendingBatch();
                pendingBatches.Add(new PendingBatch() {
                    transform = transform,
                    mesh = mesh,
                    pass = 0,
                    material = material.Clone(materialPool.Get())
                });
                //todo -- this is a real draw call, not pending since we won't pool it
            }

            public void Bake(int width, int height, in Matrix4x4 cameraMatrix, StructList<BatchDrawCall> output) {
                // do culling & n shit
                // set material properties
                // for each pending batch, check culling
                // for everything in batch that passes culling -> bake into mesh & material, add to output

                // if two pending batches use the same material hash (not sure how to compute this)
                //     if there is nothing transparent between them, can be baked into one
                // repeat this until there are no more non mergeable batches
            }

            public void Clear() { }

        }

        private void RenderView(UIView view) {
            UIElement[] visibleElements = view.visibleElements.Array;
            int count = view.visibleElements.Count;

            if (count == 0) return;

            for (int i = 0; i < count; i++) {
                UIElement element = visibleElements[i];
                renderInfos.TryGetValue(element.id, out RenderInfo renderInfo);

                if (renderInfo.renderMethod == RenderMethod.None) {
                    continue;
                }

                // todo -- support using elements as masks and optionally also render the element

                float x = element.layoutResult.screenPosition.x;
                float y = element.layoutResult.screenPosition.y;
                float width = element.layoutResult.actualSize.width;
                float height = element.layoutResult.actualSize.height;

                // if must render
                // material.SetTexture(currentTexture);

                // batcher.SetTexture();
                // if material is the same
                // if material is same shader w/ different keywords
                // if all keywords are additive -> a-ok
                // if material

                // ctx.GetSharedMaterial("MaterialName");
                // material.SetStencilState(stencil);
                // material.SetBlendState(blend);
                // material.GetMaterialProperties(MaterialPropertyBlock block);
                // material.GetMaterialInstance();
                // material.SetTexture("texture", texture);

                switch (renderInfo.renderMethod) {
                    case RenderMethod.None:
                        break;

                    case RenderMethod.Text:
                        TextInfo textInfo = ((UITextElement) element).textInfo;
                        fillMaterial.SetTextureProperty(ShaderKey.FontAtlas, textInfo.spanList[0].textStyle.font.atlas);
                        break;

                    case RenderMethod.Painter:
                        ctx.SaveState();
                        // renderInfo.painter.Paint(element, ctx, SVGXMatrix.identity);
                        ctx.RestoreState();
                        break;

                    case RenderMethod.NoBorderFilled:
                        ctx.SetFillColor(renderInfo.backgroundColor);
                        ctx.SetFillMaterial(fillMaterial);
                        ctx.FillRect(x, y, width, height);
                        break;

                    // need new geometry when layout changes
                    // need new geometry when values stored in geometry change

                    case RenderMethod.UniformBorder:
                        if (renderInfo.backgroundImage != null) {
                            fillMaterial.SetTextureProperty(ShaderKey.MainTexture, renderInfo.backgroundImage);

                            // VertigoMaterial.GetPooledMaterial("Vertigo/Default");

                            ShapeGenerator shapeGenerator = new ShapeGenerator();
                            GeometryGenerator geometryGenerator = new GeometryGenerator();

                            geometryGenerator.Fill(shapeGenerator, new RangeInt(0, 1), ShapeMode.SDF, geometryCache);
                            
                            geometryGenerator.SetFillColor(Color.red);
                            
                            geometryGenerator.SetUVTiling(VertexChannel.TextureCoord0, 1, 1);
                            geometryGenerator.SetUVOffset(VertexChannel.TextureCoord1, 1, 1);
                            geometryGenerator.SetDefaultChannels(VertexChannel.Color);
                            
                            geometryGenerator.FillRectSDF(geometryCache, x, y, width, height, VertexChannel.Color | VertexChannel.Normal | VertexChannel.TextureCoord0);
                            // how do i handle pooling materials
                            // accept user materials
                            // respect the moment they drew something
                            // either ignore changes
                            // or clone material 
                            
//                            if (renderInfo.hasBackgroundTransform) {
//                                fillMaterial.EnableKeyword("BackgroundTransform");
//                                fillMaterial.SetFloatProperty(ShaderKey.BackgroundRotation, renderInfo.backgroundRotation);
//                                fillMaterial.SetFloatProperty(ShaderKey.BackgroundTiling, renderInfo.backgroundRotation);
//                                fillMaterial.SetFloatProperty(ShaderKey.BackgroundOffset, renderInfo.backgroundRotation);
                            // tiling & offset is a vector4, can probably do this cpu side when we need to for standard shapes (ie rects)
//                            }


                            // if shader is the same
                            ctx.Draw(geometryCache, fillMaterial);

                            ctx.GetMaterial(unitymaterial | materialName | shader);
                            ctx.GetSharedMaterial(unitymaterial | materialName | shader, keyword[]); //looks in pool for given material, creates if not there, mark shared
                            // shared materials are not cloned when passed into draw methods
                            ctx.GetMaterialInstance(unitymaterial | materialName | shader); // instance materials are cloned when passed to draw methods
                            
                            fillMaterial.SetStencil(1, 0, 1, CompareFunction.Always);
                            fillMaterial.EnableKeyword(keyword);
                            fillMaterial.SetMainTexture(null);

                            ctx.Draw(geometryCache, 0, fillMaterial);
                            ctx.Draw(geometryCache, 0, shineMaterial);

                            if (material.isShared) {
//                                use material
                            }
                            
                            fillMaterial.Reset();
                            fillMaterial.SetShader("shaderName");
                            fillMaterial.EnableKeyword("");
                            fillMaterial.EnableOptionalKeyword("");
                            fillMaterial.SetMainTexture(null);

                            ctx.SetMaterial(material);
                            ctx.SetStrokeMaterial(material);
                            fillMaterial.SetMainTexture(null);

                            geometryGenerator.ComputeTextureCoord0(new Vector2(), new Vector2());
                            
                            ctx.Draw(cache, idx, fillMaterial);
                            
                            cache.Clear();
                            
                            ctx.SetShader();
                            ctx.EnableKeyword();
                            ctx.SetKeywords();
                            ctx.SetMainTexture();
                            ctx.SetFloatProperty();

                            ctx.ClearMaterial();

                            ctx.FillRect(100, 100, 200, 200);

                            ctx.SetUVRect();
                            ctx.EnableUVTiling();
                            ctx.DisableUVTiling();

                            geometryCache.GetTextureCoord2();
                        }

                        ctx.FillRect(x, y, width, height, fillMaterial);
                        ctx.SetStrokeWidth(renderInfo.borderSize.x);
                        ctx.StrokeRect(x, y, width, height);
                        break;

                    case RenderMethod.UniformBorderRadius:
                        ctx.FillRoundedRect(0, 0, 100, 100, 0, 0, 0, 0);
                        break;

                    case RenderMethod.MixedBorder:
                        break;

                    case RenderMethod.MixedBorderRadius:
                        ctx.FillRoundedRect(0, 0, 100, 100, 0, 0, 0, 0);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private bool CanUseSharedMaterial(in RenderInfo info) {
            // if info.material != default return false;

            if (info.renderMethod == RenderMethod.Painter) {
                return false;
            }

            if (info.backgroundImage != null) {
                if (info.backgroundRotation != 0 || info.backgroundScale.x != 1 || info.backgroundScale.y != 1) {
                    return false;
                }
            }

            if (info.uvOffset.x != 0 || info.uvOffset.y != 0) {
                return false;
            }

            return true;
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            // todo -- each view can take its own camera
            // each view has its own camera origin
            // each view can attach to a different camera event
        }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) {
            RenderInfo renderInfo = new RenderInfo();
            UIStyleSet style = element.style;
            renderInfo.backgroundColor = style.BackgroundColor;
            renderInfo.backgroundRotation = style.BackgroundImageRotation.value; // todo -- resolve this to a float
            renderInfo.opacity = style.Opacity;
            // todo resolve to float
            renderInfo.backgroundScale = new Vector2(style.BackgroundImageScaleX.value, style.BackgroundImageScaleY.value);
            renderInfo.visibility = style.Visibility;
            renderInfo.uvRect = new Rect(0, 0, 1, 1);
            renderInfo.uvOffset = new Vector2(0, 0);
            renderInfo.uvTiling = new Vector2(1, 1);
            renderInfo.backgroundTint = new Color32(255, 255, 255, 255);
//            renderInfo.borderRadius = new Vector4(element.layoutResult.style.BorderRadiusTopRight, style.BorderRadiusTopLeft, style.BorderRadiusBottomLeft, style.BorderRadiusBottomRight);
            renderInfo.clipRect = Vector4.zero;
            renderInfo.borderSize = element.layoutResult.border;
            renderInfo.textGlowColor = style.TextGlowColor;
            renderInfo.textColor = style.TextColor;
            renderInfo.borderColorTop = Color.black;
            renderInfo.borderColorRight = Color.black;
            renderInfo.borderColorBottom = Color.black;
            renderInfo.borderColorLeft = Color.black;
            renderInfo.requiresRendering = true;
            renderInfos.geometryId = -1;
            renderInfos[element.id] = renderInfo;
        }

        public readonly Action PaintElement;

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        private void OnStylePropertiesWillChange() { }

        private VertigoMaterial sharedMaterial;

        private void OnStylePropertiesDidChanged(UIElement element) {
            RenderInfo info = renderInfos[element.id];

            // recompute render info here
            bool canUseDefaultShared = CanUseSharedMaterial(info);
            if (!canUseDefaultShared) {
                // see if we can use a non shared one w/o creating a new one
                // if not get new material from the pool & set properties on it as needed
            }

            if (info.material != sharedMaterial && CanUseSharedMaterial(info)) {
                // materialPool.Release(info.material);
                info.material = sharedMaterial;
            }
        }

        private void OnStylePropertyChanged() {
            // recompute render type 
            // maybe update geometry
        }

        public void SetCamera(Camera camera) {
            this.camera = camera; // todo -- should be handled by the view
        }

    }

}