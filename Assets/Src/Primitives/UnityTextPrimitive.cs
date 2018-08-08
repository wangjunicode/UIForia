using UnityEngine.UI;

namespace Src {

    public class UnityTextPrimitive : TextPrimitive {

        private readonly Text textComponent;

        public UnityTextPrimitive(Text textComponent) {
            this.textComponent = textComponent;
        }

        public override string Text {
            get { return textComponent.text; }
            set { textComponent.text = value; }
        }

    }

}