using System;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public UIView view;
        public Type type;
        public string typeName;
        public new Camera camera;
        
        public void Start() {
            type = Type.GetType(typeName);
            
            if (type == null) return;
        
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = Application.Game.AddView(rectTransform.rect, type);
            Application.Game.SetCamera(camera);
        }

        private void Update() {
            if (type == null) return;
            Application.Game.Update();
        }

        public void RefreshView() {
            if (type == null) return;
            view?.Refresh();
        }

    }

}