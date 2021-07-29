using System;
using System.Runtime.InteropServices;
using UIForia.Layout;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {

    // [PainterParameters(typeof(parameters))]
    public class StuffPainter {

        public struct Parameters {
            
        }

        public void AddParameters(UIElement element, Parameters parameters) {
            // do what you want with this 
            // if you need to know if an element was disabled, diff w/ last frame list but thats up to you 
        }

        [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
        unsafe struct Job : IJob {

            public StyleTables* styleTables;
            public DataList<ElementId> elements;
            
            public void Execute() {
                
            }

        }

        public void Update(UIElement element, bool isCulled) {
            if (isCulled) return;
        }

        public void SetupFrame() { }

        public void TeardownFrame() { }

        private bool[] hasForeground;
        
        public void PaintForeground(UIRenderContext2D ctx, UIElement element, int callIndex) {
            
            if (!hasForeground[callIndex]) return;
            
            Size size = element.GetLayoutSize();
            
            Texture bgTexture = default;
            
            UIShaderParameter pScale = default;
            UIShaderParameter pRadius = default;
            UIShaderParameter pBevel = default;
            UIShaderParameter pBgColor = default;
            UIShaderParameter pBgTexture = default;
            
            ElementRenderInfo renderInfo = default;
            if (renderInfo.backgroundColor.a <= 0 &&
                renderInfo.backgroundTint.a <= 0 &&
                (size.width == 0 || size.height == 0) &&
                bgTexture == null) {
                return;
            }
            
            ctx.SetFloat(pScale, 4f);
            ctx.SetVector(pRadius, new float4());
            ctx.SetVector(pBevel, new float4());
            ctx.SetColor(pBgColor, Color.red);
            ctx.SetTexture(pBgTexture, bgTexture);
            
            // ctx.SetShaderParameters(&parameters, sizeof(StructWithParameters);

            // figure out which material variant we need for each element

            // I think these can be automatic if enabled
            // probably don't need painter to handle this
            
            // we generate shader variants that include these automatically
            // materials can opt-out of them 
            // always applies to _MainTex sampling and total output 
            
            // if (ctx.HasUVTransformation(element.elementId)) { }
            // if (ctx.HasMeshFill(element.elementId)) { }

            ctx.SetMaterial(default);
            
            // setup -> init data
            // update -> apply animations or whatever
            // paint foreground (can be done in parallel since we just zip & sort by draw calls later, but should be on same thread imo)
            // paint background (can be done in parallel since we just zip & sort by draw calls later, but should be on same thread imo)
            // teardown 
            
            // how do we parameterize? 
            // probably with a struct push that maps to arguments
            // painters can opt into burst by overriding ScheduleUpdate/Foreground/Background
            // up to the user to do any required data conversion
            // if ScheduleXYZ() returns a default job handle
            // then we run the non burst version of that method
            
            // maybe we have AfterSetup, AfterUpdate, AfterForeground, AfterBackground hooks that run on main thread
            // could be used for stuff like texture atlasing, resource releasing, etc
            // could also be used to build meshes, would need away to invoke a SetMesh(drawId) in order to do that 
            
            for (int i = 0; i < 10; i++) {
                
                if (!hasForeground[i]) continue;
                
                ctx.BeginElement(element.elementId); // needed in order to setup material defaults and properly set the sortId
                // we can run painter work on seperate threads, each painter gets its own renderContext (shared if on same thread)
                // should probably expose the default element painting commands as a method that other painters can call 
                // still not sure how to paint other hierarchies, effects maybe? painters should stay 1-1 with elements 
                ctx.SetFloat(pScale, 4f);
                ctx.SetVector(pRadius, new float4());
                ctx.SetVector(pBevel, new float4());
                ctx.SetColor(pBgColor, Color.red);
                ctx.SetTexture(pBgTexture, bgTexture);
                ctx.DrawRect(0, 0, size.width, size.height);
            }

        }

        public JobHandle ScheduleUpdate() {
            // i need to figure out what I can expose here
            // probably all style properties must be accessible
            // and layout results
            // and matrices
            // and culling info 
            // and other stuff
            // should probably be in readonly form 
            return new Job().Schedule();
        }

    }

    internal struct MaterialDesc {

        public int materialId;
        public int floatCount;
        public int vectorCount;
        public int textureCount;

    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct UIShaderParameter {

        [FieldOffset(0)] internal readonly ushort materialId;
        [FieldOffset(2)] internal readonly byte offset;
        [FieldOffset(3)] internal readonly MaterialPropertyType type;
        [FieldOffset(0)] internal readonly int cmp;

    }

    public unsafe class UIRenderContext2D {

        public CommandBuffer commandBuffer;

        internal DataList<float4x4> matrices;
        internal DataList<int> matrixStack;
        internal DataList<AxisAlignedBounds2D> bounds;
        internal DataList<byte> parameterBuffer;
        internal MaterialDesc materialDesc;

        internal float4* vectorOffset;
        internal float* floatOffset;
        private int currentMatrixId;
        private bool needsMaterialFlush;

        internal UIRenderContext2D() {
            this.matrices = new DataList<float4x4>(64, Allocator.Persistent);
        }

        public void SetMaterial(ModuleAndNameKey key, ElementId initialValueElement = default) {
            // lookup
            if (initialValueElement != default) {
                // init
            }
            else {
                // setup defaults from material 
            }

            floatOffset = (float*) parameterBuffer.array;
            vectorOffset = (float4*) (floatOffset + materialDesc.floatCount);
        }

        public void SetFloat(UIShaderParameter id, float value) {
            if (id.materialId != materialDesc.materialId || id.type != MaterialPropertyType.Float) {
                return; // error
            }

            if (floatOffset[id.offset] == value) {
                return;
            }

            floatOffset[id.offset] = value;
            needsMaterialFlush = true;

        }
        
        public void SetColor(UIShaderParameter id, Color value) {
            if (id.materialId != materialDesc.materialId || id.type != MaterialPropertyType.Vector) {
                return; // error
            }

            if (vectorOffset[id.offset].Equals(*(float4*)&value)) {
                return;
            }

            vectorOffset[id.offset] = *(float4*)&value;
            needsMaterialFlush = true;
        }

        public void SetVector(UIShaderParameter id, float4 value) {

            if (id.materialId != materialDesc.materialId || id.type != MaterialPropertyType.Vector) {
                return; // error
            }

            if (vectorOffset[id.offset].Equals(value)) {
                return;
            }

            vectorOffset[id.offset] = value;
            needsMaterialFlush = true;

        }

        public void SetMatrix(UIShaderParameter id, float4x4 value) {
            if (id.materialId != materialDesc.materialId || id.type != MaterialPropertyType.Matrix) {
                return; // error
            }

            float4x4* matrixOffset = (float4x4*) (vectorOffset + materialDesc.vectorCount);
            matrixOffset[id.offset] = value;
            needsMaterialFlush = true;
        }

        public void SetTexture(UIShaderParameter id, Texture texture) { }

        public void PushMatrix(float4x4 matrix) {
            currentMatrixId = matrices.size;
            // need a stack i guess
            matrixStack.Add(currentMatrixId);
            matrices.Add(matrix);
        }

        public void PopMatrix() {
            currentMatrixId = matrixStack.Pop();
        }

        private struct DrawInfo {

            public int threadId; // ushort
            public int renderState;  // ushort
            public int materialId;  // ushort
            public int materialParameters;
            public int matrixId;    // ushort 
            public DrawSortId sortId;
            public ShapeType shapeType; // ushort probably 

        }


        internal DataList<byte> materialParameterResult;
        internal int materialByteSize;
        internal int renderStateId;
        internal DrawSortId sortId;

        internal struct DrawSortId {

            public ushort globalIndex;
            public ushort localIndex;

        }

        internal enum ShapeType {

            Rect,

        }

        internal DataList<int> matrixIds;
        internal DataList<float4> shapeParameters;

        public void DrawRect(float x, float y, float width, float height) {
            
            bounds.Add(new AxisAlignedBounds2D(x, y, x + width, y + height));
            matrices.Add(currentMatrixId);
            
            // shapes know how many items to read but for alignment reasons we should always push float4s 
            // maaaaaybe in some cases (the 0 case and the full value case) we could / should use stored indices instead 
            shapeParameters.Add(new float4());
            shapeParameters.Add(new float4());
            
            if (needsMaterialFlush) {
                materialParameterResult.AddRange(parameterBuffer.array, parameterBuffer.size);
                needsMaterialFlush = false;
            }

            // maybe figure out of if this thing can be drawn opaque (accept user hint maybe)
            
            new DrawInfo() {
                materialId = materialDesc.materialId,
                shapeType = ShapeType.Rect,
                materialParameters = parameterBuffer.size - materialByteSize,
                renderState = renderStateId,
                threadId = 0,
                matrixId = currentMatrixId,
                sortId = sortId
            };

            sortId.localIndex++;

        }

        internal void Dispose() {
            commandBuffer?.Dispose();
            matrices.Dispose();
        }

        public void BeginElement(ElementId elementId) {
            // looks up sortId for this element 
            throw new NotImplementedException();
        }

    }

}