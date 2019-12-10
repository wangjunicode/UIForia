using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Sound;

namespace Documentation.Features {
    
    [Template("Documentation/Features/SoundSystemDemo.xml")]
    public class SoundSystemDemo : UIElement {

        public UISoundData currentSound;
        
        public override void OnCreate() {
            Application.SoundSystem.onSoundPlayed += evt => { currentSound = evt.SoundData; };
            Application.SoundSystem.onSoundStopped += evt => { currentSound = default; };
        }
    }
}
