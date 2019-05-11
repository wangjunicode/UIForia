using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

namespace Vertigo {

    public class UIForiaRenderContext {

        private UIForiaShader activeShader;
        private Dictionary<string, UIForiaShader> shaderMap;
        private LightList<UIForiaShader> shadersToRelease;
        private readonly ShapeDataCache geometryGenerator;
        private readonly ushort contextId;
        private StructList<PendingBatch> pendingBatches;

        private Matrix4x4 transform;

        private bool batchBroken;
        private StructList<ShapeMeshData> pendingDrawCalls;
        private StructList<FinalDrawCall> drawCalls;
        private bool transformChanged;
        private readonly StructList<Matrix4x4> transformList;

        private readonly Stack<RenderTexture> renderTextures;
        private readonly StructList<RenderCall> renderCalls;
        private readonly LightList<RenderTexture> renderTexturesToRelease;
        private readonly VertigoMesh.MeshPool meshPool;

        private static ushort s_ContextIdGen;

        private RenderState renderState;

        public UIForiaRenderContext() {
            this.contextId = s_ContextIdGen++;
            this.geometryGenerator = new ShapeDataCache();
            this.shadersToRelease = new LightList<UIForiaShader>();
            this.shaderMap = new Dictionary<string, UIForiaShader>();
            this.activeShader = UIForiaShader.Default;
            this.renderTextures = new Stack<RenderTexture>();
            this.renderTexturesToRelease = new LightList<RenderTexture>();
            this.transformList = new StructList<Matrix4x4>();
            this.meshPool = new VertigoMesh.MeshPool();

            this.renderCalls = new StructList<RenderCall>(); // list of draw calls + a texture to draw to
            this.drawCalls = new StructList<FinalDrawCall>(); // final list of shapes to draw
            this.pendingDrawCalls = new StructList<ShapeMeshData>(); // shapes we might draw 
            this.pendingBatches = new StructList<PendingBatch>(); // 

            transformList.Add(Matrix4x4.identity);
        }

        public void Clear() {
            transformList.QuickClear();
            transformChanged = false;
            pendingBatches.Clear();
            transform = Matrix4x4.identity;
            transformList.Add(transform);

            // might be taken care of already
            for (int i = 0; i < shadersToRelease.Count; i++) {
                shadersToRelease[i].Release();
            }

            shadersToRelease.QuickClear();

            for (int i = 0; i < renderTexturesToRelease.Count; i++) {
                RenderTexture.ReleaseTemporary(renderTexturesToRelease[i]);
            }

            geometryGenerator.Clear();

            for (int i = 0; i < renderCalls.Count; i++) {
                renderCalls[i].Release();
            }

            for (int i = 0; i < pendingBatches.size; i++) {
                //pendingBatches[i].renderedShapes
            }

            pendingDrawCalls.Clear();
            renderCalls.Clear();
            renderTextures.Clear();
            renderTexturesToRelease.Clear();
        }

        public void SetShader(string shaderName) {
            if (activeShader.shaderName == shaderName) {
                return;
            }

            if (shaderMap.TryGetValue(shaderName, out UIForiaShader shaderRoot)) {
                if (!activeShader.isPooled) {
                    activeShader.isPooled = true;
                    shadersToRelease.Add(activeShader);
                }

                activeShader = shaderRoot.GetInstance();
            }
        }

        public void SetFillColor(Color color) {
            renderState.fillColor = color;
        }

        public void SetTexCoordChannel(TextureCoordChannel channel) {
            renderState.texCoordChannel = channel;
        }

        public void SetFloatProperty(int key, float value) {
            if (pendingDrawCalls.size > 0) {
                pendingBatches.Add(new PendingBatch() {
                    shader = activeShader,
                    renderedShapes = pendingDrawCalls
                });
                activeShader = activeShader.Clone();
                pendingDrawCalls = StructList<ShapeMeshData>.Get();
            }

            activeShader.SetFloatProperty(key, value);
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
                }
            }

            else {
                for (int i = 0; i < shapeList.size; i++) {
                    ShapeMeshData meshData = shapeList[i];
                    int vertexStart = meshData.shape.vertexStart;
                    int vertexCount = meshData.shape.vertexCount;
                    int triangleStart = meshData.shape.triangleStart;
                    int triangleCount = meshData.shape.triangleCount;

                    s_NormalList.AddRange(geometryGenerator.normalList, vertexStart, vertexCount);
                    s_ColorList.AddRange(geometryGenerator.colorList, vertexStart, vertexCount);
                    s_TexCoordList0.AddRange(geometryGenerator.texCoordList0, vertexStart, vertexCount);
                    s_TexCoordList1.AddRange(geometryGenerator.texCoordList1, vertexStart, vertexCount);
                    s_TexCoordList2.AddRange(geometryGenerator.texCoordList2, vertexStart, vertexCount);
                    s_TexCoordList3.AddRange(geometryGenerator.texCoordList3, vertexStart, vertexCount);
                    s_TriangleList.AddRange(geometryGenerator.triangleList, triangleStart, triangleCount);
                }
            }

            ListAccessor<Vector3>.SetArray(s_ScratchVector3, s_PositionList.array, s_PositionList.size);
            mesh.mesh.SetVertices(s_ScratchVector3);

            ListAccessor<Vector3>.SetArray(s_ScratchVector3, s_NormalList.array, s_NormalList.size);
            mesh.mesh.SetNormals(s_ScratchVector3);

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
        public struct DrawMeshCall {

            public VertigoMesh mesh;
            public UIForiaShader material;
            public Matrix4x4 transform;

            public DrawMeshCall(VertigoMesh mesh, UIForiaShader shader, in Matrix4x4 transform) {
                this.mesh = mesh;
                this.material = shader;
                this.transform = transform;
            }

        }

        private struct PendingBatch {

            public UIForiaShader shader;
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
                    drawCalls[i].material.Release();
                    drawCalls[i].mesh.Release();
                }

                StructList<DrawMeshCall>.Release(ref drawCalls);
            }

        }

        private struct FinalDrawCall {

            public UIForiaShader shader;
            public StructList<ShapeMeshData> shapes;

            public FinalDrawCall(UIForiaShader shader, StructList<ShapeMeshData> shapes) {
                this.shader = shader;
                this.shapes = shapes;
            }

            public void Release() {
                StructList<ShapeMeshData>.Release(ref shapes);
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

    }

}