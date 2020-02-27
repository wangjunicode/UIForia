using System;
using UIForia.Selectors;
using UIForia.Rendering;
using Unity.Collections;

namespace UIForia {
    

    public struct RunCommand {

        public RunCommandType type;
        public RunCommandState state;
        public RunCommandAction action;
        public NativeArray<char> cmd;

    }

    public enum RunCommandAction {

        Start,
        Pause,
        Stop,
        Resume

    }

    public enum RunCommandState {

        Enter,
        Exit

    }

    public enum RunCommandType {

        Sound,
        Event,
        Animation,
        Transition,
        Selector

    }

}