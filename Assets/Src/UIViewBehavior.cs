using System.Collections.Generic;
using Debugger;
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

    public class UIViewBehavior : MonoBehaviour {

        public UIGameObjectView view;
        public Font tempFont;
        
        public void Start() {
            RectTransform rectTransform = transform.Find("UIRoot").GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = new UIGameObjectView(typeof(Inspector), rectTransform);
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