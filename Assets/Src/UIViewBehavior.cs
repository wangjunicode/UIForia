using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src {

    [Template("TestTemplate.xml")]
    public class TempUIType : UIElement { }

    [Template("TestRepeat.xml")]
    public class TestRepeat : UIElement {

        public List<Color> colors = new List<Color>() {
            Color.black, 
            Color.red,
            Color.white,
            Color.blue
        };

    }

    public class UIViewBehavior : MonoBehaviour {

        public Font font;
        public UIView view;

        public void Start() {

            view = new UIView();
            view.font = font;
            view.gameObject = gameObject;
            view.templateType = typeof(TestRepeat);
            view.OnCreate();

        }

        public void RefreshView() {
            if (view != null) {
                view.Refresh();
            }
        }
        
        public void Update() {
            view.Update();
        }

    }

}