using System;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics.ShapeKit;
using UnityEngine;
using UnityEngine.UI;

namespace ThisOtherThing.UI.Shapes {

    [AddComponentMenu("UI/Shapes/Polygon", 30)]
    public class Polygon : MaskableGraphic, IShape {

        public Color fillColor;

        public PointListsProperties PointListsProperties =
            new PointListsProperties();

        public PolygonProperties PolygonProperties =
            new PolygonProperties();

        public ShadowsProperties ShadowProperties = new ShadowsProperties();

        public AntiAliasingProperties AntiAliasingProperties =
            new AntiAliasingProperties();

        PointsData[] pointsListData =
            new PointsData[] {new PointsData()};

        EdgeGradientData edgeGradientData;

        Rect pixelRect;

        public void ForceMeshUpdate() {
            if (pointsListData == null || pointsListData.Length != PointListsProperties.PointListProperties.Length) {
                Array.Resize(ref pointsListData, PointListsProperties.PointListProperties.Length);
            }
            //
            // for (int i = 0; i < pointsListData.Length; i++) {
            //     pointsListData[i].NeedsUpdate = true;
            // }

            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnEnable() {
            for (int i = 0; i < pointsListData.Length; i++) {
                pointsListData[i].isClosed = true;
            }

            base.OnEnable();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            if (pointsListData == null || pointsListData.Length != PointListsProperties.PointListProperties.Length) {
                Array.Resize(ref pointsListData, PointListsProperties.PointListProperties.Length);
            }

            for (int i = 0; i < pointsListData.Length; i++) {
                // pointsListData[i].NeedsUpdate = true;
                pointsListData[i].isClosed = true;
            }

            //AntiAliasingProperties.OnCheck();

            ForceMeshUpdate();
        }
#endif

        protected void OnPopulateMesh(UIVertexHelper vh) {
            vh.Clear();

            // if (pointsListData == null || pointsListData.Length != PointListsProperties.PointListProperties.Length) {
            //     Array.Resize(ref pointsListData, PointListsProperties.PointListProperties.Length);
            //
            //     for (int i = 0; i < pointsListData.Length; i++) {
            //         pointsListData[i].NeedsUpdate = true;
            //         pointsListData[i].IsClosed = true;
            //     }
            // }
            //
            // pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
            //
            // AntiAliasingProperties.UpdateAdjusted(canvas.scaleFactor);
            // ShadowProperties.UpdateAdjusted();
            //
            // for (int i = 0; i < PointListsProperties.PointListProperties.Length; i++) {
            //     PointListsProperties.PointListProperties[i].GeneratorData.skipLastPosition = true;
            //     PointListsProperties.PointListProperties[i].SetPoints();
            // }
            //
            // for (int i = 0; i < PointListsProperties.PointListProperties.Length; i++) {
            //     if (
            //         PointListsProperties.PointListProperties[i].Positions != null &&
            //         PointListsProperties.PointListProperties[i].Positions.Length > 2
            //         ) {
            //         PolygonProperties.UpdateAdjusted(PointListsProperties.PointListProperties[i]);
            //
            //         // shadows
            //         if (ShadowProperties.ShadowsEnabled) {
            //             for (int j = 0; j < ShadowProperties.Shadows.Length; j++) {
            //                 edgeGradientData.SetActiveData(
            //                     1.0f - ShadowProperties.Shadows[j].Softness,
            //                     ShadowProperties.Shadows[j].Size,
            //                     AntiAliasingProperties.Adjusted
            //                 );
            //
            //                 Polygons.AddPolygon(
            //                     ref vh,
            //                     PolygonProperties,
            //                     PointListsProperties.PointListProperties[i],
            //                     ShadowProperties.GetCenterOffset(pixelRect.center, j),
            //                     ShadowProperties.Shadows[j].Color,
            //                     GeoUtils.ZeroV2,
            //                     ref pointsListData[i],
            //                     edgeGradientData
            //                 );
            //             }
            //         }
            //     }
            // }
            //
            // for (int i = 0; i < PointListsProperties.PointListProperties.Length; i++) {
            //     if (
            //         PointListsProperties.PointListProperties[i].Positions != null &&
            //         PointListsProperties.PointListProperties[i].Positions.Length > 2
            //         ) {
            //         PolygonProperties.UpdateAdjusted(PointListsProperties.PointListProperties[i]);
            //
            //         // fill
            //         if (ShadowProperties.ShowShape) {
            //             if (AntiAliasingProperties.Adjusted > 0.0f) {
            //                 edgeGradientData.SetActiveData(
            //                     1.0f,
            //                     0.0f,
            //                     AntiAliasingProperties.Adjusted
            //                 );
            //             }
            //             else {
            //                 edgeGradientData.Reset();
            //             }
            //
            //             Polygons.AddPolygon(
            //                 ref vh,
            //                 PolygonProperties,
            //                 PointListsProperties.PointListProperties[i],
            //                 pixelRect.center,
            //                 fillColor,
            //                 GeoUtils.ZeroV2,
            //                 ref pointsListData[i],
            //                 edgeGradientData
            //             );
            //         }
            //     }
            // }

        }

    }

}