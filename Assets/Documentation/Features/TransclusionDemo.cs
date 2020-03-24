using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace TransclusionDemo {

    [Template("Features/TransclusionDemo.xml")]
    public class TransclusionDemo : UIElement {

        public RepeatableList<Color> colors;

        public override void OnCreate() {
            colors = new RepeatableList<Color>();
            colors.Add(Color.red);
            colors.Add(Color.green);
            colors.Add(Color.cyan);
            colors.Add(Color.magenta);
            colors.Add(Color.white);
            colors.Add(Color.yellow);
            colors.Add(Color.grey);
            colors.Add(Color.black);
            colors.Add(Color.blue);
        }

    }

}