using System.Collections.Generic;
using Src;
using Src.Input;
using UnityEngine;

namespace Debugger {

    [Template("Debugger/Inspector.xml")]
    public class Inspector : UIElement {

        public float time;
        public Vector2 mousePosition;
        public List<float> values;

        public Inspector() {
            this.values = new List<float>();
            values.Add(1f);
            values.Add(2f);
            values.Add(3f);
        }
        
        public override void OnUpdate() {
            time = Time.realtimeSinceStartup;
        }

        public void OnMouseEnter(MouseInputEvent evt) {
            Debug.Log("Entered! " + evt.mousePosition);
        }

        public void OnMouseDown(MouseInputEvent evt) {
            Debug.Log("Down");
            style.backgroundColor = Color.yellow;
        }
        
    }

}