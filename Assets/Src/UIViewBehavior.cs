using Rendering;
using UnityEngine;

namespace Src {

    [Template("TestTemplate.xml")]
    public class TempUIType : UIElement { }

    public class UIViewBehavior : MonoBehaviour {

        public Font font;
        public UIView view;

        public void Start() {

            view = new UIView();
            view.font = font;
            view.gameObject = gameObject;
            view.templateType = typeof(TempUIType);
            view.OnCreate();

        }

        public void Update() {
            view.Update();
        }

    }

}