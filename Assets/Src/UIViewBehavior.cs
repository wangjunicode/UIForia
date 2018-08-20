using System.Collections.Generic;
using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    [Template("TestTemplate.xml")]
    public class TempUIType : UIElement {

        [Prop] public float r;
        [Prop] public float g;
        [Prop] public float b;

    }

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
        public float frameTime;

        public Color color;
        
        public void Start() {

            view = new UIView(gameObject);
            view.font = font;
            view.templateType = typeof(TempUIType);
            view.OnCreate();
            TempUIType t = (TempUIType) view.root;
        }

        public void RefreshView() {
            view?.Refresh();
        }
        
        public void Update() {
//            Vector3[] corners = new Vector3[4];
//            GetComponent<RectTransform>().GetWorldCorners(corners);
//            Rect newRect = new Rect(corners[0], corners[2] - corners[0]);
//            GetComponentInChildren<Text>().text = newRect + "\n" + Input.mousePosition;
//            TempUIType t = (TempUIType) view.root;
//            t.r = color.r;
//            t.g = color.g;
//            t.b = color.b;
            float start = Time.realtimeSinceStartup;
            view.Update();
            frameTime = start - Time.realtimeSinceStartup;
        }

    }

}