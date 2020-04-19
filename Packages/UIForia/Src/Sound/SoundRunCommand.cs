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
                    element.application.SoundSystem.PlaySound(element, soundData);
                    break;

                case RunAction.Pause:
                    element.application.SoundSystem.PauseSound(element, soundData);
                    break;

                case RunAction.Resume:
                    element.application.SoundSystem.SoundResumed(element, soundData);
                    break;

                case RunAction.Stop:
                    element.application.SoundSystem.StopSound(element, soundData);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsExit { get; }

        public RunAction RunAction { get; }

    }

}