namespace ThisOtherThing.UI.ShapeUtils {

    public struct EllipseProperties {

        public EllipseFitting fitting;
        public float baseAngle;
        public RoundingResolution resolution;

        // public void UpdateAdjusted(Vector2 radius, float offset) {
        //
        //     radius.x += offset;
        //     radius.y += offset;
        //
        //     resolution.maxDistance = math.max(resolution.maxDistance, 0.1f);
        //
        //     switch (resolution.resolutionType) {
        //         default:
        //         case ResolutionType.Calculated:
        //             float circumference;
        //
        //             resolution.maxDistance = math.max(resolution.maxDistance, 0.1f);
        //
        //             if (radius.x == radius.y) {
        //                 circumference = GeoUtils.TwoPI * radius.x;
        //             }
        //             else {
        //                 circumference = Mathf.PI * (
        //                     3.0f * (radius.x + radius.y) -
        //                     math.sqrt(
        //                         (3.0f * radius.x + radius.y) *
        //                         (radius.x + 3.0f * radius.y)
        //                     )
        //                 );
        //             }
        //
        //             AdjustedResolution = (int) math.ceil(circumference / resolution.maxDistance);
        //             break;
        //
        //         case ResolutionType.Fixed:
        //             resolution.fixedResolution = math.max(resolution.fixedResolution, 3);
        //             AdjustedResolution = resolution.fixedResolution;
        //             break;
        //
        //         case ResolutionType.Auto:
        //
        //             if (radius.x == radius.y) {
        //                 circumference = GeoUtils.TwoPI * radius.x;
        //             }
        //             else {
        //                 circumference = Mathf.PI * (
        //                     3.0f * (radius.x + radius.y) -
        //                     math.sqrt(
        //                         (3.0f * radius.x + radius.y) *
        //                         (radius.x + 3.0f * radius.y)
        //                     )
        //                 );
        //             }
        //
        //             AdjustedResolution = (int) math.ceil(circumference / 4f);
        //             break;
        //     }
        // }

    }

    public enum ResolutionType {

        Auto = 0,
        Calculated = 1,
        Fixed = 2

    }

}