using System.Collections.Generic;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertigo {

    public class UIForiaRenderContext {

        private UIForiaMaterial activeShader;
        private readonly ushort contextId;
        private readonly ShapeDataCache geometryGenerator;
        private readonly LightList<UIForiaMaterial> materialsToRelease;
        private readonly StructList<PendingBatch> pendingBatches;

        private Matrix4x4 transform;

        private StructList<ShapeMeshData> pendingDrawCalls;
        private bool transformChanged;
        private readonly StructList<Matrix4x4> transformList;

        private readonly Stack<RenderTexture> renderTextures;
        private readonly StructList<RenderCall> renderCalls;
        private readonly LightList<RenderTexture> renderTexturesToRelease;
        private readonly VertigoMesh.MeshPool meshPool;
        private readonly Dictionary<string, ShaderPool> shaderMap;

        private static ushort s_ContextIdGen;

        private RenderState renderState;

        public UIForiaRenderContext(Material defaultMaterial = null) {
            this.contextId = s_ContextIdGen++;
            this.shaderMap = new Dictionary<string, ShaderPool>();
            this.geometryGenerator = new ShapeDataCache();
            this.materialsToRelease = new LightList<UIForiaMaterial>();
            this.renderTextures = new Stack<RenderTexture>();
            this.renderTexturesToRelease = new LightList<RenderTexture>();
            this.transformList = new StructList<Matrix4x4>();
            this.meshPool = new VertigoMesh.MeshPool();

            this.renderCalls = new StructList<RenderCall>(); // list of draw calls + a texture to draw to
            this.pendingDrawCalls = new StructList<ShapeMeshData>(); // shapes we might draw 
            this.pendingBatches = new StructList<PendingBatch>(); // 
            renderState.texCoordChannel = TextureCoordChannel.TextureCoord0;
            ResetUVState();

            if (defaultMaterial == null) {
                defaultMaterial = new Material(Shader.Find("Vertigo/VertigoSDF"));
            }

            this.activeShader = CloneMaterial(defaultMaterial);

            transformList.Add(Matrix4x4.identity);
        }

        private UIForiaMaterial CloneMaterial(Material material) {
            string shaderName = material.shader.name;
            if (shaderMap.TryGetValue(shaderName, out ShaderPool pool)) {
                return pool.GetClonedInstance(material);
            }
            else {
                Shader shader = material.shader;
                pool = new ShaderPool(shader);
                shaderMap.Add(shaderName, pool);
                return pool.GetClonedInstance(material);
            }
        }

        private bool TryGetDefaultMaterial(string shaderName, out UIForiaMaterial retn) {
            if (shaderMap.TryGetValue(shaderName, out ShaderPool pool)) {
                retn = pool.GetDefaultInstance();
                return true;
            }
            else {
                Shader shader = Shader.Find(shaderName);
                if (ReferenceEquals(shader, null)) {
                    retn = default;
                    return false;
                }

                pool = new ShaderPool(shader);
                shaderMap.Add(shaderName, pool);
                retn = pool.GetDefaultInstance();
                return true;
            }
        }

        public void Clear() {
            transformList.QuickClear();
            transformChanged = false;
            transform = Matrix4x4.identity;
            transformList.Add(transform);

            for (int i = 0; i < materialsToRelease.Count; i++) {
                materialsToRelease[i].Release();
            }

            for (int i = 0; i < renderTexturesToRelease.Count; i++) {
                RenderTexture.ReleaseTemporary(renderTexturesToRelease[i]);
            }

            for (int i = 0; i < renderCalls.Count; i++) {
                renderCalls[i].Release();
            }

            PendingBatch[] pendingBatchArray = pendingBatches.array;
            for (int i = 0; i < pendingBatches.size; i++) {
                StructList<ShapeMeshData>.Release(ref pendingBatchArray[i].renderedShapes);
            }
            
            geometryGenerator.Clear();
            materialsToRelease.QuickClear();
            pendingBatches.QuickClear();
            pendingDrawCalls.QuickClear();
            renderCalls.QuickClear();
            renderTextures.Clear();
            renderTexturesToRelease.Clear();
        }

        public void SetMaterial(Material material) {
            if (!ReferenceEquals(material, null)) {
                FinalizeCurrentBatch();
                materialsToRelease.Add(activeShader);
                activeShader = CloneMaterial(material);
            }
        }

        public void SetShader(string shaderName) {
            if (TryGetDefaultMaterial(shaderName, out UIForiaMaterial nextMaterial)) {
                FinalizeCurrentBatch();
                materialsToRelease.Add(activeShader);
                activeShader = nextMaterial;
            }
        }

        public void SetFillColor(Color color) {
            renderState.fillColor = color;
        }

        public void SetTexCoordChannel(TextureCoordChannel channel) {
            renderState.texCoordChannel = channel;
        }

        public void SetMainTexture(Texture texture) {
            FinalizeCurrentBatch();
            activeShader.material.SetTexture(ShaderKey.MainTexture, texture);
        }

        public void SetFloatProperty(int key, float value) {
            FinalizeCurrentBatch();
            activeShader.material.SetFloat(key, value);
        }

        private void FinalizeCurrentBatch() {
            if (pendingDrawCalls.size > 0) {
                pendingBatches.Add(new PendingBatch() {
                    shader = activeShader,
                    renderedShapes = pendingDrawCalls
                });
                materialsToRelease.Add(activeShader);
                activeShader = activeShader.Clone();
                pendingDrawCalls = StructList<ShapeMeshData>.Get();
            }
        }

        public void Draw(GeometryCache geometryCache, int shapeIndex) {
            // geometryGenerator.AddGeometry(geometryCache, shapeIndex);
        }

        public ShapeId FillRect(float x, float y, float width, float height) {
            GeometryShape shape = geometryGenerator.FillRect(x, y, width, height, renderState);

            if (transformChanged) {
                transformList.Add(transform);
                transformChanged = false;
            }

            pendingDrawCalls.Add(new ShapeMeshData(shape, transformList.size - 1));

            return new ShapeId(contextId, (ushort) (geometryGenerator.shapes.size - 1));
        }

        public ShapeId DrawText(float x, float y, TextInfo textInfo, int textureKey = -1) {
//            GeometryShape shape = (x, y, width, height, renderState);

            // for now just support a single span, eventually we need to re-work text
            GeometryShape shape = geometryGenerator.Text(x, y, textInfo, renderState);
            pendingDrawCalls.Add(new ShapeMeshData(shape, transformList.size - 1));

            if (transformChanged) {
                transformList.Add(transform);
                transformChanged = false;
            }

            return new ShapeId(contextId, (ushort) (geometryGenerator.shapes.size - 1));
        }


        public RenderTexture Render() {
            RenderTexture targetTexture = RenderTexture.active;

            if (renderTextures.Count > 0) {
                targetTexture = renderTextures.Peek();
            }

            int width = Screen.width;
            int height = Screen.height;

            if (!ReferenceEquals(targetTexture, null)) {
                width = targetTexture.width;
                height = targetTexture.height;
            }

            Matrix4x4 cameraMatrix = Matrix4x4.identity;

            StructList<DrawMeshCall> drawMeshCalls = StructList<DrawMeshCall>.Get();
//            Matrix4x4 rootMat = Matrix4x4.TRS(new Vector3(-(width / 2), height / 2), Quaternion.identity, Vector3.one);

            if (pendingDrawCalls.size > 0) {
                pendingBatches.Add(new PendingBatch() {
                    shader = activeShader,
                    renderedShapes = pendingDrawCalls
                });
                pendingDrawCalls = StructList<ShapeMeshData>.Get();
            }

            PendingBatch[] batches = pendingBatches.array;
            int count = pendingBatches.size;

            for (int i = 0; i < count; i++) {
                PendingBatch batch = batches[i];
                if (batch.renderedShapes.size == 1) {
                    VertigoMesh mesh = BakeMesh(batch.renderedShapes, false);
                    if (mesh != null) {
                        int transformId = batch.renderedShapes[i].transformId;
                        drawMeshCalls.Add(new DrawMeshCall(mesh, batch.shader, transformList.array[transformId]));
                    }
                }
                else {
                    VertigoMesh mesh = BakeMesh(batch.renderedShapes, true);
                    if (mesh != null) {
                        int transformId = batch.renderedShapes[i].transformId;
                        drawMeshCalls.Add(new DrawMeshCall(mesh, batch.shader, transformList.array[transformId]));
                    }
                }
            }

            renderCalls.Add(new RenderCall() {
                texture = targetTexture,
                drawCalls = drawMeshCalls
            });

            return targetTexture;
        }

        private static readonly StructList<Vector3> s_PositionList = new StructList<Vector3>(64);
        private static readonly StructList<Vector3> s_NormalList = new StructList<Vector3>(64);
        private static readonly StructList<Color> s_ColorList = new StructList<Color>(64);
        private static readonly StructList<Vector4> s_TexCoordList0 = new StructList<Vector4>(64);
        private static readonly StructList<Vector4> s_TexCoordList1 = new StructList<Vector4>(64);
        private static readonly StructList<Vector4> s_TexCoordList2 = new StructList<Vector4>(64);
        private static readonly StructList<Vector4> s_TexCoordList3 = new StructList<Vector4>(64);
        private static readonly StructList<int> s_TriangleList = new StructList<int>(64);

        private static readonly List<Vector3> s_ScratchVector3 = new List<Vector3>(0);
        private static readonly List<Vector4> s_ScratchVector4 = new List<Vector4>(0);
        private static readonly List<Color> s_ScratchColor = new List<Color>(0);
        private static readonly List<int> s_ScratchInt = new List<int>(0);

        private VertigoMesh BakeMesh(StructList<ShapeMeshData> shapeList, bool transformVertices) {
            // todo -- cull check
            // todo -- if shapes aren't using certain channels don't send them to the mesh

            VertigoMesh mesh = meshPool.GetDynamic();

            if (transformVertices) {
                for (int i = 0; i < shapeList.size; i++) {
                    ShapeMeshData meshData = shapeList[i];
                    int vertexStart = meshData.shape.vertexStart;
                    int vertexCount = meshData.shape.vertexCount;
                    int triangleStart = meshData.shape.triangleStart;
                    int triangleCount = meshData.shape.triangleCount;
                    int idxStart = s_TriangleList.size;

                    int start = s_PositionList.size;
                    s_PositionList.AddRange(geometryGenerator.positionList, vertexStart, vertexCount);
                    Vector3[] positions = s_PositionList.array;
                    Matrix4x4 matrix = transformList[meshData.transformId];

                    for (int j = start; j < s_PositionList.size; j++) {
                        positions[j] = matrix.MultiplyVector(positions[j]);
                    }

                    s_NormalList.AddRange(geometryGenerator.normalList, vertexStart, vertexCount);
                    s_ColorList.AddRange(geometryGenerator.colorList, vertexStart, vertexCount);
                    s_TexCoordList0.AddRange(geometryGenerator.texCoordList0, vertexStart, vertexCount);
                    s_TexCoordList1.AddRange(geometryGenerator.texCoordList1, vertexStart, vertexCount);
                    s_TexCoordList2.AddRange(geometryGenerator.texCoordList2, vertexStart, vertexCount);
                    s_TexCoordList3.AddRange(geometryGenerator.texCoordList3, vertexStart, vertexCount);
                    s_TriangleList.AddRange(geometryGenerator.triangleList, triangleStart, triangleCount);

                    int[] tris = s_TriangleList.array;
                    for (int j = idxStart; j < s_TriangleList.size; j++) {
                        tris[j] -= vertexStart;
                    }
                    
                }
            }

            else {
                for (int i = 0; i < shapeList.size; i++) {
                    ShapeMeshData meshData = shapeList[i];
                    int vertexStart = meshData.shape.vertexStart;
                    int vertexCount = meshData.shape.vertexCount;
                    int triangleStart = meshData.shape.triangleStart;
                    int triangleCount = meshData.shape.triangleCount;

                    int idxStart = s_TriangleList.size;
                    s_PositionList.AddRange(geometryGenerator.positionList, vertexStart, vertexCount);
                    s_NormalList.AddRange(geometryGenerator.normalList, vertexStart, vertexCount);
                    s_ColorList.AddRange(geometryGenerator.colorList, vertexStart, vertexCount);
                    s_TexCoordList0.AddRange(geometryGenerator.texCoordList0, vertexStart, vertexCount);
                    s_TexCoordList1.AddRange(geometryGenerator.texCoordList1, vertexStart, vertexCount);
                    s_TexCoordList2.AddRange(geometryGenerator.texCoordList2, vertexStart, vertexCount);
                    s_TexCoordList3.AddRange(geometryGenerator.texCoordList3, vertexStart, vertexCount);
                    s_TriangleList.AddRange(geometryGenerator.triangleList, triangleStart, triangleCount);
                    
                    int[] tris = s_TriangleList.array;
                    for (int j = idxStart; j < s_TriangleList.size; j++) {
                        tris[j] -= vertexStart;
                    }
                }
            }

            ListAccessor<Vector3>.SetArray(s_ScratchVector3, s_PositionList.array, s_PositionList.size);
            mesh.mesh.SetVertices(s_ScratchVector3);

            ListAccessor<Vector3>.SetArray(s_ScratchVector3, s_NormalList.array, s_NormalList.size);
            mesh.mesh.SetNormals(s_ScratchVector3);

            ListAccessor<Color>.SetArray(s_ScratchColor, s_ColorList.array, s_ColorList.size);
            mesh.mesh.SetColors(s_ScratchColor);

            ListAccessor<Vector4>.SetArray(s_ScratchVector4, s_TexCoordList0.array, s_TexCoordList0.size);
            mesh.mesh.SetUVs(0, s_ScratchVector4);

            ListAccessor<Vector4>.SetArray(s_ScratchVector4, s_TexCoordList1.array, s_TexCoordList1.size);
            mesh.mesh.SetUVs(1, s_ScratchVector4);

            ListAccessor<Vector4>.SetArray(s_ScratchVector4, s_TexCoordList2.array, s_TexCoordList2.size);
            mesh.mesh.SetUVs(2, s_ScratchVector4);

            ListAccessor<Vector4>.SetArray(s_ScratchVector4, s_TexCoordList3.array, s_TexCoordList3.size);
            mesh.mesh.SetUVs(3, s_ScratchVector4);

            ListAccessor<int>.SetArray(s_ScratchInt, s_TriangleList.array, s_TriangleList.size);
            mesh.mesh.SetTriangles(s_ScratchInt, 0);

            s_PositionList.size = 0;
            s_ColorList.size = 0;
            s_NormalList.size = 0;
            s_TexCoordList0.size = 0;
            s_TexCoordList1.size = 0;
            s_TexCoordList2.size = 0;
            s_TexCoordList3.size = 0;
            s_TriangleList.size = 0;

            return mesh;
        }

        public void PushRenderTexture(int width, int height, RenderTextureFormat format = RenderTextureFormat.Default) {
            const int DepthBufferBits = 24;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, DepthBufferBits, format);
            renderTextures.Push(renderTexture);
            renderTexturesToRelease.Add(renderTexture);
        }

        public void PopRenderTexture() {
            if (renderTextures.Count != 0) {
                renderTextures.Pop();
            }
        }

        public void Flush(Camera camera, CommandBuffer commandBuffer) {
            commandBuffer.Clear();

            Matrix4x4 cameraMatrix = camera.worldToCameraMatrix;

            for (int i = 0; i < renderCalls.Count; i++) {
                RenderTexture renderTexture = renderCalls[i].texture ? renderCalls[i].texture : RenderTexture.active;
                commandBuffer.SetRenderTarget(renderTexture);

                if (!ReferenceEquals(renderTexture, null)) { // use render texture
                    int width = renderTexture.width;
                    int height = renderTexture.height;
                    commandBuffer.ClearRenderTarget(true, true, Color.red);
                    Matrix4x4 projection = Matrix4x4.Ortho(-width, width, -height, height, 0.3f, 999999);
                    commandBuffer.SetViewProjectionMatrices(cameraMatrix, projection);
                }
                else { // use screen
                    commandBuffer.SetViewProjectionMatrices(cameraMatrix, camera.projectionMatrix);
                }

//                Render(renderCalls[i].drawCalls);
                int count = renderCalls[i].drawCalls.size;
                DrawMeshCall[] meshCalls = renderCalls[i].drawCalls.array;

                for (int j = 0; j < count; j++) {
                    Mesh mesh = meshCalls[j].mesh.mesh;
                    Material material = meshCalls[j].material.material;
                    // UpdateMaterialPropertyBlock(calls[j].state);
                    int passCount = material.passCount;
                    // todo -- only render specified passes
                    commandBuffer.DrawMesh(mesh, meshCalls[j].transform, material, 0, 0, null);
                    for (int k = 0; k < passCount; k++) { }
                }
            }
        }

        // final draw call translates 1-1 with commandBuffer.DrawMesh
        internal struct DrawMeshCall {

            public VertigoMesh mesh;
            public UIForiaMaterial material;
            public Matrix4x4 transform;

            public DrawMeshCall(VertigoMesh mesh, UIForiaMaterial material, in Matrix4x4 transform) {
                this.mesh = mesh;
                this.material = material;
                this.transform = transform;
            }

        }

        private struct PendingBatch {

            public UIForiaMaterial shader;
            public StructList<ShapeMeshData> renderedShapes;

        }

        private struct RenderCall {

            public RenderTexture texture;
            public StructList<DrawMeshCall> drawCalls;

            public RenderCall(RenderTexture texture, StructList<DrawMeshCall> drawCalls) {
                this.texture = texture;
                this.drawCalls = drawCalls;
            }

            public void Release() {
                for (int i = 0; i < drawCalls.size; i++) {
                    drawCalls[i].mesh.Release();
                }

                StructList<DrawMeshCall>.Release(ref drawCalls);
            }

        }

        private struct ShapeMeshData {

            public readonly int transformId;
            public readonly GeometryShape shape;

            public ShapeMeshData(in GeometryShape shape, int transformId) {
                this.shape = shape;
                this.transformId = transformId;
            }

        }

        public void SetUVTiling(float x, float y) {
            renderState.uvTiling.x = x;
            renderState.uvTiling.y = y;
        }

        public void SetUVOffset(float x, float y) {
            renderState.uvOffset.x = x;
            renderState.uvOffset.y = y;
        }

        public void SetUVRotation(float rotation) {
            renderState.uvRotation = rotation;
        }

        public void SetUVPivot(float x, float y) {
            renderState.uvPivot.x = x;
            renderState.uvPivot.y = y;
        }

        public void SetUVRect(float x, float y, float width, float height) {
            renderState.uvRect.x = x;
            renderState.uvRect.y = y;
            renderState.uvRect.width = width;
            renderState.uvRect.height = height;
        }

        public void ResetUVState() {
            renderState.uvOffset.x = 0;
            renderState.uvOffset.y = 0;
            renderState.uvRotation = 0;
            renderState.uvPivot.x = 0.5f;
            renderState.uvPivot.y = 0.5f;
            renderState.uvTiling.x = 1;
            renderState.uvTiling.y = 1;
            renderState.uvRect.x = 0;
            renderState.uvRect.y = 0;
            renderState.uvRect.width = 1;
            renderState.uvRect.height = 1;
        }

        public void SetTexCoord1(ShapeId id, Vector4 uv) {
            if (id.originId != contextId) return;
            if (id.index >= geometryGenerator.shapeCount) {
                return;
            }

            GeometryShape shape = geometryGenerator.shapes[id.index];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Vector4[] texCoord1 = geometryGenerator.texCoordList1.array;
            for (int i = start; i < end; i++) {
                texCoord1[i] = uv;
            }
        }

        public void SetTexCoord2(ShapeId id, Vector4 uv) {
            if (id.originId != contextId) return;
            if (id.index >= geometryGenerator.shapeCount) {
                return;
            }

            GeometryShape shape = geometryGenerator.shapes[id.index];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Vector4[] texCoord2 = geometryGenerator.texCoordList2.array;
            for (int i = start; i < end; i++) {
                texCoord2[i] = uv;
            }
        }

        public void SetTexCoord3(ShapeId id, Vector4 uv) {
            if (id.originId != contextId) return;
            if (id.index >= geometryGenerator.shapeCount) {
                return;
            }

            GeometryShape shape = geometryGenerator.shapes[id.index];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Vector4[] texCoord3 = geometryGenerator.texCoordList3.array;
            for (int i = start; i < end; i++) {
                texCoord3[i] = uv;
            }
        }

        public void SetColor(ShapeId id, Color color) {
            if (id.originId != contextId) return;
            if (id.index >= geometryGenerator.shapeCount) {
                return;
            }

            GeometryShape shape = geometryGenerator.shapes[id.index];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Color[] colors = geometryGenerator.colorList.array;
            for (int i = start; i < end; i++) {
                colors[i] = color;
            }
        }

        public void SetColor(ShapeId id, Vector3 normal) {
            if (id.originId != contextId) return;
            if (id.index >= geometryGenerator.shapeCount) {
                return;
            }

            GeometryShape shape = geometryGenerator.shapes[id.index];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Vector3[] normals = geometryGenerator.normalList.array;
            for (int i = start; i < end; i++) {
                normals[i] = normal;
            }
        }

        public void SetPositions(ShapeId id, Vector3[] vertices) {
            if (id.originId != contextId) return;
            if (id.index >= geometryGenerator.shapeCount) {
                return;
            }

            GeometryShape shape = geometryGenerator.shapes[id.index];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            if (vertices.Length < shape.vertexCount) {
                return;
            }

            Vector3[] positions = geometryGenerator.positionList.array;
            int idx = 0;
            for (int i = start; i < end; i++) {
                positions[i] = vertices[idx++];
            }
        }

    }

}