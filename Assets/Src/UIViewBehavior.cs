using System.Collections.Generic;
using Rendering;
using Src.Input;
using UnityEngine;

namespace Src {

    [Template("TestTemplate.xml")]
    public class TempUIType : UIElement {

        [Prop] public float r;
        [Prop] public float g;
        [Prop] public float b;

    }

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

    public class UIViewBehavior : MonoBehaviour {

        public UIGameObjectView view;
        public Font tempFont;
        
        public void Start() {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = new UIGameObjectView(tempFont, typeof(Inspector), rectTransform);
            view.Initialize();
        }

        public void RefreshView() {
            view?.Refresh();
        }

        public void Update() {
            view?.UpdateViewport();
            view?.Update();
        }

    }

}