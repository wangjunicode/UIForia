using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace SeedLib {

    [Template("SeedLib/ProgressBar/ProgressBar.xml")]
    public class ProgressBar : UIElement {

        private float _progress;
        private Color _color = Color.black;
        private UIElement fillBar;

        public Color color {
            get => _color;
            set {
                _color = value;
                fillBar?.style.SetBackgroundColor(_color, StyleState.Normal);
            }
        }

        public float progress {
            get => _progress;
            set {
                _progress = Mathf.Clamp01(value);
                fillBar?.style.SetMeshFillAmount(_progress, StyleState.Normal);
            }
        }

        public override void OnEnable() {
            fillBar = fillBar ?? FindById("fill-bar");
            fillBar.style.SetBackgroundColor(_color, StyleState.Normal);
            fillBar.style.SetMeshFillAmount(_progress, StyleState.Normal);
        }

    }

}