using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/FlexDemo.xml")]
    public class FlexDemo : UIElement {

        public float rotation;
        public override void OnUpdate() {
            rotation += 50 * Time.deltaTime;
            if (rotation > 360) rotation = 0;
        }

    }

}