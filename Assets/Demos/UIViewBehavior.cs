using Demo;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public UIView view;

        public void Start() {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = Application.Game.AddView(rectTransform.rect, typeof(ChatWindow));
            Application.Game.SetCamera(Camera.main);
        }

        private void Update() {
            Application.Game.Update();
        }

        public void RefreshView() {
            view?.Refresh();
        }

    }

}