using System;
using Src.Systems;
using SVGX;
using UIForia.Rendering.Vertigo;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Rendering {

    public struct ClipBatch {

        public Texture2D texture;
        public StructList<Vector4> objectData;
        public StructList<Vector4> colorData;
        public StructList<Matrix4x4> transforms;
        public PooledMesh pooledMesh;

    }

    public class ClipContext {

        internal StructList<Vector3> positionList;
        internal StructList<Vector4> texCoordList0;
        internal StructList<Vector4> texCoordList1;
        internal StructList<int> triangleList;

        internal LightList<ClipData> clippers;

        private Material reset;
        private Material firstPass;
        private Material clearMaterial;
        private Material countMaterial;
        private Material blitCountMaterial;
        private Material clearCountMaterial;

        private ClipMaterialPool clipMaterialPool;
        private StructList<ClipBatch> batchesToRender;
        private readonly SimpleRectPacker maskPackerR = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private readonly SimpleRectPacker maskPackerG = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private readonly SimpleRectPacker maskPackerB = new SimpleRectPacker(Screen.width, Screen.height, 4);
        private readonly SimpleRectPacker maskPackerA = new SimpleRectPacker(Screen.width, Screen.height, 0);
        private RenderTexture clipTexture;

        private readonly MeshPool meshPool;
        private static readonly int s_Color = Shader.PropertyToID("_Color");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");

        public ClipContext() {
            Material clipMaterial = new Material(Shader.Find("UIForia/UIForiaPathSDF")); // todo fix this
            this.firstPass = new Material(clipMaterial);
            
            this.clearMaterial = new Material(Shader.Find("UIForia/UIForiaClearClipRegions"));
            this.clearCountMaterial = new Material(clearMaterial);
            
            this.clearMaterial.SetColor(s_Color, Color.white);
            this.clearCountMaterial.SetColor(s_Color, new Color(0, 0, 0, 0));
            
            this.countMaterial = new Material(Shader.Find("UIForia/UIForiaClipCount"));
            this.blitCountMaterial = new Material(Shader.Find("UIForia/UIForiaClipBlit"));
            this.clipMaterialPool = new ClipMaterialPool(clipMaterial);
            this.positionList = new StructList<Vector3>();
            this.texCoordList0 = new StructList<Vector4>();
            this.texCoordList1 = new StructList<Vector4>();
            this.triangleList = new StructList<int>();
            this.batchesToRender = new StructList<ClipBatch>();
            this.clippers = new LightList<ClipData>();
            this.meshPool = new MeshPool();
            this.clipTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);

            BlendState blendState = BlendState.Default;
            blendState.sourceBlendMode = BlendMode.One;
            blendState.destBlendMode = BlendMode.One;
            blendState.blendOp = BlendOp.Min;
            DepthState depthState = DepthState.Default;
            depthState.compareFunction = CompareFunction.Equal;
            depthState.writeEnabled = true;

            MaterialUtil.SetupState(firstPass, new FixedRenderState(blendState, depthState));
        }

        internal void AddClipper(ClipData clipData) {
            clippers.Add(clipData);
        }

        // todo for tomorrow
        // stop allocating meshes, pool materials
        // handle not drawing screen aligned clipping bounds
        // handle multiple clip shapes in a path
        // maybe handle textures
        // handle lots of clip shapes
        // handle masking channels
        // profile
        
        public void ConstructClipData() {
            maskPackerR.Clear();
            maskPackerG.Clear();
            maskPackerB.Clear();
            maskPackerA.Clear();
            // todo -- profile sorting by size
            // might not need the -1 if screen level one is never sent
            // also don't need to render clipper for a view since it is automatically rectangular
            // basically any pure screen aligned rectangle doesn't need to be rendered out
            for (int i = 0; i < clippers.size - 1; i++) {
                ClipData clipData = clippers.array[i];

                clipData.zIndex = i + 1;

                int width = (int) (clipData.screenSpaceBounds.z - clipData.screenSpaceBounds.x);
                int height = (int) (clipData.screenSpaceBounds.w - clipData.screenSpaceBounds.y);
                SimpleRectPacker.PackedRect region;

                if (maskPackerA.TryPackRect(width, height, out region)) {
                    // note this is in 2 point form, not height & width
                    clipData.clipTexture = clipTexture;
                    clipData.textureChannel = 3;
                    clipData.textureRegion = region;
                    clipData.clipUVs = new Vector4(
                        region.xMin / (float) clipTexture.width,
                        region.yMin / (float) clipTexture.height,
                        region.xMax / (float) clipTexture.width,
                        region.yMax / (float) clipTexture.height
                    );
                }
                else {
                    Debug.Log($"Can't fit {width}, {height} into clip texture");
                }
            }
        }

        private void Gather() {
            for (int i = 0; i < clippers.size; i++) {
                ClipData clipper = clippers.array[i];
                ClipData ptr = clipper.parent;
                while (ptr != null) {
                    ptr.dependents.Add(clipper);
                    ptr = ptr.parent;
                }
            }
        }

        public void Clip(Camera camera, CommandBuffer commandBuffer) {
            for (int i = 0; i < batchesToRender.size; i++) {
                batchesToRender[i].pooledMesh.Release();
                StructList<Matrix4x4>.Release(ref batchesToRender.array[i].transforms);
                StructList<Vector4>.Release(ref batchesToRender.array[i].colorData);
                StructList<Vector4>.Release(ref batchesToRender.array[i].colorData);
            }

            batchesToRender.Clear();
            Gather();

            maskPackerA.Clear();

            commandBuffer.SetRenderTarget(clipTexture);
            commandBuffer.ClearRenderTarget(true, true, Color.black);
            Vector3 cameraOrigin = camera.transform.position;
            cameraOrigin.x -= 0.5f * Screen.width;
            cameraOrigin.y += (0.5f * Screen.height);
            cameraOrigin.z += 2;

            Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);

            LightList<ClipData> texturedClippers = LightList<ClipData>.Get();

            ClipBatch batch = new ClipBatch();
            batch.transforms = new StructList<Matrix4x4>();
            batch.colorData = new StructList<Vector4>();
            batch.objectData = new StructList<Vector4>();

            PooledMesh regionMesh = GetRegionMesh(); // todo -- release this or store it

            commandBuffer.DrawMesh(regionMesh.mesh, origin, clearMaterial, 0, 0);

            // todo -- handle multiple shapes from one path

            for (int i = 0; i < clippers.size - 1; i++) {
                ClipData clipData = clippers[i];
                ClipShape clipShape = clippers[i].clipShape;
                Path2D path = clipShape.path;

                path.UpdateGeometry(); // should early out if no update required

                if (AnyShapeUsesTextures(path)) {
                    // todo -- handle textures
                    // todo -- handle text
                    continue;
                }

                batch = DrawShapesInPath(batch, path, clipData, clipData);

                for (int j = 0; j < clipData.dependents.size; j++) {
                    batch = DrawShapesInPath(batch, path, clipData, clipData.dependents[j]);
                }
            }

            FinalizeBatch(batch);


            for (int i = 0; i < batchesToRender.size; i++) {
                ref ClipBatch clipBatch = ref batchesToRender.array[i];

                ClipPropertyBlock propertyBlock = clipMaterialPool.GetPropertyBlock(clipBatch.transforms.size);

                propertyBlock.SetData(clipBatch);

                commandBuffer.DrawMesh(clipBatch.pooledMesh.mesh, origin, firstPass, 0, 0, propertyBlock.matBlock);
            }

            RenderTexture countTexture = RenderTexture.GetTemporary(clipTexture.width, clipTexture.height, 24, RenderTextureFormat.ARGB32);

            commandBuffer.SetRenderTarget(countTexture);
#if DEBUG
            commandBuffer.BeginSample("UIForia Clip Count");
#endif
            commandBuffer.ClearRenderTarget(true, true, Color.black);
            
            commandBuffer.DrawMesh(regionMesh.mesh, origin, clearCountMaterial, 0, 0);

            for (int i = 0; i < batchesToRender.size; i++) {
                ref ClipBatch clipBatch = ref batchesToRender.array[i];

                ClipPropertyBlock propertyBlock = clipMaterialPool.GetPropertyBlock(clipBatch.transforms.size);

                propertyBlock.SetData(clipBatch);

                commandBuffer.DrawMesh(batchesToRender[i].pooledMesh.mesh, origin, countMaterial, 0, 0, propertyBlock.matBlock);
            }
#if DEBUG
            commandBuffer.EndSample("UIForia Clip Count");
#endif

            commandBuffer.SetRenderTarget(clipTexture);

            PooledMesh blitMesh = GetBlitMesh(clipTexture.width, clipTexture.height);

            blitCountMaterial.SetTexture(s_MainTex, countTexture);
            
            commandBuffer.DrawMesh(regionMesh.mesh, origin, blitCountMaterial, 0, 0);

            commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            LightList<ClipData>.Release(ref texturedClippers);
        }

        private PooledMesh GetBlitMesh(int width, int height) {
            PooledMesh retn = meshPool.Get();
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
            positionList.Add(new Vector3(0, 0, 0));
            positionList.Add(new Vector3(width, 0, 0));
            positionList.Add(new Vector3(width, -height, 0));
            positionList.Add(new Vector3(0, -height, 0));
            
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
            retn.SetVertices(positionList.array, 4);
            retn.SetTextureCoord0(texCoordList0.array, 4);
            retn.SetTriangles(triangleList.array, 6);
            return retn;
        }

        private ClipBatch FinalizeBatch(ClipBatch clipBatch) {
            clipBatch.pooledMesh = meshPool.Get();

            clipBatch.pooledMesh.SetVertices(positionList.array, positionList.size);
            clipBatch.pooledMesh.SetTextureCoord0(texCoordList0.array, texCoordList0.size);
            clipBatch.pooledMesh.SetTextureCoord1(texCoordList1.array, texCoordList1.size);
            clipBatch.pooledMesh.SetTriangles(triangleList.array, triangleList.size);

            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
            // todo handle texture n stuff
            batchesToRender.Add(clipBatch);

            clipBatch = new ClipBatch();
            clipBatch.transforms = StructList<Matrix4x4>.Get();
            clipBatch.colorData = StructList<Vector4>.Get();
            clipBatch.objectData = StructList<Vector4>.Get();

            return clipBatch;
        }


        private bool BatchCanHandleShape(in ClipBatch batch, in SVGXDrawCall2 drawCall) {
            return true;
        }

        private bool AnyShapeUsesTextures(Path2D path) {
            return false;
        }

        private ClipBatch DrawShapesInPath(ClipBatch clipBatch, Path2D path, ClipData clipData, ClipData target) {
            int vertexAdjustment = 0;

            for (int drawCallIndex = 0; drawCallIndex < path.drawCallList.size; drawCallIndex++) {
                ref SVGXDrawCall2 drawCall = ref path.drawCallList.array[drawCallIndex];

                if (!BatchCanHandleShape(clipBatch, drawCall)) {
                    // handle batch breaking here    
                    clipBatch = FinalizeBatch(clipBatch);
                    throw new NotImplementedException();
                }

                int objectStart = drawCall.objectRange.start;
                int objectEnd = drawCall.objectRange.end;
                int insertIdx = clipBatch.objectData.size;

                // todo -- keep looping until batch would break
                // break conditions: texture change (can mitigate)
                //                   too many vertices
                //                   too many objects

                clipBatch.objectData.EnsureAdditionalCapacity(objectEnd - objectStart);
                clipBatch.colorData.EnsureAdditionalCapacity(objectEnd - objectStart);
                clipBatch.transforms.EnsureAdditionalCapacity(objectEnd - objectStart);

                GeometryRange range = drawCall.geometryRange;
                int vertexCount = range.vertexEnd - range.vertexStart;
                int triangleCount = range.triangleEnd - range.triangleStart;

                int start = positionList.size;

                positionList.AddRange(path.geometry.positionList, range.vertexStart, vertexCount);
                texCoordList0.AddRange(path.geometry.texCoordList0, range.vertexStart, vertexCount);
                texCoordList1.AddRange(path.geometry.texCoordList1, range.vertexStart, vertexCount);

                Vector4[] texCoord1 = texCoordList1.array;

                for (int objIdx = objectStart; objIdx < objectEnd; objIdx++) {
                    clipBatch.objectData.array[insertIdx] = path.objectDataList.array[objIdx].objectData;
                    clipBatch.colorData.array[insertIdx] = path.objectDataList.array[objIdx].colorData;
                    Matrix4x4 matrix;
                    if (path.transforms != null) {
                        matrix = path.transforms.array[drawCall.transformIdx];
                    }
                    else {
                        matrix = Matrix4x4.identity;
                    }

                    float x = matrix.m03;
                    float y = matrix.m13;

                    float xDiff = target.textureRegion.xMin - x;
                    float yDiff = target.textureRegion.yMin - y;

                    if (target != clipData) {
                        xDiff += (clipData.screenSpaceBounds.x - target.screenSpaceBounds.x);
                        yDiff += clipData.screenSpaceBounds.y - target.screenSpaceBounds.y;
                    }

                    matrix.m03 = x + xDiff;
                    matrix.m13 = -(y + yDiff);
                    matrix.m23 = target.zIndex;

                    clipBatch.transforms[insertIdx] = matrix;

                    ref Path2D.ObjectData objectData = ref path.objectDataList.array[objIdx];
                    int geometryStart = objectData.geometryRange.vertexStart;
                    int geometryEnd = objectData.geometryRange.vertexEnd;
                    for (int s = geometryStart; s < geometryEnd; s++) {
                        texCoord1[start + (s - vertexAdjustment)].w = insertIdx;
                    }

                    insertIdx++;
                }

                clipBatch.objectData.size = insertIdx;
                clipBatch.colorData.size = insertIdx;
                clipBatch.transforms.size = insertIdx;

                triangleList.EnsureAdditionalCapacity(triangleCount);

                int offset = triangleList.size;
                int[] triangles = triangleList.array;
                int[] geometryTriangles = path.geometry.triangleList.array;

                for (int t = 0; t < triangleCount; t++) {
                    triangles[offset + t] = start + (geometryTriangles[range.triangleStart + t] - range.vertexStart);
                }

                triangleList.size += triangleCount;
            }

            return clipBatch;
        }

        private PooledMesh GetRegionMesh() {
            positionList.EnsureAdditionalCapacity(clippers.size * 4);
            texCoordList0.EnsureAdditionalCapacity(clippers.size * 4);
            texCoordList1.EnsureAdditionalCapacity(clippers.size * 4);
            triangleList.EnsureAdditionalCapacity(clippers.size * 6);
            int vertIdx = 0;
            int triIdx = 0;

            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            for (int i = 0; i < clippers.size - 1; i++) {
                ClipData clipData = clippers.array[i];
                SimpleRectPacker.PackedRect region = clipData.textureRegion;
                int cnt = -1; // minus 1 accounts for screen
                ClipData ptr = clipData.parent;
                while (ptr != null) {
                    // todo -- only add 1 to cnt if parent clipper is actually rendered!
                    cnt++;
                    ptr = ptr.parent;
                }
                
                positions[vertIdx + 0] = new Vector3(region.xMin, -region.yMin, clippers.array[i].zIndex);
                positions[vertIdx + 1] = new Vector3(region.xMax, -region.yMin, clippers.array[i].zIndex);
                positions[vertIdx + 2] = new Vector3(region.xMax, -region.yMax, clippers.array[i].zIndex);
                positions[vertIdx + 3] = new Vector3(region.xMin, -region.yMax, clippers.array[i].zIndex);
                
                texCoord0[vertIdx + 0] = new Vector4(clipData.clipUVs.x, clipData.clipUVs.y, cnt, 0);
                texCoord0[vertIdx + 1] = new Vector4(clipData.clipUVs.z, clipData.clipUVs.y, cnt, 0);
                texCoord0[vertIdx + 2] = new Vector4(clipData.clipUVs.z, clipData.clipUVs.w, cnt, 0);
                texCoord0[vertIdx + 3] = new Vector4(clipData.clipUVs.x, clipData.clipUVs.w, cnt, 0);
                
                triangles[triIdx++] = vertIdx + 0;
                triangles[triIdx++] = vertIdx + 1;
                triangles[triIdx++] = vertIdx + 2;
                triangles[triIdx++] = vertIdx + 2;
                triangles[triIdx++] = vertIdx + 3;
                triangles[triIdx++] = vertIdx + 0;
                vertIdx += 4;
            }

            // no need to set sizes for lists!

            PooledMesh pooledMesh = meshPool.Get();
            pooledMesh.SetVertices(positions, vertIdx);
            pooledMesh.SetTextureCoord0(texCoord0, vertIdx);
            pooledMesh.SetTriangles(triangles, triIdx);

            return pooledMesh;
        }

        public void Clear() {
            clippers.Clear();
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
        }

    }

}