using System;
using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Vertigo {

    public class GraphicsContext : Geometry {

        internal readonly StructList<RenderCall> renderCalls = new StructList<RenderCall>();
        internal readonly StructList<DrawCall> pendingBatches = new StructList<DrawCall>(16);
        internal readonly StructList<MaterialProperty> materialProperties = new StructList<MaterialProperty>(16);
        internal readonly StructList<Matrix4x4> transformList = new StructList<Matrix4x4>();

        internal readonly LightList<PooledMaterial> materialsToRelease = new LightList<PooledMaterial>();
        internal readonly LightList<RenderTexture> renderTexturesToRelease = new LightList<RenderTexture>(4);
        internal readonly LightStack<RenderTexture> renderTextures = new LightStack<RenderTexture>(4);
        internal readonly Dictionary<string, ShaderPool> shaderMap = new Dictionary<string, ShaderPool>();
        internal readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        internal readonly LightList<GraphicsInterface> graphicsInterfaces = new LightList<GraphicsInterface>();

        internal readonly PooledMesh.MeshPool meshPool = new PooledMesh.MeshPool();
        internal PooledMaterial activeMaterial;

        // todo -- use these for render targets to avoid requiring a camera, probably want a stack 
        internal Matrix4x4 viewMatrix;
        internal Matrix4x4 projectionMatrix;

        internal int lastRenderIndex = 0;
        internal bool renderStateChanged;
        internal DrawCall currentBatch;

#if DEBUG
        internal Action<Mesh, Matrix4x4, Material, int, int, MaterialPropertyBlock> onDebugCommandBuffer;
#endif

        public GraphicsContext() {
            transformList.Add(Matrix4x4.identity);
        }

        public void SetMaterial(Material material) {
            FinalizeCurrentBatch(false);

            string shaderName = material.shader.name;
            if (shaderMap.TryGetValue(shaderName, out ShaderPool pool)) {
                activeMaterial = pool.GetClonedInstance(material);
            }
            else {
                Shader shader = material.shader;
                pool = new ShaderPool(shader);
                shaderMap.Add(shaderName, pool);
                activeMaterial = pool.GetClonedInstance(material);
            }
        }

        public void EnableKeyword(string keyword) {
            FinalizeCurrentBatch();
            activeMaterial.material.EnableKeyword(keyword);
        }

        public void DisableKeyword(string keyword) {
            FinalizeCurrentBatch();
            activeMaterial.material.DisableKeyword(keyword);
        }

        public bool IsKeywordEnabled(string keyword) {
            return activeMaterial.material.IsKeywordEnabled(keyword);
        }

        public void SetTexture(int key, Texture value) {
            FinalizeCurrentBatch();
            materialProperties.Add(new MaterialProperty(key, value));
            currentBatch.propertyRange.length++;
        }

        public void SetFloatProperty(int key, float value) {
            FinalizeCurrentBatch();
            materialProperties.Add(new MaterialProperty(key, value));
            currentBatch.propertyRange.length++;
        }

        public void SetIntProperty(int key, int value) {
            FinalizeCurrentBatch();
            materialProperties.Add(new MaterialProperty(key, value));
            currentBatch.propertyRange.length++;
        }

        public void SetVectorProperty(int key, in Vector4 value) {
            FinalizeCurrentBatch();
            materialProperties.Add(new MaterialProperty(key, value));
            currentBatch.propertyRange.length++;
        }

        public void SetColorProperty(int key, in Color value) {
            FinalizeCurrentBatch();
            materialProperties.Add(new MaterialProperty(key, value));
            currentBatch.propertyRange.length++;
        }

        public void SetTransform(in Matrix4x4 transform) {
            transformList.Add(transform);
            if (vertexCount > 0) {
                // if currently drawing stuff, multiply vertices appropriately
                // could be optimized to do this only when another draw call hits
                int count = positionList.Count;
                Vector3[] positions = positionList.array;
                Matrix4x4 inverse = transform.inverse;
                // todo -- jobify 
                // todo -- defer until we actually need to do this
                // this is an inlined version of Matrix4x4.MultiplyPoint3x4(position)
                float m00 = inverse.m00;
                float m01 = inverse.m01;
                float m02 = inverse.m02;
                float m03 = inverse.m03;
                float m10 = inverse.m10;
                float m11 = inverse.m11;
                float m12 = inverse.m12;
                float m13 = inverse.m13;
                float m20 = inverse.m20;
                float m21 = inverse.m21;
                float m22 = inverse.m22;
                float m23 = inverse.m23;
                for (int i = 0; i < count; i++) {
                    Vector3 point = positions[i];
                    Vector3 vector3;
                    vector3.x = (m00 * point.x + m01 * point.y + m02 * point.z) + m03;
                    vector3.y = (m10 * point.x + m11 * point.y + m12 * point.z) + m13;
                    vector3.z = (m20 * point.x + m21 * point.y + m22 * point.z) + m23;
                    positions[i] = vector3;
                }
            }

            currentBatch.transformRange.length++;
        }

        public void SetStencilRef(int stencilRef) {
            FinalizeCurrentBatch();

            // copy shader if needed
            // render state push 
            // if active shader.HasProperty("_StencilRef")
            // finalize batch
            // material needs cloning = true
            // material needs render state update = true
        }

        public void SetStencilState(in StencilState stencilState) {
            // copy shader if needed
            // render state push 
        }

        public void SetBlendState(in RenderTargetBlendState blendState) {
            // copy shader if needed
            // render state push 
        }

        public void SetDepthState(in DepthState depthState) {
            // copy shader if needed
            // render state push 
        }

        public void PushRenderTexture(int width, int height, RenderTextureFormat format = RenderTextureFormat.Default) { }

        public void PopRenderTexture() { }

        // todo -- explore going directly to mesh data, might not need an intermediate copy since we can't manipulate vertices here anyway
        // culling is the only real reason to defer, i have other ways to do culling though (higher level, etc)
        public void Draw(Geometry geometry) {
            if (activeMaterial.material == null) {
                return;
            }

            if (vertexCount > ushort.MaxValue - 2000) {
                FinalizeCurrentBatch();    
            }
            
            int vertexOffset = vertexCount;
            int triangleStart = triangleCount;

            positionList.AddRange(geometry.positionList);
            normalList.AddRange(geometry.normalList);
            colorList.AddRange(geometry.colorList);
            texCoordList0.AddRange(geometry.texCoordList0);
            texCoordList1.AddRange(geometry.texCoordList1);
            texCoordList2.AddRange(geometry.texCoordList2);
            texCoordList3.AddRange(geometry.texCoordList3);

            int[] geometryTriangles = geometry.triangleList.array;
            int geometryTriangleCount = geometry.triangleList.size;
            triangleList.EnsureAdditionalCapacity(geometryTriangleCount);

            int[] triangles = triangleList.array;

            for (int i = 0; i < geometryTriangleCount; i++) {
                triangles[triangleStart + i] = geometryTriangles[i] + vertexOffset;
            }

            triangleList.size += geometryTriangleCount;

        }

        public void Draw(ShapeCache geometry, int shapeIndex) {
            if (activeMaterial.material == null) {
                return;
            }
            
//            if (vertexCount > 100) {//ushort.MaxValue - 2000) {
//                FinalizeCurrentBatch();    
//            }

            int vertexOffset = vertexCount;
            int triangleStart = triangleCount;

            GeometryShape shape = geometry.shapes[shapeIndex];
            int geometryVertexStart = shape.vertexStart;
            int geometryVertexCount = shape.vertexCount;

            positionList.AddRange(geometry.positionList, geometryVertexStart, geometryVertexCount);
            normalList.AddRange(geometry.normalList, geometryVertexStart, geometryVertexCount);
            colorList.AddRange(geometry.colorList, geometryVertexStart, geometryVertexCount);
            texCoordList0.AddRange(geometry.texCoordList0, geometryVertexStart, geometryVertexCount);
            texCoordList1.AddRange(geometry.texCoordList1, geometryVertexStart, geometryVertexCount);
            texCoordList2.AddRange(geometry.texCoordList2, geometryVertexStart, geometryVertexCount);
            texCoordList3.AddRange(geometry.texCoordList3, geometryVertexStart, geometryVertexCount);

            int[] geometryTriangles = geometry.triangleList.array;
            int geometryTriangleStart = shape.triangleStart;
            int geometryTriangleCount = shape.triangleCount;
            triangleList.EnsureAdditionalCapacity(geometryTriangleCount);

            int[] triangles = triangleList.array;

            for (int i = 0; i < geometryTriangleCount; i++) {
                triangles[triangleStart + i] = geometryTriangles[geometryTriangleStart + i] + vertexOffset;
            }

            triangleList.size += geometryTriangleCount;

        }

        public void Draw(ShapeCache geometry, RangeInt shapeRange) {
            if (activeMaterial.material == null) {
                return;
            }

            if (vertexCount > ushort.MaxValue - 2000) {
                FinalizeCurrentBatch();    
            }

            int vertexOffset = 0; // todo -- vertex count?
            int triangleStart = triangleCount;

            GeometryShape startShape = geometry.shapes[shapeRange.start];
            GeometryShape endShape = geometry.shapes[shapeRange.end - 1];
            int geometryVertexStart = startShape.vertexStart;
            int geometryVertexCount = endShape.vertexStart + endShape.vertexCount - startShape.vertexStart;

            positionList.AddRange(geometry.positionList, geometryVertexStart, geometryVertexCount);
            normalList.AddRange(geometry.normalList, geometryVertexStart, geometryVertexCount);
            colorList.AddRange(geometry.colorList, geometryVertexStart, geometryVertexCount);
            texCoordList0.AddRange(geometry.texCoordList0, geometryVertexStart, geometryVertexCount);
            texCoordList1.AddRange(geometry.texCoordList1, geometryVertexStart, geometryVertexCount);
            texCoordList2.AddRange(geometry.texCoordList2, geometryVertexStart, geometryVertexCount);
            texCoordList3.AddRange(geometry.texCoordList3, geometryVertexStart, geometryVertexCount);

            int[] geometryTriangles = geometry.triangleList.array;
            int geometryTriangleStart = startShape.triangleStart;
            int geometryTriangleCount = endShape.triangleStart + endShape.triangleCount - startShape.triangleStart;
            triangleList.EnsureAdditionalCapacity(geometryTriangleCount);

            int[] triangles = triangleList.array;

            int start = shapeRange.start;
            int end = shapeRange.end;
            int triIdx = triangleStart;
            for (int i = start; i < end; i++) {
                GeometryShape shape = geometry.shapes[i];
                for (int j = 0; j < shape.triangleCount; j++) {
                    triangles[triIdx++] = geometryTriangles[geometryTriangleStart++] + vertexOffset;
                }

                vertexOffset += shape.vertexCount;
            }

            triangleList.size += geometryTriangleCount;
            
        }

        public void Draw(ShapeCache geometry, IList<int> shapeIndices) {
            if (shapeIndices == null || activeMaterial.material == null) {
                return;
            }

            for (int i = 0; i < shapeIndices.Count; i++) {
                Draw(geometry, shapeIndices[i]);
            }
        }

        public void Draw(Mesh mesh) {
            if (activeMaterial.material == null) {
                return;
            }

            if (mesh.vertexCount < 100 && mesh.isReadable) {
                // push to geometry 
            }
            else {
                // create own draw call & break batch
            }
        }

        public void Draw(Sprite sprite) {
            if (activeMaterial.material == null) {
                return;
            }
        }
        
        public void Render() {
            RenderTexture targetTexture = null;

            if (renderTextures.Count > 0) {
                targetTexture = renderTextures.Peek();
            }
            else {
                targetTexture = RenderTexture.active;
            }

            FinalizeCurrentBatch();

            renderCalls.Add(new RenderCall() {
                renderTexture = targetTexture,
                drawMeshCallStart = lastRenderIndex,
                drawMeshCallEnd = pendingBatches.size
            });

            lastRenderIndex = pendingBatches.size;
        }

        public void BeginFrame() {
            lastRenderIndex = 0;
            transformList.QuickClear();
            transformList.Add(Matrix4x4.identity);
            colorList.size = 0;
            triangleList.size = 0;
            normalList.size = 0;
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            texCoordList2.size = 0;
            texCoordList3.size = 0;

            DrawCall[] drawMeshCalls = pendingBatches.array;
            for (int i = 0; i < pendingBatches.size; i++) {
                drawMeshCalls[i].mesh.Release();
                drawMeshCalls[i].material.Release();
                drawMeshCalls[i].mesh = default;
                drawMeshCalls[i].material = default;
            }

            for (int i = 0; i < renderTexturesToRelease.Count; i++) {
                renderTexturesToRelease[i].Release();
            }

            for (int i = 0; i < materialsToRelease.Count; i++) {
                materialsToRelease[i].Release();
            }

            materialsToRelease.QuickClear();
            renderStateChanged = false;
            currentBatch = default;
            renderTexturesToRelease.QuickClear();
            materialProperties.QuickClear();
            renderCalls.QuickClear();
            pendingBatches.QuickClear();
            for (int i = 0; i < graphicsInterfaces.Count; i++) {
                graphicsInterfaces[i].BeginFrame();
            }
        }

        public void SetViewMatrix(in Matrix4x4 viewMatrix) {
            this.viewMatrix = viewMatrix;
        }

        public void SetProjectionMatrix(in Matrix4x4 projectionMatrix) {
            this.projectionMatrix = projectionMatrix;
        }

        public void EndFrame(CommandBuffer commandBuffer, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) {
            commandBuffer.Clear();

            if (vertexCount > 0) {
                Render();
            }

            for (int i = 0; i < renderCalls.size; i++) {
                IssueDrawCommands(viewMatrix, projectionMatrix, commandBuffer, renderCalls[i]);
            }

            for (int i = 0; i < graphicsInterfaces.Count; i++) {
                graphicsInterfaces[i].EndFrame();
            }

        }

        private void IssueDrawCommands(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, CommandBuffer commandBuffer, in RenderCall renderCall) {
            RenderTexture renderTexture = renderCall.renderTexture ? renderCall.renderTexture : RenderTexture.active;
            commandBuffer.SetRenderTarget(renderTexture);

            if (!ReferenceEquals(renderTexture, null)) { // use render texture
                int width = renderTexture.width;
                int height = renderTexture.height;
                commandBuffer.ClearRenderTarget(true, true, Color.clear);
                Matrix4x4 projection = Matrix4x4.Ortho(-width, width, -height, height, 0.3f, 999999);
                commandBuffer.SetViewProjectionMatrices(viewMatrix, projection);
            }
            else { // use screen
                commandBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            }

            int start = renderCall.drawMeshCallStart;
            int end = renderCall.drawMeshCallEnd;

            DrawCall[] drawMeshCalls = pendingBatches.array;

            for (int i = start; i < end; i++) {
                Mesh mesh = drawMeshCalls[i].mesh.mesh;
                Material material = drawMeshCalls[i].material.material;

                for (int k = drawMeshCalls[i].propertyRange.start; k < drawMeshCalls[i].propertyRange.end; k++) {
                    switch (materialProperties[k].type) {
                        case MaterialPropertyType.Int:
                            propertyBlock.SetInt(materialProperties[k].key, materialProperties[k].intVal);
                            break;
                        case MaterialPropertyType.Float:
                            propertyBlock.SetFloat(materialProperties[k].key, materialProperties[k].floatVal);
                            break;
                        case MaterialPropertyType.Texture:
                            propertyBlock.SetTexture(materialProperties[k].key, materialProperties[k].textureValue);
                            break;
                        case MaterialPropertyType.Color:
                            propertyBlock.SetColor(materialProperties[k].key, materialProperties[k].colorValue);
                            break;
                        case MaterialPropertyType.Vector:
                            propertyBlock.SetVector(materialProperties[k].key, materialProperties[k].vectorValue);
                            break;
                    }
                }
#if DEBUG
                onDebugCommandBuffer?.Invoke(mesh, transformList[drawMeshCalls[i].transformRange.end], material, 0, 0, propertyBlock);
#endif
                commandBuffer.DrawMesh(mesh, transformList[drawMeshCalls[i].transformRange.end], material, 0, 0, propertyBlock);
                propertyBlock.Clear();
            }
        }


        private void FinalizeCurrentBatch(bool cloneMaterial = true) {
            // if have pending things to draw, create batch from them

            if (vertexCount == 0) {
                return;
            }

            PooledMesh mesh = meshPool.GetDynamic();

            mesh.SetVertices(positionList.array, vertexCount);
            mesh.SetNormals(normalList.array, vertexCount);
            mesh.SetColors(colorList.array, vertexCount);
            mesh.SetTextureCoord0(texCoordList0.array, vertexCount);
            mesh.SetTextureCoord1(texCoordList1.array, vertexCount);
            mesh.SetTextureCoord2(texCoordList2.array, vertexCount);
            mesh.SetTextureCoord3(texCoordList3.array, vertexCount);
            mesh.SetTriangles(triangleList.array, triangleCount);

            positionList.size = 0;
            normalList.size = 0;
            colorList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            texCoordList2.size = 0;
            texCoordList3.size = 0;
            triangleList.size = 0;

            currentBatch.material = cloneMaterial ? activeMaterial.Clone() : activeMaterial;
            currentBatch.mesh = mesh;

            pendingBatches.Add(currentBatch);

            materialsToRelease.Add(activeMaterial);

            currentBatch = new DrawCall() {
                propertyRange = new RangeInt(currentBatch.propertyRange.end, 0),
                transformRange = new RangeInt(currentBatch.transformRange.end, 0)
            };
        }

        public T GetRenderInterface<T>() where T : GraphicsInterface {
            for (int i = 0; i < graphicsInterfaces.Count; i++) {
                if (graphicsInterfaces[i] is T) {
                    return (T) graphicsInterfaces[i];
                }
            }

            T retn = (T) Activator.CreateInstance(typeof(T), this);
            graphicsInterfaces.Add(retn);
            retn.BeginFrame();
            return retn;
        }

        internal struct RenderCall {

            public RenderTexture renderTexture;
            public int drawMeshCallStart;
            public int drawMeshCallEnd;

        }

        internal struct DrawCall {

            public PooledMesh mesh;
            public PooledMaterial material;
            public RangeInt propertyRange;
            public RangeInt transformRange;

        }

    }


}