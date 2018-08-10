using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src {

    [Template("Temp/TestTemplate.xml")]
    public class TempUIType : UIElement { }
    

    public class UIViewBehavior : MonoBehaviour {

        public Font font;
        public UIView view;
        
        public void Start() {
            
//            view = new UIView();
//            view.font = font;
//            view.gameObject = gameObject;
//            view.templateType = typeof(TempUIType);
//            
//            UIPanel panel = new UIPanel();
//            UIText text0 = new UIText();
//            UIText text1 = new UIText();
//            UIText text2 = new UIText();
//            panel.children = new List<UIElement>() {
//                text0, text1, text2
//            };
//            for (int i = 0; i < panel.children.Count; i++) {
//                panel.children[i].parent = panel;
//            }
//            view.root = panel;
//            panel.view = view;
//            text0.view = view;
//            text1.view = view;
//            text2.view = view;
//            text0.label = "Text 0 dfa eaf eafae aaef aaf aefafaefarafafeafeafgageagae";
//            text1.label = "Text 1";
//            text2.label = "Text 2";
//            panel.OnCreate();
//            text0.OnCreate();
//            text1.OnCreate();
//            text2.OnCreate();
//            panel.style.background.color = Color.yellow;
//            view.MarkForRendering(panel);
//            view.MarkForRendering(text0);
//            view.MarkForRendering(text1);
//            view.MarkForRendering(text2);
        }

        public void Update() {
         //   view.Update();
        }
    }

}