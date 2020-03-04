using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace Documentation.DocumentationElements {

    public class WorldContainer : UIContainerElement {
        
        private UIElement element;
        
        public Vector3 worldPosition;
        public int activationRange = -1;
        private Camera cameraSystem;
        public override void OnEnable() {
            element = GetChild(0);
            cameraSystem = GameObject.Find("Camera").GetComponent<Camera>();
        }

        public override void OnUpdate() {
            GetScreenSpaceCoordinates(out float positionX, out float positionY, out float positionZ);
            PlaceUiElement(positionX, positionY);

            bool inFrontOfCamera = positionZ > 0;
            bool visible = inFrontOfCamera;

            if (visible && activationRange != -1) {
                visible = GetDistanceFromCamera() >= activationRange;
            }

            element.SetEnabled(visible);
        }

        private void GetScreenSpaceCoordinates(out float positionX, out float positionY, out float positionZ) {
            Vector3 pos = cameraSystem.WorldToScreenPoint(worldPosition);
            positionX = pos.x;
            positionY = cameraSystem.pixelRect.height - pos.y;
            positionZ = pos.z;
        }

        private float GetDistanceFromCamera() {
            return Vector3.Distance(cameraSystem.transform.position, worldPosition);
        }

        private void PlaceUiElement(float positionX, float positionY) {
            element.style.SetTransformPositionX(positionX, StyleState.Normal);
            element.style.SetTransformPositionY(positionY, StyleState.Normal);
        }
    }
}
