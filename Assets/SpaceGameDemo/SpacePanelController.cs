using UIForia.Animation;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Sound;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace SpaceGameDemo {

    public class Controllers {

        private static SpacePanelController spacePanelController;

        public static SpacePanelController GetSpacePanelController() {
            if (spacePanelController == null) {
                spacePanelController = new SpacePanelController();
            }

            return spacePanelController;
        }

    }

    public class SpacePanelController {

        public string currentActivePanel = "SinglePlayer";

        private Transform lookCameraTransform;

        private AnimationTask animation;

        public SpacePanelController() {
            lookCameraTransform = GameObject.Find("Look_Camera").transform;
        }

        public string GetSpacePanelState(string spacePanel) {
            if (currentActivePanel == spacePanel && spacePanel == "StartMenu") {
                return "default";
            }

            return currentActivePanel == spacePanel ? "onscreen" : "offscreen";
        }

        public void UpdateRotation() {
            if (animation?.state != UITaskState.Running) {
                lookCameraTransform.Rotate(Vector3.forward, 1.0f * Time.deltaTime);
            }
        }

        public void LookAtRandomSpace(string nextPanel, UIElement element) {
            if (nextPanel == currentActivePanel) {
                return;
            }

            currentActivePanel = nextPanel;
            Quaternion rotation = lookCameraTransform.rotation;

            float targetX = (20 + 160f * Random.value) - 90;
            float targetY = (20 + 160f * Random.value) - 90;

            // I salvage the element for the animator
            UIElement rootElement = element.View.RootElement;

            if (animation?.state == UITaskState.Running) {
                rootElement.Animator.StopAnimation(animation.animationData);
            }

            animation = rootElement.Animator.PlayAnimation(new AnimationData() {
                options = new AnimationOptions() {
                    timingFunction = EasingFunction.QuadraticEaseInOut,
                    duration = new UITimeMeasurement(1500)
                },
                onTick = evt => {
                    float t = Mathf.Clamp01(Easing.Interpolate(MathUtil.PercentOfRange(evt.state.iterationProgress, 0, 1), evt.options.timingFunction.Value));

                    float lerpedX = Mathf.Lerp(rotation.x, targetX, t);
                    float lerpedY = Mathf.Lerp(rotation.y, targetY, t);
                    Quaternion quaternion = Quaternion.Lerp(rotation, Quaternion.Euler(lerpedX, lerpedY, rotation.normalized.z), t);
                    lookCameraTransform.rotation = quaternion;
                },
                onCompleted = evt => { lookCameraTransform.rotation = Quaternion.Euler(targetX, targetY, rotation.normalized.z); }
            });
        }

    }

}