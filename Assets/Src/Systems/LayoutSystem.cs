using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem {

        private RectTransform rectTransform;

        public LayoutSystem(GameObject gameObject) {
            this.rectTransform = gameObject.GetComponent<RectTransform>();
        }

        public void Register(RegistrationData data) { }

        public void Update() {
            
        }

    }

}