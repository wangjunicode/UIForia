using System;
using UIForia.Rendering.Vertigo;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;
using PooledMesh = UIForia.Rendering.Vertigo.PooledMesh;

namespace UIForia.Rendering {

    public class UIForiaPropertyBlock {

        public readonly int size;
        public readonly Material material;
        public readonly MaterialPropertyBlock matBlock;
        public readonly Matrix4x4[] transformData;
        public readonly Vector4[] colorData;
        public readonly Vector4[] objectData;

        public static readonly int s_TransformDataKey = Shader.PropertyToID("_TransformData");
        public static readonly int s_ColorDataKey = Shader.PropertyToID("_ColorData");
        public static readonly int s_ObjectDataKey = Shader.PropertyToID("_ObjectData");
        public static readonly int s_FontDataScales = Shader.PropertyToID("_FontScales");
        public static readonly int s_FontTextureSize = Shader.PropertyToID("_FontTextureSize");
        public static readonly int s_FontTexture = Shader.PropertyToID("_FontTexture");

        public UIForiaPropertyBlock(Material material, int size) {
            this.size = size;
            this.material = material;
            this.matBlock = new MaterialPropertyBlock();
            this.transformData = new Matrix4x4[size];
            this.colorData = new Vector4[size];
            this.objectData = new Vector4[size];
        }

        public void SetData(UIForiaData data) {
            
            Array.Copy(data.transformData.array, 0, transformData, 0, data.transformData.size);
            Array.Copy(data.colors.array, 0, colorData, 0, data.colors.size);
            Array.Copy(data.objectData0.array, 0, objectData, 0, data.objectData0.size);

            matBlock.SetMatrixArray(s_TransformDataKey, transformData);
            matBlock.SetVectorArray(s_ColorDataKey, colorData);
            matBlock.SetVectorArray(s_ObjectDataKey, objectData);
            
            if (data.fontData.fontAsset != null) {
                FontData fontData = data.fontData;
                matBlock.SetVector(s_FontDataScales, new Vector4(fontData.gradientScale, fontData.scaleRatioA, fontData.scaleRatioB, fontData.scaleRatioC));
                matBlock.SetVector(s_FontTextureSize, new Vector4(fontData.textureWidth, fontData.textureHeight, 0, 0));
                matBlock.SetTexture(s_FontTexture, fontData.fontAsset.atlas);
            }
            
        }

    }

    public class UIForiaMaterialPool {

        public Material small;
        public Material medium;
        public Material large;
        public Material huge;
        public Material massive;

        // todo -- get stats on how often each is used 
        private readonly UIForiaPropertyBlock smallBlock;
        private readonly UIForiaPropertyBlock mediumBlock;
        private readonly UIForiaPropertyBlock largeBlock;
        private readonly UIForiaPropertyBlock hugeBlock;
        private readonly UIForiaPropertyBlock massiveBlock;

        public UIForiaMaterialPool(Material material) {
            this.small = new Material(material);
            this.medium = new Material(material);
            this.large = new Material(material);
            this.huge = new Material(material);
            this.massive = new Material(material);

            this.small.EnableKeyword("BATCH_SIZE_SMALL");
            this.medium.EnableKeyword("BATCH_SIZE_MEDIUM");
            this.large.EnableKeyword("BATCH_SIZE_LARGE");
            this.huge.EnableKeyword("BATCH_SIZE_HUGE");
            this.massive.EnableKeyword("BATCH_SIZE_MASSIVE");

            this.smallBlock = new UIForiaPropertyBlock(small, RenderContext.k_ObjectCount_Small);
            this.mediumBlock = new UIForiaPropertyBlock(medium, RenderContext.k_ObjectCount_Medium);
            this.largeBlock = new UIForiaPropertyBlock(large, RenderContext.k_ObjectCount_Large);
            this.hugeBlock = new UIForiaPropertyBlock(huge, RenderContext.k_ObjectCount_Huge);
            this.massiveBlock = new UIForiaPropertyBlock(massive, RenderContext.k_ObjectCount_Massive);
        }

        public UIForiaPropertyBlock GetPropertyBlock(int objectCount) {
            if (objectCount <= RenderContext.k_ObjectCount_Small) {
                return smallBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Medium) {
                return mediumBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Large) {
                return largeBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Huge) {
                return hugeBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Massive) {
                return massiveBlock;
            }

            throw new Exception($"Batch size is too big. Tried to draw {objectCount} objects but batching supports at most {RenderContext.k_ObjectCount_Massive}");
        }

    }

    public struct FontData {

        public FontAsset fontAsset;
        public float gradientScale;
        public float scaleRatioA;
        public float scaleRatioB;
        public float scaleRatioC;
        public int textureWidth;
        public int textureHeight;

    }

    public class RenderContext {

        internal const int k_ObjectCount_Small = 8;
        internal const int k_ObjectCount_Medium = 16;
        internal const int k_ObjectCount_Large = 32;
        internal const int k_ObjectCount_Huge = 64;
        internal const int k_ObjectCount_Massive = 128;

        public StructList<Vector3> positionList;
        public StructList<Vector3> normalList;
        public StructList<Vector4> colorList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<Vector4> texCoordList2;
        public StructList<Vector4> texCoordList3;
        public StructList<int> triangleList;
        private StructList<Batch> pendingBatches;

        private Batch currentBatch;
        private Material activeMaterial;

        private readonly MeshPool uiforiaMeshPool;
        private readonly UIForiaMaterialPool uiforiaMaterialPool;
        private readonly StructStack<Rect> clipStack;

        internal RenderContext(Material batchedMaterial) {
            this.pendingBatches = new StructList<Batch>();
            this.uiforiaMeshPool = new MeshPool();
            this.uiforiaMaterialPool = new UIForiaMaterialPool(batchedMaterial);
            this.positionList = new StructList<Vector3>(128);
            this.texCoordList0 = new StructList<Vector4>(128);
            this.texCoordList1 = new StructList<Vector4>(128);
            this.triangleList = new StructList<int>(128 * 3);
            this.clipStack = new StructStack<Rect>();
        }

        public void SetMaterial(Material material) {
            if (currentBatch.batchType == BatchType.Custom) {
                if (activeMaterial != material) {
                    FinalizeCurrentBatch(false);
                }
            }

            activeMaterial = material;
        }

        public void DrawMesh(Mesh mesh, Material material, in Matrix4x4 transform) {
            FinalizeCurrentBatch(false);
        }

        public void DrawGeometry(Geometry geometry, Material material = null) {
            if (material == null) {
                material = activeMaterial;
            }
        }

        public void DrawBatchedText(UIForiaGeometry geometry, in GeometryRange range, in Matrix4x4 transform, in FontData fontData) {
            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch(false);
            }

            if (currentBatch.batchType == BatchType.Unset) {
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            // todo -- in the future see if we can use atlased font textures, only need to filter by channel 
            if (currentBatch.uiforiaData.fontData.fontAsset != null && currentBatch.uiforiaData.fontData.fontAsset != fontData.fontAsset) {
                FinalizeCurrentBatch(false);
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            currentBatch.uiforiaData.transformData.Add(transform);
            currentBatch.uiforiaData.objectData0.Add(geometry.objectData);
            currentBatch.uiforiaData.colors.Add(geometry.packedColors);
            currentBatch.uiforiaData.fontData = fontData;

            UpdateUIForiaGeometry(geometry, range);
            
        }

        public void DrawBatchedGeometry(UIForiaGeometry geometry, in GeometryRange range, in Matrix4x4 transform) {
            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch(false);
            }

            if (currentBatch.batchType == BatchType.Unset) {
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            currentBatch.uiforiaData.colors.Add(geometry.packedColors);
            currentBatch.uiforiaData.objectData0.Add(geometry.objectData);
            currentBatch.uiforiaData.transformData.Add(transform);

            UpdateUIForiaGeometry(geometry, range);
        }

        private void UpdateUIForiaGeometry(UIForiaGeometry geometry, in GeometryRange range) {
            int start = positionList.size;

            positionList.AddRange(geometry.positionList, range.vertexStart, range.vertexEnd);
            texCoordList0.AddRange(geometry.texCoordList0, range.vertexStart, range.vertexEnd);
            texCoordList1.AddRange(geometry.texCoordList1, range.vertexStart, range.vertexEnd);

            for (int i = range.vertexStart; i < range.vertexEnd; i++) {
                texCoordList1.array[start + i].w = currentBatch.drawCallSize;
            }

            currentBatch.drawCallSize++;

            triangleList.EnsureAdditionalCapacity(range.triangleEnd - range.triangleStart);

            int offset = triangleList.size;
            int[] triangles = triangleList.array;
            int[] geometryTriangles = geometry.triangleList.array;

            for (int i = range.triangleStart; i < range.triangleEnd; i++) {
                triangles[offset + i] = start + geometryTriangles[i];
            }

            triangleList.size += (range.triangleEnd - range.triangleStart);
        }

        private void FinalizeCurrentBatch(bool cloneMaterial = true) {
            // if have pending things to draw, create batch from them

            if (positionList.size == 0) {
                return;
            }

            if (currentBatch.batchType == BatchType.UIForia) {
                // select material based on batch size
                PooledMesh mesh = uiforiaMeshPool.Get(); // todo -- maybe worth trying to find a large mesh
                UIForiaPropertyBlock propertyBlock = uiforiaMaterialPool.GetPropertyBlock(currentBatch.drawCallSize);

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

                currentBatch.uiforiaPropertyBlock = propertyBlock;
                currentBatch.pooledMesh = mesh;
                pendingBatches.Add(currentBatch);

                currentBatch = new Batch();
            }
            else if (currentBatch.batchType == BatchType.Path) { }

            // todo -- only set for enabled channels
//            mesh.SetVertices(positionList.array, vertexCount);
//            mesh.SetNormals(normalList.array, vertexCount);
//            mesh.SetColors(colorList.array, vertexCount);
//            mesh.SetTextureCoord0(texCoordList0.array, vertexCount);
//            mesh.SetTextureCoord1(texCoordList1.array, vertexCount);
//            mesh.SetTextureCoord2(texCoordList2.array, vertexCount);
//            mesh.SetTextureCoord3(texCoordList3.array, vertexCount);
//            mesh.SetTriangles(triangleList.array, triangleCount);
//
//            positionList.size = 0;
//            normalList.size = 0;
//            colorList.size = 0;
//            texCoordList0.size = 0;
//            texCoordList1.size = 0;
//            texCoordList2.size = 0;
//            texCoordList3.size = 0;
//            triangleList.size = 0;
//
//
//            currentBatch.material = cloneMaterial ? activeMaterial.Clone() : activeMaterial;
//            currentBatch.mesh = mesh;
//
//            pendingBatches.Add(currentBatch);
//
//            materialsToRelease.Add(activeMaterial);
//
//            currentBatch = new Batch();
        }

        public void Render(Camera camera, CommandBuffer commandBuffer) {
            commandBuffer.Clear();

#if DEBUG
            commandBuffer.BeginSample("UIForia Render Main");
#endif
            FinalizeCurrentBatch(false);

            Vector3 cameraOrigin = camera.transform.position;
            cameraOrigin.x -= 0.25f * Screen.width;
            cameraOrigin.y += (0.25f * Screen.height) - 1;
            cameraOrigin.z += 2;

            Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);

            Matrix4x4 cameraMatrix = camera.cameraToWorldMatrix;

            commandBuffer.SetViewProjectionMatrices(cameraMatrix, camera.projectionMatrix);

            Batch[] batches = pendingBatches.array;

            // order reversed since we traverse in depth first order
            for (int i = pendingBatches.size - 1; i >= 0; i--) {
                ref Batch batch = ref batches[i];

                if (batch.batchType == BatchType.UIForia) {
                    UIForiaPropertyBlock uiForiaPropertyBlock = uiforiaMaterialPool.GetPropertyBlock(batch.drawCallSize);

                    uiForiaPropertyBlock.SetData(batch.uiforiaData);

                    commandBuffer.DrawMesh(batch.pooledMesh.mesh, origin, uiForiaPropertyBlock.material, 0, 0, uiForiaPropertyBlock.matBlock);
                }

                //Graphics.DrawMesh(pendingBatches[i].pooledMesh.mesh, origin, pendingBatches[i].material, 0, Camera.main, 0, propertyBlock, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            }
#if DEBUG

            commandBuffer.EndSample("UIForia Render Main");
#endif

            Graphics.ExecuteCommandBuffer(commandBuffer);
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
            currentBatch = new Batch();

            for (int i = 0; i < pendingBatches.size; i++) {
                //pendingBatches[i].pooledMesh.Release();
                // pendingBatches[i].uiforiaData?.Release();
            }

            pendingBatches.QuickClear();
        }

    }

}