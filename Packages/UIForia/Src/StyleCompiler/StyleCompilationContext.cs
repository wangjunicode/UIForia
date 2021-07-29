using UIForia.Style;
using UIForia.Util;

namespace UIForia.Compilers {

    ///  <see cref="UIForiaStyleCompiler.StyleContext"/>  todo - could we maybe merge this with StyleContext?
    internal struct StyleCompilationContext {

        public StyleFileShell shell;

        // public LightList<CompiledStyleBlock> blockBuffer;
        public StructList<CompiledProperty> propertyBuffer;
        public StructList<CompiledTransition> transitionBuffer;
        public StructList<AnimationActionData> animationActionBuffer;
        public ManagedByteBuffer propertyValueBuffer;

        /// <summary>
        /// That is the output of <see cref="UIForiaStyleCompiler.CompileStyleNode"/>
        /// </summary>
        public StructList<CompiledStyle> compiledStyles;
        public StructList<CompiledAnimation> compiledAnimations;

        public void Release() {
            // blockBuffer.Release();
            propertyBuffer.Release();
            compiledStyles.Release();
            transitionBuffer.Release();
            animationActionBuffer.Release();
            compiledAnimations.Release();
        }

    }

}