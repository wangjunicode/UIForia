using Src;

namespace Rendering {

    public partial class UIStyleSet {

        public UIMeasurement positionX {
            get { return computedStyle.transform.position.x; }
            set { SetTransformPositionX(value, StyleState.Normal); }
        }

    }

}