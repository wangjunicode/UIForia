using Documentation.DocumentationElements;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;

[Template("Documentation/Features/TransformPlayground.xml")]
public class TransformPlayground : UIElement {

    public AnchorTarget AnchorTarget;

    public override void OnCreate() {
        AnchorTarget = AnchorTarget.Parent;
    }

    public void ChangeAnchorTo(AnchorTarget target) {

        AnchorTarget = target;
        
        
    }
    
}