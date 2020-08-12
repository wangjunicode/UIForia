using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UnityEngine;

[Template("CompilerDemo/Tmp.xml")]
public class Tmp : UIElement {

    public LayoutBehavior layoutBehavior = LayoutBehavior.TranscludeChildren;

    // [OnKeyDown(KeyCode.A)]
    // public void ToggleLayoutBehavior() {
    //     if (layoutBehavior == LayoutBehavior.Ignored) {
    //         layoutBehavior = LayoutBehavior.Normal;
    //     }
    //     else {
    //         layoutBehavior = LayoutBehavior.Ignored;
    //     }
    // }
        
    [OnKeyDown(KeyCode.D)]
    public void ToggleLayoutTranscludedBehavior() {
        if (layoutBehavior == LayoutBehavior.TranscludeChildren) {
            layoutBehavior = LayoutBehavior.Normal;
        }
        else {
            layoutBehavior = LayoutBehavior.TranscludeChildren;
        }
    }
   

}