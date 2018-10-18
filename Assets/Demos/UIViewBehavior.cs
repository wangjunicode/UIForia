using Rendering;
using UnityEngine;

namespace Src {

    public class UIViewBehavior : MonoBehaviour {

        public UIGameObjectView view;
        private UnityEngine.UI.Text debugText;

        public void Start() {
            RectTransform rectTransform = transform.Find("UIRoot").GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            view = new UIGameObjectView(typeof(ChatWindow), rectTransform);
            view.Initialize();
//            GameObject debug = new GameObject("Debug Text");
//            RectTransform debugTransform = debug.AddComponent<RectTransform>();
//            debugTransform.SetParent(transform);
//            debugText = debugTransform.gameObject.AddComponent<UnityEngine.UI.Text>();
//            debugTransform.anchorMin = new Vector2(1, 0);
//            debugTransform.anchorMax = new Vector2(1, 0);
//            debugTransform.pivot = new Vector2(0.5f, 0.5f);
//            debugTransform.anchoredPosition = new Vector2(-110f, 15f);
//            debugTransform.sizeDelta = new Vector2(220, 30f);
//            debugText.color = Color.red;
//            debugText.fontSize = 24;
//            debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public void RefreshView() {
            view?.Refresh();
        }

        public void Update() {
            // float start = Time.realtimeSinceStartup;
            view?.Update();
            // debugText.text = "Frame Time: " + (Time.realtimeSinceStartup - start).ToString("F");
        }

    }

}