using UIForia;
using UIForia.Rendering;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/KlangProgressBar/KlangProgressBar.xml")]
    public class KlangProgressBar : UIElement {

        public float progress;
        public Color fillColor;
        public string toolTip;
        public ToolTipDirection toolTipDirection;
        public UIMeasurement size = 5f;

        public override void OnReady() {
            style.SetPreferredHeight(size, StyleState.Normal);
        }

        [OnPropertyChanged(nameof(size))]
        private void OnSizeChanged() {
            style.SetPreferredHeight(size, StyleState.Normal);
        }

    }

}