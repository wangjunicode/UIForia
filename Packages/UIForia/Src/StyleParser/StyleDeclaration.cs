using System.Runtime.InteropServices;
using UIForia.Parsing;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Style {

    internal struct StyleDeclaration {

        public DeclarationId prevSibling;
        public StyleDeclarationType declType;
        public DeclarationData declarationData;

    }

    internal struct PropertyDeclarationData {

        public PropertyKeyInfoFlag flags;
        public int propertyId;
        public RangeInt keyRange;
        public RangeInt valueRange;

    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct DeclarationData {

        [FieldOffset(0)] public MixinDeclarationData mixinDeclarationData;
        [FieldOffset(0)] public TransitionDeclarationData transitionDeclarationData;
        [FieldOffset(0)] public PropertyDeclarationData propertyDeclarationData;
        [FieldOffset(0)] public AnimationActionData animationActionData;

    }

    internal struct AnimationActionData {
        public RangeInt animationName;
        public RangeInt moduleName;
        public HookEvent hookEvent;
        public HookType hookType;
    }

    internal struct MixinDeclarationData {

        public RangeInt keyRange;
        public RangeInt valueRange;
        public int mixinVariableCount;
        public MixinVariableId mixinVariableStart;

    }

    internal struct TransitionDeclarationData {

        public int transitionId;
        public RangeInt customPropertyRange;
        public int propertyId;
        public int shortHandId;

    }

    internal struct TransitionDeclaration {

        public int delay;
        public int duration;
        public EasingFunction easing;
        public Bezier bezier;

    }

}