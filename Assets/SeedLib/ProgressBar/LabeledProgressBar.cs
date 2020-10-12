using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SeedLib {

    [Template("SeedLib/ProgressBar/LabeledProgressBar.xml")]
    public class LabeledProgressBar : UIElement {

        public Color color;
        public float progress;

        private UIElement progressBar;

        public override void OnEnable() {
            progressBar = progressBar ?? FindFirstByType<ProgressBar>();
            progressBar.SetAttribute("variant", GetAttribute("variant"));
        }

        protected override void OnSetAttribute(string attrName, string newValue, string oldValue) {
            progressBar?.SetAttribute(attrName, newValue);
        }

    }

}