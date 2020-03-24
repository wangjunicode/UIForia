using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation {
    
    [Template("WorldRoot.xml")]
    public class WorldRoot : UIElement {

        public Vector3 position;
        
        public override void OnUpdate() {
            position = GameObject.Find("Cube").transform.position;
        }

    }
}
