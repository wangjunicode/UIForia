using JetBrains.Annotations;
using UIForia;
using UIForia.Elements;

namespace UI {

    public class OptionalPropAttribute : System.Attribute {}
    public class RequiredPropAttribute : System.Attribute {}

    [Template("Klang/Seed/UIForia/KlangButton/KlangButton.xml")]
    public class KlangButton : UIElement {

        public string path;
        
        [RequiredProp]
        [OptionalProp]
        private string _label;
        private UITextElement textElement;
        
        public string label {
            get { return _label;}
            set {
                _label = value;
                textElement?.SetText(value);
            }
        }
        
        public override void OnCreate() {
            textElement = FindById<UITextElement>("text");
            textElement.SetText(_label);
        }

       
    }

    [Template("Klang/Seed/UIForia/KlangButton/KlangLinkButton.xml")]
    public class KlangLinkButton : KlangButton {

        [OnMouseUp]
        public void OnClick() {
            if (path == null) {
                return;
            }
            view.Application.Router.GoTo(path);
        }

    }

}