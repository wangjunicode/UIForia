using System;
using System.Collections.Generic;
using Src.Systems;
using SVGX;
using UIForia.Layout;
using UIForia.Rendering.Vertigo;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;
using PooledMesh = UIForia.Rendering.Vertigo.PooledMesh;

namespace UIForia.Rendering {

    public struct FontData {

        public FontAsset fontAsset;
        public float gradientScale;
        public float scaleRatioA;
        public float scaleRatioB;
        public float scaleRatioC;
        public int textureWidth;
        public int textureHeight;

    }

    internal enum RenderOperationType {

        DrawBatch,
        PushRenderTexture,
        ClearRenderTextureRegion,
        BlitRenderTexture,
        SetScissorRect,
        SetCameraViewMatrix,
        SetCameraProjectionMatrix,

        PopRenderTexture

    }

    internal struct RenderOperation {

        public int batchIndex;
        public RenderOperationType operationType;
        public RenderTexture renderTexture;
        public SimpleRectPacker.PackedRect rect;
        public Color color;

        public RenderOperation(int batchIndex) {
            this.batchIndex = batchIndex;
            this.operationType = RenderOperationType.DrawBatch;

            this.rect = default;
            this.renderTexture = null;
            this.color = default;
        }

    }

    public class RenderContext {

        internal const int k_ObjectCount_Small = 8;
        internal const int k_ObjectCount_Medium = 16;
        internal const int k_ObjectCount_Large = 32;
        internal const int k_ObjectCount_Huge = 64;
        internal const int k_ObjectCount_Massive = 128;

        internal StructList<Vector3> positionList;
        internal StructList<Vector4> texCoordList0;
        internal StructList<Vector4> texCoordList1;
        internal StructList<int> triangleList;

        internal StructList<FixedRenderState> fixedRenderStateList;

        private Batch currentBatch;
        private Material activeMaterial;

        private readonly MeshPool uiforiaMeshPool;
        private readonly UIForiaMaterialPool uiforiaMaterialPool;
        private readonly UIForiaMaterialPool pathMaterialPool;
        private readonly StructStack<Rect> clipStack;

        private int defaultRTDepth;

        private static readonly int s_MaxTextureSize;

        private readonly Material effectBlitMaterial;
        private readonly StructList<ScratchRenderTexture> scratchTextures;
        private readonly StructList<RenderOperation> renderCommandList;
        private readonly StructList<Batch> pendingBatches;
        private RenderTexture pingPongTexture;
        private readonly StructStack<RenderArea> areaStack;
        private Material pathMaterial;
        internal ClipContext clipContext;
        private TexturePacker texturePacker;
        private RenderTexture spriteAtlas;
        private Material spriteAtlasMaterial;
        private MaterialPropertyBlock propertyBlock;
        private readonly LightList<PooledMesh> meshesToRelease;

        static RenderContext() {
            int maxTextureSize = SystemInfo.maxTextureSize;
            s_MaxTextureSize = Mathf.Min(maxTextureSize, 4096);
        }

        internal RenderContext(Material batchedMaterial) {
            this.pendingBatches = new StructList<Batch>();
            this.uiforiaMeshPool = new MeshPool();
            this.uiforiaMaterialPool = new UIForiaMaterialPool(batchedMaterial);
            this.positionList = new StructList<Vector3>(128);
            this.texCoordList0 = new StructList<Vector4>(128);
            this.texCoordList1 = new StructList<Vector4>(128);
            this.triangleList = new StructList<int>(128 * 3);
            this.clipStack = new StructStack<Rect>();
            this.renderCommandList = new StructList<RenderOperation>();
            this.scratchTextures = new StructList<ScratchRenderTexture>();
            this.areaStack = new StructStack<RenderArea>();
            this.fixedRenderStateList = new StructList<FixedRenderState>();
            this.clipContext = new ClipContext();
            this.pathMaterial = new Material(Shader.Find("UIForia/UIForiaPathSDF")); // temp
            this.pathMaterialPool = new UIForiaMaterialPool(pathMaterial);
            this.spriteAtlas = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            this.spriteAtlas.name = "UIForia Sprite Atlas";
            this.spriteAtlasMaterial = new Material(Shader.Find("UIForia/UIForiaSpriteAtlas"));
            this.texturePacker = new TexturePacker(Screen.width, Screen.height);
            this.propertyBlock = new MaterialPropertyBlock();
            this.meshesToRelease = new LightList<PooledMesh>();
        }

        public void DrawMesh(Mesh mesh, Material material, in Matrix4x4 transform) {
            FinalizeCurrentBatch();
            currentBatch = new Batch();
            currentBatch.transformData = StructList<Matrix4x4>.Get();
            currentBatch.material = material;
            currentBatch.batchType = BatchType.Mesh;
            currentBatch.unpooledMesh = mesh;
            currentBatch.drawCallSize++;
            currentBatch.uiforiaData = new UIForiaData();
            currentBatch.transformData.Add(transform);
            FinalizeCurrentBatch();
        }

        public void DrawGeometry(UIForiaGeometry geometry, Material material) {
            if (currentBatch.batchType != BatchType.Custom) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.material != material) {
                FinalizeCurrentBatch();
            }

            int start = positionList.size;
            GeometryRange range = new GeometryRange(0, geometry.positionList.size, 0, geometry.triangleList.size);

            positionList.AddRange(geometry.positionList, range.vertexStart, range.vertexEnd);
            texCoordList0.AddRange(geometry.texCoordList0, range.vertexStart, range.vertexEnd);
            texCoordList1.AddRange(geometry.texCoordList1, range.vertexStart, range.vertexEnd);

            currentBatch.drawCallSize++;
            currentBatch.material = material;
            currentBatch.batchType = BatchType.Custom;

            triangleList.EnsureAdditionalCapacity(range.triangleEnd - range.triangleStart);

            int offset = triangleList.size;
            int[] triangles = triangleList.array;
            int[] geometryTriangles = geometry.triangleList.array;

            for (int i = range.triangleStart; i < range.triangleEnd; i++) {
                triangles[offset + i] = start + geometryTriangles[i];
            }

            triangleList.size += (range.triangleEnd - range.triangleStart);

            FinalizeCurrentBatch();
        }


        public void DrawBatchedTextLine(Size size, UIForiaData geometry, GeometryRange range, FontData fontData, in Matrix4x4 matrix, ClipData clipper = null) {
            // atlas built per frame for now, optimize this later w/ region tracking
            // need a way to tell original caller about the uvs
            // probably done internally
            // need to store batch id
            // need bounds of line 
            // need font data
            // need clipping data? or assume we draw text unclipped?

            // if line can't fit in in atlas we need to insert a draw call and not use atlas

            if (currentBatch.transformData.size + 1 >= k_ObjectCount_Huge) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.batchType == BatchType.Unset) {
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            positionList.Add(new Vector3(0, 0, 0));
            positionList.Add(new Vector3(size.width, 0, 0));
            positionList.Add(new Vector3(size.width, -size.height, 0));
            positionList.Add(new Vector3(0, -size.height, 0));

            texCoordList0.Add(new Vector4());
            texCoordList0.Add(new Vector4());
            texCoordList0.Add(new Vector4());
            texCoordList0.Add(new Vector4());

            // is packing better if we know all the text beforehand?, maybe marginally
            // we can assume MOST text is roughly the same size or grouping of sizes
            // sort by height, pack width first?
            // word by word?
            // with words there are more output quads but less overdraw due to whitespace being skipped
            // fuck it, quads are cheap and better for texture usage probably, less overdraw
            // todo -- might need to handle float sizes or at least compensate if accuracy is bad
            //  if (textPacker.TryPackRect((int) size.width, (int) size.height, out SimpleRectPacker.PackedRect rect)) { }
        }


        public void DrawBatchedText(UIForiaGeometry geometry, in GeometryRange range, in Matrix4x4 transform, in FontData fontData) {
            if (currentBatch.transformData.size + 1 >= k_ObjectCount_Huge) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.batchType == BatchType.Unset) {
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            if (currentBatch.uiforiaData.fontData.fontAsset != null && currentBatch.uiforiaData.fontData.fontAsset != fontData.fontAsset) {
                FinalizeCurrentBatch();
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            currentBatch.transformData.Add(transform);
            currentBatch.uiforiaData.objectData0.Add(geometry.objectData);
            currentBatch.uiforiaData.objectData1.Add(geometry.miscData);
            currentBatch.uiforiaData.colors.Add(geometry.packedColors);
            currentBatch.uiforiaData.fontData = fontData;

            // add a quad + matrix w/ default styling

            UpdateUIForiaGeometry(geometry, range);
        }

        internal void DrawClipData(ClipData clipData) {
            clipContext.AddClipper(clipData);
        }

        // todo -- clean up batching code here, ugly as fuck right now
        public void DrawBatchedGeometry(UIForiaGeometry geometry, in GeometryRange range, in Matrix4x4 transform, ClipData clipper = null) {
            if (currentBatch.transformData.size + 1 >= k_ObjectCount_Huge) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch();
            }

            if (currentBatch.batchType == BatchType.Unset) {
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = UIForiaData.Get();
            }

            Texture texture = geometry.mainTexture;
            bool remapUvs = false;
            Vector4 uvs = default;

            if (geometry.mainTexture != null) {
                // todo -- if UVs are transformed don't use sprite atlas
                if (texturePacker.TryPackTexture(geometry.mainTexture, out uvs)) {
                    texture = spriteAtlas;
                    remapUvs = true;
                }
            }

            if (texture != null && currentBatch.uiforiaData.mainTexture != null && currentBatch.uiforiaData.mainTexture != texture) {
                FinalizeCurrentBatch();
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = UIForiaData.Get();
            }

            currentBatch.uiforiaData.mainTexture = texture != null ? texture : currentBatch.uiforiaData.mainTexture;
            currentBatch.uiforiaData.colors.Add(geometry.packedColors);
            currentBatch.uiforiaData.objectData0.Add(geometry.objectData);
            currentBatch.uiforiaData.objectData1.Add(geometry.miscData);

            if (clipper != null) {
                // todo break batch if changed
                currentBatch.uiforiaData.clipTexture = clipper.clipTexture != null ? clipper.clipTexture : currentBatch.uiforiaData.clipTexture;
                currentBatch.uiforiaData.clipUVs.Add(clipper.clipUVs);
                currentBatch.uiforiaData.clipRects.Add(clipper.packedBoundsAndChannel);
            }
            else {
                currentBatch.uiforiaData.clipUVs.Add(default);
                // in order to always draw the thing we take the max fixed float with 0.1 precision we can fit in 16 bits for clip size
                // (2 ^ 16) / 10
                currentBatch.uiforiaData.clipRects.Add(new Vector4(0, VertigoUtil.PackSizeVector(6553f, 6553f)));
            }

            currentBatch.transformData.Add(transform);

            int vertexStart = positionList.size;

            UpdateUIForiaGeometry(geometry, range);

            float Map(float s, float a1, float a2, float b1, float b2) {
                return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
            }

            if (remapUvs) {
                // remap x & y to sprite sheet uvs
                int vertexEnd = positionList.size;
                Vector4[] texCoord0 = texCoordList0.array;
                for (int i = vertexStart; i < vertexEnd; i++) {
                    float x = texCoord0[i].x;
                    float y = texCoord0[i].y;
                    // for now assume 0 to 1 i guess
                    texCoord0[i].x = Map(x, 0, 1, uvs.x, uvs.z);
                    texCoord0[i].y = Map(y, 0, 1, 1 - uvs.w, 1 - uvs.y);
                }
            }
        }

        private void UpdateUIForiaGeometry(UIForiaGeometry geometry, in GeometryRange range) {
            int start = positionList.size;
            int vertexCount = range.vertexEnd - range.vertexStart;
            int triangleCount = range.triangleEnd - range.triangleStart;

            positionList.AddRange(geometry.positionList, range.vertexStart, vertexCount);
            texCoordList0.AddRange(geometry.texCoordList0, range.vertexStart, vertexCount);
            texCoordList1.AddRange(geometry.texCoordList1, range.vertexStart, vertexCount);

            for (int i = start; i < start + vertexCount; i++) {
                texCoordList1.array[i].w = currentBatch.drawCallSize;
            }

            currentBatch.drawCallSize++;

            triangleList.EnsureAdditionalCapacity(triangleCount);

            int offset = triangleList.size;
            int[] triangles = triangleList.array;
            int[] geometryTriangles = geometry.triangleList.array;

            for (int i = 0; i < triangleCount; i++) {
                triangles[offset + i] = start + (geometryTriangles[range.triangleStart + i] - range.vertexStart);
            }

            triangleList.size += triangleCount;
        }

        private void FinalizeCurrentBatch() {
            switch (currentBatch.batchType) {
                // if have pending things to draw, create batch from them
                // select material based on batch size
                case BatchType.Path:
                case BatchType.UIForia: {
                    if (positionList.size == 0) return;
                    PooledMesh mesh = uiforiaMeshPool.Get(); // todo -- maybe worth trying to find a large mesh

                    int vertexCount = positionList.size;
                    int triangleCount = triangleList.size;

                    mesh.SetVertices(positionList.array, vertexCount);
                    mesh.SetTextureCoord0(texCoordList0.array, vertexCount);
                    mesh.SetTextureCoord1(texCoordList1.array, vertexCount);
                    mesh.SetTriangles(triangleList.array, triangleCount);

                    positionList.size = 0;
                    texCoordList0.size = 0;
                    texCoordList1.size = 0;
                    triangleList.size = 0;

                    currentBatch.pooledMesh = mesh;
                    pendingBatches.Add(currentBatch);
                    renderCommandList.Add(new RenderOperation(pendingBatches.size - 1));
                    break;
                }

                case BatchType.Mesh:
                    pendingBatches.Add(currentBatch);
                    renderCommandList.Add(new RenderOperation(pendingBatches.size - 1));

                    break;

                default: {
                    if (positionList.size == 0) {
                        return;
                    }

                    PooledMesh mesh = uiforiaMeshPool.Get(); // todo -- maybe worth trying to find a large mesh
                    int vertexCount = positionList.size;
                    int triangleCount = triangleList.size;
                    mesh.SetVertices(positionList.array, vertexCount);
                    mesh.SetTextureCoord0(texCoordList0.array, vertexCount);
                    mesh.SetTextureCoord1(texCoordList1.array, vertexCount);
                    mesh.SetTriangles(triangleList.array, triangleCount);

                    positionList.size = 0;
                    texCoordList0.size = 0;
                    texCoordList1.size = 0;
                    triangleList.size = 0;
                    currentBatch.pooledMesh = mesh;
                    pendingBatches.Add(currentBatch);

                    renderCommandList.Add(new RenderOperation(pendingBatches.size - 1));
                    break;
                }
            }

            currentBatch = new Batch();
            currentBatch.transformData = StructList<Matrix4x4>.Get();
        }

        private PooledMesh MakeQuadMesh(SimpleRectPacker.PackedRect rect) {
            PooledMesh mesh = uiforiaMeshPool.Get();
            positionList.Add(new Vector3(rect.xMin, -rect.yMin));
            positionList.Add(new Vector3(rect.xMax, -rect.yMin));
            positionList.Add(new Vector3(rect.xMax, -rect.yMax));
            positionList.Add(new Vector3(rect.xMin, -rect.yMax));
            texCoordList0.Add(new Vector4(0, 1, 0, 0));
            texCoordList0.Add(new Vector4(1, 1, 0, 0));
            texCoordList0.Add(new Vector4(1, 0, 0, 0));
            texCoordList0.Add(new Vector4(0, 0, 0, 0));
            triangleList.Add(0);
            triangleList.Add(1);
            triangleList.Add(2);
            triangleList.Add(2);
            triangleList.Add(3);
            triangleList.Add(0);
            mesh.SetVertices(positionList.array, 4);
            mesh.SetTextureCoord0(texCoordList0.array, 4);
            mesh.SetTriangles(triangleList.array, 6);
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
            return mesh;
        }

        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");


        public void Render(Camera camera, CommandBuffer commandBuffer) {
            commandBuffer.Clear();
            for (int i = 0; i < meshesToRelease.size; i++) {
                meshesToRelease[i].Release();
            }

            if (camera != null && camera.targetTexture != null) {
                RenderTexture targetTexture = camera.targetTexture;
                defaultRTDepth = targetTexture.depth;
            }

            StructList<TexturePacker.TextureData> spriteAtlasUpdates = StructList<TexturePacker.TextureData>.Get();
            commandBuffer.SetRenderTarget(spriteAtlas);
            texturePacker.GetTexturesToRender(spriteAtlasUpdates);
            if (spriteAtlasUpdates.size > 0) {
                Vector3 cameraOrigin = camera.transform.position;
                cameraOrigin.x -= 0.5f * Screen.width;
                cameraOrigin.y += (0.5f * Screen.height); // for some reason editor needs this minor adjustment
                cameraOrigin.z += 2;
                Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);
#if DEBUG
                commandBuffer.BeginSample("UIForia Sprite Atlas Update");
#endif
                // could do this in larger batches and have the material sample multiple textures per run
                // ideally we don't update every frame but Unity seems to have other ideas about that and
                // clears our sprite texture between frames. Ask about this
                for (int i = 0; i < spriteAtlasUpdates.size; i++) {
                    PooledMesh mesh = MakeQuadMesh(spriteAtlasUpdates[i].region);
                    propertyBlock.SetTexture(s_MainTex, spriteAtlasUpdates[i].texture);
                    commandBuffer.DrawMesh(mesh.mesh, origin, spriteAtlasMaterial, 0, 0, propertyBlock);
                    meshesToRelease.Add(mesh);
                }
#if DEBUG
                commandBuffer.EndSample("UIForia Sprite Atlas Update");
#endif
            }

            StructList<TexturePacker.TextureData>.Release(ref spriteAtlasUpdates);

            clipContext.Clip(camera, commandBuffer);

#if DEBUG
            commandBuffer.BeginSample("UIForia Render Main");
#endif
            FinalizeCurrentBatch();

            ProcessDrawCommands(camera, commandBuffer);

#if DEBUG

            commandBuffer.EndSample("UIForia Render Main");
#endif
        }

        public void PushClip(Rect clipRect) {
            // todo -- transform
            if (clipStack.size > 0) {
                clipRect = Extensions.RectExtensions.Intersect(clipStack.array[clipStack.size - 1], clipRect);
            }

            clipStack.Push(clipRect);
        }

        public void PopClip() {
            clipStack.Pop();
        }

        public void Clear() {
            currentBatch.transformData?.Release();
            clipContext.Clear();
            UIForiaData.Release(ref currentBatch.uiforiaData);
            currentBatch = new Batch();
            currentBatch.transformData = StructList<Matrix4x4>.Get();

            for (int i = 0; i < pendingBatches.size; i++) {
                pendingBatches[i].pooledMesh?.Release();
                pendingBatches[i].transformData.QuickRelease();

                if (pendingBatches[i].uiforiaData != null) {
                    UIForiaData.Release(ref pendingBatches.array[i].uiforiaData);
                }
            }

            for (int i = 0; i < scratchTextures.size; i++) {
                // todo -- pool the packer
                RenderTexture.ReleaseTemporary(scratchTextures[i].renderTexture);
            }

            if (pingPongTexture != null) {
                RenderTexture.ReleaseTemporary(pingPongTexture);
                pingPongTexture = null;
            }

            fixedRenderStateList.size = 0;
            renderCommandList.QuickClear();
            scratchTextures.QuickClear();
            pendingBatches.Clear();
        }

        public void PushPostEffect(Material material, Vector2 position, Size size) {
            SimpleRectPacker packer = null;
            RenderTexture renderTexture = null;
            SimpleRectPacker.PackedRect rect = default;

            for (int i = 0; i < scratchTextures.size; i++) {
                if (scratchTextures.array[i].packer.TryPackRect((int) size.width, (int) size.height, out rect)) {
                    packer = scratchTextures.array[i].packer;
                    renderTexture = scratchTextures.array[i].renderTexture;
                    break;
                }
            }

            // todo -- do not allocate

            if (packer == null) {
                packer = new SimpleRectPacker(Screen.width, Screen.height, 5);

                if (!packer.TryPackRect((int) size.width, (int) size.height, out rect)) {
                    throw new Exception($"Cannot fit size {size} in a render texture. Max texture size is {s_MaxTextureSize}");
                }

                renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, defaultRTDepth, RenderTextureFormat.DefaultHDR);
                scratchTextures.Add(new ScratchRenderTexture() {
                    packer = packer,
                    renderTexture = renderTexture
                });
            }

            renderCommandList.Add(new RenderOperation() {
                operationType = RenderOperationType.PushRenderTexture,
                renderTexture = renderTexture,
                rect = rect
            });
        }

        public void SetRenderTexture(RenderTexture texture) {
            if (texture == null) {
                renderCommandList.Add(new RenderOperation() {
                    operationType = RenderOperationType.PopRenderTexture,
                    renderTexture = texture
                });
            }
            else {
                renderCommandList.Add(new RenderOperation() {
                    operationType = RenderOperationType.PushRenderTexture,
                    renderTexture = texture,
                    rect = new SimpleRectPacker.PackedRect()
                });
            }
        }

        public RenderArea PushRenderArea(SizeInt size, in Color? clearColor = null) {
            SimpleRectPacker packer = null;
            RenderTexture renderTexture = null;
            SimpleRectPacker.PackedRect rect = default;

            FinalizeCurrentBatch();

            for (int i = 0; i < scratchTextures.size; i++) {
                if (scratchTextures.array[i].packer.TryPackRect(size.width, size.height, out rect)) {
                    packer = scratchTextures.array[i].packer;
                    renderTexture = scratchTextures.array[i].renderTexture;
                    break;
                }
            }

            // todo -- do not allocate

            if (packer == null) {
                packer = new SimpleRectPacker(Screen.width, Screen.height, 5);

                if (!packer.TryPackRect(size.width, size.height, out rect)) {
                    throw new Exception($"Cannot fit size {size} in a render texture. Max texture size is {s_MaxTextureSize}");
                }

                renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, defaultRTDepth, RenderTextureFormat.DefaultHDR);
                scratchTextures.Add(new ScratchRenderTexture() {
                    packer = packer,
                    renderTexture = renderTexture
                });
            }


            renderCommandList.Add(new RenderOperation() {
                operationType = RenderOperationType.PushRenderTexture,
                renderTexture = renderTexture,
                rect = rect
            });

            if (clearColor != null) {
                renderCommandList.Add(new RenderOperation() {
                    operationType = RenderOperationType.ClearRenderTextureRegion,
                    renderTexture = renderTexture,
                    rect = rect,
                    color = clearColor.Value
                });
            }

            RenderArea area = new RenderArea(renderTexture, rect);
            areaStack.Push(area);
            return area;
        }

        public void ClearRenderTarget(Color color) {
            renderCommandList.Add(new RenderOperation() {
                operationType = RenderOperationType.ClearRenderTextureRegion,
                color = color
            });
        }

        public RenderArea PopRenderArea() {
            FinalizeCurrentBatch();

            //   EffectData effectData = effectStack.PopUnchecked();

//            currentBatch.material = effectData.material;
//            currentBatch.batchType = BatchType.Custom;
//            currentBatch.drawCallSize = 1;
//            
            renderCommandList.Add(new RenderOperation() {
                operationType = RenderOperationType.BlitRenderTexture,
                // batchIndex = pendingBatches.size
            });
//            

            RenderArea area = areaStack.Pop();
            // todo -- mark area as free
            return area;
        }

        private void ProcessDrawCommands(Camera camera, CommandBuffer commandBuffer) {
            Matrix4x4 cameraMatrix = camera.cameraToWorldMatrix;
            commandBuffer.SetViewProjectionMatrices(cameraMatrix, camera.projectionMatrix);

            RenderOperation[] renderCommands = this.renderCommandList.array;
            int commandCount = renderCommandList.size;

            StructStack<RenderArea> rtStack = StructStack<RenderArea>.Get();

            rtStack.Push(new RenderArea(null, default));

            // assert camera & has texture

            Vector3 cameraOrigin = camera.transform.position;
            cameraOrigin.x -= 0.5f * Screen.width;
            cameraOrigin.y += (0.5f * Screen.height) - 2; // for some reason editor needs this minor adjustment
            cameraOrigin.z += 2;

            Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);

            Batch[] batches = pendingBatches.array;

            for (int i = 0; i < commandCount; i++) {
                ref RenderOperation cmd = ref renderCommands[i];

                switch (cmd.operationType) {
                    case RenderOperationType.DrawBatch:

                        ref Batch batch = ref batches[cmd.batchIndex];

                        switch (batch.batchType) {
                            case BatchType.UIForia: {
                                UIForiaPropertyBlock uiForiaPropertyBlock = uiforiaMaterialPool.GetPropertyBlock(batch.drawCallSize);

                                uiForiaPropertyBlock.SetData(batch.uiforiaData, batch.transformData);

                                commandBuffer.DrawMesh(batch.pooledMesh.mesh, origin, uiForiaPropertyBlock.material, 0, 0, uiForiaPropertyBlock.matBlock);
                                break;
                            }

                            case BatchType.Path: {
                                UIForiaPropertyBlock pathPropertyBlock = pathMaterialPool.GetPropertyBlock(batch.drawCallSize);

                                pathPropertyBlock.SetSDFData(batch.uiforiaData, batch.transformData);
                                Material material = pathPropertyBlock.material;
                                if (batch.renderStateId != 0) {
                                    material = new Material(pathPropertyBlock.material); // todo -- pool this!
                                    FixedRenderState fixedRenderState = fixedRenderStateList.array[batch.renderStateId - 1];
                                    MaterialUtil.SetupState(material, fixedRenderState);
                                }

                                commandBuffer.DrawMesh(batch.pooledMesh.mesh, origin, material, 0, 0, pathPropertyBlock.matBlock);
                                break;
                            }

                            case BatchType.Mesh: {
                                Matrix4x4 m = batch.transformData.array[0] * origin;
                                commandBuffer.DrawMesh(batch.unpooledMesh, m, batch.material, 0, batch.material.passCount - 1, null);
                                break;
                            }
                        }

                        break;

                    case RenderOperationType.PushRenderTexture:

                        if (rtStack.array[rtStack.size - 1].renderTexture != cmd.renderTexture) {
                            // todo -- figure out the weirdness with perspective or view when texture is larger than camera texture
                            commandBuffer.SetRenderTarget(cmd.renderTexture);
                            int width = cmd.renderTexture.width / 2;
                            int height = cmd.renderTexture.height / 2;
                            Matrix4x4 projection = camera.projectionMatrix; //Matrix4x4.Ortho(-width, width, -height, height, 0.1f, 9999);
                            commandBuffer.SetViewProjectionMatrices(cameraMatrix, projection);
                            commandBuffer.ClearRenderTarget(true, true, cmd.color);
                        }

                        // always push so pop will pop the right texture, duplicate refs are ok
                        rtStack.Push(new RenderArea(cmd.renderTexture, cmd.rect));
                        break;

                    case RenderOperationType.ClearRenderTextureRegion:
                        commandBuffer.ClearRenderTarget(true, true, cmd.color);
                        break;

                    case RenderOperationType.BlitRenderTexture:

                        // pop texture
                        // blit to next one up the stack
                        // some platforms can't use CopyTexture. Need a shader for that

                        RenderArea area = rtStack.PopUnchecked();
                        RenderArea next = rtStack.PeekUnchecked();
                        RenderTexture rt = area.renderTexture;

                        int srcWidth = area.renderArea.xMax - area.renderArea.xMin;
                        int srcHeight = area.renderArea.yMax - area.renderArea.yMin;

                        int srcX = area.renderArea.xMin;
                        int srcY = rt.height - srcHeight;
                        int dstX = 0; // todo -- need to figure out where this goes, maybe part of the push?
                        int dstY = rt.height - srcHeight;

                        if (next.renderTexture == rt) {
                            if (pingPongTexture == null) {
                                pingPongTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.DefaultHDR);
                            }

                            commandBuffer.CopyTexture(rt, 0, 0, srcX, srcY, srcWidth, srcHeight, pingPongTexture, 0, 0, dstX, dstY);
                            commandBuffer.CopyTexture(pingPongTexture, 0, 0, srcX, srcY, srcWidth, srcHeight, rt, 0, 0, dstX, dstY);
                        }
                        else {
                            if (next.renderTexture == null) {
                                commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                                // commandBuffer.CopyTexture(rt, 0, 0, srcX, srcY, srcWidth, srcHeight, BuiltinRenderTextureType.CurrentActive, 0, 0, dstX, dstY);
                            }
                            else {
                                commandBuffer.CopyTexture(rt, 0, 0, srcX, srcY, srcWidth, srcHeight, next.renderTexture, 0, 0, dstX, dstY);
                            }
                        }

                        break;

                    case RenderOperationType.PopRenderTexture:
                        commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                        break;

                    case RenderOperationType.SetScissorRect:
                        break;

                    case RenderOperationType.SetCameraViewMatrix:
                        break;

                    case RenderOperationType.SetCameraProjectionMatrix:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            StructStack<RenderArea>.Release(ref rtStack);
        }


        private struct ScratchRenderTexture {

            public RenderTexture renderTexture;
            public SimpleRectPacker packer;

        }

        public struct RenderArea {

            public readonly SimpleRectPacker.PackedRect renderArea;
            public readonly RenderTargetIdentifier rtId;
            public readonly RenderTexture renderTexture;

            public RenderArea(RenderTexture renderTexture, SimpleRectPacker.PackedRect renderArea) {
                this.renderTexture = renderTexture;
                this.renderArea = renderArea;
                this.rtId = renderTexture;
            }

        }

        public void Destroy() {
            clipContext?.Destroy();
            clipContext = null;
            UnityEngine.Object.Destroy(spriteAtlas);
            UnityEngine.Object.Destroy(spriteAtlasMaterial);

            for (int i = 0; i < meshesToRelease.size; i++) {
                meshesToRelease[i].Release();
            }

            uiforiaMeshPool.Destroy();
        }

//        public RenderTargetIdentifier GetNextRenderTarget() {
//            return renderTargetStack.Peek();
//        }

        // need to ping pong if target texture is the same one used by the area
        public Texture GetTextureFromArea(RenderArea area, RenderTargetIdentifier? outputTarget = null) {
            return area.renderTexture; //.Peek();
        }

        public void DrawPath(Path2D path) {
            // path drawing always breaks batch for now
            FinalizeCurrentBatch();
            path.UpdateGeometry();

            if (path.drawCallList.size == 0) return;

            int lastBlendStateId = -1;

            currentBatch.batchType = BatchType.Path;
            currentBatch.uiforiaData = currentBatch.uiforiaData ?? UIForiaData.Get();

            // eventually do pre-pass for texture swaps, text, and clipping as normal
            Texture lastTexture = null;

            // todo -- implement look-ahead so we can figure out if we break the batch or not
            // multiple 'draw calls' can be done in one operation, single copy instead of n

            int vertexAdjustment = 0;

            for (int i = 0; i < path.drawCallList.size; i++) {
                ref SVGXDrawCall2 drawCall = ref path.drawCallList.array[i];

                if (drawCall.material != null) {
                    continue;
                }

                Texture mainTexture = null;

                switch (drawCall.type) {
                    case DrawCallType.ShadowStroke:
                        break;

                    case DrawCallType.StandardStroke:
                        mainTexture = path.strokeStyles.array[drawCall.styleIdx].texture;
                        break;

                    case DrawCallType.ShadowFill:
                    case DrawCallType.StandardFill:
                        mainTexture = path.fillStyles.array[drawCall.styleIdx].texture;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //todo -- if range is larger than Huge batch size, split it

                if (mainTexture != null && mainTexture != lastTexture && lastTexture != null) {
                    vertexAdjustment += positionList.size;
                    FinalizeCurrentBatch();
                    currentBatch.batchType = BatchType.Path;
                    currentBatch.uiforiaData = UIForiaData.Get();
                    if (drawCall.renderStateId != lastBlendStateId) {
                        lastBlendStateId = drawCall.renderStateId;
                        fixedRenderStateList.Add(path.renderStateList[drawCall.renderStateId]);
                        currentBatch.renderStateId = fixedRenderStateList.size;
                    }
                }
                else if (drawCall.renderStateId != lastBlendStateId) {
                    lastBlendStateId = drawCall.renderStateId;
                    vertexAdjustment += positionList.size;
                    FinalizeCurrentBatch();
                    currentBatch.batchType = BatchType.Path;
                    currentBatch.uiforiaData = UIForiaData.Get();
                    fixedRenderStateList.Add(path.renderStateList[drawCall.renderStateId]);
                    currentBatch.renderStateId = fixedRenderStateList.size;
                }

                lastTexture = mainTexture ?? lastTexture;

                currentBatch.uiforiaData.mainTexture = mainTexture != null ? mainTexture : currentBatch.uiforiaData.mainTexture;
                currentBatch.uiforiaData.clipTexture = null; //geometry.clipTexture != null ? geometry.clipTexture : currentBatch.uiforiaData.clipTexture;;
                currentBatch.transformData.Add(path.transforms.array[drawCall.transformIdx]);

                int objectStart = drawCall.objectRange.start;
                int objectEnd = drawCall.objectRange.end;

                currentBatch.uiforiaData.objectData0.EnsureAdditionalCapacity(objectEnd - objectStart);
                currentBatch.uiforiaData.colors.EnsureAdditionalCapacity(objectEnd - objectStart);
                Vector4[] objectData = currentBatch.uiforiaData.objectData0.array;
                Vector4[] colorData = currentBatch.uiforiaData.colors.array;
                int insertIdx = currentBatch.uiforiaData.objectData0.size;

                for (int j = objectStart; j < objectEnd; j++) {
                    objectData[insertIdx] = path.objectDataList.array[j].objectData;
                    colorData[insertIdx] = path.objectDataList.array[j].colorData;
                    insertIdx++;
                }

                currentBatch.uiforiaData.objectData0.size = insertIdx;
                currentBatch.uiforiaData.colors.size = insertIdx;

                int start = positionList.size;

                GeometryRange range = drawCall.geometryRange;
                int vertexCount = range.vertexEnd - range.vertexStart;
                int triangleCount = range.triangleEnd - range.triangleStart;

                positionList.AddRange(path.geometry.positionList, range.vertexStart, vertexCount);
                texCoordList0.AddRange(path.geometry.texCoordList0, range.vertexStart, vertexCount);
                texCoordList1.AddRange(path.geometry.texCoordList1, range.vertexStart, vertexCount);
                triangleList.EnsureAdditionalCapacity(triangleCount);

                Vector4[] texCoord1 = texCoordList1.array;

                for (int j = drawCall.objectRange.start; j < drawCall.objectRange.end; j++) {
                    Path2D.ObjectData shape = path.objectDataList.array[j];
                    int geometryStart = shape.geometryRange.vertexStart;
                    int geometryEnd = shape.geometryRange.vertexEnd;
                    int objectIndex = currentBatch.drawCallSize++;
                    for (int s = geometryStart; s < geometryEnd; s++) {
                        texCoord1[s - vertexAdjustment].w = objectIndex;
                    }
                }

                int offset = triangleList.size;
                int[] triangles = triangleList.array;
                int[] geometryTriangles = path.geometry.triangleList.array;

                for (int t = 0; t < triangleCount; t++) {
                    triangles[offset + t] = start + (geometryTriangles[range.triangleStart + t] - range.vertexStart);
                }

                triangleList.size += triangleCount;
            }
        }

    }

}