using System;
using SVGX;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Packages.UIForia.Src.VectorGraphics {

    public class GFX2 {

        public MaterialPool materialPool;

        private Camera camera;
        private int drawCallCnt;
        private LightList<VectorDrawCall> drawCalls;
        private readonly DeferredReleasePool<BatchedVertexData> vertexDataPool;

        public GFX2(Camera camera) {
            this.camera = camera;
            this.drawCalls = new LightList<VectorDrawCall>(32);
            this.materialPool = new MaterialPool(Shader.Find("UIForia/TempStrokes"));
            this.vertexDataPool = new DeferredReleasePool<BatchedVertexData>();
        }

        public VectorContext CreateContext() {
            return new VectorContext(this);
        }

        public void Render() {
            vertexDataPool.FlushReleaseQueue();
            vertexDataPool.FlushReleaseQueue();

            Matrix4x4 origin = OriginMatrix;
            
            for (int i = 0; i < drawCalls.Count; i++) {
                
                BatchedVertexData vertexData = vertexDataPool.GetAndQueueForRelease();
                Material material = materialPool.GetAndQueueForRelease();
                
                DrawCallType drawCallType = drawCalls[i].drawCallType;
                
                switch (drawCallType) {
                    case DrawCallType.StandardStroke:
                        GenerateStrokeGeometry(vertexData, ref drawCalls.Array[i]);
                        break;
                    case DrawCallType.StandardFill:
                        break;
                    case DrawCallType.Shadow:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // later apply batching
                DrawMesh(vertexData.FillMesh(), origin, material);
            }
            
            drawCalls.QuickClear();
        }

        private Matrix4x4 OriginMatrix {
            get {
                Vector3 origin = camera.transform.position;
                origin.x -= 0.5f * Screen.width;
                origin.y += 0.5f * Screen.height;
                origin.z += 2;
                return Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            }
        }
        
        public void SetCamera(Camera camera) {
            this.camera = camera;
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 transform, Material material) {
            material.renderQueue = drawCallCnt++;
            Graphics.DrawMesh(mesh, transform, material, 0, camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
        }

        public void GenerateStrokeGeometry(BatchedVertexData vertexData, ref VectorDrawCall drawCall) {
            
            RangeInt shapeRange = drawCall.shapeRange;
            
            for (int i = shapeRange.start; i < shapeRange.end; i++) {
                
                SVGXShape shape = drawCall.ctx.shapes.Array[i];
                
                if (shape.type == SVGXShapeType.Path) {
                    if (shape.isClosed) { }
                    else {
                        //vertexData.GenerateStartCap(drawCall.ctx.points, shape, 30f, LineCap.Square);
                        vertexData.GenerateStrokeBody2(drawCall.ctx.points, shape, 30f);
                        //vertexData.GenerateEndCap(drawCall.ctx.points, shape, 30f, LineCap.Square);
                    }
                }
            }
            
           
        }

        // todo see about not copying state struct so much
        public void Stroke(VectorContext ctx, State state, RangeInt shapeRange) {
            drawCalls.Add(new VectorDrawCall(DrawCallType.StandardStroke, ctx, ref state, shapeRange));
        }

      

    }

}