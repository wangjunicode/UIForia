using System;
using TMPro;

namespace Debugger {

    public class UITextContainerElement : UIElement {

        // todo -- wrap these in an interface for interop with other text systems
        protected TMP_TextInfo m_TextInfo;
        protected TMP_FontAsset m_FontAsset;

        public UITextContainerElement() {
            flags |= UIElementFlags.TextContainer;
        }

        public TMP_TextInfo textInfo {
            get { return m_TextInfo; }
            set {
                if (m_TextInfo != null) {
                    throw new Exception("Can only set textInfo once");
                }

                m_TextInfo = value;
            }
        }

        public TMP_FontAsset fontAsset {
            get { return m_FontAsset; }
            set { m_FontAsset = value; }
        }

        protected bool IsCharacterValid(char character) {
            return m_FontAsset.HasCharacter(character);
        }

    }

}