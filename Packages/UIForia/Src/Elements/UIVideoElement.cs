using UIForia.Attributes;
using UnityEngine;
using UnityEngine.Video;

namespace UIForia.Elements {

    [TemplateTagName("Particles")]
    public class UIParticleElement : UIContainerElement {

        

    }

    [TemplateTagName("Video")]
    public class UIVideoElement : UIContainerElement {

        public string src;

        internal VideoPlayer videoPlayer;

        public bool isPlaying {
            get => videoPlayer?.isPlaying ?? false;
            set {
                if (videoPlayer == null) return;
                if (!videoPlayer.isPlaying) {
                    videoPlayer.Play();
                }
            }
        }

        public VideoClip clip {
            get => videoPlayer?.clip;
            set {
                if (videoPlayer == null) return;
                videoPlayer.clip = value;
            }
        }
        
        public override void OnEnable() {
            if (videoPlayer == null) {
                GameObject gameObject = application.GetDummyGameObject();
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        public override string GetDisplayName() {
            return "Video";
        }

    }

}