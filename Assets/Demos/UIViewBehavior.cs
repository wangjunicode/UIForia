using Rendering;
using UnityEngine;

namespace Src {

    public class UIViewBehavior : MonoBehaviour {

        public UIGameObjectView view;
        
        public void Start() {
            RectTransform rectTransform = transform.Find("UIRoot").GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = new UIGameObjectView(typeof(ChatWindow), rectTransform);
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