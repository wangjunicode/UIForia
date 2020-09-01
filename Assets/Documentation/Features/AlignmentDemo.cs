using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;

namespace Documentation.Features {

    [Template("Documentation/Features/AlignmentDemo.xml")]
    public class AlignmentDemo : UIElement {
      
        public AlignmentTarget alignmentTarget;

        public List<ISelectOption<AlignmentTarget>> alignmentTargets;

        public AlignmentBoundary alignmentBoundary;

        public List<ISelectOption<AlignmentBoundary>> alignmentBoundaries;

        public override void OnCreate() {
            alignmentTarget = AlignmentTarget.Parent;
            alignmentTargets = new List<ISelectOption<AlignmentTarget>> {
                new EnumSelectOption<AlignmentTarget>(AlignmentTarget.Parent),
                new EnumSelectOption<AlignmentTarget>(AlignmentTarget.ParentContentArea),
                new EnumSelectOption<AlignmentTarget>(AlignmentTarget.LayoutBox),
                new EnumSelectOption<AlignmentTarget>(AlignmentTarget.View),
                new EnumSelectOption<AlignmentTarget>(AlignmentTarget.Unset),
                new EnumSelectOption<AlignmentTarget>(AlignmentTarget.Mouse),
            };

            alignmentBoundary = AlignmentBoundary.Unset;
            alignmentBoundaries = new List<ISelectOption<AlignmentBoundary>> {
                new EnumSelectOption<AlignmentBoundary>(AlignmentBoundary.Unset),
                new EnumSelectOption<AlignmentBoundary>(AlignmentBoundary.Parent),
                new EnumSelectOption<AlignmentBoundary>(AlignmentBoundary.Clipper),
                new EnumSelectOption<AlignmentBoundary>(AlignmentBoundary.View),
                new EnumSelectOption<AlignmentBoundary>(AlignmentBoundary.Screen),
            };
        }

        public void ShowToolTip() {
            FindById("tooltip").style.SetVisibility(Visibility.Visible, StyleState.Normal);
        }
    }
}