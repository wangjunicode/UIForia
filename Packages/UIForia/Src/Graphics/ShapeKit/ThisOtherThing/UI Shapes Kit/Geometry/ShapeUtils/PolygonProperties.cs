using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public struct PolygonProperties {

        public PolygonCenterTypes polygonCenterType;
        public Vector2 CenterOffset;
        public Vector2 CustomCenter;
        public Vector2 AdjustedCenter;
        public CutoutProperties CutoutProperties;

        public void UpdateAdjusted(PointListProperties pointListProperties) {
            // AdjustedCenter.x = 0.0f;
            // AdjustedCenter.y = 0.0f;
            //
            // if (polygonCenterType == PolygonCenterTypes.CustomPosition) {
            //     AdjustedCenter.x = CustomCenter.x;
            //     AdjustedCenter.y = CustomCenter.y;
            // }
            // else {
            //     for (int i = 0; i < pointListProperties.Positions.Length; i++) {
            //         AdjustedCenter.x += pointListProperties.Positions[i].x;
            //         AdjustedCenter.y += pointListProperties.Positions[i].y;
            //     }
            //
            //     AdjustedCenter.x /= (float) pointListProperties.Positions.Length;
            //     AdjustedCenter.y /= (float) pointListProperties.Positions.Length;
            // }
            //
            // if (polygonCenterType == PolygonCenterTypes.Cutout) {
            //     float safeRotationOffset = CutoutProperties.RotationOffset;
            //
            //     if (safeRotationOffset < 0.0f) {
            //         safeRotationOffset = GeoUtils.TwoPI + safeRotationOffset;
            //     }
            //
            //     float step = (GeoUtils.TwoPI / CutoutProperties.Resolution);
            //
            //     safeRotationOffset %= step;
            //     safeRotationOffset -= step * 0.5f;
            //
            //     GeoUtils.SetUnitPositionData(ref CutoutProperties.UnitPositionData, CutoutProperties.Resolution, safeRotationOffset);
            // }
            //
            // if (polygonCenterType == PolygonCenterTypes.Offset || polygonCenterType == PolygonCenterTypes.Cutout) {
            //     AdjustedCenter.x += CenterOffset.x;
            //     AdjustedCenter.y += CenterOffset.y;
            // }
        }

    }

    public enum PolygonCenterTypes {

        Calculated,
        Offset,
        CustomPosition,
        Cutout

    }

}