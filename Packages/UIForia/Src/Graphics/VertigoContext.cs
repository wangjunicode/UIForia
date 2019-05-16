using System;
using System.Collections.Generic;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertigo {

    public enum ShapeMode {

        Physical,
        SDF

    }

    [Flags]
    public enum ShaderSlot {

        Text = 1 << 0,
        FillSDF = 1 << 1,
        FillPhysical = 1 << 2,
        StrokeSDF = 1 << 3,
        StrokePhysical = 1 << 4,
        Fill = FillSDF | FillPhysical,
        Stroke = StrokeSDF | StrokePhysical,
        All = Text | Fill | Stroke

    }


    public class VertigoContext {

        private static readonly MaterialPropertyBlock s_PropertyBlock;

        public readonly MaterialPool materialPool;
    //    private readonly IDrawCallBatcher batcher;
        private Matrix4x4 transform;
        private Stack<Matrix4x4> stateStack;

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private readonly ShapeGenerator shapeGenerator;
        private readonly GeometryCache geometryCache;
        private readonly GeometryGenerator geometryGenerator;

        private readonly Stack<RenderTexture> renderTextures;
        private readonly StructList<RenderCall> renderCalls;
        private readonly LightList<RenderTexture> renderTexturesToRelease;
        private RangeInt currentShapeRange;
        private ShapeMode defaultShapeMode;
        private VertigoMaterial strokeMaterial;
        private VertigoMaterial fillMaterial;
        private VertigoMaterial textMaterial;
        public static MaterialPool DefaultMaterialPool { get; }

        private bool needsNewMaterialInstance;
        private bool transformChangedSinceLastDrawCall;
        private int drawCallWithCurrentMaterial;
        private bool passChangedSinceLastDrawCall;
        private bool renderStateChangedSinceLastDrawCall;
        private bool materialChangedSinceLastDrawCall;
        
        public VertigoContext(ShapeMode shapeMode = ShapeMode.SDF, string batcher = null, MaterialPool materialPool = null) {
            if (batcher == null) {
             //   batcher = new DefaultDrawCallBatcher();
            }

            if (materialPool == null) {
                materialPool = DefaultMaterialPool;
            }

            this.defaultShapeMode = shapeMode;

            this.renderTextures = new Stack<RenderTexture>();
            this.renderCalls = new StructList<RenderCall>();
            this.renderTexturesToRelease = new LightList<RenderTexture>();

          //  this.batcher = batcher;
            this.materialPool = materialPool;
            this.stateStack = new Stack<Matrix4x4>();
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.shapeGenerator = new ShapeGenerator();
            this.geometryGenerator = new GeometryGenerator();
            this.geometryCache = new GeometryCache();
        }

        public enum MaterialUsage {

            Instance,
            Shared

        }

        private Dictionary<string, Shader> shaderMap = new Dictionary<string, Shader>();

        private VertigoMaterial FindMaterial(string shaderName) {
            shaderMap.TryGetValue(shaderName, out Shader shader);
            return materialPool.GetInstance(shaderName);
        }


        public void SetFloatProperty(int key, float value) {
            needsNewMaterialInstance = drawCallWithCurrentMaterial != 0;
            fillMaterial.SetFloatProperty(key, value);
        }

        public void SetShader(ShaderSlot slot, string shaderName, string[] keywords = null) {
            if (fillMaterial.shaderName == shaderName) {
//                if (fillMaterial.KeywordsMatch(keywords)) {
//                    return;
//                }
            }

            VertigoMaterial material = FindMaterial(shaderName);

            if (slot == ShaderSlot.All) { }
            else if (slot == ShaderSlot.Fill) {
                fillMaterial = material;
            }
            else if (slot == ShaderSlot.Stroke) { }
            else if (slot == ShaderSlot.Text) { }
        }

        public void SetMainTexture(Texture texture) { }

        public void SetColorSpace(ColorSpace colorSpace) { }

        public void SetUVRect(Rect rect) { }

        public void EnableUVTilingOffset(Vector2 uvTiling, Vector2 uvOffset) { }

        private MaterialUsage materialUsage = MaterialUsage.Instance;

        public void SetMaterialUsageType(MaterialUsage materialUsage) { }

        public void SetMaterial(Material material) {
            fillMaterial = VertigoMaterial.GetInstance(material);
        }

        public void SetMaterial(MaterialUsage materialUsage, Material material) {
            if (material.shader) {
                if (material.shaderKeywords != null) { }

                // if we haven't registered this shader before, update our keyword check
            }

            material.HasProperty(ShaderKey.Culling);
            material.HasProperty(ShaderKey.Culling);
            material.HasProperty(ShaderKey.Culling);
            material.HasProperty(ShaderKey.Culling);
            material.HasProperty(ShaderKey.Culling);
            material.HasProperty(ShaderKey.Culling);
        }

        public void SetMaterial(VertigoMaterial material) {
            this.fillMaterial = material;
            this.strokeMaterial = material;
            this.textMaterial = material;
        }

        public void SetTextMaterial(VertigoMaterial material) {
            this.textMaterial = material;
        }

        public void SetFillMaterial(VertigoMaterial material) {
            this.fillMaterial = material;
        }

        public void SetStrokeMaterial(VertigoMaterial material) {
            this.strokeMaterial = material;
        }

        public void FillText(float x, float y, TextInfo textInfo, VertigoMaterial material = null) {
            material = material ?? textMaterial;
            if (material == null) return;
            geometryGenerator.FillText(new Vector3(x, y, 0), textInfo, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, transform);
        }

        public void FillRect(float x, float y, float width, float height, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            if (material == null) return;
            shapeGenerator.Rect(x, y, width, height);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(shapeGenerator.shapes.Count - 1, 1), defaultShapeMode, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, transform);
        }

        public void FillRhombus(float x, float y, float width, float height, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            if (material == null) return;
            int pathId = shapeGenerator.Rhombus(x, y, width, height);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, transform);
        }

        public void FillCircle(float x, float y, float radius, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            if (material == null) return;
            int pathId = shapeGenerator.Circle(x, y, radius);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, transform);
        }

        public void FillEllipse(float x, float y, float rw, float rh, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            if (material == null) return;
            int pathId = shapeGenerator.Ellipse(x, y, rw, rh);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, transform);
        }

        public void FillRoundedRect(float x, float y, float width, float height, float rTL, float rTR, float rBL, float rBR, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            if (material == null) return;
            int pathId = shapeGenerator.RoundedRect(x, y, width, height, rTL, rTR, rBL, rBR);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, transform);
        }

        public void SaveState() {
            stateStack.Push(transform);
        }

        public void RestoreState() {
            if (stateStack.Count == 0) {
                return;
            }

            transform = stateStack.Pop();
        }

        public void Draw(GeometryCache cache, VertigoMaterial material) {
            //batcher.AddDrawCall(cache, new RangeInt(0, cache.shapes.size), material, transform);
        }

        public void Draw(GeometryCache cache, RangeInt range, VertigoMaterial material) {
            //batcher.AddDrawCall(cache, range, material, transform);
        }

        public void Draw(GeometryCache cache, int idx, VertigoMaterial material) {
            if (transformChangedSinceLastDrawCall) {
                // multiply vertices by current transform matrix    
            }

            if (needsNewMaterialInstance) {
                material = material.Clone(material);
            }

            if (passChangedSinceLastDrawCall) { }

            //batcher.AddDrawCall(cache, new RangeInt(idx, 1), material, transform);
        }

        public void SetStrokeColor(Color32 color) {
            geometryGenerator.SetStrokeColor(color);
        }

        public void SetStrokeWidth(float strokeWidth) {
            geometryGenerator.SetStrokeWidth(strokeWidth);
        }

        public void SetLineCap(LineCap lineCap) {
            geometryGenerator.SetLineCap(lineCap);
        }

        public void SetLineJoin(LineJoin lineJoin) {
            geometryGenerator.SetLineJoin(lineJoin);
        }

        public int Rect(float x, float y, float width, float height) {
            shapeGenerator.Rect(x, y, width, height);
            currentShapeRange.length++;
            return shapeGenerator.shapes.size - 1;
        }

        public void Circle(float x, float y, float radius) {
            shapeGenerator.Circle(x, y, radius);
            currentShapeRange.length++;
        }

        public void BeginShapeRange() {
            currentShapeRange.start = shapeGenerator.shapes.size;
            currentShapeRange.length = 0;
        }

        public void SetUVRect(in Rect rect) {
            uvRect = rect;
        }

        public void ResetUVState() {
            uvRect = new Rect(0, 0, 1, 1);
        }

        public void SetUVTiling(float x, float y) {
            throw new NotImplementedException();
        }

        public void SetUVOffset(float x, float y) {
            throw new NotImplementedException();
        }

        public void Fill(GeometryCache cache, RangeInt range, VertigoMaterial material) {
            throw new NotImplementedException();
        }

        public void DrawSprite(Sprite sprite, Rect rect, VertigoMaterial material) {
            int start = geometryCache.shapeCount;
            geometryGenerator.FillSprite(sprite, rect, geometryCache);
            //batcher.AddDrawCall(geometryCache, new RangeInt(start, 1), material, transform);
        }

        public void DrawSprite(Sprite sprite, VertigoMaterial material) {
            int start = geometryCache.shapeCount;
            geometryGenerator.FillSprite(sprite, default, geometryCache);
           // batcher.AddDrawCall(geometryCache, new RangeInt(start, 1), material, transform);
        }

        private Rect uvRect = new Rect(0, 0, 1, 1);
        private static readonly StructList<Vector4> s_ScratchVector4 = new StructList<Vector4>();

        public void Fill(VertigoMaterial material) {
            if (currentShapeRange.length == 0) {
                return;
            }

            int start = geometryCache.shapeCount;
            geometryGenerator.Fill(shapeGenerator, currentShapeRange, defaultShapeMode, geometryCache);
            int count = geometryCache.shapeCount - start;

            if (uvRect.x != 0 || uvRect.y != 0 || uvRect.width != 1 || uvRect.height != 1) {
                geometryCache.GetTextureCoord0(start, s_ScratchVector4);
                Vector4[] uvs = s_ScratchVector4.array;
                float minX = uvRect.x;
                float minY = uvRect.y;
                // todo -- might only work when uv rect is smaller than original
                for (int i = 0; i < s_ScratchVector4.size; i++) {
                    // map bounds uvs to different rect uvs
                    uvs[i].x = minX + (uvs[i].x * uvRect.width);
                    uvs[i].y = minY + (uvs[i].y * uvRect.height);
                }

                geometryCache.SetTexCoord0(start, s_ScratchVector4);
            }

         //   batcher.AddDrawCall(geometryCache, new RangeInt(start, count), material, transform);
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

         //   StructList<BatchDrawCall> drawCalls = StructList<BatchDrawCall>.Get();
//            Matrix4x4 rootMat = Matrix4x4.TRS(new Vector3(-(width / 2), height / 2), Quaternion.identity, Vector3.one);
         //   batcher.Bake(width, height, cameraMatrix, drawCalls);
         //   batcher.Clear();

            renderCalls.Add(new RenderCall() {
                // matrix?
                texture = targetTexture,
       //         drawCalls = drawCalls
            });

            return targetTexture;
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
//
////                Render(renderCalls[i].drawCalls);
//                int count = renderCalls[i].drawCalls.size;
//        //        BatchDrawCall[] calls = renderCalls[i].drawCalls.array;
//
//                for (int j = 0; j < count; j++) {
//                    Mesh mesh = calls[j].mesh.mesh;
//                    Material material = calls[j].material.material;
//                    // UpdateMaterialPropertyBlock(calls[j].state);
//                    int passCount = material.passCount;
//                    // todo -- only render specified passes
//                    commandBuffer.DrawMesh(mesh, calls[j].transform, material, 0, 0, s_PropertyBlock);
//                    for (int k = 0; k < passCount; k++) { }
//                }
            }
        }

        public void SetMask(Texture texture, float softness) { }

        static VertigoContext() {
            s_PropertyBlock = new MaterialPropertyBlock();
            DefaultMaterialPool = new MaterialPool();
        }

        public void Clear() {
            for (int i = 0; i < renderTexturesToRelease.Count; i++) {
                RenderTexture.ReleaseTemporary(renderTexturesToRelease[i]);
            }

            currentShapeRange = new RangeInt(0, 0);
            shapeGenerator.Clear();
            geometryCache.Clear();
            geometryGenerator.ResetRenderState();
            transform = Matrix4x4.identity;

            for (int i = 0; i < renderCalls.Count; i++) {
        //        StructList<BatchDrawCall> drawCalls = renderCalls[i].drawCalls;
        //        for (int j = 0; j < drawCalls.Count; j++) {
                    //drawCalls[j].material.Release();
       //             drawCalls[j].mesh.Release();
        //        }

         //       StructList<BatchDrawCall>.Release(ref drawCalls);
            }

            renderCalls.Clear();
            renderTextures.Clear();
            renderTexturesToRelease.Clear();
        }

        public void SetPosition(Vector3 position) {
            this.position = position;
            transform = Matrix4x4.TRS(position, rotation, scale);
        }

        public void SetPosition(Vector2 position) {
            this.position = new Vector3(position.x, position.y, this.position.z);
            transform = Matrix4x4.TRS(position, rotation, scale);
        }

        public void SetRotation(float angle) {
            rotation = Quaternion.Euler(0, 0, angle);
            transform = Matrix4x4.TRS(position, rotation, scale);
        }

        public void SetFillColor(Color32 color) {
            geometryGenerator.SetFillColor(color);
        }

        private struct RenderCall {

            public RenderTexture texture;
        //    public StructList<BatchDrawCall> drawCalls;

        }

        public void StrokeRect(float x, float y, float width, float height) {
            shapeGenerator.Rect(x, y, width, height);
            geometryGenerator.Stroke(shapeGenerator, geometryCache);
        //    batcher.AddDrawCall(geometryCache, strokeMaterial, transform);
        }

    }

}