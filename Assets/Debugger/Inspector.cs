using System.Collections.Generic;
using Src;
using Src.Input;
using UnityEngine;

namespace Debugger {

    [Template("Debugger/Inspector.xml")]
    public class Inspector : UIElement {

        public float time;
        public Vector2 mousePosition;
        public List<int> values;
        public bool showMe;
        public int selectedValue;

        public Inspector() {
            selectedValue = 0;
            this.values = new List<int>();
            values.Add(1);
            values.Add(2);
            values.Add(3);
        }

        public override void OnUpdate() {
//            time = Time.realtimeSinceStartup;
        }

        public void OnMouseEnter(MouseInputEvent evt) {
//            Debug.Log("Entered! " + evt.mousePosition);
        }

        public void OnMouseContext() {
            values.RemoveAt(values.Count - 1);
        }

        public void OnMouseDown(MouseInputEvent evt) {
            selectedValue = (selectedValue + 1) % values.Count;
            Debug.Log(selectedValue);
            values.Add(values.Count);
        }

    }

}