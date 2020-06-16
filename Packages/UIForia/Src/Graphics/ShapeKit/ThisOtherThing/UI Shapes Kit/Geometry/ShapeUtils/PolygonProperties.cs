using Unity.Mathematics;

namespace ThisOtherThing.UI.ShapeUtils {

    public struct PolygonProperties {

        public PolygonCenterTypes polygonCenterType;
        public float2 centerOffset;
        public CutoutProperties cutoutProperties;

    }

    public enum PolygonCenterTypes {

        Calculated,
        Offset,
        CustomPosition,
        Cutout

    }

}