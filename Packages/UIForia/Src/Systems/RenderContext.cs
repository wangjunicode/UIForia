using System;
using System.Collections.Generic;
using UIForia.Extensions;
using UIForia.Rendering.Vertigo;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Vertigo;
using PooledMesh = UIForia.Rendering.Vertigo.PooledMesh;

namespace UIForia.Rendering {

    public class UIForiaMaterialPool {

        public Material small;
        public Material medium;
        public Material large;
        public Material huge;
        public Material massive;

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
        }

        public Material Get(int objectCount) {
            if (objectCount <= RenderContext.k_ObjectCount_Small) {
                return small;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Medium) {
                return medium;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Large) {
                return large;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Huge) {
                return huge;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Massive) {
                return massive;
            }

            throw new Exception($"Batch size is too big. Tried to draw {objectCount} objects but batching supports at most {RenderContext.k_ObjectCount_Massive}");
        }

    }

    public class RenderContext {

        private int vertexCount;
        private int triangleCount;

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

        public void DrawBatchedText(UIForiaGeometry geometry) {
            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch(false);
            }
        }

        public void DrawBatchedGeometry(UIForiaGeometry geometry) {
            if (currentBatch.batchType == BatchType.Custom) {
                FinalizeCurrentBatch(false);
            }
            FinalizeCurrentBatch(false);

            if (currentBatch.batchType == BatchType.Unset) {
                currentBatch.batchType = BatchType.UIForia;
                currentBatch.uiforiaData = new UIForiaData(); // todo -- pool
            }

            // textureAsset = texture + uvs, but uvs should be set already via renderbox
//            if (geometry.backgroundTexture != null) {
//                
//            }

            // mask texture
            // background texture
            // font texture
            // other?

            // if texture is in an atlas, get atlas instead (check via object id)

            // if any changed, break batch
            // if font & sdf font & font texture set contains font, a-ok

            currentBatch.uiforiaData.colors.Add(geometry.packedColors);
            currentBatch.uiforiaData.objectData0.Add(new Vector4());

            currentBatch.drawCallSize++;

            int start = vertexCount;

            positionList.AddRange(geometry.positionList);
            texCoordList0.AddRange(geometry.texCoordList0);
            texCoordList1.AddRange(geometry.texCoordList1);

            for (int i = 0; i < geometry.texCoordList1.size; i++) {
                texCoordList1.array[start + i].w = currentBatch.drawCallSize;
            }

            triangleList.EnsureAdditionalCapacity(geometry.triangleList.size);

            int offset = triangleList.size;
            int[] triangles = triangleList.array;

            int geometryTriangleCount = geometry.triangleList.size;
            int[] geometryTriangles = geometry.triangleList.array;

            for (int i = 0; i < geometryTriangleCount; i++) {
                triangles[offset + i] = geometryTriangles[i];
            }

            vertexCount = positionList.size;
            triangleCount += geometryTriangleCount;
        }

        private void FinalizeCurrentBatch(bool cloneMaterial = true) {
            // if have pending things to draw, create batch from them

            if (vertexCount == 0) {
                return;
            }

            if (currentBatch.batchType == BatchType.UIForia) {
                // select material based on batch size
                PooledMesh mesh = uiforiaMeshPool.Get();
                Material material = uiforiaMaterialPool.Get(currentBatch.drawCallSize);

                mesh.SetVertices(positionList.array, vertexCount);
                mesh.SetTextureCoord0(texCoordList0.array, vertexCount);
                mesh.SetTextureCoord1(texCoordList1.array, vertexCount);
                mesh.SetTriangles(triangleList.array, triangleCount);

                positionList.size = 0;
                texCoordList0.size = 0;
                texCoordList1.size = 0;
                triangleList.size = 0;

                currentBatch.material = material;
                currentBatch.pooledMesh = mesh;
                pendingBatches.Add(currentBatch);

                currentBatch = new Batch();

                vertexCount = 0;
                triangleCount = 0;
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

        private MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        public void Render(Camera camera, CommandBuffer commandBuffer) {
            commandBuffer.Clear();
            FinalizeCurrentBatch(false);

            Vector3 cameraOrigin = camera.transform.position;
          //  cameraOrigin.x -= 0.5f * Screen.width;
         //   cameraOrigin.y += (0.5f * Screen.height) - 1;
            cameraOrigin.z += 2;

            Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);

            Matrix4x4 cameraMatrix = camera.cameraToWorldMatrix;

            commandBuffer.SetViewProjectionMatrices(cameraMatrix, camera.projectionMatrix);

            for (int i =  pendingBatches.size -1 ; i >= 0; i--) {
                
                if (pendingBatches[i].batchType == BatchType.UIForia) {
                    propertyBlock.SetVectorArray("_ColorData", pendingBatches[i].uiforiaData.colors);
                }

                //  commandBuffer.DrawMesh(pendingBatches[i].pooledMesh.mesh, origin, pendingBatches[i].material, 0, 0, propertyBlock);
                Graphics.DrawMesh(pendingBatches[i].pooledMesh.mesh, origin, pendingBatches[i].material, 0, Camera.main, 0, propertyBlock, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            }

            // todo -- I think draw order is wrong still
            // todo -- for some reason when batched together unity drops all but 4 vertices, occulsion maybe?
            // Graphics.ExecuteCommandBuffer(commandBuffer);
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