using Rendering;
using UnityEngine;

namespace Src {

    [Template("TestTemplate.xml")]
    public class TempUIType : UIElement {

        [Prop] public float r;
        [Prop] public float g;
        [Prop] public float b;

    }

    public class UIViewBehavior : MonoBehaviour {

        public UIView view;
        
        public void Start() {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            RectTransform rectTransform  = canvas.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = new UIGameObjectView(typeof(TempUIType), rectTransform);
            view.OnCreate();
        }

        public void RefreshView() {
            view?.Refresh();
        }
        
        public void Update() {
            view.Update();
        }

    }

}