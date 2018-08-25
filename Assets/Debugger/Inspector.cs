using Src;
using UnityEngine;

namespace Debugger {

    [Template("Debugger/Inspector.xml")]
    public class Inspector : UIElement {

        public Vector2 mousePosition;
        public float time;
        
        public override void OnUpdate() {
            time = Time.realtimeSinceStartup;
        }

    }

}