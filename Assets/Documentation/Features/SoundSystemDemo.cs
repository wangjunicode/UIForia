using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Sound;

namespace Documentation.Features {
    
    [Template("Features/SoundSystemDemo.xml")]
    public class SoundSystemDemo : UIElement {

        public UISoundData currentSound;
        
        public override void OnCreate() {
            application.SoundSystem.onSoundPlayed += evt => { currentSound = evt.SoundData; };
            application.SoundSystem.onSoundStopped += evt => { currentSound = default; };
        }
    }
}
