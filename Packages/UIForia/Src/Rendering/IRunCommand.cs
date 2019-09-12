using UIForia.Elements;

namespace UIForia.Rendering {
    public interface IRunCommand {
        void Run(UIElement element);
        bool IsExit { get; }

    }
}
