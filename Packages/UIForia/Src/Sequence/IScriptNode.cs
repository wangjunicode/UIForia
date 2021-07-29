using UIForia.Util;

namespace UIForia {

    public interface IScriptNode {

        bool IsComplete { get; }

        void Reset();
        
        void Update(SequenceContext context, StructList<ElementId> targets);

    }

}