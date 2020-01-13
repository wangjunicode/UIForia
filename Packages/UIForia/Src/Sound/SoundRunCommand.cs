using System;
using UIForia.Elements;
using UIForia.Rendering;

namespace UIForia.Sound {
    public class SoundRunCommand : IRunCommand {

        public UISoundData soundData;

        public SoundRunCommand(bool isExit, RunAction runAction = RunAction.Run) {
            IsExit = isExit;
            RunAction = runAction;
        }

        public void Run(UIElement element) {
            switch (RunAction) {
                case RunAction.Run:
                    element.Application.SoundSystem.PlaySound(element, soundData);
                    break;
                case RunAction.Pause:
                    element.Application.SoundSystem.PauseSound(element, soundData);
                    break;
                case RunAction.Resume:
                    element.Application.SoundSystem.SoundResumed(element, soundData);
                    break;
                case RunAction.Stop:
                    element.Application.SoundSystem.StopSound(element, soundData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsExit { get; }
        
        public RunAction RunAction { get; }

    }
}
