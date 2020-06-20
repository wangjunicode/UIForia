using UIForia.ListTypes;
using Unity.Collections;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public enum DrawMode {

        Normal,
        Shadow

    }

    public struct ShadowData {

        public float size;
        public float softness;

    }

    public partial struct ShapeKit {

        private List_float2 unitPositionBuffer;
        private List_float2 cornerBuffer;
        private List_float2 positionBuffer;
        private PointsData scratchPointData;

        private float lastEllipseBaseAngle;
        private float lastEllipseDirection;
        private EdgeGradientData edgeGradientData;
        private float adjustedAA;
        private float baseAA;
        private RangeInt topLeftCornerRange;
        private RangeInt topRightCornerRange;
        private RangeInt bottomLeftCornerRange;
        private RangeInt bottomRightCornerRange;
        private Allocator allocator;
        private DrawMode drawMode;
        private float dpiScale;
        private ShadowData shadowData;
        
        public ShapeKit(Allocator allocator) : this() {
            this.dpiScale = 1f;
            this.baseAA = 1.25f;
            this.drawMode = DrawMode.Normal;
            this.allocator = allocator;
            this.edgeGradientData = default;
            SetAntiAliasWidth(baseAA);
        }

        public void SetDpiScale(float dpiScale) {
            if (dpiScale <= 0) {
                dpiScale = 1f;
            }

            this.dpiScale = dpiScale;
            SetAntiAliasWidth(baseAA);
        }

        public void SetAntiAliasWidth(float aa) {
            aa = aa < 0 ? 0 : aa;
            baseAA = aa;
            if (dpiScale > 0) {
                adjustedAA = aa * (1.0f / dpiScale);
            }
            else {
                adjustedAA = aa;
            }

            UpdateEdgeGradient();
        }

        public void SetShadow(ShadowData shadowData) {
            this.shadowData = shadowData;
            if (drawMode == DrawMode.Shadow) {
                UpdateEdgeGradient();
            }
        }

        public void SetDrawMode(DrawMode drawMode) {
            if (this.drawMode == drawMode) {
                return;
            }

            this.drawMode = drawMode;
            UpdateEdgeGradient();
        }

        private void UpdateEdgeGradient() {
            if (drawMode == DrawMode.Normal) {
                if (adjustedAA > 0.0f) {
                    edgeGradientData.SetActiveData(1.0f, 0.0f, adjustedAA);
                }
                else {
                    edgeGradientData.Reset();
                }
            }
            else {
                edgeGradientData.SetActiveData(1.0f - shadowData.softness, shadowData.size, adjustedAA);
            }
        }
        
        public void Dispose() {
            cornerBuffer.Dispose();
            unitPositionBuffer.Dispose();
            positionBuffer.Dispose();
            scratchPointData.Dispose();
        }

    }

}