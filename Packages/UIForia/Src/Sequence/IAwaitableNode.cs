using UIForia.Util;

namespace UIForia {

    public interface IAwaitableNode : IScriptNode {

        void Await(SequenceContext context, StructList<ElementId> targets);

    }

}