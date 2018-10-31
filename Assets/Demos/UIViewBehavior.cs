using Demo;
using Rendering;
using UnityEngine;

namespace Src {

    public class UIViewBehavior : MonoBehaviour {

        public UIGameObjectView view;
        private UnityEngine.UI.Text debugText;

        public void Start() {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = new UIGameObjectView(typeof(ChatWindow), rectTransform);
            view.Initialize();
        }

        public void RefreshView() {
            view?.Refresh();
        }

        public void Update() {
            view?.Update();
        }

    }

}