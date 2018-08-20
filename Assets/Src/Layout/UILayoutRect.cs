using Src;

namespace Rendering {

    public class UILayoutRect {

        public UIMeasurement x;
        public UIMeasurement y;
        public UIMeasurement width;
        public UIMeasurement height;

        public UILayoutRect() {
            x = UIStyle.UnsetMeasurementValue;
            y = UIStyle.UnsetMeasurementValue;
            width = UIStyle.UnsetMeasurementValue;
            height = UIStyle.UnsetMeasurementValue;
        }
        
        public UILayoutRect Clone() {
            UILayoutRect clone = new UILayoutRect();
            clone.x = x;
            clone.y = y;
            clone.width = width;
            clone.height = height;
            return clone;
        }

    }

}