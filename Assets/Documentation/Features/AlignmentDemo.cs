using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Documentation.Features {

    [Template("Features/AlignmentDemo.xml")]
    public class AlignmentDemo : UIElement {
      
        public AlignmentTarget alignmentTarget;

        public override void OnCreate() {
            alignmentTarget = AlignmentTarget.Parent;
        }

        public void SetAlignmentTarget(AlignmentTarget target) {

            alignmentTarget = target;
        
        
        }
    }

}