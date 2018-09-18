using UnityEngine;

namespace Src.Systems {

    public class ElementMeasurements {

        public Extents childExtents;
        public ReadOnlyVector2 scrollOffset;
        
        public ReadOnlyVector2 localPosition;
        public ReadOnlyVector2 viewPosition;
        public ReadOnlyVector2 screenPosition;
        
        public float width;
        public float height;
        
        public ContentBoxRect contentBox;

        public bool IsOverflowingX => width < childExtents.max.x - childExtents.min.x;
        public bool IsOverflowingY => height < childExtents.max.y - childExtents.min.y;

        public bool IsOverflowing => IsOverflowingX || IsOverflowingY;

    }

}